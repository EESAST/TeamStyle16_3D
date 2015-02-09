#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class MiniFrame : MonoBehaviour
{
	private void Awake() { Delegates.ScreenSizeChanged += RefreshFrameRect; }

	private void OnDestroy() { Delegates.ScreenSizeChanged -= RefreshFrameRect; }

	private void RefreshFrameRect()
	{
		GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Data.MapSize.y * Data.MiniMap.ScaleFactor + Settings.MiniMap.Border.horizontal);
		GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Data.MapSize.x * Data.MiniMap.ScaleFactor + Settings.MiniMap.Border.vertical);
	}

	private void Start() { RefreshFrameRect(); }
}