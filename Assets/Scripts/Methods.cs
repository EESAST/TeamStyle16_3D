#region

using UnityEngine;

#endregion

namespace GameStatics
{
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

		public static void ChangeLayer(this GameObject entity, int layer)
		{
			entity.layer = layer;
			foreach (Transform child in entity.transform)
				child.gameObject.ChangeLayer(layer);
		}

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

		public static Vector3[] TransformPoints(this Transform transform, Vector3[] points, out Vector3 center)
		{
			var results = new Vector3[points.Length];
			center = Vector3.zero;
			for (var i = 0; i < results.Length; i++)
				center += results[i] = transform.TransformPoint(points[i]);
			center /= results.Length;
			return results;
		}

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
			public static Vector3 ExternalToInternal(float externalX, float externalY, float externalZ = 0) { return new Vector3((Data.MapSize.y - 1 + Settings.MapSizeOffset.w - externalY) * Settings.ScaleFactor, Settings.HeightOfLayer[Mathf.RoundToInt(externalZ)], ((externalX + Settings.MapSizeOffset.x) * Settings.ScaleFactor)); }

			public static Vector3 ExternalToInternal(Vector3 externalCoordinates) { return ExternalToInternal(externalCoordinates.x, externalCoordinates.y, externalCoordinates.z); }

			public static Vector3 ExternalToInternal(Vector2 externalCoordinates, float externalZ) { return ExternalToInternal(externalCoordinates.x, externalCoordinates.y, externalZ); }

			public static Vector2 ExternalToMiniMapBasedScreen(Vector2 externalCoordinates) { return new Vector2(Screen.width - Settings.MiniMap.BorderOffset - (Data.MapSize.y - externalCoordinates.y - 0.5f) * Data.MiniMap.ScaleFactor, Screen.height - Settings.MiniMap.BorderOffset - (externalCoordinates.x + 0.5f) * Data.MiniMap.ScaleFactor); }

			public static Vector2 ExternalToMiniMapRatios(Vector2 externalCoordinates) { return new Vector2(externalCoordinates.y / (Data.MapSize.y - 1), 1 - externalCoordinates.x / (Data.MapSize.x - 1)); }

			public static Vector3 InternalToExternal(Vector3 internalCoordinates)
			{
				var layer = 0;
				while (layer < Settings.HeightOfLayer.Length - 1 && Mathf.Abs(internalCoordinates.y - Settings.HeightOfLayer[layer]) > Mathf.Epsilon)
					layer++;
				return new Vector3(internalCoordinates.z / Settings.ScaleFactor - Settings.MapSizeOffset.x, Data.MapSize.y - 1 + Settings.MapSizeOffset.w - internalCoordinates.x / Settings.ScaleFactor, layer);
			}

			public static Vector2 InternalToMiniMapBasedScreen(Vector3 internalCoordinates) { return ExternalToMiniMapBasedScreen(InternalToExternal(internalCoordinates)); }

			public static Vector2 InternalToMiniMapRatios(Vector3 internalCoordinates) { return ExternalToMiniMapRatios(InternalToExternal(internalCoordinates)); }

			public static Vector3 IntersectToGround(Vector3 lhs, Vector3 rhs)
			{
				var t = (Settings.GroundHeight - lhs.y) / (rhs.y - lhs.y);
				return (1 - t) * lhs + t * rhs;
			}

			public static Vector3[] IntersectToGround(Vector3 origin, Vector3[] farCorners)
			{
				var flags = new bool[2];
				for (var i = 0; i < 2; i++)
					flags[i] = (origin.y - Settings.GroundHeight) * (farCorners[i].y - Settings.GroundHeight) < 0;
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

			public static void Resume()
			{
				Data.GamePaused = false;
				Time.timeScale = 1;
			}
		}
	}
}