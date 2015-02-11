#region

using UnityEngine;

#endregion

public class Scout : Plane
{
	private static readonly Material[][] materials = new Material[2][];

	protected override int AmmoOnce() { return 1; }

	public override Vector3 Center() { return new Vector3(0.00f, 0.00f, 0.07f); }

	protected override Vector3 Dimensions() { return new Vector3(1.59f, 0.82f, 2.51f); }

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
			offset.y = (offset.y + Time.deltaTime) % 1;
			materials[1][team].mainTextureOffset = offset;
		}
	}

	protected override void Start()
	{
		base.Start();
		transform.FindChild("Airframe").GetComponent<MeshRenderer>().materials = new[] { materials[1][team], materials[0][team] };
	}
}