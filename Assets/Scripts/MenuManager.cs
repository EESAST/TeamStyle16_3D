#region

using GameStatics;
using UnityEngine;

#endregion

public class MenuManager : MonoBehaviour
{
	private Rect aboutAreaRect;
	private Rect aboutContentRect;
	private Vector2 aboutScroll = Vector2.zero;
	private Rect confirmAreaRect;
	private Rect confirmContentRect;
	private Rect defaultAreaRect;
	private Rect defaultContentRect;
	private bool guiInitialized;
	private Vector2 lastPhysicalScreenSize;
	private Vector2 lastScreenSize;
	public Texture2D mainMenuBackground;
	private GUIStyle mainMenuStyle;
	private Rect optionAreaRect;
	private Rect optionContentRect;
	private Vector2 optionScroll = Vector2.zero;
	private int optionSelected;
	private MenuState stagedState;
	private MenuState state;
	public Texture2D subMenuBackground;
	private GUIStyle subMenuStyle;

	private bool Confirm(string message)
	{
		var value = false;
		GUILayout.BeginArea(confirmAreaRect, subMenuStyle);
		GUILayout.BeginArea(confirmContentRect);
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.Label("确认" + message + "?", Data.GUI.Label.LargeMiddle);
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("是", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
			value = true;
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("否", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
			stagedState = MenuState.Default;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUILayout.EndArea();
		return value;
	}

	private void DrawAbout()
	{
		GUILayout.BeginArea(aboutAreaRect, subMenuStyle);
		GUILayout.BeginArea(aboutContentRect);
		GUILayout.BeginVertical();
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
			stagedState = MenuState.Default;
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void DrawDefaultMenu()
	{
		GUILayout.BeginArea(defaultAreaRect, mainMenuStyle);
		GUILayout.BeginArea(defaultContentRect);
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("继  续", Data.GUI.Button.Medium))
			SwitchGameState();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("选  项", Data.GUI.Button.Medium))
		{
			stagedState = MenuState.Options;
			for (var i = 0; i < 4; i++)
				Data.GUI.StagedTeamColors[i] = Data.TeamColor.Desired[i];
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("关  于", Data.GUI.Button.Medium))
			stagedState = MenuState.About;
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("回到主界面", Data.GUI.Button.Medium))
			stagedState = MenuState.Back;
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("退  出", Data.GUI.Button.Medium))
			stagedState = MenuState.Quit;
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void DrawOptions()
	{
		GUILayout.BeginArea(optionAreaRect, subMenuStyle);
		GUILayout.BeginArea(optionContentRect);
		GUILayout.BeginVertical();
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
			stagedState = MenuState.Default;
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("取消", Data.GUI.Button.Small) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
			stagedState = MenuState.Default;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void InitializeGUI()
	{
		mainMenuStyle = new GUIStyle { normal = { background = mainMenuBackground }, border = new RectOffset(80, 20, 40, 40) };
		subMenuStyle = new GUIStyle { normal = { background = subMenuBackground }, border = new RectOffset(30, 60, 60, 40) };
		guiInitialized = true;
	}

	private void OnGUI()
	{
		if (Event.current.type == EventType.Layout)
			state = stagedState;
		if (Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape) && state <= MenuState.Default)
			SwitchGameState();
		if (state == MenuState.None)
			return;
		if (!guiInitialized)
			InitializeGUI();
		RefreshStyles();
		DrawDefaultMenu();
		switch (state)
		{
			case MenuState.Options:
				DrawOptions();
				break;
			case MenuState.About:
				DrawAbout();
				break;
			case MenuState.Back:
				if (Confirm("回到主界面"))
					Application.LoadLevel("MainInterface");
				break;
			case MenuState.Quit:
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
			defaultAreaRect = new Rect(0, Screen.height * 0.2f, Screen.width * 0.25f, Screen.height * 0.6f);
			defaultContentRect = new Rect(mainMenuStyle.border.left, mainMenuStyle.border.top, defaultAreaRect.width - mainMenuStyle.border.left - mainMenuStyle.border.right, defaultAreaRect.height - mainMenuStyle.border.top - mainMenuStyle.border.bottom);
			optionAreaRect = new Rect(defaultAreaRect.width, Screen.height * 0.1f, (Screen.width - defaultAreaRect.width) * 0.8f, Screen.height * 0.8f);
			optionContentRect = new Rect(subMenuStyle.border.left, subMenuStyle.border.top, optionAreaRect.width - subMenuStyle.border.left - subMenuStyle.border.right, optionAreaRect.height - subMenuStyle.border.top - subMenuStyle.border.bottom);
			aboutAreaRect = new Rect(defaultAreaRect.width, Screen.height * 0.2f, (Screen.width - defaultAreaRect.width) * 0.4f, Screen.height * 0.6f);
			aboutContentRect = new Rect(subMenuStyle.border.left, subMenuStyle.border.top, aboutAreaRect.width - subMenuStyle.border.left - subMenuStyle.border.right, aboutAreaRect.height - subMenuStyle.border.top - subMenuStyle.border.bottom);
			confirmAreaRect = new Rect(defaultAreaRect.width, Screen.height * 0.25f, (Screen.width - defaultAreaRect.width) * 0.4f, Screen.height * 0.5f);
			confirmContentRect = new Rect(subMenuStyle.border.left, subMenuStyle.border.top, confirmAreaRect.width - subMenuStyle.border.left - subMenuStyle.border.right, confirmAreaRect.height - subMenuStyle.border.top - subMenuStyle.border.bottom);
			lastScreenSize = screenSize;
		}
	}

	private void SwitchGameState()
	{
		stagedState = stagedState == MenuState.None ? MenuState.Default : MenuState.None;
		if (stagedState == MenuState.None)
			Methods.Game.Resume();
		else
			Methods.Game.Pause();
		Camera.main.GetComponent<Blur>().enabled = Data.GamePaused;
	}

	private enum MenuState
	{
		None,
		Default,
		Options,
		About,
		Back,
		Quit
	}
}