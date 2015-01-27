#region

using UnityEngine;

#endregion

namespace GameStatics
{
	public static class Settings
	{
		private static readonly float _shadowDistance = 50;
		public static int CloudNumber = 50;
		public static Vector4 MapSizeOffset = new Vector4(30, 10, 45, 45); //-x x -y y, in external space
		public static float ScaleFactor = 4;
		public static int TextGranularity = 3;
		public static float DeltaHeight { get { return _deltaHeight * ScaleFactor; } }
		public static float GroundHeight { get { return _groundHeight * ScaleFactor; } }
		public static float[] HeightOfLayer { get { return Methods.Array.Multiply(_heightOfLayer, ScaleFactor); } }
		public static float ShadowDistance { get { return _shadowDistance * ScaleFactor; } }

		public static class Camera
		{
			private static readonly float _farClipPlane = 100;
			public static float FarClipPlane { get { return _farClipPlane * ScaleFactor; } }

			public static class Movement
			{
				private static readonly float _defaultHeight = _groundHeight;
				private static readonly float _rate = 10;
				public static float DefaultHeight { get { return _defaultHeight * ScaleFactor; } }
				public static float Rate { get { return _rate * ScaleFactor; } }
			}

			public static class Rotation
			{
				public static bool AutoRevert = true;
				public static bool Locked = false;
			}

			public static class Zoom
			{
				private static readonly float _default = 8;
				private static readonly float _max = 12;
				private static readonly float _min = 4;
				private static readonly float _rate = 600;
				public static float Default { get { return _default * ScaleFactor; } }
				public static float Max { get { return _max * ScaleFactor; } }
				public static float Min { get { return _min * ScaleFactor; } }
				public static float Rate { get { return _rate * ScaleFactor; } }
			}
		}

		public static class HealthBar
		{
			public static Color EmptyColor = Color.gray;
			public static Color FullColor = new Color(0.6f, 0, 0);
			public static float VerticalPositionOffset = 1;
		}

		public static class MiniMap
		{
			public static float BorderOffset = 10;
			public static int Granularity = 2; //必须为偶数
			public static Color LandColor = new Color(0, 0.8f, 0, 0.6f);
			public static Color SeaColor = new Color(0, 0, 0.6f, 0.4f);

			public static class ViewLine
			{
				private static readonly float _thickness = 1;
				public static Color Color = new Color(1, 0.92f, 0.016f, 0.8f);
				public static int Granularity = 2; //必须为偶数
				public static float Thickness { get { return _thickness * Granularity; } }
			}
		}

		public static class Sea
		{
			private static readonly float _verticalPositionOffset = _deltaHeight * 0.75f;
			public static Color ReflectionColor = new Color(0.5f, 0.5f, 1, 0.5f);
			public static Color RefractionColor = new Color(0, 0, 0.4f, 0.6f);
			public static float VerticalPositionOffset { get { return _verticalPositionOffset * ScaleFactor; } }
		}

		public static class TeamColor
		{
			public static float Tolerance = 0.01f;
			public static float TransitionRate = 0.1f;
		}

		public static class Terrain
		{
			public static bool CastShadows = false;
			public static float Smoothness = 8;

			public static class Detail
			{
				private static readonly float _maxDimension = 0.12f;
				private static readonly float _minDimension = 0.05f;
				public static float Density = 1;
				public static float MaxVisibleDistance = 250;
				public static float MaxDimension { get { return _maxDimension * ScaleFactor; } }
				public static float MinDimension { get { return _minDimension * ScaleFactor; } }

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
				public static float Density =0.15f;
				public static float VerticalPositionOffset = -0.1f;
				public static float BillboardDistance { get { return _billboardDistance * ScaleFactor; } }
			}
		}

		private static readonly float _deltaHeight = 1;
		private static readonly float _groundHeight = 1.5f;
		private static readonly float[] _heightOfLayer = { _groundHeight - _deltaHeight, _groundHeight, _groundHeight + _deltaHeight };
	}
}