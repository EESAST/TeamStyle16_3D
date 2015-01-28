#region

using GameStatics;
using UnityEngine;

#endregion

public class Fighter : Unit
{
	private static readonly Material[][] materials = new Material[1][];

	protected override Vector3 Center() { return new Vector3(0.00f, 0.24f, -0.05f); }

	protected override Vector3 Dimensions() { return new Vector3(4.78f, 1.93f, 5.82f); }

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

	protected override int MaxHP() { return 70; }

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 1; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override void SetPosition(float externalX, float externalY) { transform.position = Methods.Coordinates.ExternalToInternal(externalX, externalY, 3); }

	protected override void Start()
	{
		base.Start();
		foreach (Transform child in transform)
			child.GetComponent<MeshRenderer>().material = materials[0][team];
	}
}