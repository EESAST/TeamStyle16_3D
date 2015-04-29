#region

using System;
using System.Linq;
using UnityEngine;

#endregion

public class Monitor : MonoBehaviour
{
	public static Vector2 ScreenSize;
	private readonly Color[] teamColor = new Color[4];
	private float fontSizeScaleFactor = 1;
	private int markPatternIndex;
	private float markScaleFactor = 1;

	private void Awake()
	{
		if (!Data.GlobalMonitor)
			DontDestroyOnLoad(Data.GlobalMonitor = gameObject);
		else
			Destroy(gameObject);
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
			return;
		var resolution = Screen.resolutions.Last();
		Screen.SetResolution(resolution.width, resolution.height, true);
	}

	private void Update()
	{
		#region Current Scores

		if (!Data.GamePaused && !Equals(Data.Replay.CurrentScores, Data.Replay.TargetScores))
			for (var i = 0; i < 2; i++)
				Data.Replay.CurrentScores[i] = Mathf.Lerp(Data.Replay.CurrentScores[i], Data.Replay.TargetScores[i], Settings.TransitionRate * Time.unscaledDeltaTime);

		#endregion

		#region Current Team Color

		if (!Data.GamePaused && !Methods.Array.Equals(Data.TeamColor.Current, Data.TeamColor.Target))
		{
			for (var i = 0; i < 4; i++)
				Data.TeamColor.Current[i] = Color.Lerp(Data.TeamColor.Current[i], Data.TeamColor.Target[i], Settings.TransitionRate * Time.unscaledDeltaTime);
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

		#region Font Size Scale Factor

		if (Math.Abs(fontSizeScaleFactor - Data.GUI.FontSizeScaleFactor) > Mathf.Epsilon)
		{
			Methods.GUI.ResizeFonts();
			fontSizeScaleFactor = Data.GUI.FontSizeScaleFactor;
		}

		#endregion

		#region Screen Size

		var screenSize = new Vector2(Screen.width, Screen.height);
		if (ScreenSize != screenSize)
		{
			Methods.GUI.OnScreenSizeChanged();
			if (Delegates.ScreenSizeChanged != null)
				Delegates.ScreenSizeChanged();
			ScreenSize = screenSize;
		}

		#endregion

		#region Target Team Color

		if (Data.GUI.Initialized && !Methods.Array.Equals(teamColor, Data.TeamColor.Target))
		{
			Methods.GUI.RefreshTeamColoredStyles();
			for (var i = 0; i < 4; i++)
				teamColor[i] = Data.TeamColor.Target[i];
		}

		#endregion
	}
}