#region

using UnityEngine;

#endregion

public class Mine : EntityBehaviour
{
	private static readonly Vector3 dimensions = new Vector3(3.98f, 3.11f, 2.71f);
	private static readonly int maxHP = 100;
	private readonly Quaternion _defaultRotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
	protected override Quaternion DefaultRotation { get { return _defaultRotation; } }

	protected override void Awake()
	{
		base.Awake();
		team = 2;
	}

	protected override Vector3 Dimensions() { return dimensions; }

	protected override int MaxHP() { return maxHP; }
}