#region

using UnityEditor;
using UnityEngine;

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

	private static void Line(this Texture2D texture, Vector2 lhs, Vector2 rhs, Color lineColor, float lineThickness)
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
				for (var yp = -dy / 2; yp <= dy / 2; yp++)
				{
					var posX = Mathf.RoundToInt(x);
					var posY = Mathf.RoundToInt(y + yp);
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
				for (var xp = -dx / 2; xp <= dx / 2; xp++)
				{
					var posX = Mathf.RoundToInt(x + xp);
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

	public static void RefreshMiniMap()
	{
		Data.MiniMap.ScaleFactor = (Screen.width + Screen.height) / (Vector2.Dot(Data.MapSize, Vector2.one * 4));
		var bl = Coordinates.ExternalToMiniMapBasedScreen(Vector2.right * Data.MapSize.x);
		var tr = Coordinates.ExternalToMiniMapBasedScreen(Vector2.up * Data.MapSize.y);
		Data.MiniMap.Rect = new Rect(bl.x, bl.y, (tr - bl).x, (tr - bl).y);
	}

	public static Vector3[] TransformPoints(this Transform transform, Vector3[] points, out Vector3 center)
	{
		var results = new Vector3[points.Length];
		center = Vector3.zero;
		for (var i = 0; i < results.Length; i++)
			center += results[i] = transform.TransformPoint(points[i]);
		center /= results.Length;
		return results;
	}

	public static Vector3 WorldCenterOfEntity(this Transform transform) { return transform.TransformPoint(transform.GetComponent<Entity>().Center()); }

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
				if (((Vector4)(lhs[i] - rhs[i])).magnitude > Settings.TeamColor.Tolerance)
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

		public static Vector3[] Multiply(Vector3[] terms, float multiplier)
		{
			var result = new Vector3[terms.Length];
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
		public static Vector3 ExternalToInternal(float externalX, float externalY, float externalZ = 0) { return new Vector3((Data.MapSize.y - 1 + Settings.MapSizeOffset.w - externalY) * Settings.ScaleFactor, Settings.HeightOfLevel[Mathf.RoundToInt(externalZ)], ((externalX + Settings.MapSizeOffset.x) * Settings.ScaleFactor)); }

		public static Vector3 ExternalToInternal(Vector3 externalCoordinates) { return ExternalToInternal(externalCoordinates.x, externalCoordinates.y, externalCoordinates.z); }

		public static Vector3 ExternalToInternal(Vector2 externalCoordinates, float externalZ) { return ExternalToInternal(externalCoordinates.x, externalCoordinates.y, externalZ); }

		public static Vector2 ExternalToMiniMapBasedScreen(Vector2 externalCoordinates) { return new Vector2(Screen.width - Settings.MiniMap.BorderOffset - (Data.MapSize.y - externalCoordinates.y - 0.5f) * Data.MiniMap.ScaleFactor, Screen.height - Settings.MiniMap.BorderOffset - (externalCoordinates.x + 0.5f) * Data.MiniMap.ScaleFactor); }

		public static Vector2 ExternalToMiniMapRatios(Vector2 externalCoordinates) { return new Vector2(externalCoordinates.y / (Data.MapSize.y - 1), 1 - externalCoordinates.x / (Data.MapSize.x - 1)); }

		public static Vector2 InternalToExternal(Vector3 internalCoordinates) { return new Vector3(internalCoordinates.z / Settings.ScaleFactor - Settings.MapSizeOffset.x, Data.MapSize.y - 1 + Settings.MapSizeOffset.w - internalCoordinates.x / Settings.ScaleFactor); }

		public static Vector2 InternalToMiniMapBasedScreen(Vector3 internalCoordinates) { return ExternalToMiniMapBasedScreen(InternalToExternal(internalCoordinates)); }

		public static Vector2 InternalToMiniMapRatios(Vector3 internalCoordinates) { return ExternalToMiniMapRatios(InternalToExternal(internalCoordinates)); }

		public static Vector3 IntersectToGround(Vector3 lhs, Vector3 rhs)
		{
			var t = (Settings.HeightOfLevel[2] - lhs.y) / (rhs.y - lhs.y);
			return (1 - t) * lhs + t * rhs;
		}

		public static Vector3[] IntersectToGround(Vector3 origin, Vector3[] farCorners)
		{
			var flags = new bool[2];
			for (var i = 0; i < 2; i++)
				flags[i] = (origin.y - Settings.HeightOfLevel[2]) * (farCorners[i].y - Settings.HeightOfLevel[2]) < 0;
			if (!flags[0] && !flags[1])
				return null;
			return new[] { IntersectToGround(flags[0] ? origin : farCorners[1], farCorners[0]), IntersectToGround(flags[1] ? origin : farCorners[0], farCorners[1]), IntersectToGround(flags[1] ? origin : farCorners[3], farCorners[2]), IntersectToGround(flags[0] ? origin : farCorners[2], farCorners[3]) };
		}

		public static bool IsOccupied(Vector2 externalCoordinates) { return (Data.IsOccupied[Mathf.RoundToInt(externalCoordinates.x), Mathf.RoundToInt(externalCoordinates.y)]); }

		public static Vector2 MiniMapBasedScreenToExternal(Vector2 screenPosition) { return new Vector2((Screen.height - Settings.MiniMap.BorderOffset - screenPosition.y) / Data.MiniMap.ScaleFactor - 0.5f, Data.MapSize.y - (Screen.width - Settings.MiniMap.BorderOffset - screenPosition.x) / Data.MiniMap.ScaleFactor - 0.5f); }

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
			GUILayout.Label("第十六届电子系队式程序设计大赛专用3D回放引擎", Data.GUI.Label.Small);
			GUILayout.Label("电子系科协软件部队式3D组出品", Data.GUI.Label.Small);
			GUILayout.FlexibleSpace();
			GUILayout.Label("开发者：", Data.GUI.Label.Large);
			GUILayout.Label("林圣杰", Data.GUI.Label.Small);
			GUILayout.Label("钟元熠", Data.GUI.Label.Small);
			GUILayout.Label("鸣谢：", Data.GUI.Label.Large);
			GUILayout.Label("翁喆", Data.GUI.Label.Small);
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("确定", Data.GUI.Button.Medium) || Event.current.type == EventType.KeyUp && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
				stagedState = MenuState.Default;
		}

		public static void DrawOptions(ref MenuState stagedState)
		{
			Data.GUI.OptionSelected = GUILayout.Toolbar(Data.GUI.OptionSelected, new[] { "队伍颜色", "图例" }, Data.GUI.Button.Large);
			GUILayout.BeginVertical("box");
			GUILayout.FlexibleSpace();
			switch (Data.GUI.OptionSelected)
			{
				case 0:
					Data.GUI.OptionScroll = GUILayout.BeginScrollView(Data.GUI.OptionScroll);
					GUILayout.BeginVertical();
					for (var i = 0; i < 3; i++)
					{
						GUILayout.BeginHorizontal("box");
						if (GUILayout.Button(Data.GUI.Random, Data.GUI.Button.Large, GUILayout.ExpandHeight(true)))
							Data.TeamColor.Desired[i] = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
						GUILayout.BeginVertical();
						GUILayout.BeginHorizontal();
						GUILayout.Label(Data.GUI.TeamDescriptions[i], Data.GUI.Label.TeamColored[i]);
						GUILayout.Box("", Data.GUI.TeamColoredBoxes[i]);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.Label("红", Data.GUI.Label.RGB[0]);
						Data.TeamColor.Desired[i].r = GUILayout.HorizontalSlider(Data.TeamColor.Desired[i].r, 0, 1);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.Label("绿", Data.GUI.Label.RGB[1]);
						Data.TeamColor.Desired[i].g = GUILayout.HorizontalSlider(Data.TeamColor.Desired[i].g, 0, 1);
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
						GUILayout.Label("蓝", Data.GUI.Label.RGB[2]);
						Data.TeamColor.Desired[i].b = GUILayout.HorizontalSlider(Data.TeamColor.Desired[i].b, 0, 1);
						GUILayout.EndHorizontal();
						GUILayout.EndVertical();
						GUILayout.EndHorizontal();
						if (i < 2)
							GUILayout.FlexibleSpace();
					}
					GUILayout.EndVertical();
					GUILayout.EndScrollView();
					break;
				case 1:
					GUILayout.BeginVertical("box");
					GUILayout.Label("尺寸", Data.GUI.Label.Large);
					GUILayout.FlexibleSpace();
					GUILayout.BeginHorizontal();
					GUILayout.Label(Data.MarkScaleFactor.ToString("F"), Data.GUI.Label.Small);
					Data.MarkScaleFactor = GUILayout.HorizontalSlider(Data.MarkScaleFactor, 1, 10);
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					GUILayout.FlexibleSpace();
					GUILayout.BeginVertical("box");
					GUILayout.Label("图案", Data.GUI.Label.Large);
					GUILayout.FlexibleSpace();
					Data.MarkPatternIndex = GUILayout.Toolbar(Data.MarkPatternIndex, new[] { "默认", "方块" }, Data.GUI.Button.Medium);
					GUILayout.EndVertical();
					break;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.BeginHorizontal("box");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("确定", Data.GUI.Button.Small) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
				stagedState = MenuState.Default;
			;
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("取消", Data.GUI.Button.Small) || Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
			{
				stagedState = MenuState.Default;
				for (var i = 0; i < 4; i++)
					Data.TeamColor.Desired[i] = Data.GUI.StagedTeamColor[i];
				Data.MarkScaleFactor = Data.GUI.StagedMarkScaleFactor;
				Data.MarkPatternIndex = Data.GUI.StagedMarkPatternIndex;
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
			Data.GUI.Label.Large = new GUIStyle("label");
			Data.GUI.Label.LargeMiddle = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
			Data.GUI.Label.Small = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter };
			Data.GUI.Label.RGB[0] = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.red } };
			Data.GUI.Label.RGB[1] = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.green } };
			Data.GUI.Label.RGB[2] = new GUIStyle("label") { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.blue } };
			Data.GUI.Initialized = true;
		}

		public static void RefreshTeamColoredStyles()
		{
			for (var i = 0; i < 3; i++)
			{
				Data.GUI.TeamColoredTextures[i].SetPixel(0, 0, Data.GUI.Label.TeamColored[i].normal.textColor = Data.TeamColor.Desired[i]);
				Data.GUI.TeamColoredTextures[i].Apply();
			}
		}

		public static void ResizeFonts()
		{
			var physicalHeight = Screen.height / Screen.dpi;
			for (var i = 0; i < 3; i++)
				Data.GUI.Label.TeamColored[i].fontSize = Mathf.RoundToInt(physicalHeight * 4);
			Data.GUI.Button.Large.fontSize = Mathf.RoundToInt(physicalHeight * 5);
			Data.GUI.Button.Medium.fontSize = Mathf.RoundToInt(physicalHeight * 4);
			Data.GUI.Button.Small.fontSize = Mathf.RoundToInt(physicalHeight * 3);
			Data.GUI.Label.LargeMiddle.fontSize = Data.GUI.Label.Large.fontSize = Mathf.RoundToInt(physicalHeight * 5);
			Data.GUI.Label.Small.fontSize = Mathf.RoundToInt(physicalHeight * 4);
			for (var i = 0; i < 3; i++)
				Data.GUI.Label.RGB[i].fontSize = Mathf.RoundToInt(physicalHeight * 3);
		}

		public static void StageCurrentOptions()
		{
			for (var i = 0; i < 4; i++)
				Data.GUI.StagedTeamColor[i] = Data.TeamColor.Desired[i];
			Data.GUI.StagedMarkScaleFactor = Data.MarkScaleFactor;
			Data.GUI.StagedMarkPatternIndex = Data.MarkPatternIndex;
		}
	}
}