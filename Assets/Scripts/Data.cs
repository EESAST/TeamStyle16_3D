#region

using System.IO;
using UnityEngine;

#endregion

namespace GameStatics
{
	public static class Data
	{
		public static JSONObject BattleData = new JSONObject(File.ReadAllText("Assets/Files/Battles/success.battle").Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));
		public static bool GamePaused;
		public static bool[,] IsOccupied;
		public static Vector2 MapSize;
		public static float SeaLevel;

		public static class MiniMap
		{
			public static Rect Rect;
			public static float ScaleFactor;
		}

		public static class TeamColor
		{
			public static Color[] Current;
			public static Color[] Desired;
		}
	}
}