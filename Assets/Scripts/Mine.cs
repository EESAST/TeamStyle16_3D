#region

using UnityEngine;

#endregion

public class Mine : Resource
{
	private static readonly Material[] materials = new Material[2];

	public override Vector3 Center() { return new Vector3(-0.15f, -0.21f, -0.22f); }

	protected override Vector3 Dimensions() { return new Vector3(3.98f, 3.11f, 2.71f); }

	public static void LoadMaterial()
	{
		string[] name = { "M", "O" };
		for (var id = 0; id < 2; id++)
			materials[id] = Resources.Load<Material>("Mine/Materials/" + name[id]);
	}

	protected override int MaxHP() { return 1000; }

	protected override void Start()
	{
		base.Start();
		transform.FindChild("Minerals").GetComponent<MeshRenderer>().material = materials[0];
		transform.FindChild("Ore").GetComponent<MeshRenderer>().material = materials[1];
	}
}