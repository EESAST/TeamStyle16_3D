#region

using UnityEngine;

#endregion

<<<<<<< HEAD
public class OilField : EntityBehaviour
{
	private static readonly int maxHP = 100;
	private static readonly Vector3 unitExtents = new Vector3(3.15f, 1.33f, 3.14f);
	private readonly Quaternion _defaultRotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
	protected override Quaternion DefaultRotation { get { return _defaultRotation; } }

	protected override void Awake()
	{
		base.Awake();
		team = 2;
	}

	protected override Vector3 Dimensions() { return unitExtents; }

	protected override int MaxHP() { return maxHP; }
=======
public class OilField : Entity
{
	protected override void Awake()
	{
		base.Awake();
		team = 3;
	}

	protected override Vector3 Dimensions() { return new Vector3(3.15f, 1.33f, 3.14f); }

	protected override int MaxHP() { return 100; }
>>>>>>> initial commit on another computer
}