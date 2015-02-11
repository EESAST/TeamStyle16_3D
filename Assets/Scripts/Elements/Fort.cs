#region

using System.Collections;
using UnityEngine;

#endregion

public class Fort : Building
{
	private static readonly Material[][] materials = new Material[3][];
	public int targetTeam;

	protected override int AmmoOnce() { return 4; }

	protected override void Awake()
	{
		base.Awake();
		targetTeam = -1;
	}

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

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (targetTeam == -1)
			return;
		if (team < 2)
			--Data.FortNum[team];
		var fort = (Instantiate(Resources.Load("Fort/Fort")) as GameObject).GetComponent<Fort>();
		fort.StartCoroutine(fort.Reborn(transform.position, index, targetTeam, targetFuel, targetAmmo, targetMetal));
	}

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		if (team < 2)
			++Data.FortNum[team];
	}

	private IEnumerator Reborn(Vector3 internalPosition, int index, int team, int fuel, int ammo, int metal) //TODO:maybe using a sine lerp
	{
		Data.Replay.Elements.Add(this.index = index, this);
		++Data.Replay.UnitNums[this.team = team];
		targetHP = MaxHP();
		targetFuel = fuel;
		targetAmmo = ammo;
		targetFuel = metal;
		++Data.FortNum[team];
		transform.position = internalPosition - Vector3.up * RelativeSize * Settings.ScaleFactor;
		while ((internalPosition - transform.position).y > Settings.Tolerance)
		{
			transform.position = Vector3.Lerp(transform.position, internalPosition, 0.1f);
			yield return new WaitForSeconds(0.04f);
		}
		--Data.Replay.AttacksLeft;
	}

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

	protected override void Start()
	{
		base.Start();
		transform.FindChild("Accessory").GetComponent<MeshRenderer>().material = materials[1][team];
		transform.FindChild("Base").GetComponent<MeshRenderer>().material = materials[0][team];
		transform.FindChild("Cannon").GetComponent<MeshRenderer>().materials = new[] { materials[1][team], materials[2][team] };
	}
}