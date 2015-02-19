#region

using System;
using System.Collections;
using UnityEngine;

#endregion

public class Carrier : Ship	//TODO: add attack effects
{
	private static readonly Material[][] materials = new Material[1][];

	protected override IEnumerator AimAtPosition(Vector3 targetPosition) { throw new NotImplementedException(); }

	protected override int AmmoOnce() { return 2; }

	public override Vector3 Center() { return new Vector3(-0.02f, 0.26f, 0.15f); }

	protected override Vector3 Dimensions() { return new Vector3(1.14f, 1.82f, 3.01f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		throw new NotImplementedException();
		--Data.Replay.AttacksLeft; //TODO:当拦截机就绪时执行
	}

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase)
	{
		throw new NotImplementedException();
		--Data.Replay.AttacksLeft;
	}

	protected override int Kind() { return 6; }

	public static void LoadMaterial()
	{
		string[] name = { "C" };
		for (var id = 0; id < 1; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Carrier/Materials/" + name[id] + "_" + team);
		}
	}

	protected override int MaxHP() { return 120; }

	protected override int Population() { return 4; }

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 1; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override int Speed() { return 5; }

	protected override void Start()
	{
		base.Start();
		foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
			meshRenderer.material = materials[0][team];
	}
}