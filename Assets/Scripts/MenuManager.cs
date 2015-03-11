#region

using System.Linq;
using UnityEngine;

#endregion

public class MenuManager : MonoBehaviour
{
	private readonly Color[] lastTargetTeamColor = new Color[2];
	private Rect aboutAreaRect;
	private Rect aboutContentRect;
	private Rect confirmAreaRect;
	private Rect confirmContentRect;
	private Rect defaultAreaRect;
	private Rect defaultContentRect;
	private bool guiInitialized;
	public Texture2D mainMenuBackground;
	private GUIStyle mainMenuStyle;
	private Rect optionAreaRect;
	private Rect optionContentRect;
	private MenuState stagedState;
	private MenuState state;
	public Texture2D subMenuBackground;
	private GUIStyle subMenuStyle;

	private void Awake() { Delegates.ScreenSizeChanged += ResizeGUIRects; }

	private bool Confirm(string message)
	{
		var value = false;
		GUILayout.BeginArea(confirmAreaRect, subMenuStyle);
		GUILayout.BeginArea(confirmContentRect);
		Methods.GUI.Confirm(message, ref value, ref stagedState);
		GUILayout.EndArea();
		GUILayout.EndArea();
		return value;
	}

	private void DrawAbout()
	{
		GUILayout.BeginArea(aboutAreaRect, subMenuStyle);
		GUILayout.BeginArea(aboutContentRect);
		Methods.GUI.DrawAbout(ref stagedState);
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void DrawDefaultMenu()
	{
		GUILayout.BeginArea(defaultAreaRect, mainMenuStyle);
		GUILayout.BeginArea(defaultContentRect);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("继  续", Data.GUI.Button.Medium))
			SwitchGameState();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("选  项", Data.GUI.Button.Medium))
		{
			stagedState = MenuState.Options;
			Methods.GUI.StageCurrentOptions();
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
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void DrawOptions()
	{
		GUILayout.BeginArea(optionAreaRect, subMenuStyle);
		GUILayout.BeginArea(optionContentRect);
		Methods.GUI.DrawOptions(ref stagedState);
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void InitializeGUI()
	{
		Methods.GUI.InitializeStyles();
		mainMenuStyle = new GUIStyle { normal = { background = mainMenuBackground }, border = new RectOffset(80, 20, 40, 40) };
		subMenuStyle = new GUIStyle { normal = { background = subMenuBackground }, border = new RectOffset(25, 50, 30, 35) };
		guiInitialized = true;
		ResizeGUIRects();
	}

	private void OnDestroy() { Delegates.ScreenSizeChanged -= ResizeGUIRects; }

	private void OnGUI()
	{
		GUI.depth = -1;
		if (!guiInitialized)
			InitializeGUI();
		if (Event.current.type == EventType.Layout)
			state = stagedState;
		if (Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape) && state <= MenuState.Default)
			SwitchGameState();
		if (state == MenuState.None)
			return;
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

	private void ResizeGUIRects()
	{
		if (!guiInitialized)
			return;
		defaultContentRect = new Rect(mainMenuStyle.border.left, mainMenuStyle.border.top, Screen.width * 0.15f, Screen.height * 0.4f);
		defaultAreaRect = new Rect(0, Screen.height * 0.35f, defaultContentRect.width + mainMenuStyle.border.horizontal, defaultContentRect.height + mainMenuStyle.border.vertical);
		optionContentRect = new Rect(subMenuStyle.border.left, subMenuStyle.border.top, Screen.width * 0.5f, Screen.height * 0.6f);
		optionAreaRect = new Rect(defaultAreaRect.width, Screen.height * 0.2f, optionContentRect.width + subMenuStyle.border.horizontal, optionContentRect.height + subMenuStyle.border.vertical);
		aboutContentRect = new Rect(subMenuStyle.border.left, subMenuStyle.border.top, Screen.width * 0.3f, Screen.height * 0.5f);
		aboutAreaRect = new Rect(defaultAreaRect.width, Screen.height * 0.3f, aboutContentRect.width + subMenuStyle.border.horizontal, aboutContentRect.height + subMenuStyle.border.vertical);
		confirmContentRect = new Rect(subMenuStyle.border.left, subMenuStyle.border.top, Screen.width * 0.25f, Screen.height * 0.3f);
		confirmAreaRect = new Rect(defaultAreaRect.width, Screen.height * 0.4f, confirmContentRect.width + subMenuStyle.border.horizontal, confirmContentRect.height + subMenuStyle.border.vertical);
	}

	private void SwitchGameState()
	{
		stagedState = stagedState == MenuState.None ? MenuState.Default : MenuState.None;
		if (stagedState == MenuState.None)
		{
			if (!Methods.Array.Equals(lastTargetTeamColor, Data.TeamColor.Target.Take(2).ToArray()))
				Data.Replay.Instance.RefreshCharts();
			Methods.Game.Resume();
		}
		else
		{
			for (var i = 0; i < 2; ++i)
				lastTargetTeamColor[i] = Data.TeamColor.Target[i];
			Methods.Game.Pause();
		}
		Camera.main.GetComponent<Blur>().enabled = Data.GamePaused;
		GetComponent<SelectionManager>().enabled = !Data.GamePaused;
	}
}