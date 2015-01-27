#define thread

#region

using System.IO;
using UnityEngine;
#if thread
using System.Threading;

#endif

#endregion

public class FileBrowser
{
	//public 
	//Optional Parameters
	public string name = "File Browser"; //Just hbPos name to identify the file browser with
	//GUI Options
	public GUISkin guiSkin; //The GUISkin to use

	public int layoutType { get { return layout; } } //returns the currentHeight Layout type

	public Texture2D fileTexture, directoryTexture, backTexture, driveTexture; //textures used to represent file types

	public GUIStyle backStyle, cancelStyle, selectStyle; //styles used for specific buttons

	public Color selectedColor = new Color(0.5f, 0.5f, 0.9f); //the color of the selected file

	public bool isVisible { get { return visible; } } //check if the file browser is currently visible
	//File Options
	public string searchPattern = "*"; //search pattern used to find files
	//Output
	public FileInfo outputFile; //the selected output file
	public bool forceOutput;
	//Search
	public bool showSearch = false; //show the search bar
	public bool forceSearch;
	public bool searchRecursively = false; //search currentHeight folder and sub folders
	//Protected	
	//GUI
	protected Vector2 fileScroll = Vector2.zero, folderScroll = Vector2.zero, driveScroll = Vector2.zero;

	protected Color defaultColor;
	protected int layout;
	protected Rect guiSize;
	protected GUISkin oldSkin;
	protected bool visible = false;
	//Search
	protected string searchBarString = ""; //string used in search bar
	protected bool isSearching; //do not show the search bar if searching
	protected bool forceSelectFile;
	//File Information
	protected DirectoryInfo currentDirectory;
	protected FileInformation[] files;

	protected DirectoryInformation[] directories, drives;

	protected DirectoryInformation parentDir;

	protected bool getFiles = true, showDrives;

	protected int selectedFile = -1;
	//Threading
	protected float startSearchTime;
#if thread
	protected Thread t;
#endif

	//Constructors
	public FileBrowser(string directory, int layoutStyle, Rect guiRect)
	{
		currentDirectory = new DirectoryInfo(directory);
		layout = layoutStyle;
		guiSize = guiRect;
	}

#if (UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8)
		public FileBrowser(string directory,int layoutStyle):this(directory,layoutStyle,new Rect(0,0,Screen.width,Screen.height)){}
		public FileBrowser(string directory):this(directory,1){}
#else
	public FileBrowser(string directory, int layoutStyle) : this(directory, layoutStyle, new Rect(Screen.width * 0.125f, Screen.height * 0.125f, Screen.width * 0.75f, Screen.height * 0.75f)) { }

	public FileBrowser(string directory) : this(directory, 0) { }
#endif

	public FileBrowser(Rect guiRect) : this() { guiSize = guiRect; }

	public FileBrowser(int layoutStyle) : this(Directory.GetCurrentDirectory(), layoutStyle) { }

	public FileBrowser() : this(Directory.GetCurrentDirectory()) { }

	//set variables
	public void setDirectory(string dir) { currentDirectory = new DirectoryInfo(dir); }

	public void setLayout(int l) { layout = l; }

	public void setGUIRect(Rect r) { guiSize = r; }

	//gui function to be called during OnGUI
	public bool draw()
	{
		if (forceOutput)
		{
			forceOutput = false;
			return true;
		}
		if (getFiles)
		{
			getFileList(currentDirectory);
			getFiles = false;
		}
		if (guiSkin)
		{
			oldSkin = GUI.skin;
			GUI.skin = guiSkin;
		}
		GUILayout.BeginArea(guiSize);
		GUILayout.BeginVertical("box");
		switch (layout)
		{
			case 0:
				GUILayout.BeginHorizontal("box");
				GUILayout.FlexibleSpace();
				GUILayout.Label(currentDirectory.FullName);
				GUILayout.FlexibleSpace();
				if (showSearch)
				{
					drawSearchField();
					GUILayout.Space(10);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal("box");
				GUILayout.BeginVertical(GUILayout.MaxWidth(300));
				folderScroll = GUILayout.BeginScrollView(folderScroll);
				if (showDrives)
				{
					foreach (var di in drives)
						if (di.button())
							getFileList(di.di);
				}
				else if ((backStyle != null) ? parentDir.button(backStyle) : parentDir.button())
					getFileList(parentDir.di);
				foreach (var di in directories)
					if (di.button())
						getFileList(di.di);
				GUILayout.EndScrollView();
				GUILayout.EndVertical();
				GUILayout.BeginVertical("box");
				if (isSearching)
					drawSearchMessage();
				else
				{
					fileScroll = GUILayout.BeginScrollView(fileScroll);
					for (var fi = 0; fi < files.Length; fi++)
					{
						if (selectedFile == fi)
						{
							defaultColor = GUI.color;
							GUI.color = selectedColor;
						}
						GUI.SetNextControlName(fi.ToString());
						if (files[fi].button())
						{
							GUI.FocusControl((selectedFile = fi).ToString());
							outputFile = files[selectedFile].fi;
						}
						if (selectedFile == fi)
							GUI.color = defaultColor;
					}
					if (forceSelectFile)
					{
						forceSelectFile = false;
						if (selectedFile != -1)
						{
							outputFile = files[selectedFile].fi;
							GUI.FocusControl(selectedFile.ToString());
						}
					}
					GUILayout.EndScrollView();
				}
				GUILayout.BeginHorizontal("box");
				GUILayout.FlexibleSpace();
				if ((selectStyle == null) ? GUILayout.Button("确定") : GUILayout.Button("确定", selectStyle))
					return true;
				GUILayout.FlexibleSpace();
				if ((cancelStyle == null) ? GUILayout.Button("取消") : GUILayout.Button("取消", cancelStyle))
				{
					outputFile = null;
					return true;
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				break;
			case 1: //mobile preferred layout
			default:
				if (showSearch)
				{
					GUILayout.BeginHorizontal("box");
					GUILayout.FlexibleSpace();
					drawSearchField();
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}

				fileScroll = GUILayout.BeginScrollView(fileScroll);
				if (isSearching)
					drawSearchMessage();
				else
				{
					if (showDrives)
					{
						GUILayout.BeginHorizontal();
						foreach (var di in drives)
							if (di.button())
								getFileList(di.di);
						GUILayout.EndHorizontal();
					}
					else if ((backStyle != null) ? parentDir.button(backStyle) : parentDir.button())
						getFileList(parentDir.di);

					foreach (var di in directories)
						if (di.button())
							getFileList(di.di);
					for (var fi = 0; fi < files.Length; fi++)
					{
						if (selectedFile == fi)
						{
							defaultColor = GUI.color;
							GUI.color = selectedColor;
						}
						GUI.SetNextControlName(fi.ToString());
						if (files[fi].button())
						{
							GUI.FocusControl((selectedFile = fi).ToString());
							outputFile = files[selectedFile].fi;
						}
						if (selectedFile == fi)
							GUI.color = defaultColor;
					}
					if (forceSelectFile)
					{
						forceSelectFile = false;
						if (selectedFile != -1)
						{
							outputFile = files[selectedFile].fi;
							GUI.FocusControl(selectedFile.ToString());
						}
					}
				}
				GUILayout.EndScrollView();

				if ((selectStyle == null) ? GUILayout.Button("确定") : GUILayout.Button("确定", selectStyle))
					return true;
				if ((cancelStyle == null) ? GUILayout.Button("取消") : GUILayout.Button("取消", cancelStyle))
				{
					outputFile = null;
					return true;
				}
				break;
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
		if (guiSkin)
			GUI.skin = oldSkin;
		return false;
	}

	protected void drawSearchField()
	{
		if (isSearching)
			GUILayout.Label("正在搜索：\"" + searchBarString + "\"");
		else
		{
			GUI.SetNextControlName("searchBar");
			searchBarString = ToValidFileName(GUILayout.TextField(searchBarString, GUILayout.MinWidth(150)));
			if (GUI.GetNameOfFocusedControl() == string.Empty)
				GUI.FocusControl("searchBar");
			if (GUILayout.Button("搜索") || forceSearch)
			{
				forceSearch = false;
				if (searchBarString.Length > 0)
				{
					isSearching = true;
#if thread
					startSearchTime = Time.time;
					t = new Thread(threadSearchFileList);
					t.Start(true);
#else
					searchFileList(currentDirectory);
#endif
				}
				else
					getFileList(currentDirectory);
				forceSelectFile = true;
			}
		}
	}

	protected void drawSearchMessage()
	{
		var tt = Time.time - startSearchTime;
		if (tt > 1)
			GUILayout.Button("正在");
		if (tt > 2)
			GUILayout.Button("搜索");
		if (tt > 3)
			GUILayout.Button("\"" + searchBarString + "\"");
		if (tt > 4)
			GUILayout.Button("……");
		if (tt > 5)
			GUILayout.Button("这将");
		if (tt > 6)
			GUILayout.Button("花费");
		if (tt > 7)
			GUILayout.Button("一点");
		if (tt > 8)
			GUILayout.Button("时间");
		if (tt > 9)
			GUILayout.Button("……");
	}

	public void getFileList(DirectoryInfo di)
	{
		selectedFile = -1;
		//set currentHeight directory
		currentDirectory = di;
		//get parent
		if (backTexture)
			parentDir = (di.Parent == null) ? new DirectoryInformation(di, backTexture) : new DirectoryInformation(di.Parent, backTexture);
		else
			parentDir = (di.Parent == null) ? new DirectoryInformation(di) : new DirectoryInformation(di.Parent);
		showDrives = di.Parent == null;

		//get drives
		var drvs = Directory.GetLogicalDrives();
		drives = new DirectoryInformation[drvs.Length];
		for (var v = 0; v < drvs.Length; v++)
			drives[v] = (driveTexture == null) ? new DirectoryInformation(new DirectoryInfo(drvs[v])) : new DirectoryInformation(new DirectoryInfo(drvs[v]), driveTexture);

		//get directories
		var dia = di.GetDirectories();
		directories = new DirectoryInformation[dia.Length];
		for (var d = 0; d < dia.Length; d++)
			if (directoryTexture)
				directories[d] = new DirectoryInformation(dia[d], directoryTexture);
			else
				directories[d] = new DirectoryInformation(dia[d]);

		//get files
		var fia = di.GetFiles(searchPattern);
		//FileInfo[] fia = searchDirectory(di,searchPattern);
		files = new FileInformation[fia.Length];
		for (var f = 0; f < fia.Length; f++)
			if (fileTexture)
				files[f] = new FileInformation(fia[f], fileTexture);
			else
				files[f] = new FileInformation(fia[f]);
	}

	public void searchFileList(DirectoryInfo di) { searchFileList(di, fileTexture != null); }

	protected void searchFileList(DirectoryInfo di, bool hasTexture)
	{
		//(searchBarString.IndexOf("*") >= 0)?searchBarString:"*"+searchBarString+"*"; //this allows for more intuitive searching for strings in file names
		var fia = di.GetFiles((searchBarString.IndexOf("*") >= 0) ? searchBarString : "*" + searchBarString + "*", (searchRecursively) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		files = new FileInformation[fia.Length];
		for (var f = 0; f < fia.Length; f++)
			if (hasTexture)
				files[f] = new FileInformation(fia[f], fileTexture);
			else
				files[f] = new FileInformation(fia[f]);
		if (fia.Length == 0)
			selectedFile = -1;
		else
			selectedFile = 0;

#if thread
#else
		isSearching = false;
#endif
	}

	protected void threadSearchFileList(object hasTexture)
	{
		searchFileList(currentDirectory, (bool)hasTexture);
		isSearching = false;
	}

	//search hbPos directory by hbPos search pattern, this is optionally recursive
	public static FileInfo[] searchDirectory(DirectoryInfo di, string sp, bool recursive) { return di.GetFiles(sp, (recursive) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly); }

	public static FileInfo[] searchDirectory(DirectoryInfo di, string sp) { return searchDirectory(di, sp, false); }

	public float brightness(Color c) { return c.r * .3f + c.g * .59f + c.b * .11f; }

	public string ToValidFileName(string fileName)
	{
		if (fileName != string.Empty && new string(Path.GetInvalidFileNameChars()).Contains(fileName.Substring(fileName.Length - 1)))
			return fileName.Remove(fileName.Length - 1);
		return fileName;
	}

	//to string
	public override string ToString() { return "Name: " + name + "\nVisible: " + isVisible + "\nDirectory: " + currentDirectory + "\nLayout: " + layout + "\nGUI Size: " + guiSize + "\nDirectories: " + directories.Length + "\nFiles: " + files.Length; }
}