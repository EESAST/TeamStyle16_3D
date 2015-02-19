#region

using System.Collections;
using System.Linq;
using JSON;
using UnityEngine;

#endregion

public class Replayer : MonoBehaviour
{
	private readonly string[] descriptions = { "攻击命中", "攻击丢失", "占领据点", "摧毁敌军", "采集燃料", "采集金属", "建造单位", "维修次数", "移动里程", "有效补给" };
	private readonly int[] lastScores = new int[2];
	private readonly float[] scoreFontSize = new float[2];
	private bool cancelDetail;
	//private JSONObject productionLists;
	private int currentFrame;
	private int currentRectId;
	//private JSONObject commands;
	private JSONObject elements;
	private JSONObject events;
	private bool guiInitialized;
	private Rect infoAreaRect;
	private Rect infoContentRect;
	public Texture2D panelBackground;
	private GUIStyle panelStyle;
	private GUILineGraph populationGraph;
	private bool resizingInfoRect;
	private GUILineGraph scoreGraph;
	private bool showDetail;
	private bool stagedShowDetail;
	private Vector2 summaryScroll;
	private GUILineGraph unitNumGraph;

	private void AddProductionEntries()
	{
		foreach (var productionEntryAddition in events.list.Where(productionEntryAddition => productionEntryAddition["__class__"].str == "AddProductionEntry"))
			(Instantiate(Resources.Load("ProductionEntry")) as GameObject).GetComponent<ProductionEntry>().Setup(productionEntryAddition["team"].i, productionEntryAddition["kind"].i);
	}

	private IEnumerator Attacks()
	{
		Data.Replay.IsAttacking = true;
		foreach (var attack in events.list)
			switch (attack["__class__"].str)
			{
				case "AttackUnit":
					{
						++Data.Replay.AttacksLeft;
						var attacker = Data.Replay.Elements[attack["index"].i] as UnitBase;
						attacker.StartCoroutine(attacker.AttackUnitBase(Data.Replay.Elements[attack["target"].i] as UnitBase, attack["damage"].i));
						++Data.Replay.Statictics[0, attacker.team];
					}
					break;
				case "AttackMiss":
					{
						++Data.Replay.AttacksLeft;
						var attacker = Data.Replay.Elements[attack["index"].i] as UnitBase;
						attacker.StartCoroutine(attacker.AttackPosition(Methods.Coordinates.JSONToInternal(attack["target_pos"])));
						++Data.Replay.Statictics[1, attacker.team];
					}
					break;
				case "Capture":
					++Data.Replay.AttacksLeft;
					++Data.Replay.Statictics[2, (Data.Replay.Elements[attack["index"].i] as Fort).targetTeam = attack["team"].i];
					break;
			}
		while (Data.Replay.AttacksLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		Data.Replay.IsAttacking = false;
	}

	private void Awake()
	{
		Delegates.PhysicalScreenHeightChanged += ResizeFonts;
		Delegates.ScreenSizeChanged += ResizeGUIRects;
	}

	public static IEnumerator Beam(Transform beamer, Component target, float elapsedTime)
	{
		var beamFX = (Instantiate(Resources.Load("Beam")) as GameObject).particleSystem;
		beamFX.transform.parent = beamer;
		beamFX.transform.position = beamer.GetComponent<Element>() ? beamer.WorldCenterOfElement() : beamer.position;
		beamFX.startSpeed = Settings.BeamSpeed;
		beamFX.Play();
		var element = beamer.GetComponentInParent<Element>();
		for (var startTime = Time.time; ((Time.time - startTime) / elapsedTime) < 1;)
		{
			var v = target.transform.WorldCenterOfElement() - beamFX.transform.position;
			beamFX.transform.rotation = Quaternion.LookRotation(v);
			beamFX.startLifetime = v.magnitude / beamFX.startSpeed;
			if (element is Mine)
				beamFX.startColor = new Color32(245, 245, 220, 255);
			else if (element is Oilfield)
				beamFX.startColor = Color.green;
			else
				beamFX.startColor = Data.TeamColor.Current[element.team];
			yield return null;
		}
		beamFX.Stop();
		Destroy(beamFX.gameObject, beamFX.startLifetime);
	}

	private IEnumerator Collects()
	{
		Data.Replay.IsCollecting = true;
		foreach (var collect in events.list.Where(collect => collect["__class__"].str == "Collect"))
		{
			++Data.Replay.CollectsLeft;
			var collector = Data.Replay.Elements[collect["index"].i] as Cargo;
			var fuel = collect["fuel"].i;
			var metal = collect["metal"].i;
			collector.StartCoroutine(collector.Collect(Data.Replay.Elements[collect["target"].i] as Resource, fuel, metal));
			Data.Replay.Statictics[4, collector.team] += fuel;
			Data.Replay.Statictics[5, collector.team] += metal;
		}
		while (Data.Replay.CollectsLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		Data.Replay.IsCollecting = false;
	}

	private IEnumerator Creates()
	{
		foreach (var create in events.list.Where(create => create["__class__"].str == "Create"))
		{
			++Data.Replay.CreatesLeft;
			var typeName = Constants.TypeNames[create["kind"].i];
			var unit = ((Instantiate(Resources.Load(typeName + '/' + typeName)) as GameObject).GetComponent(typeName) as Unit);
			unit.StartCoroutine(unit.Create(elements[create["index"].i.ToString()]));
			++Data.Replay.Statictics[6, unit.team];
			yield return null;
		}
		while (Data.Replay.CreatesLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		Data.Replay.IsCreating = false;
	}

	private IEnumerator Fixes()
	{
		Data.Replay.IsFixing = true;
		foreach (var fix in events.list.Where(fix => fix["__class__"].str == "Fix"))
		{
			++Data.Replay.FixesLeft;
			var fixer = Data.Replay.Elements[fix["index"].i] as Base;
			fixer.StartCoroutine(fixer.Fix(Data.Replay.Elements[fix["target"].i] as Unit, fix["metal"].i, fix["health_increase"].i));
			++Data.Replay.Statictics[7, fixer.team];
		}
		while (Data.Replay.FixesLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		Data.Replay.IsFixing = false;
	}

	private IEnumerator FortCaptureScores()
	{
		for (var i = 0; i < 2; ++i)
		{
			Data.Replay.TargetScores[i] += Constants.Score.PerFortPerRound * Data.Replay.Forts[i].Count;
			foreach (var fort in Data.Replay.Forts[i])
				StartCoroutine(ShowMessageAt(fort.transform.WorldCenterOfElement() + Vector3.up * fort.RelativeSize * Settings.Map.ScaleFactor / 2, "+ " + Constants.Score.PerFortPerRound + " pts", Settings.DefaultMessageTime));
		}
		if (Data.Replay.Forts.Any(fortList => fortList.Count > 0))
			yield return new WaitForSeconds(Settings.DefaultMessageTime);
	}

	private Rect GetInfoContentRect()
	{
		var id = -1;
		return GetInfoContentRect(ref id);
	}

	private Rect GetInfoContentRect(ref int id)
	{
		var rects = new[] { new Rect(panelStyle.border.left, panelStyle.border.top, Screen.width * 0.24f, Screen.height * 0.16f), new Rect(panelStyle.border.left, panelStyle.border.top, Screen.width * 0.24f, Screen.height * 0.32f), new Rect(panelStyle.border.left, panelStyle.border.top, Screen.width * 0.6f, Screen.height * 0.8f) };
		return rects[id = id < 0 ? stagedShowDetail ? (currentFrame == Data.Replay.FrameCount ? 2 : 1) : 0 : id];
	}

	private void InitializeGUI()
	{
		Methods.GUI.InitializeStyles();
		panelStyle = new GUIStyle { normal = { background = panelBackground }, border = new RectOffset(20, 15, 30, 15) };
		guiInitialized = true;
		ResizeGUIRects();
	}

	private IEnumerator LoadFrame(int frame, bool shallAnimate = true)
	{
		var startTime = Time.time;
		var keyFrame = Data.Battle["key_frames"]; //a key frame is the snapshot of the initial state of a round
		elements = keyFrame[frame][0]; //an object within which lie a list of key-vals, i.e elements[i] is the ith element (key-val pair)
		//productionLists = keyFrame[frame][1]; //an array comprised of two sub-arrays, standing for two teams, e.g. productionLists[1][i] stands for the ith production of team 1, which is still a two-entry array itself, i.e. [kind, framesLeft]
		//commands = Data.Battle["history"]["command"][frame - 1]; //e.g. commands[0][i] stands for the ith command (object) of team 0
		events = Data.Battle["history"]["event"][frame - 1]; //e.g. events[i] stands for the ith event (object)
		AddProductionEntries();
		yield return StartCoroutine(Attacks());
		StartCoroutine(Supplies());
		StartCoroutine(Fixes());
		while (Data.Replay.IsSupplying || Data.Replay.IsFixing)
			yield return new WaitForSeconds(Settings.DeltaTime);
		yield return StartCoroutine(Moves());
		yield return StartCoroutine(Collects());
		if (Time.time > startTime + Settings.MaxTimePerFrame)
			Debug.LogError("Additional " + (Time.time - startTime - Settings.MaxTimePerFrame) + " seconds required to handle all animations!");
		if (Data.Replay.ProductionLists.Any(productionList => productionList.Any(productionEntry => !productionEntry.ready)))
		{
			Data.Replay.ProductionTimeScale = 5;
			yield return new WaitForSeconds((startTime + Settings.MaxTimePerFrame - Time.time) / Data.Replay.ProductionTimeScale);
			Data.Replay.ProductionTimeScale = 1;
		}
		Data.Replay.ProductionTimeScale = 0;
		yield return StartCoroutine(Creates());
		yield return StartCoroutine(FortCaptureScores());
		Data.Replay.ProductionTimeScale = 1;
	}

	private IEnumerator Moves()
	{
		Data.Replay.IsMoving = true;
		foreach (var move in events.list.Where(move => move["__class__"].str == "Move"))
		{
			++Data.Replay.MovesLeft;
			var mover = Data.Replay.Elements[move["index"].i] as Unit;
			var nodes = move["nodes"];
			mover.StartCoroutine(mover.Move(nodes));
			Data.Replay.Statictics[8, mover.team] += nodes.Count - 1;
		}
		foreach (var plane in Data.Replay.Elements.Values.Select(element => element as Plane).Where(plane => plane))
			if (plane.isHovering)
				--plane.targetFuel;
			else
				plane.isHovering = true;
		while (Data.Replay.MovesLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		Data.Replay.IsMoving = false;
	}

	private void OnDestroy()
	{
		Delegates.PhysicalScreenHeightChanged -= ResizeFonts;
		Delegates.ScreenSizeChanged -= ResizeGUIRects;
	}

	private void OnGUI()
	{
		if (!guiInitialized)
			InitializeGUI();
		if (Event.current.type == EventType.Layout)
			showDetail = stagedShowDetail;
		GUILayout.BeginArea(infoAreaRect, panelStyle);
		GUILayout.BeginArea(infoContentRect);
		GUILayout.BeginHorizontal(GUILayout.Height(Data.GUI.Label.LargeMiddle.CalcHeight(GUIContent.none, 0)));
		GUILayout.Label(currentFrame == Data.Replay.FrameCount ? "比赛结束" : "第 " + currentFrame + " 回合", Data.GUI.Label.LargeMiddle);
		if (resizingInfoRect || Data.GamePaused)
			GUILayout.Button("…", Data.GUI.Button.Large, GUILayout.Width(Screen.width * 0.03f));
		else if (GUILayout.Button(showDetail ? "-" : "+", Data.GUI.Button.Large, GUILayout.Width(Screen.width * 0.03f)))
		{
			stagedShowDetail = !showDetail;
			StartCoroutine(ResizeInfoRect());
		}
		GUILayout.Space(infoContentRect.width * 0.05f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal(GUILayout.Height(Data.GUI.Label.Huge.CalcHeight(GUIContent.none, 0)));
		GUILayout.Label(Mathf.RoundToInt(Data.Replay.CurrentScores[0]).ToString(), new GUIStyle(Data.GUI.Label.TeamColored[0]) { fontSize = Mathf.RoundToInt(scoreFontSize[0]) }, GUILayout.Width(infoContentRect.width * 0.35f), GUILayout.ExpandHeight(true));
		GUILayout.Label("积分", Data.GUI.Label.SmallMiddle, GUILayout.ExpandHeight(true));
		GUILayout.Label(Mathf.RoundToInt(Data.Replay.CurrentScores[1]).ToString(), new GUIStyle(Data.GUI.Label.TeamColored[1]) { fontSize = Mathf.RoundToInt(scoreFontSize[1]) }, GUILayout.Width(infoContentRect.width * 0.35f), GUILayout.ExpandHeight(true));
		GUILayout.EndHorizontal();
		if (showDetail)
			if (currentFrame == Data.Replay.FrameCount)
			{
				summaryScroll = GUILayout.BeginScrollView(summaryScroll);
				GUILayout.Label("积分", Data.GUI.Label.LargeLeft);
				scoreGraph.Plot();
				GUILayout.Label("人口", Data.GUI.Label.LargeLeft);
				populationGraph.Plot();
				GUILayout.Label("单位", Data.GUI.Label.LargeLeft);
				unitNumGraph.Plot();
				for (var i = 0; i < 10; ++i)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(Data.Replay.Statictics[i, 0].ToString(), Data.GUI.Label.TeamColored[0], GUILayout.Width(infoContentRect.width * 0.35f));
					GUILayout.Label(descriptions[i], Data.GUI.Label.SmallMiddle);
					GUILayout.Label(Data.Replay.Statictics[i, 1].ToString(), Data.GUI.Label.TeamColored[1], GUILayout.Width(infoContentRect.width * 0.35f));
					GUILayout.EndHorizontal();
				}
				GUILayout.EndScrollView();
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(Data.Replay.Populations[0].ToString(), Data.GUI.Label.TeamColored[0], GUILayout.Width(infoContentRect.width * 0.35f));
				GUILayout.Label("人口", Data.GUI.Label.SmallMiddle);
				GUILayout.Label(Data.Replay.Populations[1].ToString(), Data.GUI.Label.TeamColored[1], GUILayout.Width(infoContentRect.width * 0.35f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label(Data.Replay.UnitNums[0].ToString(), Data.GUI.Label.TeamColored[0], GUILayout.Width(infoContentRect.width * 0.35f));
				GUILayout.Label("单位", Data.GUI.Label.SmallMiddle);
				GUILayout.Label(Data.Replay.UnitNums[1].ToString(), Data.GUI.Label.TeamColored[1], GUILayout.Width(infoContentRect.width * 0.35f));
				GUILayout.EndHorizontal();
			}
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void RefreshGraphs()
	{
		scoreGraph = new GUILineGraph(Screen.width / 2, Screen.height / 3, "score");
		unitNumGraph = new GUILineGraph(Screen.width / 2, Screen.height / 3, "unit_num");
		populationGraph = new GUILineGraph(Screen.width / 2, Screen.height / 3, "population");
	}

	private void RefreshInfoAreaRect(float t) { infoAreaRect = new Rect((Screen.width - infoContentRect.width - panelStyle.border.horizontal) / 2 * (2 - t), Mathf.Lerp(Data.MiniMap.Rect.height + Settings.MiniMap.Border.vertical, (Screen.height - infoContentRect.height - panelStyle.border.vertical) / 2, t), infoContentRect.width + panelStyle.border.horizontal, infoContentRect.height + panelStyle.border.vertical); }

	private void ResizeFonts()
	{
		for (var i = 0; i < 2; ++i)
			scoreFontSize[i] = Data.GUI.Label.TeamColored[i].fontSize;
	}

	private void ResizeGUIRects()
	{
		if (!guiInitialized)
			return;
		resizingInfoRect = false;
		infoContentRect = GetInfoContentRect();
		RefreshInfoAreaRect(currentRectId == 2 ? 1 : 0);
		RefreshGraphs();
	}

	private IEnumerator ResizeInfoRect(int targetRectId = -1, float time = 1)
	{
		resizingInfoRect = true;
		var startRect = infoContentRect;
		var targetContentRect = GetInfoContentRect(ref targetRectId);
		var from = currentRectId == 2 ? 1 : 0;
		var to = targetRectId == 2 ? 1 : 0;
		currentRectId = targetRectId;
		for (float t = 0, startTime = Time.unscaledTime; 1 - (t = Mathf.Lerp(t, 1, (Time.unscaledTime - startTime) / time)) > Settings.Tolerance;)
		{
			if (!resizingInfoRect)
				yield break;
			infoContentRect = Methods.RectLerp(startRect, targetContentRect, t);
			RefreshInfoAreaRect(Mathf.Lerp(from, to, t));
			yield return null;
		}
		infoContentRect = targetContentRect;
		RefreshInfoAreaRect(to);
		resizingInfoRect = false;
	}

	public static IEnumerator ShowMessageAt(Vector3 position, string message, float elapsedTime)
	{
		var textFX = (Instantiate(Resources.Load("TextFX")) as GameObject).GetComponent<EffectManager>();
		textFX.transform.position = position;
		textFX.SetText(message);
		textFX.PlayAnimation();
		for (var startTime = Time.time; ((Time.time - startTime) / elapsedTime) < 1&&textFX;)
		{
			textFX.transform.rotation = Quaternion.LookRotation(position - Camera.main.transform.position);
			yield return null;
		}
	}

	private IEnumerator ShowSummary()
	{
		if (stagedShowDetail)
		{
			stagedShowDetail = false;
			yield return StartCoroutine(ResizeInfoRect());
		}
		stagedShowDetail = true;
		StartCoroutine(ResizeInfoRect());
	}

	private IEnumerator Start()
	{
		for (var i = 0; i < 2; ++i)
		{
			if (!Data.Battle["team_names"][i].IsNull)
				Data.Replay.TeamNames[i] = Data.Battle["team_names"][i].str;
			Data.Replay.CurrentScores[i] = Data.Replay.TargetScores[i] = lastScores[i] = Data.Battle["history"]["score"][i].i;
		}
		while (++currentFrame < Data.Replay.FrameCount)
			yield return StartCoroutine(LoadFrame(currentFrame));
		StartCoroutine(ShowSummary());
	}

	private IEnumerator Supplies()
	{
		Data.Replay.IsSupplying = true;
		foreach (var supply in events.list.Where(supply => supply["__class__"].str == "Supply"))
		{
			++Data.Replay.SuppliesLeft;
			var supplier = Data.Replay.Elements[supply["index"].i] as UnitBase;
			var fuel = supply["fuel"].i;
			var ammo = supply["ammo"].i;
			var metal = supply["metal"].i;
			supplier.StartCoroutine(supplier.Supply(Data.Replay.Elements[supply["target"].i] as UnitBase, fuel, ammo, metal));
			if (fuel + ammo + metal > 0)
				++Data.Replay.Statictics[9, supplier.team];
		}
		while (Data.Replay.SuppliesLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		Data.Replay.IsSupplying = false;
	}

	private void Update()
	{
		Data.Replay.ProductionTimer += Time.smoothDeltaTime * Data.Replay.ProductionTimeScale;
		if (!guiInitialized)
			return;
		if (showDetail && currentFrame == Data.Replay.FrameCount && Input.GetKeyUp(KeyCode.Escape))
			cancelDetail = true;
		if (cancelDetail && !resizingInfoRect)
		{
			stagedShowDetail = false;
			StartCoroutine(ResizeInfoRect());
			cancelDetail = false;
		}
		for (var i = 0; i < 2; ++i)
		{
			if (lastScores[i] != Mathf.RoundToInt(Data.Replay.CurrentScores[i]))
			{
				scoreFontSize[i] = Data.GUI.Label.Huge.fontSize;
				lastScores[i] = Mathf.RoundToInt(Data.Replay.CurrentScores[i]);
			}
			if (Mathf.Abs(Data.GUI.Label.TeamColored[i].fontSize - scoreFontSize[i]) > Settings.Tolerance)
				scoreFontSize[i] = Mathf.Lerp(scoreFontSize[i], Data.GUI.Label.TeamColored[i].fontSize, Settings.TransitionRate * Time.smoothDeltaTime);
		}
	}

	private struct GUILineGraph
	{
		private readonly Texture2D graph;

		public GUILineGraph(int width, int height, string key)
		{
			graph = new Texture2D(width, height);
			for (var x = 0; x < graph.width; ++x)
				for (var y = 0; y < graph.height; ++y)
					graph.SetPixel(x, y, x % 10 == 0 || y % 5 == 0 ? Color.gray : Color.black);
			var values = Data.Battle["history"][key];
			float maxVal = 0;
			for (var i = 0; i < Data.Replay.FrameCount; ++i)
				for (var j = 0; j < 2; ++j)
					maxVal = Mathf.Max(maxVal, values[i][j].n);
			var deltaX = (float)width / Data.Replay.FrameCount;
			var deltaY = height / (maxVal + 1);
			var p0 = new Vector2(0, values[0][0].n * deltaY);
			var p1 = new Vector2(0, values[0][1].n * deltaY);
			for (var i = 1; i < Data.Replay.FrameCount; ++i)
				if (values[i][0].i == values[i][1].i && values[i - 1][0].i == values[i - 1][1].i)
				{
					var p = new Vector2(i * deltaX, values[i][0].n * deltaY);
					var c = (Data.TeamColor.Target[0] + Data.TeamColor.Target[1]) / 2;
					graph.Line(p0, p, c, 1);
					graph.Line(p1, p, c, 1);
					p1 = p0 = p;
				}
				else
				{
					var p = new Vector2(i * deltaX, values[i][0].n * deltaY);
					graph.Line(p0, p, Data.TeamColor.Target[0], 1);
					p0 = p;
					p = new Vector2(i * deltaX, values[i][1].n * deltaY);
					graph.Line(p1, p, Data.TeamColor.Target[1], 1);
					p1 = p;
				}
			graph.Apply();
		}

		public void Plot() { GUILayout.Box(graph); }
	};
}