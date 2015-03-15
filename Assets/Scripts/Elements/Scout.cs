#region

using System.Collections;
using UnityEngine;

#endregion

public class Scout : Plane
{
	private static readonly Material[][] materials = new Material[2][];

	public override Vector3 Center() { return new Vector3(-0.00f, 0.04f, -0.20f); }

	protected override Vector3 Dimensions() { return new Vector3(1.59f, 0.82f, 2.51f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		++explosionsLeft;
		(Instantiate(Resources.Load("Bomb"), transform.position, transform.rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetPosition, BombManager.Level.Small);
		isAiming = false;
		while (explosionsLeft > 0)
			yield return null;
		--Data.Replay.AttacksLeft;
	}

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase)
	{
		++explosionsLeft;
		(Instantiate(Resources.Load("Bomb"), transform.position, transform.rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetUnitBase, BombManager.Level.Small);
		isAiming = false;
		while (explosionsLeft > 0)
			yield return null;
		--Data.Replay.AttacksLeft;
	}

	protected override int Kind() { return 9; }

	public static void LoadMaterial()
	{
		string[] name = { "S", "R" };
		for (var id = 0; id < 2; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Scout/Materials/" + name[id] + "_" + team);
		}
	}

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 2; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	public static void RefreshTextureOffset()
	{
		for (var team = 0; team < 3; team++)
		{
			var offset = materials[1][team].mainTextureOffset;
			offset.y = (offset.y + Time.deltaTime) % 1;
			materials[1][team].mainTextureOffset = offset;
		}
	}

	protected override void Start()
	{
		base.Start();
		transform.Find("Airframe").GetComponent<MeshRenderer>().materials = new[] { materials[1][team], materials[0][team] };
	}
}