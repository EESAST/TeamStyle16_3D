#region

using System.Collections.Generic;
using System.IO;
using JSON;
using UnityEngine;

#endregion

public static class Data
{
	public static JSONObject Battle = new JSONObject(File.ReadAllText("Assets/Files/Battles/aihuman.battle").Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));
	public static bool GamePaused;
	public static bool[,] IsOccupied;
	public static Vector2 MapSize;

	public static class GUI
	{
		public static Vector2 AboutScroll = Vector2.zero;
		public static Texture2D Dice;
		public static bool Initialized;
		public static Vector2 LegendScroll = Vector2.zero;
		public static int OptionSelected;
		public static GUIContent Random = new GUIContent();
		public static int StagedMarkPatternIndex;
		public static float StagedMarkScaleFactor;
		public static Color[] StagedTeamColor = new Color[4];
		public static GUIStyle[] TeamColoredBoxes = new GUIStyle[3];
		public static Texture2D[] TeamColoredTextures = new Texture2D[3];
		public static Vector2 TeamColorScroll = Vector2.zero;

		public static class Button
		{
			public static GUIStyle Large;
			public static GUIStyle Medium;
			public static GUIStyle Small;
		}

		public static class Label
		{
			public static GUIStyle Huge;
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
		public static Base[] Bases;
		public static int CollectsLeft;
		public static int CreatesLeft;
		public static float[] CurrentScores;
		public static Dictionary<int, Element> Elements;
		public static int FixesLeft;
		public static List<Fort>[] Forts;
		public static int FrameCount;
		public static bool IsAttacking;
		public static bool IsCollecting;
		public static bool IsCreating;
		public static bool IsFixing;
		public static bool IsMoving;
		public static bool IsSupplying;
		public static int MovesLeft;
		public static int[] Populations;
		public static float ProductionEntrySize;
		public static List<ProductionEntry>[] ProductionLists;
		public static float ProductionTimer;
		public static float ProductionTimeScale = 1;
		public static int[,] Statictics;
		public static int SuppliesLeft;
		public static int[] TargetScores;
		public static string[] TeamNames;
		public static int[] UnitNums;
	}

	public static class TeamColor
	{
		public static Color[] Current = new Color[4];
		public static Color[] Target = { Color.magenta, Color.cyan, Color.gray, Color.white };
	}
}