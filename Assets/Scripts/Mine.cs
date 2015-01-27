#region

using UnityEngine;

#endregion

public class Mine : Entity
{
	protected override void Awake()
	{
		base.Awake();
		team = 3;
	}

	protected override Vector3 Dimensions() { return new Vector3(3.98f, 3.11f, 2.71f); }

	protected override int MaxHP() { return 35; }
}