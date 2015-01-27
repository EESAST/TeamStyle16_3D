#region

using System;
using UnityEngine;

#endregion

[Serializable]
public class Moba_Camera_Requirements
{
	// Objects that are requirements for the script to work
	public Camera camera = null;
	public Transform offset = null;
	public Transform pivot = null;
}