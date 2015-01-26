#region

using GameStatics;
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
		var rows = Mathf.RoundToInt(Data.MapSize.x);
		var cols = Mathf.RoundToInt(Data.MapSize.y);
		var granularity = Settings.MiniMap.Granularity;
		GetComponent<RawImage>().texture = miniMapTexture = new Texture2D(cols * granularity, rows * granularity)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		for (var i = 0; i < cols * granularity; i++)
			for (var j = 0; j < rows * granularity; j++)
				miniMapTexture.SetPixel(i, j, mapData[rows - j / granularity - 1][i / granularity].n < Mathf.Epsilon ? Settings.MiniMap.SeaColor : Settings.MiniMap.LandColor);
		miniMapTexture.Apply();
	}
}