#region

using System.Collections.Generic;
using System.Linq;
using JSON;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

#endregion

public static class Methods
{
	public static void CalculateTangents(this Mesh mesh)
	{
		var triangleCount = mesh.triangles.Length / 3;
		var vertexCount = mesh.vertices.Length;
		var tan1 = new Vector3[vertexCount];
		var tan2 = new Vector3[vertexCount];
		var tangents = new Vector4[vertexCount];

		for (var i = 0; i < triangleCount; i += 3)
		{
			var i1 = mesh.triangles[i + 0];
			var i2 = mesh.triangles[i + 1];
			var i3 = mesh.triangles[i + 2];

			var v1 = mesh.vertices[i1];
			var v2 = mesh.vertices[i2];
			var v3 = mesh.vertices[i3];

			var w1 = mesh.uv[i1];
			var w2 = mesh.uv[i2];
			var w3 = mesh.uv[i3];

			var x1 = v2.x - v1.x;
			var x2 = v3.x - v1.x;
			var y1 = v2.y - v1.y;
			var y2 = v3.y - v1.y;
			var z1 = v2.z - v1.z;
			var z2 = v3.z - v1.z;

			var s1 = w2.x - w1.x;
			var s2 = w3.x - w1.x;
			var t1 = w2.y - w1.y;
			var t2 = w3.y - w1.y;

			var r = 1.0f / (s1 * t2 - s2 * t1);
			var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
			var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

			tan1[i1] += sdir;
			tan1[i2] += sdir;
			tan1[i3] += sdir;

			tan2[i1] += tdir;
			tan2[i2] += tdir;
			tan2[i3] += tdir;
		}

		for (var i = 0; i < vertexCount; ++i)
		{
			var n = mesh.normals[i];
			var t = tan1[i];
			var tmp = (t - n * Vector3.Dot(n, t)).normalized;
			tangents[i] = new Vector4(tmp.x, tmp.y, tmp.z, Mathf.Sign(Vector3.Dot(Vector3.Cross(n, t), tan2[i])));
		}

		mesh.tangents = tangents;
	}

	public static Rect FitScreen(this Rect rect) { return new Rect(Mathf.Min(Mathf.Max(rect.x, 0), Screen.width - rect.width), Mathf.Min(Mathf.Max(rect.y, 0), Screen.height - rect.height), rect.width, rect.height); }

	public static void Line(this Texture2D texture, Vector2 lhs, Vector2 rhs, Color lineColor, float lineThickness)
	{
		var tmpPixels = texture.GetPixels32();
		var width = texture.width;
		var textureRect = new Rect(0, 0, width, texture.height);
		var deltaX = rhs.x - lhs.x;
		var deltaY = rhs.y - lhs.y;
		var k = deltaY / deltaX;
		if (Mathf.Abs(k) < 1)
		{
			var left = deltaX > 0 ? lhs : rhs;
			var right = deltaX > 0 ? rhs : lhs;
			var stepY = k;
			var dy = Mathf.Sqrt(1 + stepY * stepY) * lineThickness;
			for (float x = left.x, y = left.y; x <= right.x; x++, y += stepY)
				for (var yPrime = -dy / 2; yPrime <= dy / 2; yPrime++)
				{
					var posX = Mathf.RoundToInt(x);
					var posY = Mathf.RoundToInt(y + yPrime);
					if (textureRect.Contains(new Vector2(posX, posY)))
						tmpPixels[posX + width * posY] = lineColor;
				}
		}
		else
		{
			var down = deltaY > 0 ? lhs : rhs;
			var up = deltaY > 0 ? rhs : lhs;
			var stepX = 1 / k;
			var dx = Mathf.Sqrt(1 + stepX * stepX) * lineThickness;
			for (float x = down.x, y = down.y; y <= up.y; x += stepX, y++)
				for (var xPrime = -dx / 2; xPrime <= dx / 2; xPrime++)
				{
					var posX = Mathf.RoundToInt(x + xPrime);
					var posY = Mathf.RoundToInt(y);
					if (textureRect.Contains(new Vector2(posX, posY)))
						tmpPixels[posX + width * posY] = lineColor;
				}
		}
		texture.SetPixels32(tmpPixels);
	}

	public static void Polygon(this Texture2D texture, Vector2[] points, Color lineColor, float lineThickness)
	{
		for (var i = 0; i < points.Length; i++)
			texture.Line(points[i], points[(i + 1) % points.Length], lineColor, lineThickness);
	}

	public static Rect RectLerp(this Rect from, Rect to, float t) { return new Rect { x = Mathf.Lerp(from.x, to.x, t), y = Mathf.Lerp(from.y, to.y, t), width = Mathf.Lerp(from.width, to.width, t), height = Mathf.Lerp(from.height, to.height, t) }; }

	public static Vector3[] TransformPoints(this Transform transform, Vector3[] points, out Vector3 center)
	{
		var results = new Vector3[points.Length];
		center = Vector3.zero;
		for (var i = 0; i < results.Length; i++)
			center += results[i] = transform.TransformPoint(points[i]);
		center /= results.Length;
		return results;
	}

	public static Vector3 WorldCenterOfElement(this Transform transform) { return transform.TransformPoint(transform.GetComponent<Element>().Center()); }

	public static class Array
	{
		public static Vector3[] Add(Vector3[] terms, Vector3 addend)
		{
			var result = new Vector3[terms.Length];
			for (var i = 0; i < result.Length; i++)
				result[i] = terms[i] + addend;
			return result;
		}

		public static float[] Divide(float dividend, float[] terms)
		{
			var result = new float[terms.Length];
			for (var i = 0; i < result.Length; i++)
				result[i] = dividend / terms[i];
			return result;
		}

		public static float[] Dot(Vector3[] terms, Vector3 multiplier)
		{
			var result = new float[terms.Length];
			for (var i = 0; i < result.Length; i++)
				result[i] = Vector3.Dot(terms[i], multiplier);
			return result;
		}

		public static bool Equals(Color[] lhs, Color[] rhs)
		{
			var len = lhs.Length;
			if (len != rhs.Length)
				return false;
			for (var i = 0; i < len; i++)
				if (((Vector4)(lhs[i] - rhs[i])).magnitude > Settings.Tolerance)
					return false;
			return true;
		}

		public static float[] Multiply(float[] terms, float multiplier)
		{
			var result = new float[terms.Length];
			for (var i = 0; i < result.Length; i++)
				result[i] = terms[i] * multiplier;
			return result;
		}

		public static Vector3[] Multiply(Vector3[] terms, float[] multipliers)
		{
			var result = new Vector3[terms.Length];
			for (var i = 0; i < result.Length; i++)
				result[i] = terms[i] * multipliers[i];
			return result;
		}
	}

	public static class Coordinates
	{
		private static Vector3 ExternalToInternal(float externalX, float externalY, float externalZ = 0) { return new Vector3((Data.MapSize.y - 1 + Settings.Map.MapSizeOffset.right - externalY) * Settings.DimensionScaleFactor, Settings.Map.HeightOfLevel[Mathf.RoundToInt(externalZ)], ((externalX + Settings.Map.MapSizeOffset.top) * Settings.DimensionScaleFactor)); }

		public static Vector3 ExternalToInternal(Vector3 externalCoordinates) { return ExternalToInternal(externalCoordinates.x, externalCoordinates.y, externalCoordinates.z); }

		public static Vector3 ExternalToInternal(Vector2 externalCoordinates, float externalZ) { return ExternalToInternal(externalCoordinates.x, externalCoordinates.y, externalZ); }

		public static Vector2 ExternalToMiniMapBasedScreen(Vector2 externalCoordinates) { return new Vector2(Screen.width - Settings.MiniMap.Border.right - (Data.MapSize.y - externalCoordinates.y - 0.5f) * Data.MiniMap.ScaleFactor, Screen.height - Settings.MiniMap.Border.top - (externalCoordinates.x + 0.5f) * Data.MiniMap.ScaleFactor); }

		private static Vector2 ExternalToMiniMapRatios(Vector2 externalCoordinates) { return new Vector2((externalCoordinates.y + 0.5f) / Data.MapSize.y, 1 - (externalCoordinates.x + 0.5f) / Data.MapSize.x); }

		public static Vector2 InternalToExternal(Vector3 internalCoordinates) { return new Vector3(internalCoordinates.z / Settings.DimensionScaleFactor - Settings.Map.MapSizeOffset.top, Data.MapSize.y - 1 + Settings.Map.MapSizeOffset.right - internalCoordinates.x / Settings.DimensionScaleFactor); }

		public static Vector2 InternalToMiniMapRatios(Vector3 internalCoordinates) { return ExternalToMiniMapRatios(InternalToExternal(internalCoordinates)); }

		private static Vector3 IntersectToCameraPivotHeight(Vector3 lhs, Vector3 rhs)
		{
			var t = (Camera.main.transform.root.position.y - lhs.y) / (rhs.y - lhs.y);
			return (1 - t) * lhs + t * rhs;
		}

		public static Vector3[] IntersectToCameraPivotHeight(Vector3 origin, Vector3[] farCorners)
		{
			var flags = new bool[2];
			for (var i = 0; i < 2; i++)
				flags[i] = (origin.y - Camera.main.transform.root.position.y) * (farCorners[i].y - Camera.main.transform.root.position.y) < 0;
			if (!flags[0] && !flags[1])
				return null;
			return new[] { IntersectToCameraPivotHeight(flags[0] ? origin : farCorners[1], farCorners[0]), IntersectToCameraPivotHeight(flags[1] ? origin : farCorners[0], farCorners[1]), IntersectToCameraPivotHeight(flags[1] ? origin : farCorners[3], farCorners[2]), IntersectToCameraPivotHeight(flags[0] ? origin : farCorners[2], farCorners[3]) };
		}

		public static bool IsOccupied(Vector2 externalCoordinates) { return (Data.IsOccupied[Mathf.RoundToInt(externalCoordinates.x), Mathf.RoundToInt(externalCoordinates.y)]); }

		private static Vector3 JSONToExternal(JSONObject jsonPos)
		{
			float posX, posY, posZ;
			JSONToExternal(jsonPos, out posX, out posY, out posZ);
			return new Vector3(posX, posY, posZ);
		}

		public static Vector3 JSONToExternal(JSONObject jsonPos, out float posX, out float posY)
		{
			float posZ;
			JSONToExternal(jsonPos, out posX, out posY, out posZ);
			return new Vector3(posX, posY, posZ);
		}

		private static void JSONToExternal(JSONObject jsonPos, out float posX, out float posY, out float posZ)
		{
			if (jsonPos["__class__"].str == "Rectangle")
			{
				posX = (jsonPos["upper_left"]["x"].n + jsonPos["lower_right"]["x"].n) / 2;
				posY = (jsonPos["upper_left"]["y"].n + jsonPos["lower_right"]["y"].n) / 2;
				posZ = (jsonPos["upper_left"]["z"].n + jsonPos["lower_right"]["z"].n) / 2;
			}
			else
			{
				posX = jsonPos["x"].n;
				posY = jsonPos["y"].n;
				posZ = jsonPos["z"].n;
			}
			if (Mathf.Abs(posZ - 2) < Mathf.Epsilon)
				posZ = 3;
			else if (Mathf.Abs(posZ - 1) < Mathf.Epsilon)
				posZ += Data.Battle["gamebody"]["map_info"]["types"][Mathf.RoundToInt(posX)][Mathf.RoundToInt(posY)].n;
		}

		public static Vector3 JSONToInternal(JSONObject jsonPos) { return ExternalToInternal(JSONToExternal(jsonPos)); }

		private static Vector2 MiniMapBasedScreenToExternal(Vector2 screenPosition) { return new Vector2((Screen.height - Settings.MiniMap.Border.top - screenPosition.y) / Data.MiniMap.ScaleFactor - 0.5f, Data.MapSize.y - (Screen.width - Settings.MiniMap.Border.right - screenPosition.x) / Data.MiniMap.ScaleFactor - 0.5f); }

		public static Vector3 MiniMapBasedScreenToInternal(Vector2 screenPosition) { return ExternalToInternal(MiniMapBasedScreenToExternal(screenPosition)); }
	}

	public static class Game
	{
		public static void Pause()
		{
			Data.GamePaused = true;
			Time.timeScale = 0;
		}

		public static void Quit()
		{
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		public static void Resume()
		{
			Data.GamePaused = false;
			Time.timeScale = 1;
		}
	}

	public static class GUI
	{
		public static void Confirm(string message, ref bool value, ref MenuState stagedState)
		{
			GUILayout.FlexibleSpace();
			GUILayout.Label("确认" + message + "?", Data.GUI.Label.LargeMiddle);
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("是", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
				value = true;
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("否", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
				stagedState = MenuState.Default;
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
		}

		public static void DrawAbout(ref MenuState stagedState)
		{
			GUILayout.BeginVertical("box");
			Data.GUI.AboutScroll = GUILayout.BeginScrollView(Data.GUI.AboutScroll);
			GUILayout.Label("第十六届电子系队式程序设计大赛3D回放引擎", Data.GUI.Label.SmallMiddle);
			GUILayout.FlexibleSpace();
			GUILayout.Label("电子系科协软件部队式开发3D组出品", Data.GUI.Label.SmallMiddle);
			GUILayout.FlexibleSpace();
			GUILayout.Label("开发者：", Data.GUI.Label.LargeLeft);
			GUILayout.Label("林圣杰", Data.GUI.Label.SmallMiddle);
			GUILayout.Label("钟元熠", Data.GUI.Label.SmallMiddle);
			GUILayout.FlexibleSpace();
			GUILayout.Label("鸣谢：", Data.GUI.Label.LargeLeft);
			GUILayout.Label("队式开发组全体成员", Data.GUI.Label.SmallMiddle);
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("确定", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
				stagedState = MenuState.Default;
		}

		public static void DrawOptions(ref MenuState stagedState)
		{
			Data.GUI.OptionSelected = GUILayout.Toolbar(Data.GUI.OptionSelected, new[] { "队伍颜色", "图例", "字体" }, Data.GUI.Button.Large);
			GUILayout.BeginVertical("box");
			GUILayout.FlexibleSpace();
			switch (Data.GUI.OptionSelected)
			{
				case 0:
					Data.GUI.TeamColorScroll = GUILayout.BeginScrollView(Data.GUI.TeamColorScroll);
					for (var i = 0; i < 3; i++)
					{
						GUILayout.BeginHorizontal("box");
						if (GUILayout.Button(Data.GUI.Random, Data.GUI.Button.Large, GUILayout.ExpandHeight(true)))
							Data.TeamColor.Target[i] = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
						GUILayout.BeginVertical();
						GUILayout.BeginHorizontal();
						GUILayout.Label(Data.Replay.TeamNames[i], Data.GUI.Label.TeamColored[i]);
						GUILayout.Box("", Data.GUI.TeamColoredBoxes[i]);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.Label("红", Data.GUI.Label.RGB[0]);
						Data.TeamColor.Target[i].r = GUILayout.HorizontalSlider(Data.TeamColor.Target[i].r, 0, 1);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.Label("绿", Data.GUI.Label.RGB[1]);
						Data.TeamColor.Target[i].g = GUILayout.HorizontalSlider(Data.TeamColor.Target[i].g, 0, 1);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.Label("蓝", Data.GUI.Label.RGB[2]);
						Data.TeamColor.Target[i].b = GUILayout.HorizontalSlider(Data.TeamColor.Target[i].b, 0, 1);
						GUILayout.EndHorizontal();
						GUILayout.EndVertical();
						GUILayout.EndHorizontal();
						if (i < 2)
							GUILayout.FlexibleSpace();
					}
					GUILayout.EndScrollView();
					break;
				case 1:
					Data.GUI.LegendScroll = GUILayout.BeginScrollView(Data.GUI.LegendScroll);
					GUILayout.BeginVertical("box");
					GUILayout.Label("尺寸", Data.GUI.Label.LargeLeft);
					GUILayout.FlexibleSpace();
					GUILayout.BeginHorizontal();
					GUILayout.Label(Data.MiniMap.MarkScaleFactor.ToString("F"), Data.GUI.Label.SmallMiddle);
					Data.MiniMap.MarkScaleFactor = GUILayout.HorizontalSlider(Data.MiniMap.MarkScaleFactor, 1, 10);
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					GUILayout.FlexibleSpace();
					GUILayout.BeginVertical("box");
					GUILayout.Label("图案", Data.GUI.Label.LargeLeft);
					GUILayout.FlexibleSpace();
					Data.MiniMap.MarkPatternIndex = GUILayout.Toolbar(Data.MiniMap.MarkPatternIndex, new[] { "默认", "方块" }, Data.GUI.Button.Medium);
					GUILayout.EndVertical();
					GUILayout.EndScrollView();
					break;
				case 2:
					Data.GUI.FontScroll = GUILayout.BeginScrollView(Data.GUI.FontScroll);
					GUILayout.BeginVertical("box");
					GUILayout.Label("尺寸", Data.GUI.Label.LargeLeft);
					GUILayout.FlexibleSpace();
					GUILayout.BeginHorizontal();
					GUILayout.Label(Data.GUI.FontSizeScaleFactor.ToString("F"), Data.GUI.Label.SmallMiddle);
					Data.GUI.FontSizeScaleFactor = GUILayout.HorizontalSlider(Data.GUI.FontSizeScaleFactor, 0.75f, 1.5f);
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					GUILayout.EndScrollView();
					break;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.BeginHorizontal("box");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("确定", Data.GUI.Button.Small) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
				stagedState = MenuState.Default;
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("取消", Data.GUI.Button.Small) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
			{
				stagedState = MenuState.Default;
				for (var i = 0; i < 4; i++)
					Data.TeamColor.Target[i] = Data.GUI.StagedTeamColor[i];
				Data.MiniMap.MarkScaleFactor = Data.MiniMap.StagedMarkScaleFactor;
				Data.MiniMap.MarkPatternIndex = Data.MiniMap.StagedMarkPatternIndex;
				Data.GUI.FontSizeScaleFactor = Data.GUI.StagedFontSizeScaleFactor;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		public static void InitializeStyles()
		{
			if (Data.GUI.Initialized)
				return;
			for (var i = 0; i < 3; i++)
			{
				Data.GUI.TeamColoredTextures[i] = new Texture2D(1, 1);
				Data.GUI.TeamColoredBoxes[i] = new GUIStyle("box") { normal = { background = Data.GUI.TeamColoredTextures[i] } };
				Data.GUI.Label.TeamColored[i] = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
			}
			Data.GUI.Button.Large = new GUIStyle("button");
			Data.GUI.Button.Medium = new GUIStyle("button");
			Data.GUI.Button.Small = new GUIStyle("button");
			Data.GUI.Label.HugeMiddle = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
			Data.GUI.Label.LargeLeft = new GUIStyle("label");
			Data.GUI.Label.LargeMiddle = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
			Data.GUI.Label.SmallLeft = new GUIStyle("label");
			Data.GUI.Label.SmallMiddle = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
			Data.GUI.Label.RGB[0] = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.red } };
			Data.GUI.Label.RGB[1] = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.green } };
			Data.GUI.Label.RGB[2] = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.blue } };
			Data.GUI.TextField = new GUIStyle("textField");
			Data.GUI.Initialized = true;
			ResizeFonts();
		}

		public static bool MouseOver() { return Data.GUI.OccupiedRects.Any(rect => rect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))); }

		public static void OnScreenSizeChanged()
		{
			var screenArea = Screen.width * Screen.height;
			Data.MiniMap.ScaleFactor = Mathf.Sqrt(screenArea / Data.MapSize.x / Data.MapSize.y) / 4;
			var bl = Coordinates.ExternalToMiniMapBasedScreen(Vector2.right * Data.MapSize.x - Vector2.one * 0.5f);
			var tr = Coordinates.ExternalToMiniMapBasedScreen(Vector2.up * Data.MapSize.y - Vector2.one * 0.5f);
			Data.MiniMap.MapRect = new Rect(bl.x, bl.y, (tr - bl).x, (tr - bl).y);
			Data.GUI.ProductionEntrySize = Mathf.Sqrt(screenArea) / 10;
			Data.GUI.LineThickness = Settings.GUI.LineThickness * Mathf.Sqrt(screenArea) / 1000;
			Data.GUI.Dice = Object.Instantiate(Resources.Load("Dice")) as Texture2D;
			TextureScale.Bilinear(Data.GUI.Dice, Screen.height / 8, Screen.height / 8);
			Data.GUI.Random.image = Data.GUI.Dice;
			ResizeFonts();
		}

		public static void RefreshTeamColoredStyles()
		{
			for (var i = 0; i < 3; i++)
			{
				Data.GUI.TeamColoredTextures[i].SetPixel(0, 0, Data.GUI.Label.TeamColored[i].normal.textColor = Data.TeamColor.Target[i]);
				Data.GUI.TeamColoredTextures[i].Apply();
			}
		}

		public static void ResizeFonts()
		{
			if (!Data.GUI.Initialized)
				return;
			var baseFontSize = Mathf.RoundToInt(Screen.height / (Screen.dpi > 0 ? Screen.dpi : 120) * Data.GUI.FontSizeScaleFactor);
			for (var i = 0; i < 3; i++)
				Data.GUI.Label.TeamColored[i].fontSize = baseFontSize * 4;
			Data.GUI.Button.Large.fontSize = baseFontSize * 5;
			Data.GUI.Button.Medium.fontSize = baseFontSize * 4;
			Data.GUI.Button.Small.fontSize = baseFontSize * 3;
			Data.GUI.Label.HugeMiddle.fontSize = baseFontSize * 8;
			Data.GUI.Label.LargeMiddle.fontSize = Data.GUI.Label.LargeLeft.fontSize = baseFontSize * 5;
			Data.GUI.Label.SmallMiddle.fontSize = Data.GUI.Label.SmallLeft.fontSize = baseFontSize * 4;
			for (var i = 0; i < 3; i++)
				Data.GUI.Label.RGB[i].fontSize = baseFontSize * 3;
			Data.GUI.TextField.fontSize = baseFontSize * 3;
		}

		public static void StageCurrentOptions()
		{
			for (var i = 0; i < 4; i++)
				Data.GUI.StagedTeamColor[i] = Data.TeamColor.Target[i];
			Data.MiniMap.StagedMarkScaleFactor = Data.MiniMap.MarkScaleFactor;
			Data.MiniMap.StagedMarkPatternIndex = Data.MiniMap.MarkPatternIndex;
			Data.GUI.StagedFontSizeScaleFactor = Data.GUI.FontSizeScaleFactor;
		}
	}

	public static class Replay
	{
		public static void ClearData()
		{
			Data.Replay.ProductionTimeScale = 1;
			Data.Replay.AttacksLeft = 0;
			Data.Replay.CollectsLeft = 0;
			Data.Replay.CreatesLeft = 0;
			Data.Replay.FixesLeft = 0;
			Data.Replay.MovesLeft = 0;
			Data.Replay.SuppliesLeft = 0;
		}

		public static void InitializeData()
		{
			Data.Replay.Bases = new Base[2];
			Data.Replay.CurrentScores = new float[2];
			Data.Replay.Elements = new Dictionary<int, Element>();
			Data.Replay.Forts = new[] { new List<Fort>(), new List<Fort>() };
			Data.Replay.Populations = new int[2];
			Data.Replay.ProductionLists = new[] { new List<ProductionEntry>(), new List<ProductionEntry>() };
			Data.Replay.TargetScores = new int[2];
			Data.Replay.UnitNums = new int[2];
			ClearData();
		}
	}
}