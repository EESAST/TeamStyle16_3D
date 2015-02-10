#region

using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public class Replayer : MonoBehaviour
{
	//private readonly float defaultDeltaTime = 1;
	//private readonly float timePerFrame = 2; // 2 second(s)
	//private JSONObject commands;
	//private float dt, elapsedTime;
	private JSONObject elements;
	private JSONObject events;
	private bool guiInitialized;
	private Rect infoAreaRect;
	private Rect infoContentRect;
	public Texture2D panelBackground;
	private GUIStyle panelStyle;
	//private JSONObject productionLists;
	private int round = -1;
	private int roundCount;

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
		var scores = round == roundCount ? Data.Battle["gamebody"]["scores"] : Data.Battle["history"]["score"][round];
		Data.Replay.TargetScores[0] = Mathf.RoundToInt(scores[0].n);
		Data.Replay.TargetScores[1] = Mathf.RoundToInt(scores[1].n);
		if (round == 0)
			return;
		foreach (var create in events.list.Where(create => create["__class__"].str == "Create"))
		{
			var typeName = Constants.TypeNames[Mathf.RoundToInt(create["kind"].n)];
			var info = (round == roundCount ? Data.Battle["gamebody"]["map_info"]["elements"] : elements)[Mathf.RoundToInt(create["index"].n).ToString()];
			((Instantiate(Resources.Load(typeName + '/' + typeName)) as GameObject).GetComponent(typeName) as Element).Initialize(info);
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

	private void InitializeGUI()
	{
		Methods.GUI.InitializeStyles();
		panelStyle = new GUIStyle { normal = { background = panelBackground }, border = new RectOffset(20, 15, 30, 15) };
		guiInitialized = true;
		ResizeGUIRects();
	}

	private IEnumerator LoadFrame( /*int round, bool shallAnimate = true*/)
	{
		//Data.Game.Ready = false;
		//Data.Replay.CurrentScores = Data.Battle["history"]["score"][round];
		//Data.Replay.Populations = Data.Battle["history"]["population"][round]; //TODO:these should be updated during the corresponding period, not in the beginning
		var keyFrame = Data.Battle["key_frames"]; //a key frame is the snapshot of the initial state of a round, which means the last key frame can't reflect the final state of the game, requiring the additional Data.Battle["gamebody"]
		elements = keyFrame[round][0]; //an object within which lie a list of key-vals, i.e elements[i] is the ith element (key-val pair)
		//productionLists = keyFrame[round][1]; //an array comprised of two sub-arrays, stands for two teams, e.g. productionLists[1][i] stands for the ith production of team 1, which is still a two-entry array itself, i.e. [kind, roundLeft]
		Creates();

		//commands = Data.Battle["history"]["command"][round]; //e.g. commands[0][i] stands for the ith command (object) of team 0
		events = Data.Battle["history"]["event"][round]; //e.g. events[i] stands for the ith event (object)

		AddProductionEntries();
		yield return StartCoroutine(Attacks());
		StartCoroutine(Supplies());
		StartCoroutine(Fixes());
		while (Data.Replay.IsSupplying || Data.Replay.IsFixing)
			yield return new WaitForSeconds(0.04f);
		yield return StartCoroutine(Moves());
		StartCoroutine(Collects());
	}

	private void LoadNextFrame()
	{
		++round;
		if (round < roundCount)
			StartCoroutine(LoadFrame());
		else
		{
			Creates();
			CancelInvoke("LoadNextFrame");
		}
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

	private void OnDestroy()
	{
		Delegates.ScreenSizeChanged -= ResizeGUIRects;
		CancelInvoke();
	}

	private void OnGUI()
	{
		if (!guiInitialized)
			InitializeGUI();
		GUILayout.BeginArea(infoAreaRect, panelStyle);
		GUILayout.BeginArea(infoContentRect);
		GUILayout.Label("第 " + (round + 1) + " 回合", Data.GUI.Label.LargeMiddle);
		GUILayout.FlexibleSpace();
		GUILayout.Label(Mathf.RoundToInt(Data.Replay.CurrentScores[0]) + " : " + Mathf.RoundToInt(Data.Replay.CurrentScores[1]), Data.GUI.Label.SmallMiddle);
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

	private void Start()
	{
		roundCount = Data.Battle["key_frames"].Count;
		InvokeRepeating("LoadNextFrame", 0, Settings.TimePerFrame); //Settings.TimePerFrame must suffice to allow for all procedures
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