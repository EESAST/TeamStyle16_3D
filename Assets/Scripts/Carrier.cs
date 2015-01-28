#region

using GameStatics;
using UnityEngine;

#endregion

public class Carrier : Unit
{
	private static readonly Material[][] materials = new Material[1][];

	protected override Vector3 Center() { return new Vector3(0.00f, 13.00f, -0.01f); }

	protected override Vector3 Dimensions() { return new Vector3(45.01f, 71.31f, 118.43f); }

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

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 1; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override void SetPosition(float externalX, float externalY) { transform.position = Methods.Coordinates.ExternalToInternal(externalX, externalY, 1); }

	protected override void Start()
	{
		base.Start();
		foreach (Transform child in transform)
			child.GetComponent<MeshRenderer>().material = materials[0][team];
	}
}