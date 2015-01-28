#region

using GameStatics;
using UnityEngine;

#endregion

public class OilField : Entity
{
	protected override void Awake()
	{
		base.Awake();
		team = 3;
	}

	protected override Vector3 Center() { return new Vector3(0.00f, 0.04f, 0.02f); }

	protected override Vector3 Dimensions() { return new Vector3(2.67f, 2.41f, 2.71f); }

	protected override int MaxHP() { return 100; }

	protected override void SetPosition(float externalX, float externalY) { transform.position = Methods.Coordinates.ExternalToInternal(externalX, externalY, 2); }
}