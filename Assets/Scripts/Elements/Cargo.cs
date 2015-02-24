#region

using System;
using System.Collections;
using UnityEngine;

#endregion

public class Cargo : Ship
{
	private static readonly Material[][] materials = new Material[1][];

	protected override IEnumerator AimAtPosition(Vector3 targetPosition) { yield return StartCoroutine(AdjustOrientation(Vector3.Scale(targetPosition - transform.position, new Vector3(1, 0, 1)))); }

	protected override int AmmoOnce() { throw new NotImplementedException(); }

	public override Vector3 Center() { return new Vector3(0.00f, 0.03f, 0.09f); }

	public IEnumerator Collect(Resource target, int fuel, int metal)
	{
		yield return AimAtPosition(target.transform.WorldCenterOfElement());
		var elapsedTime = Mathf.Max((fuel + metal) / Settings.Replay.CollectRate, 0.1f);
		target.StartCoroutine(target.Beam(this, elapsedTime, BeamType.Collect));
		yield return new WaitForSeconds((transform.TransformPoint(Center()) - target.transform.WorldCenterOfElement()).magnitude / Settings.Replay.BeamSpeed);
		var effectedFuel = 0;
		var effectedMetal = 0;
		for (float t, startTime = Time.time; (t = (Time.time - startTime) / elapsedTime) < 1;)
		{
			var deltaFuel = Mathf.RoundToInt(fuel * t - effectedFuel);
			if (deltaFuel > 0)
			{
				targetFuel += deltaFuel;
				target.targetFuel -= deltaFuel;
				effectedFuel += deltaFuel;
			}
			var deltaMetal = Mathf.RoundToInt(metal * t - effectedMetal);
			if (deltaMetal > 0)
			{
				targetMetal += deltaMetal;
				target.targetMetal -= deltaMetal;
				effectedMetal += deltaMetal;
			}
			Data.Replay.TargetScores[team] += Constants.Score.PerCollectedResource * (deltaFuel + deltaMetal);
			yield return null;
		}
		targetFuel += fuel - effectedFuel;
		target.targetFuel -= fuel - effectedFuel;
		targetMetal += metal - effectedMetal;
		target.targetMetal -= metal - effectedMetal;
		Data.Replay.TargetScores[team] += Constants.Score.PerCollectedResource * (fuel - effectedFuel + metal - effectedMetal);
		string message;
		if (metal == 0)
			message = fuel > 0 ? "F: +" + fuel + "!" : "0";
		else
			message = (fuel > 0 ? "F: +" + fuel + "! " : "") + "M: +" + metal + "!";
		yield return StartCoroutine(Data.Replay.Instance.ShowMessageAt(this, message));
		--Data.Replay.CollectsLeft;
	}

	protected override Vector3 Dimensions() { return new Vector3(0.72f, 0.38f, 1.17f); }

	protected override IEnumerator FireAtPosition(Vector3 targetPosition) { throw new NotImplementedException(); }

	protected override IEnumerator FireAtUnitBase(UnitBase targetUnitBase) { throw new NotImplementedException(); }

	protected override int Kind() { return 7; }

	public static void LoadMaterial()
	{
		string[] name = { "C" };
		for (var id = 0; id < 1; id++)
		{
			materials[id] = new Material[3];
			for (var team = 0; team < 3; team++)
				materials[id][team] = Resources.Load<Material>("Cargo/Materials/" + name[id] + "_" + team);
		}
	}

	protected override int MaxHP() { return 60; }

	protected override int Population() { return 1; }

	public static void RefreshMaterialColor()
	{
		for (var id = 0; id < 1; id++)
			for (var team = 0; team < 3; team++)
				materials[id][team].SetColor("_Color", Data.TeamColor.Current[team]);
	}

	protected override int Speed() { return 8; }

	protected override void Start()
	{
		base.Start();
		foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
			meshRenderer.material = materials[0][team];
	}
}