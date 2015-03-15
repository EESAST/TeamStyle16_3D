#region

using System.Collections;
using UnityEngine;

#endregion

public class Fighter : Plane
{
	private static readonly Material[][] materials = new Material[1][];
	private Transform[] missiles;

	protected override void Awake()
	{
		base.Awake();
		missiles = new[] { transform.Find("Airframe/Barrel_FL"), transform.Find("Airframe/Barrel_FR"), transform.Find("Airframe/Barrel_RL"), transform.Find("Airframe/Barrel_RR") };
	}

	public override Vector3 Center() { return new Vector3(0.00f, 0.42f, 0.22f); }

	protected override Vector3 Dimensions() { return new Vector3(4.78f, 1.93f, 5.82f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		explosionsLeft += 4;
		for (var i = 0; i < 4; ++i)
			(Instantiate(Resources.Load("Bomb"), missiles[i].position, missiles[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetPosition, BombManager.Level.Small);
		isAiming = false;
		while (explosionsLeft > 0)
			yield return null;
		--Data.Replay.AttacksLeft;
	}

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase)
	{
		explosionsLeft += 4;
		for (var i = 0; i < 4; ++i)
			(Instantiate(Resources.Load("Bomb"), missiles[i].position, missiles[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetUnitBase, BombManager.Level.Small);
		isAiming = false;
		while (explosionsLeft > 0)
			yield return null;
		--Data.Replay.AttacksLeft;
	}

	protected override int Kind() { return 8; }

	public static void LoadMaterial()
	{
		string[] name = { "F" };
		for (var id = 0; id < 1; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Fighter/Materials/" + name[id] + "_" + team);
		}
	}

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 1; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override void Start()
	{
		base.Start();
		foreach (Transform child in transform)
			child.GetComponent<MeshRenderer>().material = materials[0][team];
	}
}