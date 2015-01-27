#region

using GameStatics;
using UnityEngine;

#endregion

public class Base : Unit
{
	private static readonly Material[][] materials = new Material[2][];
	protected override Quaternion DefaultRotation { get { return Quaternion.Euler(0, 45, 0); } }
	protected override float RelativeSize { get { return 3; } }

	protected override Vector3 Dimensions() { return new Vector3(6.23f, 5.25f, 6.23f); }

	public static void LoadMaterial()
	{
		string[] name = { "CC", "PF" };
		for (var id = 0; id < 2; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Base/Materials/" + name[id] + "_" + team);
		}
	}

	protected override int MaxHP() { return 2000; }

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 2; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override void Start()
	{
		base.Start();
		transform.FindChild("Bearing").GetComponent<MeshRenderer>().material = materials[1][team];
		transform.FindChild("Body").GetComponent<MeshRenderer>().materials = new[] { materials[1][team], materials[0][team] };
		var head = transform.FindChild("Head");
		head.GetComponent<MeshRenderer>().material = materials[0][team];
		head.FindChild("BigGuns").GetComponent<MeshRenderer>().material = materials[1][team];
		head.FindChild("SmallGuns").GetComponent<MeshRenderer>().material = materials[1][team];
	}
}