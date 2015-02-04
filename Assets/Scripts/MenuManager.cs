#region

using UnityEngine;

#endregion

public class MenuManager : MonoBehaviour
{
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
		GUILayout.BeginVertical();
		Methods.GUI.Confirm(message, ref value, ref stagedState);
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
		Methods.GUI.DrawAbout(ref stagedState);
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
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void DrawOptions()
	{
		GUILayout.BeginArea(optionAreaRect, subMenuStyle);
		GUILayout.BeginArea(optionContentRect);
		GUILayout.BeginVertical();
		Methods.GUI.DrawOptions(ref stagedState);
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUILayout.EndArea();
	}

	private void InitializeGUI()
	{
		mainMenuStyle = new GUIStyle { normal = { background = mainMenuBackground }, border = new RectOffset(80, 20, 40, 40) };
		subMenuStyle = new GUIStyle { normal = { background = subMenuBackground }, border = new RectOffset(30, 60, 60, 40) };
		Methods.GUI.InitializeStyles();
		guiInitialized = true;
		ResizeGUIRects();
	}

	private void OnDestroy() { Delegates.ScreenSizeChanged -= ResizeGUIRects; }

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
		defaultAreaRect = new Rect(0, Screen.height * 0.2f, Screen.width * 0.25f, Screen.height * 0.6f);
		defaultContentRect = new Rect(mainMenuStyle.border.left, mainMenuStyle.border.top, defaultAreaRect.width - mainMenuStyle.border.left - mainMenuStyle.border.right, defaultAreaRect.height - mainMenuStyle.border.top - mainMenuStyle.border.bottom);
		optionAreaRect = new Rect(defaultAreaRect.width, Screen.height * 0.1f, (Screen.width - defaultAreaRect.width) * 0.8f, Screen.height * 0.8f);
		optionContentRect = new Rect(subMenuStyle.border.left, subMenuStyle.border.top, optionAreaRect.width - subMenuStyle.border.left - subMenuStyle.border.right, optionAreaRect.height - subMenuStyle.border.top - subMenuStyle.border.bottom);
		aboutAreaRect = new Rect(defaultAreaRect.width, Screen.height * 0.2f, (Screen.width - defaultAreaRect.width) * 0.4f, Screen.height * 0.6f);
		aboutContentRect = new Rect(subMenuStyle.border.left, subMenuStyle.border.top, aboutAreaRect.width - subMenuStyle.border.left - subMenuStyle.border.right, aboutAreaRect.height - subMenuStyle.border.top - subMenuStyle.border.bottom);
		confirmAreaRect = new Rect(defaultAreaRect.width, Screen.height * 0.25f, (Screen.width - defaultAreaRect.width) * 0.4f, Screen.height * 0.5f);
		confirmContentRect = new Rect(subMenuStyle.border.left, subMenuStyle.border.top, confirmAreaRect.width - subMenuStyle.border.left - subMenuStyle.border.right, confirmAreaRect.height - subMenuStyle.border.top - subMenuStyle.border.bottom);
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
}