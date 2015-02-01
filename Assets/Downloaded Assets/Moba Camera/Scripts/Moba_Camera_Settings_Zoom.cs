#region

using System;

#endregion

[Serializable]
public class Moba_Camera_Settings_Zoom
{
	// Changed direction zoomed

	// Starting Zoom value
	public bool constZoomRate = false;
	public float defaultZoom = 15;
	public bool invertZoom = false;
	// Minimum and Maximum zoom values
	public float maxZoom = 20;
	public float minZoom = 10;
	// How fast the cameraPivot zooms in and out
	public float thresholdTime = 0.25f;
	public float transitionRate = 5;
	public float zoomRate = 10;
}