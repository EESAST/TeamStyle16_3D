#region

using System;
using UnityEngine;

#endregion

[Serializable]
public class Moba_Camera_Settings_Rotation
{
	public bool cameraRotationAutoRevert = true;
	public Vector2 cameraRotationRate = Vector2.one * 100;
	// Rotation rate does not change based on speed of mouse
	public bool constRotationRate = false;
	public Vector2 defaultRotation = new Vector2(-45, 0);
	// Lock the rotations axies
	public bool lockRotationX = false;
	public bool lockRotationY = false;
	// rotationOffsetToLocal that is used when the game starts
	public float thresholdTime = 0.1f;
	public float transitionRate = 0.1f;
}