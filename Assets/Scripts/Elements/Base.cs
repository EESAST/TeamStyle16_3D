#region

using System.Collections;
using System.Linq;
using JSON;
using UnityEngine;

#endregion

public class Base : Building
{
	private static readonly Material[][] materials = new Material[2][];
	private Transform[] bigBombs;
	private Transform bigGuns;
	private Transform head;
	private Component[] idleFXs;
	private Transform lightPod;
	private Transform[] smallBombs;
	private Transform smallGuns;
	protected override Transform Beamer { get { return lightPod; } }
	protected override Quaternion DefaultRotation { get { return Quaternion.identity; } }
	protected override int RelativeSize { get { return 3; } }

	protected override IEnumerator AimAtPosition(Vector3 targetPosition)
	{
		foreach (var idleFX in idleFXs.Cast<IIdleFX>())
			idleFX.Disable();
		var targetRotation = Quaternion.LookRotation(targetPosition - head.position);
		while (Quaternion.Angle(head.rotation = Quaternion.RotateTowards(head.rotation, targetRotation, Settings.SteeringRate.Base_Head * Time.deltaTime), targetRotation) > Settings.AngularTolerance)
			yield return null;
		var targetRotation_BG = Quaternion.LookRotation(targetPosition - bigGuns.position);
		var targetRotation_SG = Quaternion.LookRotation(targetPosition - smallGuns.position);
		while (Quaternion.Angle(bigGuns.rotation = Quaternion.RotateTowards(bigGuns.rotation, targetRotation_BG, Settings.SteeringRate.Base_BigGuns * Time.deltaTime), targetRotation_BG) > Settings.AngularTolerance || Quaternion.Angle(smallGuns.rotation = Quaternion.RotateTowards(smallGuns.rotation, targetRotation_SG, Settings.SteeringRate.Base_SmallGuns * Time.deltaTime), targetRotation_SG) > Settings.AngularTolerance)
			yield return null;
	}

	protected override int AmmoOnce() { return 6; }

	protected override void Awake()
	{
		base.Awake();
		head = transform.Find("Head");
		bigGuns = head.Find("BigGuns");
		smallGuns = head.Find("SmallGuns");
		lightPod = head.Find("LightPod");
		bigBombs = new[] { bigGuns.Find("BG_LSP"), bigGuns.Find("BG_RSP") };
		smallBombs = new[] { smallGuns.Find("SG_LSP"), smallGuns.Find("SG_RSP") };
		idleFXs = head.GetComponentsInChildren(typeof(IIdleFX));
	}

	public override Vector3 Center() { return new Vector3(0.01f, 2.55f, -0.01f); }

	protected override Vector3 Dimensions() { return new Vector3(6.23f, 5.25f, 6.23f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		explosionsLeft += 4;
		for (var i = 0; i < 2; ++i)
		{
			(Instantiate(Resources.Load("Bomb"), bigBombs[i].position, bigBombs[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetPosition, BombManager.Level.Large);
			(Instantiate(Resources.Load("Bomb"), smallBombs[i].position, smallBombs[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetPosition, BombManager.Level.Small);
		}
		while (explosionsLeft > 0)
			yield return null;
		foreach (var idleFX in idleFXs.Cast<IIdleFX>())
			idleFX.Enable();
		--Data.Replay.AttacksLeft;
	}

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase)
	{
		explosionsLeft += 4;
		for (var i = 0; i < 2; ++i)
		{
			(Instantiate(Resources.Load("Bomb"), bigBombs[i].position, bigBombs[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetUnitBase, BombManager.Level.Large);
			(Instantiate(Resources.Load("Bomb"), smallBombs[i].position, smallBombs[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetUnitBase, BombManager.Level.Small);
		}
		while (explosionsLeft > 0)
			yield return null;
		foreach (var idleFX in idleFXs.Cast<IIdleFX>())
			idleFX.Enable();
		--Data.Replay.AttacksLeft;
	}

	public IEnumerator Fix(Unit target, int metal, int healthIncrease)
	{
		var elapsedTime = Mathf.Max(healthIncrease / Settings.Replay.FixRate, 0.1f);
		StartCoroutine(Beam(target, elapsedTime, BeamType.Fix));
		yield return new WaitForSeconds((target.transform.WorldCenterOfElement() - Beamer.position).magnitude / Settings.Replay.BeamSpeed);
		var effectedHP = 0;
		var effectedMetal = 0;
		for (float t, startTime = Time.time; (t = (Time.time - startTime) / elapsedTime) < 1;)
		{
			var deltaHP = Mathf.RoundToInt(healthIncrease * t - effectedHP);
			if (deltaHP > 0)
			{
				target.targetHP += deltaHP;
				effectedHP += deltaHP;
			}
			var deltaMetal = Mathf.RoundToInt(metal * t - effectedMetal);
			if (deltaMetal > 0)
			{
				targetMetal -= deltaMetal;
				effectedMetal += deltaMetal;
			}
			yield return null;
		}
		target.targetHP += healthIncrease - effectedHP;
		targetMetal -= metal - effectedMetal;
		yield return StartCoroutine(Data.Replay.Instance.ShowMessageAt(target, healthIncrease > 0 ? "HP: +" + healthIncrease : "0"));
		--Data.Replay.FixesLeft;
	}

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		Data.Replay.Bases[team] = this;
	}

	protected override int Kind() { return 0; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Base")) as GameObject).GetComponent<RectTransform>(); }

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

	protected override void RefreshColor()
	{
		base.RefreshColor();
		GetComponentInChildren<Flashlight>().RefreshLightColor();
	}

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 2; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override void Start()
	{
		base.Start();
		transform.Find("Bearing").GetComponent<MeshRenderer>().material = materials[1][team];
		transform.Find("Body").GetComponent<MeshRenderer>().materials = new[] { materials[1][team], materials[0][team] };
		head.GetComponent<MeshRenderer>().material = materials[0][team];
		bigGuns.GetComponent<MeshRenderer>().material = materials[1][team];
		smallGuns.GetComponent<MeshRenderer>().material = materials[1][team];
	}
}