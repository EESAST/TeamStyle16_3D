#region

using UnityEngine;

#endregion

public abstract class Vessel : Unit
{
	protected override int Level() { return 1; }

	protected override void Start()
	{
		base.Start();
		transform.FindChild("Water Surface Splash").localPosition = Vector3.Scale(Center(), new Vector3(1, 0, 1));
	}
}