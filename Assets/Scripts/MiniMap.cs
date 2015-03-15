#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class MiniMap : MonoBehaviour
{
	private Texture2D miniMapTexture;

	private void Awake()
	{
		Delegates.ScreenSizeChanged += RefreshMapRect;
		GetComponent<RectTransform>().anchoredPosition = -new Vector2(Settings.MiniMap.Border.right, Settings.MiniMap.Border.top);
	}

	private void OnDestroy() { Delegates.ScreenSizeChanged -= RefreshMapRect; }

	private void RefreshMapRect()
	{
		GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Data.MapSize.y * Data.MiniMap.ScaleFactor);
		GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Data.MapSize.x * Data.MiniMap.ScaleFactor);
	}

	private void Start()
	{
		var mapData = Data.Battle["gamebody"]["map_info"]["types"];
		var width = Mathf.RoundToInt(Data.MapSize.y) * Settings.MiniMap.Granularity;
		var height = Mathf.RoundToInt(Data.MapSize.x) * Settings.MiniMap.Granularity;
		GetComponent<RawImage>().texture = miniMapTexture = new Texture2D(width, height) { wrapMode = TextureWrapMode.Clamp };
		var pixels = miniMapTexture.GetPixels32();
		for (var i = 0; i < width; i++)
			for (var j = 0; j < height; j++)
				pixels[i + width * j] = mapData[(height - 1 - j) / Settings.MiniMap.Granularity][i / Settings.MiniMap.Granularity].i == 0 ? Settings.MiniMap.OceanColor : Settings.MiniMap.LandColor;
		miniMapTexture.SetPixels32(pixels);
		miniMapTexture.Apply();
		RefreshMapRect();
	}
}