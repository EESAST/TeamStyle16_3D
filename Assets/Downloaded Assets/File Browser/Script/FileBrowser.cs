#region

using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

#endregion

public class FileBrowser
{
	private bool autoGUIRect;
	public GUIStyle backStyle;
	public GUIStyle cancelStyle;
	private DirectoryInfo currentDirectory;
	private Color defaultColor;
	private GUISkin defaultSkin;
	private DirectoryInformation[] directories, drives;
	private Vector2 directoryScroll = Vector2.zero;
	private Vector2 driveScroll = Vector2.zero;
	private FileInformation[] files;
	private Vector2 fileScroll = Vector2.zero;
	public Texture2D fileTexture, directoryTexture, backTexture, driveTexture;
	public bool forceOutput;
	public bool forceSearch;
	private Rect guiRect;
	public GUISkin guiSkin;
	private bool isSearching;
	private bool justFinishedSearching;
	public FileInfo outputFile;
	private DirectoryInformation parentDirectory;
	public bool recursiveSearch = true;
	private float searchStartTime;
	private string searchString = "";
	public Color selectedColor = new Color(0.5f, 0.5f, 0.9f);
	private int selectedFileIndex = -1;
	public GUIStyle selectStyle;
	private bool showDrives;
	public bool showSearchBar = true;

	public FileBrowser(string directory, Rect guiRect)
	{
		GetFileList(currentDirectory = new DirectoryInfo(directory));
		this.guiRect = guiRect;
	}

	public FileBrowser(string directory)
	{
		GetFileList(currentDirectory = new DirectoryInfo(directory));
		autoGUIRect = true;
	}

	public FileBrowser() : this(Directory.GetCurrentDirectory()) { }

	public Rect GUIRect
	{
		get { return guiRect; }
		set
		{
			guiRect = value;
			autoGUIRect = false;
		}
	}

	public bool Draw()
	{
		if (forceOutput)
		{
			forceOutput = false;
			return true;
		}
		if (justFinishedSearching && Event.current.type == EventType.Layout)
		{
			justFinishedSearching = isSearching = false;
			SelectFile(selectedFileIndex);
		}
		if (GUI.GetNameOfFocusedControl() == "")
			GUI.FocusControl("SearchBar");
		if (autoGUIRect)
			guiRect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f, Screen.width * 0.8f, Screen.height * 0.8f);
		if (guiSkin)
		{
			defaultSkin = GUI.skin;
			GUI.skin = guiSkin;
		}
		GUILayout.BeginArea(guiRect);
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal("box");
		GUILayout.FlexibleSpace();
		GUILayout.Label(currentDirectory.FullName);
		GUILayout.FlexibleSpace();
		if (showSearchBar)
			DrawSearchBar();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal("box");
		GUILayout.BeginVertical(GUILayout.Width(guiRect.width / 3));
		if (showDrives)
		{
			GUILayout.BeginVertical("box");
			driveScroll = GUILayout.BeginScrollView(driveScroll);
			foreach (var drive in drives.Where(drive => drive.Button()))
				GetFileList(drive.directoryInfo);
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
		else if (parentDirectory.Button(backStyle))
			GetFileList(parentDirectory.directoryInfo);
		GUILayout.BeginVertical("box");
		directoryScroll = GUILayout.BeginScrollView(directoryScroll);
		foreach (var directory in directories.Where(directory => directory.Button()))
			GetFileList(directory.directoryInfo);
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		GUILayout.BeginVertical("box");
		if (isSearching)
			DrawSearchMessage();
		else
		{
			fileScroll = GUILayout.BeginScrollView(fileScroll);
			for (var i = 0; i < files.Length; i++)
			{
				if (i == selectedFileIndex)
				{
					defaultColor = GUI.color;
					GUI.color = selectedColor;
				}
				GUI.SetNextControlName(i.ToString());
				if (files[i].Button())
					SelectFile(i);
				if (i == selectedFileIndex)
					GUI.color = defaultColor;
			}
			GUILayout.EndScrollView();
		}
		GUILayout.EndVertical();
		GUILayout.BeginHorizontal("box");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("确定", selectStyle ?? "button"))
			return true;
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("取消", cancelStyle ?? "button"))
		{
			SelectFile(-1);
			return true;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		if (guiSkin)
			GUI.skin = defaultSkin;
		return false;
	}

	private void DrawSearchBar()
	{
		if (isSearching)
			GUILayout.Label("正在搜索：\"" + searchString + "\"");
		else
		{
			GUI.SetNextControlName("SearchBar");
			searchString = GUILayout.TextField(searchString, GUILayout.MinWidth(guiRect.width / 5));
			if (GUILayout.Button("搜索") || forceSearch)
			{
				forceSearch = false;
				isSearching = true;
				searchStartTime = Time.time;
				new Thread(SearchFile).Start();
			}
		}
		GUILayout.Space(guiRect.width / 100);
	}

	private void DrawSearchMessage()
	{
		var elapsedTime = Time.time - searchStartTime;
		if (elapsedTime > 1)
			GUILayout.Button("正在");
		if (elapsedTime > 2)
			GUILayout.Button("搜索");
		if (elapsedTime > 3)
			GUILayout.Button("\"" + searchString + "\"");
		if (elapsedTime > 4)
			GUILayout.Button("……");
		if (elapsedTime > 5)
			GUILayout.Button("这将");
		if (elapsedTime > 6)
			GUILayout.Button("花费");
		if (elapsedTime > 7)
			GUILayout.Button("一点");
		if (elapsedTime > 8)
			GUILayout.Button("时间");
		if (elapsedTime > 9)
			GUILayout.Button("……");
	}

	private void GetFileList(DirectoryInfo directory)
	{
		SelectFile(-1);
		currentDirectory = directory;
		parentDirectory = new DirectoryInformation(directory.Parent ?? directory, backTexture);
		showDrives = directory.Parent == null;
		var driveNames = Directory.GetLogicalDrives();
		drives = new DirectoryInformation[driveNames.Length];
		for (var i = 0; i < drives.Length; i++)
			drives[i] = new DirectoryInformation(new DirectoryInfo(driveNames[i]), driveTexture);
		var directoryInfos = directory.GetDirectories();
		directories = new DirectoryInformation[directoryInfos.Length];
		for (var i = 0; i < directories.Length; i++)
			directories[i] = new DirectoryInformation(directoryInfos[i], directoryTexture);
		var fileInfos = directory.GetFiles();
		files = new FileInformation[fileInfos.Length];
		for (var i = 0; i < files.Length; i++)
			files[i] = new FileInformation(fileInfos[i], fileTexture);
	}

	public void SelectNext() { SelectFile(selectedFileIndex = (selectedFileIndex + 1) % files.Length); }

	public void SelectLast() { SelectFile(selectedFileIndex = (selectedFileIndex + files.Length - 1) % files.Length); }

	public void Refresh() { GetFileList(currentDirectory); }

	private void SearchFile()
	{
		var fileInfos = searchString == "" ? currentDirectory.GetFiles() : currentDirectory.GetFiles(searchString, recursiveSearch ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		files = new FileInformation[fileInfos.Length];
		if (fileInfos.Length == 0)
			selectedFileIndex = -1;
		else
		{
			for (var i = 0; i < files.Length; i++)
				files[i] = new FileInformation(fileInfos[i], fileTexture);
			selectedFileIndex = 0;
		}
		justFinishedSearching = true;
	}

	private void SelectFile(int index)
	{
		if ((selectedFileIndex = index) < 0)
		{
			outputFile = null;
			GUI.FocusControl("");
		}
		else
		{
			outputFile = files[index].fileInfo;
			GUI.FocusControl(index.ToString());
		}
	}
}