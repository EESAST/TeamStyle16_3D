#region

using System.IO;
using UnityEngine;

#endregion

namespace GameStatics
{
	public static class Data
	{
		public static JSONObject BattleData = new JSONObject(File.ReadAllText("Assets/Files/Battles/sample.battle").Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));
		public static bool GamePaused;
		public static bool[,] IsOccupied;
		public static Vector2 MapSize;

		public static class GUI
		{
			public static Color[] LastTeamColors = new Color[4];
			public static string[] OptionTexts = { "队伍颜色", "敬请期待……" };
			public static GUIContent Random = new GUIContent("随机", Resources.Load<Texture>("Dice"));
			public static Color[] StagedTeamColors = new Color[4];
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
				public static GUIStyle[] TeamColor = new GUIStyle[3];
			}
		}

		public static class MiniMap
		{
			public static Rect Rect;
			public static float ScaleFactor;
		}

		public static class TeamColor
		{
			public static Color[] Current;
			public static Color[] Desired = { Color.magenta, Color.cyan, Color.gray, Color.white };
		}
	}
}