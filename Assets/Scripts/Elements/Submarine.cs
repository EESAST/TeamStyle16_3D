#region

using System.Collections;
using UnityEngine;

#endregion

public class Submarine : Unit
{
	private static readonly Material[][] materials = new Material[1][];
	private AutoRotation[] autoRotations;
	private Transform[] torpedos;
	private ParticleSystem[] trails;

	protected override void Activate()
	{
		base.Activate();
		foreach (var trail in trails)
			trail.Play();
		foreach (var autoRotation in autoRotations)
			autoRotation.enabled = true;
	}

	protected override IEnumerator AimAtPosition(Vector3 targetPosition) { yield return StartCoroutine(AdjustOrientation(Vector3.Scale(targetPosition - transform.position, new Vector3(1, 0, 1)))); }

	protected override void Awake()
	{
		base.Awake();
		torpedos = new[] { transform.Find("Hull/LSP"), transform.Find("Hull/RSP") };
		trails = GetComponentsInChildren<ParticleSystem>();
		autoRotations = GetComponentsInChildren<AutoRotation>();
	}

	public override Vector3 Center() { return new Vector3(-0.00f, 0.19f, 0.04f); }

	protected override void Deactivate()
	{
		base.Deactivate();
		foreach (var trail in trails)
			trail.Stop();
		foreach (var autoRotation in autoRotations)
			autoRotation.enabled = false;
	}

	protected override Vector3 Dimensions() { return new Vector3(0.55f, 0.88f, 2.13f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		explosionsLeft += 2;
		for (var i = 0; i < 2; ++i)
			(Instantiate(Resources.Load("Bomb"), torpedos[i].position, torpedos[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetPosition);
		isAiming = false;
		while (explosionsLeft > 0)
			yield return null;
		--Data.Replay.AttacksLeft;
	}

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase)
	{
		explosionsLeft += 2;
		for (var i = 0; i < 2; ++i)
			(Instantiate(Resources.Load("Bomb"), torpedos[i].position, torpedos[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(this, targetUnitBase, BombManager.Level.Medium);
		isAiming = false;
		while (explosionsLeft > 0)
			yield return null;
		--Data.Replay.AttacksLeft;
	}

	protected override int Kind() { return 4; }

	protected override int Level() { return 0; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Submarine")) as GameObject).GetComponent<RectTransform>(); }

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

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 1; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override void Start()
	{
		base.Start();
		foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
			meshRenderer.material = materials[0][team];
		highlighter.FlashingOn();
	}
}