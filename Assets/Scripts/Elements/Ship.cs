#region

using UnityEngine;

#endregion

public abstract class Ship : Unit
{
	protected override int Level() { return 1; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Ship")) as GameObject).GetComponent<RectTransform>(); }

	protected override void Start()
	{
		base.Start();
		var waterSplash = (Instantiate(Resources.Load("Water Surface Splash")) as GameObject).transform;
		waterSplash.parent = transform;
		waterSplash.localPosition = Vector3.Scale(Center(), new Vector3(1, 0, 1));
	}
}