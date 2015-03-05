#region

using System.Collections.Generic;
using JSON;
using UnityEngine;

#endregion

public static class Data
{
	public static JSONObject Battle;
	public static bool GamePaused;
	public static GameObject GlobalMonitor;
	public static bool[,] IsOccupied;
	public static Vector2 MapSize;

	public static class GUI
	{
		public static Vector2 AboutScroll;
		public static Texture2D Dice;
		public static Vector2 FontScroll;
		public static float FontSizeScaleFactor = 1;
		public static bool Initialized;
		public static Vector2 LegendScroll;
		public static float LineThickness;
		public static readonly List<Rect> OccupiedRects = new List<Rect>();
		public static int OptionSelected;
		public static float ProductionEntrySize;
		public static readonly GUIContent Random = new GUIContent();
		public static float StagedFontSizeScaleFactor;
		public static readonly Color[] StagedTeamColor = new Color[4];
		public static readonly GUIStyle[] TeamColoredBoxes = new GUIStyle[3];
		public static readonly Texture2D[] TeamColoredTextures = new Texture2D[3];
		public static Vector2 TeamColorScroll;
		public static GUIStyle TextField;

		public static class Button
		{
			public static GUIStyle Large;
			public static GUIStyle Medium;
			public static GUIStyle Small;
		}

		public static class Label
		{
			public static GUIStyle HugeMiddle;
			public static GUIStyle LargeLeft;
			public static GUIStyle LargeMiddle;
			public static readonly GUIStyle[] RGB = new GUIStyle[3];
			public static GUIStyle SmallLeft;
			public static GUIStyle SmallMiddle;
			public static readonly GUIStyle[] TeamColored = new GUIStyle[3];
		}
	}

	public static class MiniMap
	{
		public static Rect FrameRect;
		public static Rect MapRect;
		public static int MarkPatternIndex;
		public static float MarkScaleFactor = 1;
		public static float ScaleFactor;
		public static int StagedMarkPatternIndex;
		public static float StagedMarkScaleFactor;
	}

	public static class Replay
	{
		public static int AttacksLeft;
		public static Base[] Bases;
		public static int CollectsLeft;
		public static int CreatesLeft;
		public static float[] CurrentScores;
		public static Dictionary<int, Element> Elements;
		public static int FixesLeft;
		public static List<Fort>[] Forts;
		public static int FrameCount;
		public static Dictionary<int, int> InitialStorage;
		public static Replayer Instance;
		public static int MovesLeft;
		public static int[] Populations;
		public static List<ProductionEntry>[] ProductionLists;
		public static float ProductionTimeScale;
		public static bool ShowSummary;
		public static int SuppliesLeft;
		public static int[] TargetScores;
		public static string[] TeamNames;
		public static int MaxPopulation;
		public static int[] UnitNums;
	}

	public static class TeamColor
	{
		public static readonly Color[] Current = new Color[4];
		public static readonly Color[] Target = { Color.magenta, Color.cyan, Color.gray, Color.white };
	}
}