#region

using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public class Replayer : MonoBehaviour
{
	//private JSONObject productionLists;
	private int currentFrame;
	//private JSONObject commands;
	private JSONObject elements;
	private JSONObject events;
	private int frameCount;
	private bool guiInitialized;
	private Rect infoAreaRect;
	private Rect infoContentRect;
	public Texture2D panelBackground;
	private GUIStyle panelStyle;

	private void AddProductionEntries()
	{
		foreach (var productionEntryAddition in events.list.Where(productionEntryAddition => productionEntryAddition["__class__"].str == "AddProductionEntry"))
			(Instantiate(Resources.Load("ProductionEntry")) as GameObject).GetComponent<ProductionEntry>().Setup(Mathf.RoundToInt(productionEntryAddition["team"].n), Mathf.RoundToInt(productionEntryAddition["kind"].n));
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
						var attacker = Data.Replay.Elements[Mathf.RoundToInt(attack["index"].n)] as UnitBase;
						var target = Data.Replay.Elements[Mathf.RoundToInt(attack["target"].n)] as UnitBase;
						attacker.StartCoroutine(attacker.FireAtUnitBase(target, Mathf.RoundToInt(attack["damage"].n)));
					}
					break;
				case "AttackMiss":
					{
						++Data.Replay.AttacksLeft;
						var attacker = Data.Replay.Elements[Mathf.RoundToInt(attack["index"].n)] as UnitBase;
						attacker.StartCoroutine(attacker.FireAtPosition(Methods.Coordinates.JSONToInternal(attack["target_pos"])));
					}
					break;
				case "Capture":
					++Data.Replay.AttacksLeft;
					var fort = Data.Replay.Elements[Mathf.RoundToInt(attack["index"].n)] as Fort;
					fort.targetTeam = Mathf.RoundToInt(attack["team"].n);
					break;
			}
		while (Data.Replay.AttacksLeft > 0)
			yield return new WaitForSeconds(0.04f);
		Data.Replay.IsAttacking = false;
	}

	private void Awake() { Delegates.ScreenSizeChanged += ResizeGUIRects; }

	private IEnumerator Collects()
	{
		Data.Replay.IsCollecting = true;
		foreach (var collect in events.list.Where(collect => collect["__class__"].str == "Collect"))
		{
			++Data.Replay.CollectsLeft;
			var collector = Data.Replay.Elements[Mathf.RoundToInt(collect["index"].n)] as Cargo;
			var target = Data.Replay.Elements[Mathf.RoundToInt(collect["target"].n)] as Resource;
			var fuel = Mathf.RoundToInt(collect["fuel"].n);
			var metal = Mathf.RoundToInt(collect["metal"].n);
			collector.StartCoroutine(collector.Collect(target, fuel, metal));
		}
		while (Data.Replay.CollectsLeft > 0)
			yield return new WaitForSeconds(0.04f);
		Data.Replay.IsCollecting = false;
	}

	private void Creates()
	{
		foreach (var create in events.list.Where(create => create["__class__"].str == "Create"))
		{
			var typeName = Constants.TypeNames[Mathf.RoundToInt(create["kind"].n)];
			((Instantiate(Resources.Load(typeName + '/' + typeName)) as GameObject).GetComponent(typeName) as Unit).Initialize(elements[Mathf.RoundToInt(create["index"].n).ToString()]);
		}
	}

	private IEnumerator Fixes()
	{
		Data.Replay.IsFixing = true;
		foreach (var fix in events.list.Where(fix => fix["__class__"].str == "Fix"))
		{
			++Data.Replay.FixesLeft;
			var fixer = Data.Replay.Elements[Mathf.RoundToInt(fix["index"].n)] as Base;
			var target = Data.Replay.Elements[Mathf.RoundToInt(fix["target"].n)] as Unit;
			fixer.StartCoroutine(fixer.Fix(target, Mathf.RoundToInt(fix["metal"].n), Mathf.RoundToInt(fix["health_increase"].n)));
		}
		while (Data.Replay.FixesLeft > 0)
			yield return new WaitForSeconds(0.04f);
		Data.Replay.IsFixing = false;
	}

	private void FortCaptureScores()
	{
		for (var i = 0; i < 2; ++i)
			Data.Replay.TargetScores[i] += Constants.Score.PerFortPerFrame * Data.FortNum[i];
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
			yield return new WaitForSeconds(0.04f);
		yield return StartCoroutine(Moves());
		yield return StartCoroutine(Collects());
		if (Time.time > startTime + Settings.MaxTimePerFrame)
			Debug.LogError("Additional " + (Time.time - startTime - Settings.MaxTimePerFrame) + " seconds required to handle all animations!");
		if (Data.Replay.ProductionLists.Any(productionList => productionList.Any(productionEntry => !productionEntry.ready)))
		{
			Data.Replay.ProductionTimeScale = 10;
			yield return new WaitForSeconds((startTime + Settings.MaxTimePerFrame - Time.time) / Data.Replay.ProductionTimeScale);
			Data.Replay.ProductionTimeScale = 1;
		}
		Data.Replay.ProductionPaused = true;
		Creates();
		FortCaptureScores();
		Data.Replay.ProductionPaused = false;
	}

	private IEnumerator Moves()
	{
		Data.Replay.IsMoving = true;
		foreach (var move in events.list.Where(move => move["__class__"].str == "Move"))
		{
			++Data.Replay.MovesLeft;
			var mover = Data.Replay.Elements[Mathf.RoundToInt(move["index"].n)] as Unit;
			var nodes = move["nodes"];
			mover.StartCoroutine(mover.Move(nodes));
		}
		while (Data.Replay.MovesLeft > 0)
			yield return new WaitForSeconds(0.04f);
		Data.Replay.IsMoving = false;
	}

	private void OnDestroy() { Delegates.ScreenSizeChanged -= ResizeGUIRects; }

	private void OnGUI()
	{
		if (!guiInitialized)
			InitializeGUI();
		GUILayout.BeginArea(infoAreaRect, panelStyle);
		GUILayout.BeginArea(infoContentRect);
		GUILayout.Label(currentFrame == frameCount ? "比赛结束" : "第 " + currentFrame + " 回合", Data.GUI.Label.LargeMiddle);
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.Label(Mathf.RoundToInt(Data.Replay.CurrentScores[0]).ToString(), Data.GUI.Label.TeamColored[0], GUILayout.Width(infoContentRect.width * 0.4f));
		GUILayout.Label(":", Data.GUI.Label.SmallMiddle);
		GUILayout.Label(Mathf.RoundToInt(Data.Replay.CurrentScores[1]).ToString(), Data.GUI.Label.TeamColored[1], GUILayout.Width(infoContentRect.width * 0.4f));
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void ResizeGUIRects()
	{
		if (!guiInitialized)
			return;
		infoContentRect = new Rect(panelStyle.border.left, panelStyle.border.top, Screen.width * 0.15f, Screen.height * 0.15f);
		infoAreaRect = new Rect((Screen.width - infoContentRect.width - panelStyle.border.horizontal) / 2, 0, infoContentRect.width + panelStyle.border.horizontal, infoContentRect.height + panelStyle.border.vertical);
	}

	private IEnumerator Start()
	{
		frameCount = Data.Battle["key_frames"].Count;
		while (++currentFrame < frameCount)
			yield return StartCoroutine(LoadFrame(currentFrame));
	}

	private IEnumerator Supplies()
	{
		Data.Replay.IsSupplying = true;
		foreach (var supply in events.list.Where(supply => supply["__class__"].str == "Supply"))
		{
			++Data.Replay.SuppliesLeft;
			var supplier = Data.Replay.Elements[Mathf.RoundToInt(supply["index"].n)] as UnitBase;
			var target = Data.Replay.Elements[Mathf.RoundToInt(supply["target"].n)] as UnitBase;
			var fuel = Mathf.RoundToInt(supply["fuel"].n);
			var ammo = Mathf.RoundToInt(supply["ammo"].n);
			var metal = Mathf.RoundToInt(supply["metal"].n);
			supplier.StartCoroutine(supplier.Supply(target, fuel, ammo, metal));
		}
		while (Data.Replay.SuppliesLeft > 0)
			yield return new WaitForSeconds(0.04f);
		Data.Replay.IsSupplying = false;
	}
}