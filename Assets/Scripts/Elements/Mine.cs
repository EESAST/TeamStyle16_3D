#region

using JSON;
using UnityEngine;

#endregion

public class Mine : Resource
{
	private static readonly Material[] materials = new Material[2];

	public override Vector3 Center() { return new Vector3(0.05f, 0.21f, -0.04f); }

	public override int CurrentStorage() { return Mathf.RoundToInt(currentMetal); }

	protected override Vector3 Dimensions() { return new Vector3(3.44f, 3.11f, 2.71f); }

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		currentMetal = targetMetal = info["metal"].i;
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
		transform.Find("Minerals").GetComponent<MeshRenderer>().material = materials[0];
		transform.Find("Ore").GetComponent<MeshRenderer>().material = materials[1];
	}

	protected override string StorageDescription() { return "金属"; }
}