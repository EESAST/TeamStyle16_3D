#region

using System.Collections.Generic;
using System.IO;
using UnityEngine;

#endregion

public static class Data
{
	public static Base[] Bases = new Base[2];
	public static JSONObject Battle = new JSONObject(File.ReadAllText("Assets/Files/Battles/success.battle").Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));
	public static int[] FortNum;
	public static bool GamePaused;
	public static bool[,] IsOccupied;
	public static Vector2 MapSize;
	public static bool Ready;

	public static class GUI
	{
		public static Vector2 AboutScroll = Vector2.zero;
		public static bool Initialized;
		public static Vector2 LegendScroll = Vector2.zero;
		public static int OptionSelected;
		public static GUIContent Random = new GUIContent("随机", Resources.Load<Texture>("Dice"));
		public static int StagedMarkPatternIndex;
		public static float StagedMarkScaleFactor;
		public static Color[] StagedTeamColor = new Color[4];
		public static GUIStyle[] TeamColoredBoxes = new GUIStyle[3];
		public static Texture2D[] TeamColoredTextures = new Texture2D[3];
		public static Vector2 TeamColorScroll = Vector2.zero;
		public static string[] TeamDescriptions = { "队伍1", "队伍2", "中立" };

		public static class Button
		{
			public static GUIStyle Large;
			public static GUIStyle Medium;
			public static GUIStyle Small;
		}

		public static class Label
		{
			public static GUIStyle LargeLeft;
			public static GUIStyle LargeMiddle;
			public static GUIStyle[] RGB = new GUIStyle[3];
			public static GUIStyle SmallLeft;
			public static GUIStyle SmallMiddle;
			public static GUIStyle[] TeamColored = new GUIStyle[3];
		}
	}

	public static class MiniMap
	{
		public static int MarkPatternIndex;
		public static float MarkScaleFactor = 1;
		public static Rect Rect;
		public static float ScaleFactor;
	}

	public static class Replay
	{
		public static int AttacksLeft;
		public static int CollectsLeft;
		public static float[] CurrentScores = new float[2];
		public static Dictionary<int, Element> Elements;
		public static int FixesLeft;
		public static bool IsAttacking;
		public static bool IsCollecting;
		public static bool IsFixing;
		public static bool IsMoving;
		public static bool IsSupplying;
		public static int MovesLeft;
		public static int[] Populations = new int[2];
		public static float ProductionEntrySize;
		public static List<ProductionEntry>[] ProductionLists;
		public static bool ProductionPaused;
		public static float ProductionTimer;
		public static float ProductionTimeScale = 1;
		public static int SuppliesLeft;
		public static int[] TargetScores = new int[2];
		public static int[] UnitNums = new int[2];
	}

	public static class TeamColor
	{
		public static Color[] Current = new Color[4];
		public static Color[] Target = { Color.magenta, Color.cyan, Color.gray, Color.white };
	}
}