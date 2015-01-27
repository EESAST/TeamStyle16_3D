#region

using System;

#endregion

[Serializable]
public class Moba_Camera_Settings_Movement
{
	// The rate the cameraPivot will transition from its currentHeight screenPoint to Destination

	// How fast the cameraPivot moves
	public float cameraMovementRate = 1;
	public float defaultHeight = 0;
	// Does cameraPivot move if mouse is near the edge of the screen
	public bool edgeHoverMovement = true;
	// The ObjectDistance from the edge of the screen 
	public float edgeHoverOffset = 10;
	public float transitionRate = 0.1f;
	// The default value for the height of the pivot externalY screenPoint
	public bool useDefaultHeight = true;
	public bool useLockTargetHeight = true;
}