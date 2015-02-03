#region

using GameStatics;
using UnityEngine;

#endregion

public class MenuManager : MonoBehaviour
{
	public Texture2D mainMenuBackground;
	private MenuState state;
	public Texture2D subMenuBackground;

	private bool Confirm(string message)
	{
		int y = Screen.height / 5, h = Screen.width / 2;
		GUI.DrawTexture(new Rect(310, y, 300, Screen.height / 2f), subMenuBackground);
		GUI.Label(new Rect(330, y += h / 8, 100, 20), "确认" + message + "?");
		if (GUI.Button(new Rect(330, y += 30, 100, 30), "是"))
			return true;
		if (GUI.Button(new Rect(330, y + 40, 100, 30), "否"))
			state = MenuState.MainMenu;
		return false;
	}

	private void DrawAbout()
	{
		GUI.DrawTexture(new Rect(310, Screen.height / 5f, Screen.width / 2f, Screen.height / 2f), subMenuBackground);
		GUI.Label(new Rect(330, Screen.height / 5 + 100, 300, 300), "ABOUTABOUTABOUT");
	}

	private void DrawOptions()
	{
		GUI.DrawTexture(new Rect(310, Screen.height / 5f, Screen.width / 2f, Screen.height / 2f), subMenuBackground);
		GUI.Label(new Rect(330, Screen.height / 5 + 100, 300, 300), "OPTIONSOPTIONSOPTIONS");
	}

	private void OnGUI()
	{
		if (state == MenuState.None)
			return;
		int width = 300, height = Mathf.Max(Mathf.Min(Screen.height / 2, 300), 200);
		GUI.BeginGroup(new Rect(0, (Screen.height - height) / 3f, width, height));
		GUI.DrawTexture(new Rect(0, 0, width, height), mainMenuBackground);
		GUILayout.BeginArea(new Rect(100, 50, width - 130, height - 50));
		height = height / 7 - 7;
		if (GUILayout.Button("RESUME", GUILayout.Height(height)))
			SwitchGameState();
		if (GUILayout.Button("OPTIONS", GUILayout.Height(height)))
			state = MenuState.Options;
		if (GUILayout.Button("ABOUT", GUILayout.Height(height)))
			state = MenuState.About;
		if (GUILayout.Button("BACK TO MAIN INTERFACE", GUILayout.Height(height)))
			state = MenuState.Back;
		if (GUILayout.Button("QUIT", GUILayout.Height(height)))
			state = MenuState.Quit;
		GUILayout.EndArea();
		GUI.EndGroup();
		switch (state)
		{
			case MenuState.Options:
				DrawOptions();
				break;
			case MenuState.About:
				DrawAbout();
				break;
			case MenuState.Back:
				if (Confirm("返回主界面"))
					Application.LoadLevel("MainInterface");
				break;
			case MenuState.Quit:
				if (Confirm("退出"))
					Methods.Game.Quit();
				break;
		}
	}

	private void SwitchGameState()
	{
		state = state == MenuState.None ? MenuState.MainMenu : MenuState.None;
		if (state == MenuState.None)
			Methods.Game.Resume();
		else
			Methods.Game.Pause();
		Camera.main.GetComponent<Blur>().enabled = Data.GamePaused;
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.Escape) && !Input.GetKeyDown(KeyCode.Home))
			return;
		if (state > MenuState.MainMenu)
			state = MenuState.MainMenu;
		else
			SwitchGameState();
	}

	private enum MenuState
	{
		None,
		MainMenu,
		Options,
		About,
		Back,
		Quit
	}
}