#region

using System;
using System.IO;
using GameStatics;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

public class InterfaceManager : MonoBehaviour
{
	private readonly Cloud[] clouds = new Cloud[Settings.CloudNumber];
	private readonly Color[] lastTeamColors = new Color[4];
	private readonly string[] setterTexts = { "队伍颜色", "敬请期待……" };
	private readonly GUIStyle[] teamColorBoxes = new GUIStyle[3];
	private readonly GUIStyle[] teamColorLabels = new GUIStyle[3];
	private readonly Texture2D[] teamColorTextures = new Texture2D[3];
	private readonly string[] teamDescriptions = { "队伍1", "队伍2", "中立" };
	private Rect aboutRect;
	private Vector2 aboutScroll = Vector2.zero;
	public Texture back;
	public Texture background;
	private GUIStyle blueLabel;
	public Texture[] cloudTextures;
	public Texture dice;
	public Texture directory;
	public Texture drive;
	public Texture file;
	private FileBrowser fileBrowser;
	private GUIStyle greenLabel;
	private bool guiInitialized;
	private GUIStyle largeButton;
	private GUIStyle largeLabel;
	private GUIStyle largeLabel_MiddleAligned;
	private float lastScreenHeight;
	private GUIStyle mediumButton;
	private GUIStyle redLabel;
	private Vector2 setterScroll = Vector2.zero;
	private int setterSelected;
	public Texture ship;
	private GUIStyle smallButton;
	private GUIStyle smallLabel;
	private InterfaceState stagedState;
	private Color[] stagedTeamColors = new Color[4];
	private InterfaceState state;

	private void AboutWindow(int windowID)
	{
		GUILayout.Label("关于", largeLabel_MiddleAligned);
		GUILayout.BeginVertical("box");
		aboutScroll = GUILayout.BeginScrollView(aboutScroll);
		GUILayout.Label("第十六届电子系队式程序设计大赛专用3D回放引擎", smallLabel);
		GUILayout.Label("电子系科协软件部队式3D组出品", smallLabel);
		GUILayout.FlexibleSpace();
		GUILayout.Label("开发者：", largeLabel);
		GUILayout.Label("林圣杰", smallLabel);
		GUILayout.Label("钟元熠", smallLabel);
		GUILayout.Label("鸣谢：", largeLabel);
		GUILayout.Label("翁喆", smallLabel);
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		if (GUILayout.Button("确定", mediumButton) || Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
			stagedState = InterfaceState.Default;
		GUI.DragWindow();
	}

	private void Awake()
	{
		Methods.Game.Resume();
		Data.TeamColor.Desired = new[] { Color.magenta, Color.cyan, Color.gray, Color.white };
		(fileBrowser = new FileBrowser { backTexture = back, directoryTexture = directory, driveTexture = drive, fileTexture = file }).Refresh();
		RandomizeClouds();
	}

	private bool Confirm(string message)
	{
		var value = false;
		GUILayout.BeginArea(new Rect(Screen.width * 0.4f, Screen.height * 0.4f, Screen.width * 0.2f, Screen.height * 0.2f));
		GUILayout.BeginVertical("box");
		GUILayout.Label("确认退出？", largeLabel_MiddleAligned);
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("确定", mediumButton) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
			value = true;
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("取消", mediumButton) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
			stagedState = InterfaceState.Default;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		return value;
	}

	private void DrawBackground()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background);
		for (var i = 0; i < clouds.Length; i++)
			if (clouds[i].layer == 0)
				GUI.DrawTexture(clouds[i].rect, clouds[i].cloudTexture);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), ship);
		for (var i = 0; i < clouds.Length; i++)
		{
			if (clouds[i].layer == 1)
				GUI.DrawTexture(clouds[i].rect, clouds[i].cloudTexture);
			if (clouds[i].rect.x > Screen.width || clouds[i].rect.x < -clouds[i].rect.width)
				clouds[i].Reset();
		}
	}

	private void DrawDefaultInterface()
	{
		GUILayout.BeginArea(new Rect(Screen.width * 0.75f, Screen.height * 0.6f, Screen.width * 0.08f, Screen.height * 0.3f));
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("回 放", mediumButton) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
			stagedState = InterfaceState.BrowsingFile;
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("设 置", mediumButton))
		{
			stagedState = InterfaceState.Setting;
			stagedTeamColors = Data.TeamColor.Desired.Clone() as Color[];
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("关 于", mediumButton))
		{
			stagedState = InterfaceState.About;
			GUI.FocusWindow(0);
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("退 出", mediumButton) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
			stagedState = InterfaceState.Quiting;
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();
	}

	private void DrawFileBrowser()
	{
		if (!fileBrowser.Draw())
			return;
		stagedState = InterfaceState.Default;
		if (fileBrowser.outputFile == null || fileBrowser.outputFile.Extension != ".battle")
			return;
		Data.BattleData = new JSONObject(File.ReadAllText(fileBrowser.outputFile.FullName).Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));
		Application.LoadLevel("BattleField");
	}

	private void DrawSetter()
	{
		GUILayout.BeginArea(new Rect(Screen.width * 0.15f, Screen.height * 0.1f, Screen.width * 0.7f, Screen.height * 0.8f));
		GUILayout.BeginVertical("box");
		setterSelected = GUILayout.Toolbar(setterSelected, setterTexts, largeButton);
		GUILayout.FlexibleSpace();
		if (setterSelected == 0)
		{
			GUILayout.BeginVertical("box");
			setterScroll = GUILayout.BeginScrollView(setterScroll);
			GUILayout.BeginVertical();
			for (var i = 0; i < 3; i++)
			{
				GUILayout.BeginHorizontal("box");
				if (GUILayout.Button(new GUIContent("随机", dice), largeButton, GUILayout.ExpandHeight(true)))
					stagedTeamColors[i] = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				GUILayout.Label(teamDescriptions[i], teamColorLabels[i]);
				GUILayout.Box("", teamColorBoxes[i]);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("红", redLabel);
				stagedTeamColors[i].r = GUILayout.HorizontalSlider(stagedTeamColors[i].r, 0, 1);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("绿", greenLabel);
				stagedTeamColors[i].g = GUILayout.HorizontalSlider(stagedTeamColors[i].g, 0, 1);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("蓝", blueLabel);
				stagedTeamColors[i].b = GUILayout.HorizontalSlider(stagedTeamColors[i].b, 0, 1);
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				if (i < 2)
					GUILayout.FlexibleSpace();
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
		GUILayout.BeginHorizontal("box");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("确定", smallButton) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
		{
			Data.TeamColor.Desired = stagedTeamColors;
			stagedState = InterfaceState.Default;
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("取消", smallButton) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
			stagedState = InterfaceState.Default;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	private void InitializeGUI()
	{
		for (var i = 0; i < 3; i++)
		{
			teamColorTextures[i] = new Texture2D(1, 1);
			teamColorBoxes[i] = new GUIStyle("box") { normal = { background = teamColorTextures[i] } };
			teamColorLabels[i] = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
		}
		largeButton = new GUIStyle("button");
		mediumButton = new GUIStyle("button");
		fileBrowser.cancelStyle = fileBrowser.confirmStyle = smallButton = new GUIStyle("button");
		largeLabel = new GUIStyle("label");
		largeLabel_MiddleAligned = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
		smallLabel = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
		redLabel = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.red } };
		greenLabel = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.green } };
		blueLabel = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.blue } };
		guiInitialized = true;
	}

	private void OnGUI()
	{
		if (Event.current.type == EventType.Layout)
			state = stagedState;
		if (!guiInitialized)
			InitializeGUI();
		RefreshStyles();
		DrawBackground();
		switch (state)
		{
			case InterfaceState.Default:
				DrawDefaultInterface();
				break;
			case InterfaceState.BrowsingFile:
				DrawFileBrowser();
				break;
			case InterfaceState.Setting:
				DrawSetter();
				break;
			case InterfaceState.About:
				aboutRect = GUILayout.Window(0, aboutRect, AboutWindow, "");
				break;
			case InterfaceState.Quiting:
				if (Confirm("退出"))
					Methods.Game.Quit();
				break;
		}
	}

	private void RandomizeClouds()
	{
		for (var i = 0; i < clouds.Length; i++)
		{
			clouds[i].cloudTexture = cloudTextures[Random.Range(0, cloudTextures.Length)];
			clouds[i].Reset();
			clouds[i].rect.x = Random.Range(-clouds[i].rect.width, Screen.width);
		}
	}

	private void RefreshStyles()
	{
		if (!Methods.Array.Equals(lastTeamColors, stagedTeamColors))
			for (var i = 0; i < 3; i++)
			{
				teamColorTextures[i].SetPixel(0, 0, lastTeamColors[i] = teamColorLabels[i].normal.textColor = stagedTeamColors[i]);
				teamColorTextures[i].Apply();
			}
		if (Math.Abs(lastScreenHeight - Screen.height) > Mathf.Epsilon)
		{
			RandomizeClouds();
			for (var i = 0; i < 3; i++)
				teamColorLabels[i].fontSize = Screen.height / 25;
			largeButton.fontSize = Screen.height / 20;
			mediumButton.fontSize = Screen.height / 25;
			smallButton.fontSize = Screen.height / 30;
			largeLabel_MiddleAligned.fontSize = largeLabel.fontSize = Screen.height / 20;
			smallLabel.fontSize = Screen.height / 25;
			blueLabel.fontSize = greenLabel.fontSize = redLabel.fontSize = Screen.height / 30;
			aboutRect = new Rect(Screen.width * 0.4f, Screen.height * 0.2f, Screen.width * 0.2f, Screen.height * 0.6f);
			lastScreenHeight = Screen.height;
		}
	}

	private void Update()
	{
		for (var i = 0; i < clouds.Length; i++)
			clouds[i].rect.x += clouds[i].speed * Time.deltaTime;
	}

	private struct Cloud
	{
		public Texture cloudTexture;
		public int layer;
		public Rect rect;
		public float speed;

		public void Reset()
		{
			speed = Random.Range(Screen.width * 0.08f, Screen.width * 0.2f) * (Random.Range(0, 2) - 0.5f);
			var width = Random.Range(Screen.width * 0.1f, Screen.width * 0.4f);
			var height = Random.Range(Screen.height * 0.1f, Screen.height * 0.4f);
			rect = new Rect(speed > 0 ? -width : Screen.width, Random.Range(-height, Screen.height), width, height);
			layer = Random.Range(0, 2);
		}
	}

	private enum InterfaceState
	{
		Default,
		BrowsingFile,
		Setting,
		About,
		Quiting
	}
}