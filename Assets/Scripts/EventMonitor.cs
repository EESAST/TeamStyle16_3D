#region

using GameStatics;
using UnityEngine;

#endregion

public class EventMonitor : MonoBehaviour
{
	private Vector2 screenSize;

	private void Update()
	{
		#region Team Color

		if (!Methods.Array.Equals(Data.TeamColor.Current, Data.TeamColor.Desired))
		{
			for (var i = 0; i < 3; i++)
				Data.TeamColor.Current[i] = Color.Lerp(Data.TeamColor.Current[i], Data.TeamColor.Desired[i], Settings.TeamColor.TransitionRate * Time.timeScale);
			Delegates.TeamColorChanged();
		}

		#endregion

		#region Screen Size

		var newScreenSize = new Vector2(Screen.width, Screen.height);
		if (screenSize != newScreenSize)
		{
			screenSize = newScreenSize;
			Data.MiniMap.ScaleFactor = (Screen.width + Screen.height) / (Vector2.Dot(Data.MapSize, Vector2.one * 4));
			var bl = Methods.Coordinates.ExternalToMiniMapBasedScreen(Vector2.right * Data.MapSize.x);
			var tr = Methods.Coordinates.ExternalToMiniMapBasedScreen(Vector2.up * Data.MapSize.y);
			Data.MiniMap.Rect = new Rect(bl.x, bl.y, (tr - bl).x, (tr - bl).y);
			Delegates.ScreenSizeChanged();
		}

		#endregion
	}
}