#region

using System.Collections.Generic;
using System.IO;
using UnityEngine;

#endregion

public static class Data
{
	public static JSONObject Battle = new JSONObject(File.ReadAllText("Assets/Files/Battles/sample.battle").Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));
	public static bool[,] IsOccupied;
	public static Vector2 MapSize;

	public static class Game
	{
		public static int AttacksLeft;
		public static int CollectsLeft;
		public static int FixesLeft;
		public static bool IsAttacking;
		public static bool IsCollecting;
		public static bool IsFixing;
		public static bool IsMoving;
		public static bool IsSupplying;
		public static int MovesLeft;
		public static bool Paused;
		public static bool Ready;
		public static int SuppliesLeft;
	}

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
		public static Base[] Bases = new Base[2];
		//public static JSONObject CurrentScores;
		public static float[] CurrentScores;
		public static Dictionary<int, Element> Elements;
		public static JSONObject Populations;
		public static float ProductionEntrySize;
		public static List<ProductionEntry>[] ProductionList;
		public static int[] TargetScores;
		public static JSONObject UnitNums;
	}

	public static class TeamColor
	{
		public static Color[] Current = new Color[4];
		public static Color[] Target = { Color.magenta, Color.cyan, Color.gray, Color.white };
	}
}