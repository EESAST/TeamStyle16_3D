#region

using System;
using UnityEngine;

#endregion

public class Monitor : MonoBehaviour
{
	public static int LastMarkPatternIndex;
	public static float LastMarkScaleFactor;
	public static Vector2 LastPhysicalScreenSize;
	public static Vector2 LastScreenSize;
	public static Color[] LastTeamColor = new Color[4];

	private void Update()
	{
		#region Current Team Color

		if (!Methods.Array.Equals(Data.TeamColor.Current, Data.TeamColor.Desired))
		{
			for (var i = 0; i < 4; i++)
				Data.TeamColor.Current[i] = Color.Lerp(Data.TeamColor.Current[i], Data.TeamColor.Desired[i], Settings.TeamColor.TransitionRate * Time.deltaTime);
			if (Delegates.CurrentTeamColorChanged != null)
				Delegates.CurrentTeamColorChanged();
		}

		#endregion

		#region Mark Pattern Index

		if (LastMarkPatternIndex != Data.MarkPatternIndex)
		{
			if (Delegates.MarkPatternChanged != null)
				Delegates.MarkPatternChanged();
			LastMarkPatternIndex = Data.MarkPatternIndex;
		}

		#endregion

		#region Mark Scale Factor

		if (Math.Abs(LastMarkScaleFactor - Data.MarkScaleFactor) > Mathf.Epsilon)
		{
			if (Delegates.MarkSizeChanged != null)
				Delegates.MarkSizeChanged();
			LastMarkScaleFactor = Data.MarkScaleFactor;
		}

		#endregion

		#region Screen Size

		var screenSize = new Vector2(Screen.width, Screen.height);
		if (LastScreenSize != screenSize)
		{
			Methods.RefreshMiniMap();
			if (Delegates.ScreenSizeChanged != null)
				Delegates.ScreenSizeChanged();
			LastScreenSize = screenSize;
		}

		#endregion

		#region GUI Only

		if (!Data.GUI.Initialized)
			return;

		#region Desired Team Color

		if (!Methods.Array.Equals(LastTeamColor, Data.TeamColor.Desired))
		{
			Methods.GUI.RefreshTeamColoredStyles();
			for (var i = 0; i < 4; i++)
				LastTeamColor[i] = Data.TeamColor.Desired[i];
		}

		#endregion

		#region Physical Screen Size

		var physicalScreenSize = new Vector2(Screen.width, Screen.height) / Screen.dpi;
		if (LastPhysicalScreenSize != physicalScreenSize)
		{
			Methods.GUI.ResizeFonts();
			LastPhysicalScreenSize = physicalScreenSize;
		}

		#endregion

		#endregion
	}
}