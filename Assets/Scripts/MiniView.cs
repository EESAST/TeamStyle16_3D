#region

using GameStatics;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class MiniView : MonoBehaviour
{
	private Color32[] clearPixels;
	private Texture2D miniViewTexture;
	private RectTransform viewRect;

	private void Awake()
	{
		Delegates.ScreenSizeChanged += RefreshViewRect;
		(viewRect = GetComponent<RectTransform>()).anchoredPosition = -Vector2.one * Settings.MiniMap.BorderOffset;
	}

	private void OnDestroy() { Delegates.ScreenSizeChanged -= RefreshViewRect; }

	private void RefreshViewRect()
	{
		viewRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Data.MapSize.y * Data.MiniMap.ScaleFactor);
		viewRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Data.MapSize.x * Data.MiniMap.ScaleFactor);
	}

	private void Start()
	{
		var granularity = Settings.MiniMap.ViewLine.Granularity;
		GetComponent<RawImage>().texture = miniViewTexture = new Texture2D(Mathf.RoundToInt(Data.MapSize.y) * granularity, Mathf.RoundToInt(Data.MapSize.x) * granularity)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		clearPixels = new Color32[miniViewTexture.width * miniViewTexture.height];
	}

	private void Update()
	{
		var origin = Camera.main.transform.position;
		var directions = new[] { Camera.main.ScreenPointToRay(new Vector2(0, 0)).direction, Camera.main.ScreenPointToRay(new Vector2(0, Screen.height)).direction, Camera.main.ScreenPointToRay(new Vector2(Screen.width, Screen.height)).direction, Camera.main.ScreenPointToRay(new Vector2(Screen.width, 0)).direction };
		var groundPoints = Methods.Coordinates.IntersectToGround(origin, Methods.Array.Add(Methods.Array.Multiply(directions, Methods.Array.Divide(Camera.main.farClipPlane, Methods.Array.Dot(directions, Camera.main.transform.forward))), origin));
		miniViewTexture.SetPixels32(clearPixels);
		if (groundPoints != null)
		{
			var miniMapBasedPoints = new Vector2[4];
			var scaleFactor = new Vector2(miniViewTexture.width, miniViewTexture.height);
			for (var i = 0; i < 4; i++)
				miniMapBasedPoints[i] = Vector2.Scale(Methods.Coordinates.InternalToMiniMapRatios(groundPoints[i]), scaleFactor);
			miniViewTexture.Polygon(miniMapBasedPoints, Settings.MiniMap.ViewLine.Color, Settings.MiniMap.ViewLine.Thickness);
		}
		miniViewTexture.Apply();
	}
}