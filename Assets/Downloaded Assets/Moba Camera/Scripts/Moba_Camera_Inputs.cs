#region

using System;

#endregion

[Serializable]
public class Moba_Camera_Inputs
{
	// set to true for quick testing with keycodes
	// set to false for use with Input Manager
	public Moba_Camera_Axis axis = new Moba_Camera_Axis();
	public Moba_Camera_KeyCodes keycodes = new Moba_Camera_KeyCodes();
	public bool useKeyCodeInputs = true;
}