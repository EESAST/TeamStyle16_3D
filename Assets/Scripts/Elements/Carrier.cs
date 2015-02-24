#region

using System.Collections;
using UnityEngine;

#endregion

public class Carrier : Ship
{
	private static readonly Material[][] materials = new Material[2][];
	private Interceptor[] interceptors;
	public int movingInterceptorsLeft;

	protected override IEnumerator AimAtPosition(Vector3 targetPosition)
	{
		foreach (var interceptor in interceptors)
			StartCoroutine(interceptor.AimAtPosition(targetPosition));
		while (movingInterceptorsLeft > 0)
			yield return null;
	}

	protected override int AmmoOnce() { return 2; }

	protected override void Awake()
	{
		base.Awake();
		interceptors = GetComponentsInChildren<Interceptor>();
	}

	public override Vector3 Center() { return new Vector3(-0.02f, 0.26f, 0.15f); }

	protected override Vector3 Dimensions() { return new Vector3(1.14f, 1.82f, 3.01f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition)
	{
		explosionsLeft += interceptors.Length * 2;
		foreach (var interceptor in interceptors)
		{
			interceptor.FireAtPosition(targetPosition);
			StartCoroutine(interceptor.Return());
		}
		while (explosionsLeft > 0)
			yield return null;
		StartCoroutine(MonitorInterceptorReturns());
	}

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase)
	{
		explosionsLeft += interceptors.Length * 2;
		foreach (var interceptor in interceptors)
		{
			interceptor.FireAtUnitBase(targetUnitBase);
			StartCoroutine(interceptor.Return());
		}
		while (explosionsLeft > 0)
			yield return null;
		StartCoroutine(MonitorInterceptorReturns());
	}

	protected override int Kind() { return 6; }

	public static void LoadMaterial()
	{
		string[] name = { "C", "I" };
		for (var id = 0; id < 2; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Carrier/Materials/" + name[id] + "_" + team);
		}
	}

	protected override int MaxHP() { return 120; }

	private IEnumerator MonitorInterceptorReturns()
	{
		while (movingInterceptorsLeft > 0)
			yield return null;
		--Data.Replay.AttacksLeft;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		foreach (var interceptor in interceptors)
			Destroy(interceptor.gameObject);
	}

	protected override int Population() { return 4; }

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 2; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override int Speed() { return 5; }

	protected override void Start()
	{
		base.Start();
		transform.Find("Hull").GetComponent<MeshRenderer>().material = materials[0][team];
		foreach (var interceptor in interceptors)
			interceptor.GetComponentInChildren<MeshRenderer>().material = materials[1][team];
	}
}