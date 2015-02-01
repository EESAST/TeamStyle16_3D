#region

using System.IO;
using GameStatics;
using UnityEditor;
using UnityEngine;

#endregion

public class InterfaceManager : MonoBehaviour
{
	private readonly Cloud[] clouds = new Cloud[Settings.CloudNumber];
	public Texture2D back;
	public Texture2D backgroundImage;
	public Texture2D[] cloudTextures;
	public Texture2D drive;
	public Texture2D file;
	private FileBrowser fileBrowser;
	public Texture2D folder;
	public GUISkin[] guiSkins;
	public Texture2D shipImage;
	private bool showFileBrowser;

	private void Awake()
	{
		Methods.Game.Resume();
		fileBrowser = new FileBrowser
		{
			backTexture = back,
			directoryTexture = folder,
			driveTexture = drive,
			fileTexture = file,
			guiSkin = guiSkins[0],
			extension = ".battle",
			searchRecursively = true,
			showSearch = true
		};
		for (var i = 0; i < clouds.Length; i++)
		{
			clouds[i].cloudTexture = cloudTextures[Random.Range(0, cloudTextures.Length)];
			clouds[i].Reset();
			clouds[i].x = Random.Range(-clouds[i].width, Screen.width);
		}
	}

	private void OnGUI()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backgroundImage);
		for (var i = 0; i < clouds.Length; i++)
			if (clouds[i].layer == 0)
				GUI.DrawTexture(new Rect(clouds[i].x, clouds[i].y, clouds[i].width, clouds[i].height), clouds[i].cloudTexture);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), shipImage);
		for (var i = 0; i < clouds.Length; i++)
		{
			if (clouds[i].layer == 1)
				GUI.DrawTexture(new Rect(clouds[i].x, clouds[i].y, clouds[i].width, clouds[i].height), clouds[i].cloudTexture);
			if ((clouds[i].x += clouds[i].speed * Time.deltaTime) > Screen.width || clouds[i].x < -clouds[i].width)
				clouds[i].Reset();
		}

		int x = Screen.width * 3 / 4, y = Screen.height * 3 / 4, width = Screen.width / 16, height = Screen.height / 16;
		if (GUI.Button(new Rect(x, y, width, height), "回放"))
			showFileBrowser = true;
		if (GUI.Button(new Rect(x, y + height + 20, width, height), "退出"))
			Application.Quit();
		if (showFileBrowser)
		{
			fileBrowser.backStyle = new GUIStyle("button");
			if (fileBrowser.draw())
			{
				showFileBrowser = false;
				if (fileBrowser.outputFile != null)
				{
					Data.BattleData = new JSONObject(File.ReadAllText(fileBrowser.outputFile.FullName).Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));
					Application.LoadLevel("BattleField");
				}
			}
		}
		//if (Event.current.type != EventType.Layout)
		ProcessInput();
	}

	private void ProcessInput()
	{
		if (Event.current.type == EventType.KeyUp)
			switch (Event.current.keyCode)
			{
				case KeyCode.Return:
					if (showFileBrowser)
						if (GUI.GetNameOfFocusedControl() == "searchBar")
							fileBrowser.forceSearch = true;
						else
							fileBrowser.forceOutput = true;
					else
						showFileBrowser = true;
					break;
				case KeyCode.Escape:
				case KeyCode.Home:
					if (showFileBrowser)
						showFileBrowser = false;
					else
#if UNITY_EDITOR
						EditorApplication.isPlaying = false;
#else
						Application.Quit();
#endif
					break;
			}
	}

	private struct Cloud
	{
		public Texture2D cloudTexture;
		public float height;
		public int layer;
		public float speed;
		public float width;
		public float x;
		public float y;

		public void Reset()
		{
			speed = Random.Range(40, 100) * (Random.Range(0, 2) - 0.5f);
			width = Random.Range(Screen.width / 7f, Screen.width / 2f);
			height = Random.Range(Screen.height / 8f, Screen.height / 5f);
			x = speed > 0 ? -width : Screen.width;
			y = Random.Range(-height, Screen.height);
			layer = Random.Range(0, 2);
		}
	}
}