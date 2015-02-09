#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class MiniView : MonoBehaviour
{
	private Color32[] clearPixels;
	private Texture2D miniViewTexture;
	private float lineThickness;

	private void Awake() { Delegates.ScreenSizeChanged += RefreshView; }

	private void OnDestroy() { Delegates.ScreenSizeChanged -= RefreshView; }

	private void RefreshView()
	{
		GetComponent<RawImage>().texture = miniViewTexture = new Texture2D(Mathf.RoundToInt(Data.MapSize.y * Data.MiniMap.ScaleFactor / 2) * 2, Mathf.RoundToInt(Data.MapSize.x * Data.MiniMap.ScaleFactor / 2) * 2) { wrapMode = TextureWrapMode.Clamp };
		clearPixels = new Color32[miniViewTexture.width * miniViewTexture.height];
		lineThickness = Settings.MiniMap.ViewLine.Thickness * Mathf.Sqrt(Screen.width * Screen.height) / 1000;
	}

	private void Start() { RefreshView(); }

	private void Update()
	{
		var origin = Camera.main.transform.position;
		var directions = new[] { Camera.main.ScreenPointToRay(new Vector2(0, 0)).direction, Camera.main.ScreenPointToRay(new Vector2(0, Screen.height)).direction, Camera.main.ScreenPointToRay(new Vector2(Screen.width, Screen.height)).direction, Camera.main.ScreenPointToRay(new Vector2(Screen.width, 0)).direction };
		var worldPoints = Methods.Coordinates.IntersectToDefaultHeight(origin, Methods.Array.Add(Methods.Array.Multiply(directions, Methods.Array.Divide(Camera.main.farClipPlane, Methods.Array.Dot(directions, Camera.main.transform.forward))), origin));
		miniViewTexture.SetPixels32(clearPixels);
		if (worldPoints != null)
		{
			var miniMapBasedPoints = new Vector2[4];
			var scaleFactor = new Vector2(miniViewTexture.width, miniViewTexture.height);
			for (var i = 0; i < 4; i++)
				miniMapBasedPoints[i] = Vector2.Scale(Methods.Coordinates.InternalToMiniMapRatios(worldPoints[i]), scaleFactor);
			miniViewTexture.Polygon(miniMapBasedPoints, Settings.MiniMap.ViewLine.Color, lineThickness);
		}
		miniViewTexture.Apply();
	}
}