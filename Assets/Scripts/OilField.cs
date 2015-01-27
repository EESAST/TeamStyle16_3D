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

	protected override Vector3 Dimensions() { return new Vector3(2.67f, 2.41f, 2.71f); }

	protected override int MaxHP() { return 100; }
}