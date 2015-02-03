#region

using System;

#endregion

[Serializable]
public class Moba_Camera_Axis
{
	public string button_camera_move_backward = "Moba Camera Move Backward";
	public string button_camera_move_forward = "Moba Camera Move Forward";
	// Allows cameraPivot to be rotated while pressed

	// Move cameraPivot based on cameraPivot direction
	public string button_camera_move_left = "Moba Camera Move Left";
	public string button_camera_move_right = "Moba Camera Move Right";
	public string button_char_focus = "Moba Char Focus";
	public string button_lock_camera = "Moba Lock Camera";
	public string button_rotate_camera = "Moba Rotation Camera";
	// Input Axis
	public string DeltaMouseHorizontal = "Mouse X";
	public string DeltaMouseVertical = "Mouse Y";
	public string DeltaScrollWheel = "Mouse ScrollWheel";
}