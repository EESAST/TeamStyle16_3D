#region

using System.Collections;
using System.Linq;
using JSON;
using UnityEngine;

#endregion

public class Replayer : MonoBehaviour
{
	private readonly int[] lastScores = new int[2];
	private readonly float[] scoreFontSize = new float[2];
	private bool cancelDetail;
	private int currentFrame;
	private int currentRectId;
	private JSONObject elements;
	private JSONObject events;
	private int frameSlide;
	private Rect frameSlideRect;
	private bool guiInitialized;
	private bool hideFrameSlide;
	private Rect infoAreaRect;
	private Rect infoContentRect;
	private bool IsAttacking;
	private bool IsCollecting;
	private bool IsCreating;
	private bool IsFixing;
	private bool IsMoving;
	private bool IsSupplying;
	public Texture2D panelBackground;
	private GUIStyle panelStyle;
	private LineChart populationChart;
	private IEnumerator replay;
	private bool resizingInfoRect;
	private LineChart scoreChart;
	private bool showDetail;
	private bool stagedShowDetail;
	private Vector2 summaryScroll;
	private LineChart unitNumChart;

	private void AddProductionEntries()
	{
		foreach (var productionEntryAddition in events.list.Where(productionEntryAddition => productionEntryAddition["__class__"].str == "AddProductionEntry"))
			(Instantiate(Resources.Load("ProductionEntry")) as GameObject).GetComponent<ProductionEntry>().Initialize(productionEntryAddition["team"].i, productionEntryAddition["kind"].i);
	}

	private IEnumerator Attacks()
	{
		IsAttacking = true;
		foreach (var attack in events.list)
			switch (attack["__class__"].str)
			{
				case "AttackUnit":
					{
						++Data.Replay.AttacksLeft;
						var attacker = Data.Replay.Elements[attack["index"].i] as UnitBase;
						attacker.StartCoroutine(attacker.AttackUnitBase(Data.Replay.Elements[attack["target"].i] as UnitBase, attack["damage"].i));
					}
					break;
				case "AttackMiss":
					{
						++Data.Replay.AttacksLeft;
						var attacker = Data.Replay.Elements[attack["index"].i] as UnitBase;
						attacker.StartCoroutine(attacker.AttackPosition(Methods.Coordinates.JSONToInternal(attack["target_pos"])));
					}
					break;
				case "Capture":
					++Data.Replay.AttacksLeft;
					(Data.Replay.Elements[attack["index"].i] as Fort).targetTeam = attack["team"].i;
					break;
			}
		while (Data.Replay.AttacksLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		IsAttacking = false;
	}

	private void Awake()
	{
		Delegates.ScreenSizeChanged += ResizeGUI;
		Data.Replay.Instance = this;
	}

	private IEnumerator Collects()
	{
		IsCollecting = true;
		foreach (var collect in events.list.Where(collect => collect["__class__"].str == "Collect"))
		{
			++Data.Replay.CollectsLeft;
			var collector = Data.Replay.Elements[collect["index"].i] as Cargo;
			collector.StartCoroutine(collector.Collect(Data.Replay.Elements[collect["target"].i] as Resource, collect["fuel"].i, collect["metal"].i));
		}
		while (Data.Replay.CollectsLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		IsCollecting = false;
	}

	private IEnumerator Creates()
	{
		foreach (var create in events.list.Where(create => create["__class__"].str == "Create"))
		{
			++Data.Replay.CreatesLeft;
			var typeName = Constants.TypeNames[create["kind"].i];
			var unit = ((Instantiate(Resources.Load(typeName + '/' + typeName)) as GameObject).GetComponent(typeName) as Unit);
			unit.StartCoroutine(unit.Create(elements[create["index"].i.ToString()]));
			yield return null;
		}
		while (Data.Replay.CreatesLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		IsCreating = false;
	}

	private IEnumerator Fixes()
	{
		IsFixing = true;
		foreach (var fix in events.list.Where(fix => fix["__class__"].str == "Fix"))
		{
			++Data.Replay.FixesLeft;
			var fixer = Data.Replay.Elements[fix["index"].i] as Base;
			fixer.StartCoroutine(fixer.Fix(Data.Replay.Elements[fix["target"].i] as Unit, fix["metal"].i, fix["health_increase"].i));
		}
		while (Data.Replay.FixesLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		IsFixing = false;
	}

	private IEnumerator FortCaptureScores()
	{
		for (var i = 0; i < 2; ++i)
		{
			Data.Replay.TargetScores[i] += Constants.Score.PerFortPerRound * Data.Replay.Forts[i].Count;
			foreach (var fort in Data.Replay.Forts[i])
			{
				StartCoroutine(ShowMessageAt(fort.TopCenter() + Settings.MessagePositionOffset, "PTS: +" + Constants.Score.PerFortPerRound));
				fort.audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Fort_Score"));
			}
		}
		if (Data.Replay.Forts.Any(fortList => fortList.Count > 0))
			yield return new WaitForSeconds(Settings.Replay.MessageTime);
	}

	private Rect GetInfoContentRect()
	{
		var id = -1;
		return GetInfoContentRect(ref id);
	}

	private Rect GetInfoContentRect(ref int id)
	{
		var rects = new[] { new Rect(panelStyle.border.left, panelStyle.border.top, Screen.width * 0.28f, Screen.height * 0.16f), new Rect(panelStyle.border.left, panelStyle.border.top, Screen.width * 0.28f, Screen.height * 0.28f), new Rect(panelStyle.border.left, panelStyle.border.top, Screen.width * 0.6f, Screen.height * 0.8f) };
		return rects[id = id < 0 ? stagedShowDetail ? (currentFrame == Data.Replay.FrameCount ? 2 : 1) : 0 : id];
	}

	private void InitializeGUI()
	{
		Methods.GUI.InitializeStyles();
		panelStyle = new GUIStyle { normal = { background = panelBackground }, border = new RectOffset(20, 15, 30, 15) };
		guiInitialized = true;
		ResizeGUI();
	}

	private void LoadFrame(int frame)
	{
		StopCoroutine(replay);
		Methods.Replay.ClearData();
		var keyFrame = Data.Battle["key_frames"][frame]; //a key frame is the snapshot of the end state of a round, ranging from 0 to frameCount-1
		for (var i = 0; i < Data.Replay.Elements.Count; ++i)
		{
			var item = Data.Replay.Elements.ElementAt(i);
			var fort = item.Value as Fort;
			if (fort)
				fort.targetTeam = -1;
			Data.Replay.Elements.Remove(item.Key);
			Destroy(item.Value.gameObject);
			--i;
		}
		foreach (var doodad in GameObject.FindGameObjectsWithTag("Doodad"))
			Destroy(doodad);
		foreach (var entry in keyFrame[0].list)
		{
			var typeName = entry["__class__"].str;
			((Instantiate(Resources.Load(typeName + '/' + typeName)) as GameObject).GetComponent(typeName) as Element).Initialize(entry);
		}
		for (var i = 0; i < 2; ++i)
			Data.Replay.CurrentScores[i] = Data.Replay.TargetScores[i] = lastScores[i] = Data.Battle["history"]["score"][frame][i].i;
		foreach (var productionList in Data.Replay.ProductionLists)
		{
			foreach (var productionEntry in productionList)
				Destroy(productionEntry.gameObject);
			productionList.Clear();
		}
		for (var i = 0; i < 2; ++i)
			foreach (var productionEntry in keyFrame[1][i].list)
				(Instantiate(Resources.Load("ProductionEntry")) as GameObject).GetComponent<ProductionEntry>().Initialize(i, productionEntry[0].i, productionEntry[1].i);
		StartCoroutine(replay = Replay());
	}

	private IEnumerator Moves()
	{
		IsMoving = true;
		foreach (var move in events.list.Where(move => move["__class__"].str == "Move"))
		{
			++Data.Replay.MovesLeft;
			var mover = Data.Replay.Elements[move["index"].i] as Unit;
			var nodes = move["nodes"];
			mover.StartCoroutine(mover.Move(nodes));
		}
		foreach (var plane in Data.Replay.Elements.Values.Select(element => element as Plane).Where(plane => plane))
			if (plane.isHovering)
				--plane.targetFuel;
			else
				plane.isHovering = true;
		while (Data.Replay.MovesLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		IsMoving = false;
	}

	private void OnDestroy() { Delegates.ScreenSizeChanged -= ResizeGUI; }

	private void OnGUI()
	{
		if (!guiInitialized)
			InitializeGUI();
		if (Event.current.type == EventType.Layout)
			showDetail = stagedShowDetail;
		GUILayout.BeginArea(infoAreaRect, panelStyle);
		GUILayout.BeginArea(infoContentRect);
		GUILayout.BeginHorizontal(GUILayout.Height(Data.GUI.Label.LargeMiddle.CalcHeight(GUIContent.none, 0)));
		GUILayout.Label(frameSlide == Data.Replay.FrameCount ? "比赛结束" : "第 " + frameSlide + " 回合", Data.GUI.Label.LargeMiddle);
		if (resizingInfoRect || Data.GamePaused)
			GUILayout.Button("…", Data.GUI.Button.Large, GUILayout.Width(Screen.width * 0.03f));
		else if (GUILayout.Button(showDetail ? "-" : "+", Data.GUI.Button.Large, GUILayout.Width(Screen.width * 0.03f)))
		{
			stagedShowDetail = !showDetail;
			StartCoroutine(ResizeInfoRect());
		}
		GUILayout.Space(infoContentRect.width * 0.05f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal(GUILayout.Height(Data.GUI.Label.HugeMiddle.CalcHeight(GUIContent.none, 0)));
		GUILayout.Label(Mathf.RoundToInt(Data.Replay.CurrentScores[0]).ToString(), new GUIStyle(Data.GUI.Label.TeamColored[0]) { fontSize = Mathf.RoundToInt(scoreFontSize[0]) }, GUILayout.Width(infoContentRect.width * 0.35f), GUILayout.ExpandHeight(true));
		GUILayout.Label("积分", Data.GUI.Label.SmallMiddle, GUILayout.ExpandHeight(true));
		GUILayout.Label(Mathf.RoundToInt(Data.Replay.CurrentScores[1]).ToString(), new GUIStyle(Data.GUI.Label.TeamColored[1]) { fontSize = Mathf.RoundToInt(scoreFontSize[1]) }, GUILayout.Width(infoContentRect.width * 0.35f), GUILayout.ExpandHeight(true));
		GUILayout.EndHorizontal();
		if (showDetail)
			if (currentFrame == Data.Replay.FrameCount)
			{
				summaryScroll = GUILayout.BeginScrollView(summaryScroll);
				GUILayout.Label("积分", Data.GUI.Label.LargeLeft);
				scoreChart.Plot();
				GUILayout.Label("人口", Data.GUI.Label.LargeLeft);
				populationChart.Plot();
				GUILayout.Label("单位", Data.GUI.Label.LargeLeft);
				unitNumChart.Plot();
				GUILayout.EndScrollView();
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(Data.Replay.Populations[0] + " / " + Data.Replay.MaxPopulation, Data.GUI.Label.TeamColored[0], GUILayout.Width(infoContentRect.width * 0.35f));
				GUILayout.Label("人口", Data.GUI.Label.SmallMiddle);
				GUILayout.Label(Data.Replay.Populations[1] + " / " + Data.Replay.MaxPopulation, Data.GUI.Label.TeamColored[1], GUILayout.Width(infoContentRect.width * 0.35f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label(Data.Replay.UnitNums[0].ToString(), Data.GUI.Label.TeamColored[0], GUILayout.Width(infoContentRect.width * 0.35f));
				GUILayout.Label("单位", Data.GUI.Label.SmallMiddle);
				GUILayout.Label(Data.Replay.UnitNums[1].ToString(), Data.GUI.Label.TeamColored[1], GUILayout.Width(infoContentRect.width * 0.35f));
				GUILayout.EndHorizontal();
			}
		GUILayout.EndArea();
		GUILayout.EndArea();
		if (hideFrameSlide = currentFrame == Data.Replay.FrameCount && (resizingInfoRect || currentRectId == 2))
			return;
		frameSlideRect = new Rect(infoAreaRect.xMin, infoAreaRect.yMax, infoAreaRect.width, GUI.skin.horizontalSlider.CalcHeight(GUIContent.none, 0) * 2);
		GUILayout.BeginArea(frameSlideRect, GUI.skin.box);
		frameSlide = Mathf.RoundToInt(GUILayout.HorizontalSlider(frameSlide, 0, Data.Replay.FrameCount - 1));
		GUILayout.EndArea();
	}

	private void RefreshCharts()
	{
		scoreChart = new LineChart(Screen.width / 2, Screen.height / 3, "score");
		unitNumChart = new LineChart(Screen.width / 2, Screen.height / 3, "unit_num");
		populationChart = new LineChart(Screen.width / 2, Screen.height / 3, "population");
	}

	private void RefreshInfoAreaRect(float t) { infoAreaRect = new Rect((Screen.width - infoContentRect.width - panelStyle.border.horizontal) / 2 * (2 - t), Mathf.Lerp(Data.MiniMap.MapRect.height + Settings.MiniMap.Border.vertical, (Screen.height - infoContentRect.height - panelStyle.border.vertical) / 2, t), infoContentRect.width + panelStyle.border.horizontal, infoContentRect.height + panelStyle.border.vertical); }

	private IEnumerator Replay()
	{
		yield return new WaitForSeconds(1);
		while ((frameSlide = ++currentFrame) < Data.Replay.FrameCount)
		{
			var startTime = Time.time;
			elements = Data.Battle["key_frames"][currentFrame][0]; //an object within which lie a list of key-vals, i.e elements[i] is the ith element (key-val pair)
			events = Data.Battle["history"]["event"][currentFrame - 1]; //e.g. events[i] is the ith event (object)
			AddProductionEntries();
			yield return StartCoroutine(Attacks());
			StartCoroutine(Supplies());
			StartCoroutine(Fixes());
			while (IsSupplying || IsFixing)
				yield return null;
			yield return StartCoroutine(Moves());
			yield return StartCoroutine(Collects());
			if (Data.Replay.ProductionLists.Any(productionList => productionList.Any(productionEntry => !productionEntry.ready)))
			{
				Data.Replay.ProductionTimeScale = 10;
				yield return new WaitForSeconds((startTime + Settings.MaxTimePerFrame - Time.time) / Data.Replay.ProductionTimeScale);
			}
			Data.Replay.ProductionTimeScale = 0;
			yield return StartCoroutine(Creates());
			yield return StartCoroutine(FortCaptureScores());
			Data.Replay.ProductionTimeScale = 1;
		}
		yield return new WaitForSeconds(Settings.DeltaTime);
		StartCoroutine(ShowSummary());
	}

	private void ResizeGUI()
	{
		if (!guiInitialized)
			return;
		for (var i = 0; i < 2; ++i)
			scoreFontSize[i] = Data.GUI.Label.TeamColored[i].fontSize;
		resizingInfoRect = false;
		infoContentRect = GetInfoContentRect();
		RefreshInfoAreaRect(currentRectId == 2 ? 1 : 0);
		RefreshCharts();
	}

	private IEnumerator ResizeInfoRect(int targetRectId = -1, float time = 1)
	{
		resizingInfoRect = true;
		var startRect = infoContentRect;
		var targetContentRect = GetInfoContentRect(ref targetRectId);
		var from = currentRectId == 2 ? 1 : 0;
		var to = targetRectId == 2 ? 1 : 0;
		Data.Replay.ShowSummary = targetRectId == 2;
		currentRectId = targetRectId;
		Camera.main.audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/InfoBox_" + (targetRectId == 0 ? "Out" : "In") + "Come"));
		for (float t = 0, startTime = Time.unscaledTime; 1 - (t = Mathf.Lerp(t, 1, (Time.unscaledTime - startTime) / time)) > Settings.Tolerance;)
		{
			if (!resizingInfoRect)
				yield break;
			infoContentRect = startRect.RectLerp(targetContentRect, t);
			RefreshInfoAreaRect(Mathf.Lerp(from, to, t));
			yield return null;
		}
		infoContentRect = targetContentRect;
		RefreshInfoAreaRect(to);
		resizingInfoRect = false;
	}

	public IEnumerator ShowMessageAt(Element element, string message) { yield return StartCoroutine(ShowMessageAt(element.TopCenter() + Settings.MessagePositionOffset, message)); }

	public IEnumerator ShowMessageAt(Vector3 position, string message)
	{
		var textFX = (Instantiate(Resources.Load("TextFX")) as GameObject).GetComponent<EffectManager>();
		textFX.transform.position = position;
		textFX.SetText(message);
		textFX.PlayAnimation();
		while (textFX)
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
		if (Data.GamePaused)
			yield break;
		stagedShowDetail = true;
		StartCoroutine(ResizeInfoRect());
	}

	private void Start()
	{
		for (var i = 0; i < 2; ++i)
		{
			if (!Data.Battle["team_names"][i].IsNull)
				Data.Replay.TeamNames[i] = Data.Battle["team_names"][i].str;
			Data.Replay.CurrentScores[i] = Data.Replay.TargetScores[i] = lastScores[i] = Data.Battle["history"]["score"][0][i].i;
		}
		StartCoroutine(replay = Replay());
	}

	private IEnumerator Supplies()
	{
		IsSupplying = true;
		foreach (var supply in events.list.Where(supply => supply["__class__"].str == "Supply"))
		{
			++Data.Replay.SuppliesLeft;
			var supplier = Data.Replay.Elements[supply["index"].i] as UnitBase;
			supplier.StartCoroutine(supplier.Supply(Data.Replay.Elements[supply["target"].i] as UnitBase, supply["fuel"].i, supply["ammo"].i, supply["metal"].i));
		}
		while (Data.Replay.SuppliesLeft > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
		IsSupplying = false;
	}

	private void Update()
	{
		if (!guiInitialized)
			return;
		if (currentFrame != frameSlide && !Input.GetMouseButton(0))
		{
			LoadFrame(currentFrame = frameSlide); //Loads the end state of the designated frame
			return;
		}
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
				scoreFontSize[i] = Data.GUI.Label.HugeMiddle.fontSize;
				lastScores[i] = Mathf.RoundToInt(Data.Replay.CurrentScores[i]);
			}
			if (Mathf.Abs(Data.GUI.Label.TeamColored[i].fontSize - scoreFontSize[i]) > Settings.Tolerance)
				scoreFontSize[i] = Mathf.Lerp(scoreFontSize[i], Data.GUI.Label.TeamColored[i].fontSize, Settings.TransitionRate * Time.deltaTime);
		}
		Data.GUI.OccupiedRects.Add(infoAreaRect);
		if (!hideFrameSlide)
			Data.GUI.OccupiedRects.Add(frameSlideRect);
	}

	private struct LineChart
	{
		private readonly Texture2D chart;

		public LineChart(int width, int height, string key)
		{
			chart = new Texture2D(width, height);
			for (var x = 0; x < chart.width; ++x)
				for (var y = 0; y < chart.height; ++y)
					chart.SetPixel(x, y, x % 10 == 0 || y % 5 == 0 ? Color.gray : Color.black);
			var values = Data.Battle["history"][key];
			var maxVal = values.list.Max(valueList => valueList.list.Max(value => value.n));
			var deltaX = (float)width / Data.Replay.FrameCount * 0.98f;
			var deltaY = height / maxVal * 0.95f;
			var p0 = new Vector2(0, values[0][0].n * deltaY);
			var p1 = new Vector2(0, values[0][1].n * deltaY);
			for (var i = 1; i < Data.Replay.FrameCount; ++i)
				if (values[i][0].i == values[i][1].i && values[i - 1][0].i == values[i - 1][1].i)
				{
					var p = new Vector2(i * deltaX, values[i][0].n * deltaY);
					var c = (Data.TeamColor.Target[0] + Data.TeamColor.Target[1]) / 2;
					chart.Line(p0, p, c, Data.GUI.LineThickness);
					chart.Line(p1, p, c, Data.GUI.LineThickness);
					p1 = p0 = p;
				}
				else
				{
					var p = new Vector2(i * deltaX, values[i][0].n * deltaY);
					chart.Line(p0, p, Data.TeamColor.Target[0], Data.GUI.LineThickness);
					p0 = p;
					p = new Vector2(i * deltaX, values[i][1].n * deltaY);
					chart.Line(p1, p, Data.TeamColor.Target[1], Data.GUI.LineThickness);
					p1 = p;
				}
			chart.Apply();
		}

		public void Plot() { GUILayout.Box(chart); }
	};
}