public static class Constants
{
	public static string[] BaseTypeNames = { "Base", "Fort", "Mine", "Oilfield", "Submarine", "Ship", "Ship", "Ship", "Plane", "Plane" };
	public static int[] BuildRounds = { 0, 0, 0, 0, 2, 3, 5, 3, 3, 2 };
	public static string[] ChineseNames = { "基地", "据点", "矿场", "油田", "潜艇", "驱逐舰", "航母", "运输舰", "战斗机", "侦察机" };
	public static int[] Costs = { 0, 0, 0, 0, 7, 14, 24, 12, 14, 10 };
	public static string[] TypeNames = { "Base", "Fort", "Mine", "Oilfield", "Submarine", "Destroyer", "Carrier", "Cargo", "Fighter", "Scout" };

	public static class Score
	{
		public static int PerCollectedResource = 1;
		public static int PerDamage = 1;
		public static int PerFortPerFrame = 1;
	}
}