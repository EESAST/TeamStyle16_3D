#region

using System.IO;
using UnityEngine;

#endregion

public static class Data
{
	public static JSONObject BattleData = new JSONObject(File.ReadAllText("Assets/Files/Battles/sample.battle").Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));
	public static bool GamePaused;
	public static bool[,] IsOccupied;
	public static Vector2 MapSize;
	public static int MarkPatternIndex;
	public static float MarkScaleFactor = 1;

	public static class GUI
	{
		public static Vector2 AboutScroll = Vector2.zero;
		public static bool Initialized;
		public static Vector2 OptionScroll = Vector2.zero;
		public static int OptionSelected;
		public static GUIContent Random = new GUIContent("随机", Resources.Load<Texture>("Dice"));
		public static int StagedMarkPatternIndex;
		public static float StagedMarkScaleFactor;
		public static Color[] StagedTeamColor = new Color[4];
		public static GUIStyle[] TeamColoredBoxes = new GUIStyle[3];
		public static Texture2D[] TeamColoredTextures = new Texture2D[3];
		public static string[] TeamDescriptions = { "队伍1", "队伍2", "中立" };

		public static class Button
		{
			public static GUIStyle Large;
			public static GUIStyle Medium;
			public static GUIStyle Small;
		}

		public static class Label
		{
			public static GUIStyle Large;
			public static GUIStyle LargeMiddle;
			public static GUIStyle[] RGB = new GUIStyle[3];
			public static GUIStyle Small;
			public static GUIStyle[] TeamColored = new GUIStyle[3];
		}
	}

	public static class MiniMap
	{
		public static Rect Rect;
		public static float ScaleFactor;
	}

	public static class TeamColor
	{
		public static Color[] Current = new Color[4];
		public static Color[] Desired = { Color.magenta, Color.cyan, Color.gray, Color.white };
	}
}