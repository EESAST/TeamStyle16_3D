#region

using JSON;
using UnityEngine;

#endregion

public class Oilfield : Resource
{
	private static readonly Material[] materials = new Material[4];

	public override Vector3 Center() { return new Vector3(0.10f, 0.29f, 0.21f); }

	protected override int CurrentStorage() { return Mathf.RoundToInt(currentFuel); }

	protected override Vector3 Dimensions() { return new Vector3(2.67f, 2.24f, 2.71f); }

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		currentFuel = targetFuel = info["fuel"].i;
	}

	protected override int InitialStorage() { return 1000; }

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
		transform.Find("Massif").GetComponent<MeshRenderer>().material = materials[0];
		for (var i = 0; i < 3; i++)
			transform.Find("Pool_" + i).GetComponent<MeshRenderer>().material = materials[i + 1];
	}

	protected override string StorageDescription() { return "燃料"; }
}