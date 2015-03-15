public static class Constants
{
	public static readonly int[] AmmoOnce = { 6, 4, 0, 0, 2, 4, 2, 0, 3, 1 };
	public static readonly string[] BaseTypeNames = { "Base", "Fort", "Mine", "Oilfield", "Submarine", "Ship", "Ship", "Ship", "Plane", "Plane" };
	public static readonly int[] BuildRounds = { 0, 0, 0, 0, 2, 3, 5, 3, 3, 2 };
	public static readonly string[] ChineseNames = { "基地", "据点", "矿场", "油田", "潜艇", "驱逐舰", "航母", "运输舰", "战斗机", "侦察机" };
	public static readonly int[] Costs = { 0, 0, 0, 0, 7, 14, 24, 12, 14, 10 };
	public static readonly int[] MaxHP = { 1000, 500, 0, 0, 35, 70, 120, 60, 70, 50 };
	public static readonly int[] Population = { 0, 0, 0, 0, 2, 3, 4, 1, 3, 1 };
	public static readonly int[] Speed = { 0, 0, 0, 0, 6, 7, 5, 11, 9, 10 };
	public static readonly string[] TypeNames = { "Base", "Fort", "Mine", "Oilfield", "Submarine", "Destroyer", "Carrier", "Cargo", "Fighter", "Scout" };

	public static class Score
	{
		public const int PerCollectedResource = 1;
		public const int PerDamage = 1;
		public const int PerFortPerRound = 1;
	}
}