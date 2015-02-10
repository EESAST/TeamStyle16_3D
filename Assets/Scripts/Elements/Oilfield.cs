#region

using UnityEngine;

#endregion

public class Oilfield : Resource
{
	private static readonly Material[] materials = new Material[4];

	public override Vector3 Center() { return new Vector3(0.00f, 0.04f, 0.02f); }

	protected override float CurrentStorage() { return currentFuel; }

	protected override Vector3 Dimensions() { return new Vector3(2.67f, 2.41f, 2.71f); }

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		currentFuel = targetFuel = initialStorage = Mathf.RoundToInt(info["fuel"].n);
	}

	protected override int Kind() { return 3; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Oilfield")) as GameObject).GetComponent<RectTransform>(); }

	public static void LoadMaterial()
	{
		string[] name = { "M", "P_0", "P_1", "P_2" };
		for (var id = 0; id < 4; id++)
			materials[id] = Resources.Load<Material>("Oilfield/Materials/" + name[id]);
	}

	public static void RefreshTextureOffset()
	{
		for (var i = 1; i < 4; i++)
		{
			var offset = materials[i].mainTextureOffset;
			offset.x = (offset.x + Time.deltaTime * Random.Range(-0.5f, 2)) % 1;
			offset.y = (offset.y + Time.deltaTime * Random.Range(-0.5f, 2)) % 1;
			materials[i].mainTextureOffset = offset;
		}
	}

	protected override void Start()
	{
		base.Start();
		transform.FindChild("Massif").GetComponent<MeshRenderer>().material = materials[0];
		for (var i = 0; i < 3; i++)
			transform.FindChild("Pool_" + i).GetComponent<MeshRenderer>().material = materials[i + 1];
	}

	protected override string StorageDescription() { return "燃料"; }
}