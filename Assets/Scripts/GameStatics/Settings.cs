#region

using UnityEngine;

#endregion

public static class Settings
{
	private const float _shadowDistance = 50;
	public const float AngularTolerance = 0.05f;
	public const float DeltaTime = 0.1f;
	public const float DimensionalTolerancePerUnitSpeed = 0.05f;
	public const float DimensionScaleFactor = 5;
	public const float FastAttenuation = 0.6f;
	public const int MainInterface_CloudNum = 50;
	public const float MaxTimePerFrame = 15;
	public const float SlowAttenuation = 0.9f;
	public const float Tolerance = 0.01f;
	public const float TransitionRate = 3;
	private static readonly Vector3 _messagePositionOffset = Vector3.up * 0.5f;
	public static float DimensionalTolerance { get { return Tolerance * DimensionScaleFactor; } }
	public static Vector3 MessagePositionOffset { get { return _messagePositionOffset * DimensionScaleFactor; } }
	public static float ShadowDistance { get { return _shadowDistance * DimensionScaleFactor; } }

	public static class Audio
	{
		private const float _maxAudioDistance = 25;
		public static float MaxAudioDistance { get { return _maxAudioDistance * DimensionScaleFactor; } }

		public static class Volume
		{
			public const float Background = 0.6f;
			public const float Beam = 0.6f;
			public const float Bomb = 0.8f;
			public const float Death1 = 0.8f;
			public const float Death3 = 1;
			public const float FortScore = 0.4f;
			public const float Prompt = 0.6f;
			public const float Unit = 0.5f;
		}
	}

	public static class Bomb
	{
		private const float _speed = 3;
		public const float AngularCorrectionRate = 360;
		public const float Noise = 1;
		public static float Speed { get { return _speed * DimensionScaleFactor; } }
	}

	public static class Camera
	{
		private const float _farClipPlane = 100;
		public static Color BackgroundColor = new Color(0, 0.4f, 0.7f, 1);
		public static float FarClipPlane { get { return _farClipPlane * DimensionScaleFactor; } }

		public static class Movement
		{
			private const float _rate = 10;
			public static readonly float DefaultHeight = (Map.HeightOfLevel[0] + Map.HeightOfLevel[3]) / 2;
			public static float Rate { get { return _rate * DimensionScaleFactor; } }
		}

		public static class Rotation
		{
			public const bool AutoRevert = true;
			public const bool Locked = false;
		}

		public static class Zoom
		{
			private const float _default = 8;
			private const float _max = 16;
			private const float _min = 2;
			private const float _rate = 600;
			public static float Default { get { return _default * DimensionScaleFactor; } }
			public static float Max { get { return _max * DimensionScaleFactor; } }
			public static float Min { get { return _min * DimensionScaleFactor; } }
			public static float Rate { get { return _rate * DimensionScaleFactor; } }
		}
	}

	public static class Fragment
	{
		private const float _thicknessPerUnitSize = 0.03f;
		public const float MaxLifeSpan = 24;
		public const float MinLifeSpan = 8;
		public static float ThicknessPerUnitSize { get { return _thicknessPerUnitSize * DimensionScaleFactor; } }
	}

	public static class GUI
	{
		public const float LineThickness = 3;
		public const int MaxProductionEntryNumPerRow = 5;
		public const int TextGranularity = 5;
	}

	public static class HealthBar
	{
		private static readonly Vector3 _positionOffset = Vector3.up * 0.3f;
		public static Color EmptyColor = Color.gray;
		public static Color FullColor = new Color(0.6f, 0, 0);
		public static Vector3 PositionOffset { get { return _positionOffset * DimensionScaleFactor; } }
	}

	public static class IdleRotation
	{
		public const float MaxRestTime = 4;
		public const float MinRestTime = 1;
	}

	public static class Interceptor
	{
		private const float _speed = 1;
		public const float AngularCorrectionRate = 180;
		public static float Speed { get { return _speed * DimensionScaleFactor; } }
	}

	public static class Map
	{
		private static readonly float[] _heightOfLevel = { 2, 2.8f, 3, 6 }; //stands for underwater, water surface, ground and air in sequence
		public static readonly RectOffset MapSizeOffset = new RectOffset(80, 80, 80, 80);
		public static float[] HeightOfLevel { get { return Methods.Array.Multiply(_heightOfLevel, DimensionScaleFactor); } }
	}

	public static class MiniMap
	{
		public const int Granularity = 2; //Must be even to ensure proper display of the texture
		public static readonly RectOffset Border = new RectOffset(19, 39, 31, 31); //Corresponds to the rect offset of the mini frame sprite
		public static Color LandColor = new Color(0, 0.8f, 0, 0.8f);
		public static Color LineColor = Color.yellow;
		public static Color OceanColor = new Color(0, 0, 0.6f, 0.6f);
	}

	public static class Ocean
	{
		public const float FogDensity = 0.02f;
		public static Color FogColor = new Color(0, 0.2f, 0.6f, 1);
		public static Color ReflectionColor = new Color(0.5f, 0.5f, 1, 0.5f);
		public static Color RefractionColor = new Color(0, 0, 0.4f, 0.6f);
		public static LayerMask UnderwaterCullingMask = LayerMask.GetMask("Water", "UI", "Element", "Fragment", "Bomb");
	}

	public static class Replay
	{
		private const float _beamSpeed = 1.5f;
		public const float CollectRate = 50;
		public const float CreateTime = 3;
		public const float FixRate = 30;
		public const float MessageTime = 3; //corresponds to the TextFX prefab
		public const float SupplyRate = 50;
		public static float BeamSpeed { get { return _beamSpeed * DimensionScaleFactor; } }
	}

	public static class SteeringRate
	{
		public const float Base_BigGuns = 10;
		public const float Base_Head = 100;
		public const float Base_SmallGuns = 10;
		public const float Destroyer_Barrel = 20;
		public const float Destroyer_Swivel = 100;
		public const float Fort_Cannon = 100;
	}

	public static class Terrain
	{
		public const float Smoothness = 8;

		public static class Detail
		{
			private const float _maxDimension = 0.12f;
			private const float _maxVisibleDistance = 50;
			private const float _minDimension = 0.05f;
			public const float Density = 1;
			public static float MaxDimension { get { return _maxDimension * DimensionScaleFactor; } }
			public static float MaxVisibleDistance { get { return _maxVisibleDistance * DimensionScaleFactor; } }
			public static float MinDimension { get { return _minDimension * DimensionScaleFactor; } }

			public static class Waving
			{
				public const float Amount = 0.5f;
				public const float Speed = 1;
				public const float Strength = 0.5f;
			}
		}

		public static class Tree
		{
			private const float _billboardDistance = 50;
			public const float Density = 0.15f;
			public const float VerticalPositionOffset = -0.1f;
			public static float BillboardDistance { get { return _billboardDistance * DimensionScaleFactor; } }
		}
	}
}