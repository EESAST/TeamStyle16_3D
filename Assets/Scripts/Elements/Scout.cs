#region

using System.Collections;
using UnityEngine;

#endregion

public class Scout : Plane
{
	private static readonly Material[][] materials = new Material[2][];

	protected override int AmmoOnce() { return 1; }

	public override Vector3 Center() { return new Vector3(-0.00f, 0.04f, -0.20f); }

	protected override Vector3 Dimensions() { return new Vector3(1.59f, 0.82f, 2.51f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		++explosionsLeft;
		(Instantiate(Resources.Load("Bomb"), transform.position, transform.rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetPosition);
		while (explosionsLeft > 0)
			yield return null;
		--Data.Replay.AttacksLeft;
	}

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase)
	{
		++explosionsLeft;
		(Instantiate(Resources.Load("Bomb"), transform.position, transform.rotation) as GameObject).GetComponent<BombManager>().Setup(this, targetUnitBase);
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

	protected override int MaxHP() { return 50; }

	protected override int Population() { return 1; }

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
			offset.y = (offset.y + Time.smoothDeltaTime) % 1;
			materials[1][team].mainTextureOffset = offset;
		}
	}

	protected override int Speed() { return 10; }

	protected override void Start()
	{
		base.Start();
		transform.Find("Airframe").GetComponent<MeshRenderer>().materials = new[] { materials[1][team], materials[0][team] };
	}
}