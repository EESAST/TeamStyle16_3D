#region

using System.IO;
using GameStatics;
using UnityEngine;

#endregion

public class InterfaceManager : MonoBehaviour
{
	private readonly Cloud[] clouds = new Cloud[Settings.CloudNumber];
	private Rect aboutRect;
	private Vector2 aboutScroll = Vector2.zero;
	public Texture back;
	public Texture background;
	public Texture[] cloudTextures;
	public Texture directory;
	public Texture drive;
	public Texture file;
	private FileBrowser fileBrowser;
	private bool guiInitialized;
	private Vector2 lastPhysicalScreenSize;
	private Vector2 lastScreenSize;
	private Vector2 optionScroll = Vector2.zero;
	private int optionSelected;
	public Texture ship;
	private InterfaceState stagedState;
	private InterfaceState state;

	private void AboutWindow(int windowID)
	{
		GUILayout.Label("关于", Data.GUI.Label.LargeMiddle);
		GUILayout.BeginVertical("box");
		aboutScroll = GUILayout.BeginScrollView(aboutScroll);
		GUILayout.Label("第十六届电子系队式程序设计大赛专用3D回放引擎", Data.GUI.Label.Small);
		GUILayout.Label("电子系科协软件部队式3D组出品", Data.GUI.Label.Small);
		GUILayout.FlexibleSpace();
		GUILayout.Label("开发者：", Data.GUI.Label.Large);
		GUILayout.Label("林圣杰", Data.GUI.Label.Small);
		GUILayout.Label("钟元熠", Data.GUI.Label.Small);
		GUILayout.Label("鸣谢：", Data.GUI.Label.Large);
		GUILayout.Label("翁喆", Data.GUI.Label.Small);
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("确定", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
			stagedState = InterfaceState.Default;
		GUI.DragWindow();
	}

	private void Awake()
	{
		Methods.Game.Resume();
		for (var i = 0; i < clouds.Length; i++)
		{
			clouds[i].cloudTexture = cloudTextures[Random.Range(0, cloudTextures.Length)];
			clouds[i].Reset();
			clouds[i].rect.x = Random.Range(-clouds[i].rect.width, Screen.width);
		}
	}

	private bool Confirm(string message)
	{
		var value = false;
		GUILayout.BeginArea(new Rect(Screen.width * 0.4f, Screen.height * 0.4f, Screen.width * 0.2f, Screen.height * 0.2f));
		GUILayout.BeginVertical("box");
		GUILayout.FlexibleSpace();
		GUILayout.Label("确认" + message + "?", Data.GUI.Label.LargeMiddle);
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("是", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
			value = true;
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("否", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
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
		GUILayout.BeginArea(new Rect(Screen.width * 0.75f, Screen.height * 0.6f, Screen.width * 0.1f, Screen.height * 0.3f));
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("回  放", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
			stagedState = InterfaceState.BrowsingFile;
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("选  项", Data.GUI.Button.Medium))
		{
			stagedState = InterfaceState.Options;
			for (var i = 0; i < 4; i++)
				Data.GUI.StagedTeamColors[i] = Data.TeamColor.Desired[i];
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("关  于", Data.GUI.Button.Medium))
		{
			stagedState = InterfaceState.About;
			GUI.FocusWindow(0);
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("退  出", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
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

	private void DrawOptions()
	{
		GUILayout.BeginArea(new Rect(Screen.width * 0.15f, Screen.height * 0.1f, Screen.width * 0.7f, Screen.height * 0.8f));
		GUILayout.BeginVertical("box");
		optionSelected = GUILayout.Toolbar(optionSelected, Data.GUI.OptionTexts, Data.GUI.Button.Large);
		GUILayout.FlexibleSpace();
		if (optionSelected == 0)
		{
			GUILayout.BeginVertical("box");
			optionScroll = GUILayout.BeginScrollView(optionScroll);
			GUILayout.BeginVertical();
			for (var i = 0; i < 3; i++)
			{
				GUILayout.BeginHorizontal("box");
				if (GUILayout.Button(Data.GUI.Random, Data.GUI.Button.Large, GUILayout.ExpandHeight(true)))
					Data.GUI.StagedTeamColors[i] = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				GUILayout.Label(Data.GUI.TeamDescriptions[i], Data.GUI.Label.TeamColor[i]);
				GUILayout.Box("", Data.GUI.TeamColoredBoxes[i]);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("红", Data.GUI.Label.RGB[0]);
				Data.GUI.StagedTeamColors[i].r = GUILayout.HorizontalSlider(Data.GUI.StagedTeamColors[i].r, 0, 1);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("绿", Data.GUI.Label.RGB[1]);
				Data.GUI.StagedTeamColors[i].g = GUILayout.HorizontalSlider(Data.GUI.StagedTeamColors[i].g, 0, 1);
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("蓝", Data.GUI.Label.RGB[2]);
				Data.GUI.StagedTeamColors[i].b = GUILayout.HorizontalSlider(Data.GUI.StagedTeamColors[i].b, 0, 1);
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
		if (GUILayout.Button("确定", Data.GUI.Button.Small) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
		{
			Data.TeamColor.Desired = Data.GUI.StagedTeamColors;
			stagedState = InterfaceState.Default;
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("取消", Data.GUI.Button.Small) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
			stagedState = InterfaceState.Default;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	private void InitializeGUI()
	{
		(fileBrowser = new FileBrowser { backTexture = back, directoryTexture = directory, driveTexture = drive, fileTexture = file, confirmStyle = Data.GUI.Button.Small, cancelStyle = Data.GUI.Button.Small }).Refresh();
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
			case InterfaceState.Options:
				DrawOptions();
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

	private void RefreshStyles()
	{
		if (!Methods.Array.Equals(Data.GUI.LastTeamColors, Data.GUI.StagedTeamColors))
			for (var i = 0; i < 3; i++)
			{
				Data.GUI.TeamColoredTextures[i].SetPixel(0, 0, Data.GUI.LastTeamColors[i] = Data.GUI.Label.TeamColor[i].normal.textColor = Data.GUI.StagedTeamColors[i]);
				Data.GUI.TeamColoredTextures[i].Apply();
			}
		var physicalScreenSize = new Vector2(Screen.width, Screen.height) / Screen.dpi;
		if (lastPhysicalScreenSize != physicalScreenSize)
		{
			Methods.GUI.ResizeStyles();
			lastPhysicalScreenSize = physicalScreenSize;
		}
		var screenSize = new Vector2(Screen.width, Screen.height);
		if (lastScreenSize != screenSize)
		{
			for (var i = 0; i < clouds.Length; i++)
				clouds[i].Refresh(lastScreenSize);
			aboutRect = new Rect(Screen.width * 0.4f, Screen.height * 0.2f, Screen.width * 0.2f, Screen.height * 0.6f);
			lastScreenSize = screenSize;
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

		public void Refresh(Vector2 lastScreenSize)
		{
			var ratio = lastScreenSize == Vector2.zero ? Vector2.one : new Vector2(Screen.width / lastScreenSize.x, Screen.height / lastScreenSize.y);
			speed *= ratio.x;
			rect.x *= ratio.x;
			rect.y *= ratio.y;
			rect.width *= ratio.x;
			rect.height *= ratio.y;
		}

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
		Options,
		About,
		Quiting
	}
}