#region

using UnityEngine;

#endregion

public class MiniFrame : MonoBehaviour
{
	private RectTransform frameRect;

	private void Awake()
	{
		Delegates.ScreenSizeChanged += RefreshFrameRect;
		frameRect = GetComponent<RectTransform>();
	}

	private void OnDestroy() { Delegates.ScreenSizeChanged -= RefreshFrameRect; }

	private void RefreshFrameRect()
	{
		frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Data.MapSize.y * Data.MiniMap.ScaleFactor + Settings.MiniMap.Border.horizontal);
		frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Data.MapSize.x * Data.MiniMap.ScaleFactor + Settings.MiniMap.Border.vertical);
	}

	private void Start() { RefreshFrameRect(); }

	private void Update()
	{
		var rect = frameRect.rect;
		rect.x += Screen.width;
		rect.y = -rect.y - rect.height;
		Data.GUI.OccupiedRects.Add(rect);
	}
}