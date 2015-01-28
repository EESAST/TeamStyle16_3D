#region

using GameStatics;
using UnityEngine;

#endregion

public class Mine : Entity
{
	protected override void Awake()
	{
		base.Awake();
		team = 3;
	}

	protected override Vector3 Center() { return new Vector3(-0.15f, -0.21f, -0.22f); }

	protected override Vector3 Dimensions() { return new Vector3(3.98f, 3.11f, 2.71f); }

	protected override int MaxHP() { return 35; }

	protected override void SetPosition(float externalX, float externalY) { transform.position = Methods.Coordinates.ExternalToInternal(externalX, externalY, 2); }
}