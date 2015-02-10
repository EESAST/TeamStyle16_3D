#region

using System.Collections;
using UnityEngine;

#endregion

public class Fort : Building
{
	private static readonly Material[][] materials = new Material[3][];
	public int targetTeam;

	public override Vector3 Center() { return new Vector3(0.05f, 0.60f, 0.05f); }

	protected override Vector3 Dimensions() { return new Vector3(2.47f, 1.87f, 2.47f); }

	protected override int Kind() { return 1; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Fort")) as GameObject).GetComponent<RectTransform>(); }

	public static void LoadMaterial()
	{
		string[] name = { "B", "C", "R" };
		for (var id = 0; id < 3; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Fort/Materials/" + name[id] + "_" + team);
		}
	}

	protected override int MaxHP() { return 800; }

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 3; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	public static void RefreshTextureOffset()
	{
		for (var team = 0; team < 3; team++)
		{
			var offset = materials[2][team].mainTextureOffset;
			offset.y = (offset.y + Time.deltaTime) % 1;
			materials[2][team].mainTextureOffset = offset;
		}
	}

	/*private void OnDestroy()
	{
		var fort = (Instantiate(Resources.Load("Fort/Fort")) as GameObject).GetComponent<Fort>();	//TODO:spawning new objects in OnDestroy() is dangerous
		fort.team = targetTeam;
		fort.StartCoroutine(fort.Rise(transform.position));
		Data.Elements.Add(index, fort);
	}*/

	private IEnumerator Rise(Vector3 internalTargetPosition) //TODO:maybe using a sine lerp
	{
		transform.position = internalTargetPosition - Vector3.up * RelativeSize * Settings.ScaleFactor;
		while ((internalTargetPosition - transform.position).y > Settings.Tolerance)
		{
			transform.position = Vector3.Lerp(transform.position, internalTargetPosition, 0.1f);
			yield return new WaitForSeconds(0.04f);
		}
	}

	protected override void Start()
	{
		base.Start();
		transform.FindChild("Accessory").GetComponent<MeshRenderer>().material = materials[1][team];
		transform.FindChild("Base").GetComponent<MeshRenderer>().material = materials[0][team];
		transform.FindChild("Cannon").GetComponent<MeshRenderer>().materials = new[] { materials[1][team], materials[2][team] };
	}
}