#region

using UnityEngine;

#endregion

public abstract class Plane : Unit
{
	protected override int Level() { return 3; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Plane")) as GameObject).GetComponent<RectTransform>(); }
}