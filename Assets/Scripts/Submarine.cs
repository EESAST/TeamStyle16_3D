#region

using UnityEngine;

#endregion

public class Submarine : Unit
{
	private static readonly Material[][] materials = new Material[1][];

	public override Vector3 Center() { return new Vector3(0.44f, 0.00f, 1.14f); }

	public override void Deselect()
	{
		base.Deselect();
		highlighter.FlashingOn();
	}

	protected override Vector3 Dimensions() { return new Vector3(21.10f, 34.56f, 83.97f); }

	protected override int Level() { return 0; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("SubmarineMark")) as GameObject).GetComponent<RectTransform>(); }

	public static void LoadMaterial()
	{
		string[] name = { "S" };
		for (var id = 0; id < 1; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Submarine/Materials/" + name[id] + "_" + team);
		}
	}

	protected override int MaxHP() { return 35; }

	protected override void RefreshColor()
	{
		base.RefreshColor();
		highlighter.FlashingParams(Data.TeamColor.Current[team], Color.clear, 0.6f);
	}

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 1; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	public override void Select()
	{
		base.Select();
		highlighter.FlashingOff();
	}

	protected override void Start()
	{
		base.Start();
		foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
			meshRenderer.material = materials[0][team];
		highlighter.FlashingOn(Data.TeamColor.Current[team], Color.clear, 0.6f);
	}
}