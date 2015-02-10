#region

using System;
using UnityEngine;

#endregion

public class Monitor : MonoBehaviour
{
	private static int markPatternIndex;
	private static float markScaleFactor;
	private static Vector2 physicalScreenSize;
	public static Vector2 ScreenSize;
	private static readonly Color[] teamColor = new Color[4];

	private void Update()
	{
		#region Current Scores

		if (!Equals(Data.Replay.CurrentScores, Data.Replay.TargetScores))
			for (var i = 0; i < 2; i++)
				Data.Replay.CurrentScores[i] = Mathf.Lerp(Data.Replay.CurrentScores[i], Data.Replay.TargetScores[i], Settings.TransitionRate * Time.deltaTime);

		#endregion

		#region Current Team Color

		if (!Methods.Array.Equals(Data.TeamColor.Current, Data.TeamColor.Target))
		{
			for (var i = 0; i < 4; i++)
				Data.TeamColor.Current[i] = Color.Lerp(Data.TeamColor.Current[i], Data.TeamColor.Target[i], Settings.TeamColor.TransitionRate * Time.deltaTime);
			if (Delegates.CurrentTeamColorChanged != null)
				Delegates.CurrentTeamColorChanged();
		}

		#endregion

		#region Mark Pattern Index

		if (markPatternIndex != Data.MiniMap.MarkPatternIndex)
		{
			if (Delegates.MarkPatternChanged != null)
				Delegates.MarkPatternChanged();
			markPatternIndex = Data.MiniMap.MarkPatternIndex;
		}

		#endregion

		#region Mark Scale Factor

		if (Math.Abs(markScaleFactor - Data.MiniMap.MarkScaleFactor) > Mathf.Epsilon)
		{
			if (Delegates.MarkSizeChanged != null)
				Delegates.MarkSizeChanged();
			markScaleFactor = Data.MiniMap.MarkScaleFactor;
		}

		#endregion

		#region Screen Size

		var screenSize = new Vector2(Screen.width, Screen.height);
		if (ScreenSize != screenSize)
		{
			Methods.OnScreenSizeChanged();
			if (Delegates.ScreenSizeChanged != null)
				Delegates.ScreenSizeChanged();
			ScreenSize = screenSize;
		}

		#endregion

		#region GUI Only

		if (!Data.GUI.Initialized)
			return;

		#region Target Team Color

		if (!Methods.Array.Equals(teamColor, Data.TeamColor.Target))
		{
			Methods.GUI.RefreshTeamColoredStyles();
			for (var i = 0; i < 4; i++)
				teamColor[i] = Data.TeamColor.Target[i];
		}

		#endregion

		#region Physical Screen Size

		var physicalScreenSize = new Vector2(Screen.width, Screen.height) / Screen.dpi;
		if (Monitor.physicalScreenSize != physicalScreenSize)
		{
			Methods.GUI.ResizeFonts();
			Monitor.physicalScreenSize = physicalScreenSize;
		}

		#endregion

		#endregion
	}
}