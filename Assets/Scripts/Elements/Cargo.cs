#region

using System.Collections;
using UnityEngine;

#endregion

public class Cargo : Ship
{
	private static readonly Material[][] materials = new Material[1][];

	public override Vector3 Center() { return new Vector3(-0.75f, 0.01f, 0.30f); }

	public IEnumerator Collect(Resource target, int fuel, int metal)
	{
		yield return StartCoroutine(FaceTarget(target.transform.WorldCenterOfElement()));
		targetFuel += fuel;
		targetMetal += metal;
		target.targetFuel -= fuel;
		target.targetMetal -= metal;
		--Data.Game.CollectsLeft;
	}

	protected override Vector3 Dimensions() { return new Vector3(28.48f, 15.18f, 46.16f); }

	protected override int Kind() { return 7; }

	public static void LoadMaterial()
	{
		string[] name = { "C" };
		for (var id = 0; id < 1; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Cargo/Materials/" + name[id] + "_" + team);
		}
	}

	protected override int MaxHP() { return 60; }

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 1; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override void Start()
	{
		base.Start();
		foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
			meshRenderer.material = materials[0][team];
	}
}