#region

using System.IO;
using UnityEngine;

#endregion

public class InterfaceManager : MonoBehaviour
{
	private readonly Cloud[] clouds = new Cloud[Settings.CloudNumber];
	private Rect aboutRect = new Rect(Screen.width * 0.35f, Screen.height * 0.2f, Screen.width * 0.3f, Screen.height * 0.6f);
	public Texture back;
	public Texture background;
	public Texture[] cloudTextures;
	private Rect defaultRect = new Rect(Screen.width * 0.75f, Screen.height * 0.6f, Screen.width * 0.1f, Screen.height * 0.3f);
	public Texture directory;
	public Texture drive;
	public Texture file;
	private FileBrowser fileBrowser;
	private bool guiInitialized;
	private Rect optionsRect = new Rect(Screen.width * 0.15f, Screen.height * 0.1f, Screen.width * 0.7f, Screen.height * 0.8f);
	public Texture ship;
	private MenuState stagedState = MenuState.Default;
	private MenuState state;

	private void AboutWindow(int windowID)
	{
		GUILayout.Label("关于", Data.GUI.Label.LargeMiddle);
		Methods.GUI.DrawAbout(ref stagedState);
		GUI.DragWindow();
	}

	private void Awake()
	{
		Delegates.ScreenSizeChanged += OnScreenSizeChanged;
		Methods.Game.Resume();
		Data.Replay.TeamNames = new[] { "队伍1", "队伍2", "中立" };
		for (var i = 0; i < clouds.Length; i++)
		{
			clouds[i].texture = cloudTextures[Random.Range(0, cloudTextures.Length)];
			clouds[i].Reset();
			clouds[i].rect.x = Random.Range(-clouds[i].rect.width, Screen.width);
		}
	}

	private bool Confirm(string message)
	{
		var value = false;
		GUILayout.BeginArea(new Rect(Screen.width * 0.4f, Screen.height * 0.4f, Screen.width * 0.2f, Screen.height * 0.2f), GUI.skin.box);
		Methods.GUI.Confirm(message, ref value, ref stagedState);
		GUILayout.EndArea();
		return value;
	}

	private void DrawBackground()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), background);
		for (var i = 0; i < clouds.Length; i++)
			if (clouds[i].layer == 0)
				GUI.DrawTexture(clouds[i].rect, clouds[i].texture);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), ship);
		for (var i = 0; i < clouds.Length; i++)
		{
			if (clouds[i].layer == 1)
				GUI.DrawTexture(clouds[i].rect, clouds[i].texture);
			if (clouds[i].rect.x > Screen.width || clouds[i].rect.x < -clouds[i].rect.width)
				clouds[i].Reset();
		}
	}

	private void DrawDefaultInterface()
	{
		GUILayout.BeginArea(defaultRect);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("回  放", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
			stagedState = MenuState.BrowsingFile;
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("选  项", Data.GUI.Button.Medium))
		{
			stagedState = MenuState.Options;
			Methods.GUI.StageCurrentOptions();
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("关  于", Data.GUI.Button.Medium))
		{
			stagedState = MenuState.About;
			GUI.FocusWindow(0);
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("退  出", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
			stagedState = MenuState.Quit;
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();
	}

	private void DrawFileBrowser()
	{
		if (!fileBrowser.Draw())
			return;
		stagedState = MenuState.Default;
		if (fileBrowser.outputFile == null || fileBrowser.outputFile.Extension != ".battle")
			return;
		Data.Battle = new JSONObject(File.ReadAllText(fileBrowser.outputFile.FullName).Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));
		Application.LoadLevel("BattleField");
	}

	private void DrawOptions()
	{
		GUILayout.BeginArea(optionsRect, GUI.skin.box);
		Methods.GUI.DrawOptions(ref stagedState);
		GUILayout.EndArea();
	}

	private void InitializeGUI()
	{
		Methods.GUI.InitializeStyles();
		(fileBrowser = new FileBrowser { backTexture = back, directoryTexture = directory, driveTexture = drive, fileTexture = file, confirmStyle = Data.GUI.Button.Small, cancelStyle = Data.GUI.Button.Small }).Refresh();
		guiInitialized = true;
	}

	private void OnDestroy() { Delegates.ScreenSizeChanged -= OnScreenSizeChanged; }

	private void OnGUI()
	{
		if (!guiInitialized)
			InitializeGUI();
		if (Event.current.type == EventType.Layout)
			state = stagedState;
		DrawBackground();
		switch (state)
		{
			case MenuState.Default:
				DrawDefaultInterface();
				break;
			case MenuState.BrowsingFile:
				DrawFileBrowser();
				break;
			case MenuState.Options:
				DrawOptions();
				break;
			case MenuState.About:
				aboutRect = GUILayout.Window(0, aboutRect, AboutWindow, "");
				break;
			case MenuState.Quit:
				if (Confirm("退出"))
					Application.Quit();
				break;
		}
	}

	private void OnScreenSizeChanged()
	{
		for (var i = 0; i < clouds.Length; i++)
			clouds[i].Refresh(Monitor.ScreenSize);
		defaultRect = new Rect(Screen.width * 0.75f, Screen.height * 0.6f, Screen.width * 0.1f, Screen.height * 0.3f);
		optionsRect = new Rect(Screen.width * 0.15f, Screen.height * 0.1f, Screen.width * 0.7f, Screen.height * 0.8f);
		aboutRect = new Rect(Screen.width * 0.3f, Screen.height * 0.2f, Screen.width * 0.4f, Screen.height * 0.6f);
	}

	private void Update()
	{
		for (var i = 0; i < clouds.Length; i++)
			clouds[i].rect.x += clouds[i].speed * Time.smoothDeltaTime;
	}

	private struct Cloud
	{
		public int layer;
		public Rect rect;
		public float speed;
		public Texture texture;

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
}