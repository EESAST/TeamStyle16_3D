#region

using UnityEngine;

#endregion

public static class Settings
{
	private static readonly float _beamSpeed = 1.5f;
	private static readonly float _shadowDistance = 50;
	public static float AngularTolerance = 0.05f;
	public static int CloudNumber = 50;
	public static float CreateTime = 3;
	public static float DeltaTime = 0.1f;
	public static float FastAttenuation = 0.6f;
	public static int MaxEntryPerRow = 5;
	public static float MaxTimePerFrame = 10;
	public static float SlowAttenuation = 0.9f;
	public static int TextGranularity = 5;
	public static float Tolerance = 0.01f;
	public static float TransitionRate = 3;
	public static float DefaultMessageTime = 2;
	public static float BeamSpeed { get { return _beamSpeed * Map.ScaleFactor; } }
	public static float ShadowDistance { get { return _shadowDistance * Map.ScaleFactor; } }

	public static class Camera
	{
		private static readonly float _farClipPlane = 100;
		public static Color BackgroundColor = new Color(0, 0.4f, 0.7f, 1);
		public static float FarClipPlane { get { return _farClipPlane * Map.ScaleFactor; } }

		public static class Movement
		{
			private static readonly float _rate = 10;
			public static float DefaultHeight = (Map.HeightOfLevel[0] + Map.HeightOfLevel[3]) / 2;
			public static float Rate { get { return _rate * Map.ScaleFactor; } }
		}

		public static class Rotation
		{
			public static bool AutoRevert = true;
			public static bool Locked = false;
		}

		public static class Zoom
		{
			private static readonly float _default = 8;
			private static readonly float _max = 16;
			private static readonly float _min = 2;
			private static readonly float _rate = 600;
			public static float Default { get { return _default * Map.ScaleFactor; } }
			public static float Max { get { return _max * Map.ScaleFactor; } }
			public static float Min { get { return _min * Map.ScaleFactor; } }
			public static float Rate { get { return _rate * Map.ScaleFactor; } }
		}
	}

	public static class Fragment
	{
		public static float MaxLifeSpan = 16;
		public static float MinLifeSpan = 8;
		public static float ThicknessPerUnitSize = 0.03f;
	}

	public static class HealthBar
	{
		public static Color EmptyColor = Color.gray;
		public static Color FullColor = new Color(0.6f, 0, 0);
		public static float VerticalPositionOffset = 0.3f;
	}

	public static class Map
	{
		private static readonly float[] _heightOfLevel = { 2, 2.8f, 3, 6 }; //stands for underwater, water surface, ground and air in sequence
		public static RectOffset MapSizeOffset = new RectOffset(80, 80, 80, 80);
		public static float ScaleFactor = 5;
		public static float[] HeightOfLevel { get { return Methods.Array.Multiply(_heightOfLevel, ScaleFactor); } }
	}

	public static class MiniMap
	{
		public static RectOffset Border = new RectOffset(19, 39, 31, 31); //Corresponds to the rect offset of the mini frame sprite
		public static int Granularity = 2; //Must be even to ensure proper display of the texture
		public static Color LandColor = new Color(0, 0.8f, 0, 0.8f);
		public static Color OceanColor = new Color(0, 0, 0.6f, 0.6f);

		public static class ViewLine
		{
			public static Color Color = new Color(1, 0.92f, 0.016f, 0.8f);
			public static float Thickness = 3;
		}
	}

	public static class Ocean
	{
		public static Color FogColor = new Color(0, 0.2f, 0.6f, 1);
		public static float FogDensity = 0.02f;
		public static Color ReflectionColor = new Color(0.5f, 0.5f, 1, 0.5f);
		public static Color RefractionColor = new Color(0, 0, 0.4f, 0.6f);
		public static LayerMask UnderwaterCullingMask = LayerMask.GetMask("Water", "UI", "Element", "Fragment", "Shell");
	}

	public static class TeamColor
	{
		public static float Tolerance = 0.01f;
		public static float TransitionRate = 3;
	}

	public static class Terrain
	{
		public static float Smoothness = 8;

		public static class Detail
		{
			private static readonly float _maxDimension = 0.12f;
			private static readonly float _minDimension = 0.05f;
			public static float Density = 1;
			public static float MaxVisibleDistance = 250;
			public static float MaxDimension { get { return _maxDimension * Map.ScaleFactor; } }
			public static float MinDimension { get { return _minDimension * Map.ScaleFactor; } }

			public static class Waving
			{
				public static float Amount = 0.5f;
				public static float Speed = 1;
				public static float Strength = 0.5f;
			}
		}

		public static class Tree
		{
			private static readonly float _billboardDistance = 50;
			public static float Density = 0.15f;
			public static float VerticalPositionOffset = -0.1f;
			public static float BillboardDistance { get { return _billboardDistance * Map.ScaleFactor; } }
		}
	}
}