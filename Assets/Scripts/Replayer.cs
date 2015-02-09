#region

using System;
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
		var i = 0;
		while (events[i] && events[i]["__class__"].str == "AddProductionEntry")
		{
			var team = Mathf.RoundToInt(events[i]["team"].n);
			var kind = Mathf.RoundToInt(events[i]["kind"].n);
			var productionEntry = Instantiate(Resources.Load("ProductionEntry")) as GameObject;
			productionEntry.GetComponent<ProductionEntry>().Setup(team, kind);
			++i;
		}
	}

	private void Awake() { Delegates.ScreenSizeChanged += ResizeGUIRects; }

	private void InitializeGUI()
	{
		Methods.GUI.InitializeStyles();
		panelStyle = new GUIStyle { normal = { background = panelBackground }, border = new RectOffset(20, 15, 30, 15) };
		guiInitialized = true;
		ResizeGUIRects();
	}

	private void LoadFrame( /*int round, bool shallAnimate = true*/)
	{
		++round;
		//Data.Game.Ready = false;
		//scores = Data.BattleData["history"]["score"][round];
		//populations = Data.BattleData["history"]["population"][round];	//these should be updated during the corresponding period, not in the beginning
		commands = Data.BattleData["history"]["command"][round]; //e.g. commands[0][i] stands for the ith command (object) of team 0
		events = Data.BattleData["history"]["event"][round]; //e.g. events[i] stands for the ith event (object)
		elements = Data.BattleData["key_frames"][round][0]; //an object within which lies a list of key-vals, i.e elements[i] is the ith element (key-val pair)
		productionLists = Data.BattleData["key_frames"][round][1]; //an array comprised of two sub-arrays, stands for two teams, e.g. productionLists[1][i] stands for the ith production of team 1, which is still a two-entry array itself, i.e. [kind, roundLeft]

		AddProductionEntries();	//TODO:probably updates metal storage as well
		ProcessAttacks();
		ProcessRepairs();
		ProcessMovements();
		ProcessCollectings();
		RefreshProductionLists();	//actually instantiate new entities only, the rest being handled in AddProductionEntries()

		/*foreach (var index in Data.Entities.Keys)
			if (elements[index.ToString()] == null)
				Data.Entities[index].Destruct();
		foreach (var entry in elements.list)
		{
			var index = Mathf.RoundToInt(entry["index"].n);
			if (Data.Entities[index] == null) //TODO: newly produced entities should appear at the end of the frame
			{
				var entityType = entry["__class__"].str;
				Data.Entities.Add(index, (Instantiate(Resources.Load(entityType + '/' + entityType)) as GameObject).GetComponent(entityType) as Entity);
			}
			Data.Entities[index].Info = entry; //TODO: should implicitly do all necessary updates
		}*/

		#region

		/*elapsedTime += dt;
		if (elapsedTime >= time_per_frame)
		{
			if (++round >= frameCount)
			{
				CancelInvoke();
				return;
			}
			elapsedTime = 0;
			scores = Data.BattleData["history"]["score"][round];
			populations = Data.BattleData["history"]["population"][round];

			var currentFrame = Data.BattleData["key_frames"][round];
			var currentElements = currentFrame[0];
			var productionLists = currentFrame[1];
			var nextFrame = Data.BattleData["key_frames"][round+1];
			var nextElements = nextFrame[0];
			var nextProductionLists = nextFrame[1];

			foreach (var index in Data.Entities.Keys) {
				if (currentElements[index.ToString()] == null)
					Data.Entities[index].Destruct();
			}

			foreach (var entry in currentElements.list)
			{
				var index = Mathf.RoundToInt(entry["index"].n);
				if (Data.Entities[index] == null)	//TODO: newly produced entities should appear at the end of the frame
				{
					var entityType = entry["__class__"].str;
					Data.Entities.Add(index, (Instantiate(Resources.Load(entityType + '/' + entityType)) as GameObject).GetComponent(entityType) as Entity);
				}
				Data.Entities[index].Info = entry; //TODO: should implicitly do all necessary updates
			}
		}
		// look into the next frame
		//var keyFrame = Data.BattleData["key_frames"][round + 1];
		//var elements = keyFrame[0];
		//var productionLists = keyFrame[1]; // [ [ [,],... ], [  ] ]

		// whether we moved during [round, round+1)
		var b = false;
		foreach (var index in Data.Entities.Keys)
			if (elements[index.ToString()] == null)
				Data.Entities[index].Destruct();
			else if (Methods.Coordinates.JSONToExternal(Data.Entities[index].Info) != Methods.Coordinates.JSONToExternal(elements[index.ToString()]))
			{
				StartCoroutine(Data.Entities[index].FaceTarget(Methods.Coordinates.JSONToExternal(elements[index.ToString()])));
				b = true;
			}
		foreach (var entry in elements.list)
			{
				var t = Data.Entities[Mathf.RoundToInt(entry["index"].n)];
				var target_extr_pos = Methods.Coordinates.JSONToExternal(entry);

				if (Methods.Coordinates.JSONToExternal(t.Info) != target_extr_pos)
				{
					b = true;

					// TODO: animate to adjust orientation of 't' here
					StartCoroutine(t.FaceTarget(Methods.Coordinates.ExternalToInternal(target_extr_pos)));
				}
			}
		// adjust time of this frame
		dt = b ? defaultDeltaTime : time_per_frame;
		// move from frame 'round' to 'round+1'

		//
		// struct frame {
		//     { map Data.Entities },
		//  and production_list: (?), what the hell...
		//    [ [ team0:[type,time],... ][ team1: ... ] ]
		//

		foreach (var entry in elements.list)
		{
			var t = Data.Entities[(int)entry["index"].n];
			//t.Info = entry;		// implicitly update position, etc

			// TODO: need a path finder... apparently it's incorrect to go straight
			var pos = Vector2.Lerp(Methods.Coordinates.JSONToExternal(t.Info), Methods.Coordinates.JSONToExternal(entry), Mathf.Pow(Mathf.Sin(elapsedTime / time_per_frame * Mathf.PI / 2), 2));
			t.SetPosition(pos.x, pos.y);
		}*/

		#endregion
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
		GUILayout.Label(Data.Game.Scores[0].n + " : " + Data.Game.Scores[1].n, Data.GUI.Label.Small);
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void ProcessAttacks() { throw new NotImplementedException(); }

	private void ProcessCollectings() { throw new NotImplementedException(); }

	private void ProcessMovements() { throw new NotImplementedException(); }

	private void ProcessRepairs() { throw new NotImplementedException(); }

	private void RefreshProductionLists() { throw new NotImplementedException(); }

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
		InvokeRepeating("LoadFrame", Settings.TimePerFrame, Settings.TimePerFrame);
	}
}