#region

using UnityEngine;

#endregion

public class OilField : Entity
{
	protected override void Awake()
	{
		base.Awake();
		team = 3;
	}

	protected override Vector3 Dimensions() { return new Vector3(3.15f, 1.33f, 3.14f); }

	protected override int MaxHP() { return 100; }
}