#region

using System;
using UnityEngine;

#endregion

[Serializable]
public class Moba_Camera_KeyCodes
{
	// Allows cameraPivot to be rotated while pressed

	// Move cameraPivot based on cameraPivot direction
	public KeyCode CameraMoveBackward = KeyCode.DownArrow;
	public KeyCode CameraMoveForward = KeyCode.UpArrow;
	public KeyCode CameraMoveLeft = KeyCode.LeftArrow;
	public KeyCode CameraMoveRight = KeyCode.RightArrow;
	public KeyCode characterFocus = KeyCode.Space;
	public KeyCode LockCamera = KeyCode.L;
	public KeyCode RotateCamera = KeyCode.Mouse2;
}