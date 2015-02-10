#region

using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public class Replayer : MonoBehaviour
{
	private readonly float defaultDeltaTime = 1;
	private readonly float timePerFrame = 2; // 2 second(s)
	private JSONObject commands;
	private float dt, elapsedTime;
	private JSONObject elements;
	private JSONObject events;
	private bool guiInitialized;
	private Rect infoAreaRect;
	private Rect infoContentRect;
	public Texture2D panelBackground;
	private GUIStyle panelStyle;
	private JSONObject productionLists;
	private int round, frameCount;

	private void AddProductionEntries()
	{
		foreach (var productionEntryAddition in events.list.Where(productionEntryAddition => productionEntryAddition["__class__"].str == "AddProductionEntry"))
			(Instantiate(Resources.Load("ProductionEntry")) as GameObject).GetComponent<ProductionEntry>().Setup(Mathf.RoundToInt(productionEntryAddition["team"].n), Mathf.RoundToInt(productionEntryAddition["kind"].n));
	}

	private void Attacks()
	{
		foreach (var attack in events.list)
			switch (attack["__class__"].str)
			{
				case "AttackUnit":
					(Data.Elements[Mathf.RoundToInt(attack["index"].n)] as UnitBase).FireAtElement(Data.Elements[Mathf.RoundToInt(attack["target"].n)] as UnitBase, Mathf.RoundToInt(attack["damage"].n));
					break;
				case "AttackMiss":
					(Data.Elements[Mathf.RoundToInt(attack["index"].n)] as UnitBase).FireAtPosition(Methods.Coordinates.JSONToInternal(attack["target_pos"]));
					break;
				case "Capture":
					var fort = Data.Elements[Mathf.RoundToInt(attack["index"].n)] as Fort;
					fort.targetTeam = Mathf.RoundToInt(attack["team"].n);
					break;
			}
	}

	private void Awake() { Delegates.ScreenSizeChanged += ResizeGUIRects; }

	private void Collects()
	{
		foreach (var collect in events.list.Where(collect => collect["__class__"].str == "Collect"))
		{
			var collector = Data.Elements[Mathf.RoundToInt(collect["index"].n)] as Cargo;
			var target = Data.Elements[Mathf.RoundToInt(collect["target"].n)] as Resource;
			var fuel = Mathf.RoundToInt(collect["fuel"].n);
			var metal = Mathf.RoundToInt(collect["metal"].n);
			collector.targetFuel += fuel;
			collector.targetMetal += metal;
			target.targetFuel -= fuel;
			target.targetMetal -= metal;
		}
	}

	private void Creates()
	{
		if (round == 0)
			return;
		foreach (var create in events.list.Where(create => create["__class__"].str == "Create"))
		{
			var typeName = Constants.TypeNames[Mathf.RoundToInt(create["kind"].n)];
			var info = elements[Mathf.RoundToInt(create["index"].n).ToString()];
			((Instantiate(Resources.Load(typeName + '/' + typeName)) as GameObject).GetComponent(typeName) as Element).Initialize(info);
		}
	}

	private void Fixes()
	{
		foreach (var fix in events.list.Where(fix => fix["__class__"].str == "Fix"))
		{
			(Data.Elements[Mathf.RoundToInt(fix["index"].n)] as Base).targetMetal -= Mathf.RoundToInt(fix["metal"].n);
			(Data.Elements[Mathf.RoundToInt(fix["target"].n)] as Unit).targetHP += Mathf.RoundToInt(fix["health_increase"].n);
		}
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
		Data.Game.Scores = Data.BattleData["history"]["score"][round];
		Data.Game.Populations = Data.BattleData["history"]["population"][round]; //TODO:these should be updated during the corresponding period, not in the beginning
		var keyFrame = Data.BattleData["key_frames"]; //a key frame is the snapshop of the initial state of a round
		elements = keyFrame[round][0]; //an object within which lies a list of key-vals, i.e elements[i] is the ith element (key-val pair)
		productionLists = keyFrame[round][1]; //an array comprised of two sub-arrays, stands for two teams, e.g. productionLists[1][i] stands for the ith production of team 1, which is still a two-entry array itself, i.e. [kind, roundLeft]

		Creates();

		commands = Data.BattleData["history"]["command"][round]; //e.g. commands[0][i] stands for the ith command (object) of team 0
		events = Data.BattleData["history"]["event"][round]; //e.g. events[i] stands for the ith event (object)

		AddProductionEntries();
		Attacks();
		yield return new WaitForSeconds(2);
		Supplies();
		Fixes();
		yield return new WaitForSeconds(1);
		Moves();
		Collects();

		yield return null;
	}

	private void LoadNextFrame()
	{
		StartCoroutine(LoadFrame());
		++round;
	}

	private void Moves()
	{
		foreach (var move in events.list.Where(move => move["__class__"].str == "Move"))
			Data.Elements[Mathf.RoundToInt(move["index"].n)].transform.position = Methods.Coordinates.JSONToInternal(move["nodes"].list[move["nodes"].Count - 1]); //TODO:animate movement according to nodes
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
		GUILayout.Label("第 " + round + " 回合", Data.GUI.Label.LargeMiddle);
		GUILayout.FlexibleSpace();
		GUILayout.Label(Data.Game.Scores[0].n + " : " + Data.Game.Scores[1].n, Data.GUI.Label.SmallMiddle);
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	/*private IEnumerator Replay()
	{
		Data.Game.Ready = true;
		while (round < frameCount - 1)
		{
			yield return new WaitForSeconds(defaultDeltaTime); //check status every default delta time
			if (Data.Game.Ready)
				LoadFrame(++round);
		}
	}*/

	private void ResizeGUIRects()
	{
		if (!guiInitialized)
			return;
		infoContentRect = new Rect(panelStyle.border.left, panelStyle.border.top, Screen.width * 0.15f, Screen.height * 0.15f);
		infoAreaRect = new Rect((Screen.width - infoContentRect.width - panelStyle.border.horizontal) / 2, 0, infoContentRect.width + panelStyle.border.horizontal, infoContentRect.height + panelStyle.border.vertical);
	}

	private void Start()
	{
		frameCount = Data.BattleData["key_frames"].Count;
		Data.Game.Scores = Data.BattleData["history"]["score"][0];
		Data.Game.Populations = Data.BattleData["history"]["population"][0];
		//StartCoroutine(Replay());
		InvokeRepeating("LoadNextFrame", Settings.TimePerFrame, Settings.TimePerFrame);
	}

	private void Supplies()
	{
		foreach (var supply in events.list.Where(supply => supply["__class__"].str == "Supply"))
		{
			var supplier = Data.Elements[Mathf.RoundToInt(supply["index"].n)] as UnitBase;
			var target = Data.Elements[Mathf.RoundToInt(supply["target"].n)] as UnitBase;
			var fuel = Mathf.RoundToInt(supply["fuel"].n);
			var ammo = Mathf.RoundToInt(supply["ammo"].n);
			var metal = Mathf.RoundToInt(supply["metal"].n);
			supplier.targetFuel -= fuel;
			supplier.targetAmmo -= ammo;
			supplier.targetMetal -= metal;
			target.targetFuel += fuel;
			target.targetAmmo += ammo;
			target.targetMetal += metal;
		}
	}
}