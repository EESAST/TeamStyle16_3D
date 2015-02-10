#region

using UnityEngine;

#endregion

public class Mine : Resource
{
	private static readonly Material[] materials = new Material[2];

	public override Vector3 Center() { return new Vector3(-0.15f, -0.21f, -0.22f); }

	protected override float CurrentStorage() { return currentMetal; }

	protected override Vector3 Dimensions() { return new Vector3(3.98f, 3.11f, 2.71f); }

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		currentMetal = targetMetal = initialStorage = Mathf.RoundToInt(info["metal"].n);
	}

	protected override int Kind() { return 2; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Mine")) as GameObject).GetComponent<RectTransform>(); }

	public static void LoadMaterial()
	{
		string[] name = { "M", "O" };
		for (var id = 0; id < 2; id++)
			materials[id] = Resources.Load<Material>("Mine/Materials/" + name[id]);
	}

	protected override void Start()
	{
		base.Start();
		transform.FindChild("Minerals").GetComponent<MeshRenderer>().material = materials[0];
		transform.FindChild("Ore").GetComponent<MeshRenderer>().material = materials[1];
	}

	protected override string StorageDescription() { return "金属"; }
}