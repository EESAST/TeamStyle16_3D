#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class MiniMap : MonoBehaviour
{
	private RectTransform mapRect;
	private Texture2D miniMapTexture;

	private void Awake()
	{
		Delegates.ScreenSizeChanged += RefreshMapRect;
		(mapRect = GetComponent<RectTransform>()).anchoredPosition = -Vector2.one * Settings.MiniMap.BorderOffset;
	}

	private void OnDestroy() { Delegates.ScreenSizeChanged -= RefreshMapRect; }

	private void RefreshMapRect()
	{
		mapRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Data.MapSize.y * Data.MiniMap.ScaleFactor);
		mapRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Data.MapSize.x * Data.MiniMap.ScaleFactor);
	}

	private void Start()
	{
		var mapData = Data.BattleData["gamebody"]["map_info"]["types"];
		var cols = Mathf.RoundToInt(Data.MapSize.y);
		var rows = Mathf.RoundToInt(Data.MapSize.x);
		var width = cols * Settings.MiniMap.Granularity;
		var height = rows * Settings.MiniMap.Granularity;
		GetComponent<RawImage>().texture = miniMapTexture = new Texture2D(width, height) { wrapMode = TextureWrapMode.Clamp };
		var tmpPixels = miniMapTexture.GetPixels32();
		for (var i = 0; i < width; i++)
			for (var j = 0; j < height; j++)
				tmpPixels[i + width * j] = mapData[rows - j / Settings.MiniMap.Granularity - 1][i / Settings.MiniMap.Granularity].n < Mathf.Epsilon ? Settings.MiniMap.OceanColor : Settings.MiniMap.LandColor;
		miniMapTexture.SetPixels32(tmpPixels);
		miniMapTexture.Apply();
		RefreshMapRect();
	}
}