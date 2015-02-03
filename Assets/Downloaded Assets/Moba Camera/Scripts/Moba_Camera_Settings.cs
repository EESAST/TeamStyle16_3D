#region

using System;
using UnityEngine;

#endregion

[Serializable]
public class Moba_Camera_Settings
{
	public bool cameraLocked = true;
	public Transform lockTargetTransform = null;
	// Helper classes for organization
	public Moba_Camera_Settings_Movement movement = new Moba_Camera_Settings_Movement();
	public Moba_Camera_Settings_Rotation rotation = new Moba_Camera_Settings_Rotation();
	public float tolerance = 0.01f;
	public bool useBoundaries = true;
	public bool useFixedUpdate = false;
	public Moba_Camera_Settings_Zoom zoom = new Moba_Camera_Settings_Zoom();
}