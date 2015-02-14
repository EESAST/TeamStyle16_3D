#region

using System;
using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public abstract class Unit : UnitBase
{
	private bool isOrienting;
	private Vector3 targetPos;
	private Quaternion targetRot;

	private IEnumerator AdjustOrientation(Vector3 targetOrientation)
	{
		isOrienting = true;
		var p = transform.rotation;
		var q = Quaternion.LookRotation(targetOrientation);
		var time = Quaternion.Angle(p, q) / Speed() / 30;
		for (float t, startTime = Time.time; (t = (Time.time - startTime + Time.smoothDeltaTime) / time) < 1 - Time.smoothDeltaTime / time / 2;)
		{
			targetRot = Quaternion.Slerp(p, q, t);
			yield return null;
		}
		targetRot = q;
		isOrienting = false;
	}

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		Data.Replay.Populations[team] += Population();
		foreach (var productionEntry in Data.Replay.ProductionLists[team].Where(productionEntry => productionEntry.kind == Kind()))
		{
			productionEntry.StartCoroutine(productionEntry.Done());
			break;
		}
	}

	public IEnumerator Move(JSONObject nodes)
	{
		int i;
		var internalNodes = new Vector3[nodes.Count];
		for (i = 0; i < nodes.Count; ++i)
			internalNodes[i] = Methods.Coordinates.JSONToInternal(nodes[i]);
		var time = 2.5f / Speed();
		for (i = 0; i < nodes.Count - 2;)
		{
			var u = internalNodes[i];
			var v = internalNodes[i + 1];
			var w = internalNodes[i + 2];
			var a = v - u;
			var b = w - v;
			StartCoroutine(AdjustOrientation(a));
			while (isOrienting)
				yield return null;
			if (Math.Abs(Vector3.Dot(a, b)) < Settings.Tolerance)
			{
				var p = transform.rotation;
				var q = Quaternion.LookRotation(b);
				var o = u + b;
				for (float t, startTime = Time.time; (t = (Time.time - startTime + Time.smoothDeltaTime) / time / 2) < 1 - Time.smoothDeltaTime / time / 4;)
				{
					targetPos = o + Vector3.Slerp(-b, a, t);
					targetRot = Quaternion.Slerp(p, q, t);
					yield return null;
				}
				targetPos = w;
				targetRot = q;
				i += 2;
				targetFuel -= 2;
			}
			else
			{
				for (float t, startTime = Time.time; (t = (Time.time - startTime + Time.smoothDeltaTime) / time) < 1 - Time.smoothDeltaTime / time / 2;)
				{
					targetPos = Vector3.Lerp(u, v, t);
					yield return null;
				}
				targetPos = v;
				++i;
				--targetFuel;
			}
		}
		if (i == nodes.Count - 2)
		{
			var u = internalNodes[i];
			var v = internalNodes[i + 1];
			StartCoroutine(AdjustOrientation(v - u));
			while (isOrienting)
				yield return null;
			for (float t, startTime = Time.time; (t = (Time.time - startTime + Time.smoothDeltaTime) / time) < 1 - Time.smoothDeltaTime / time / 2;)
			{
				targetPos = Vector3.Lerp(u, v, t);
				yield return null;
			}
			targetPos = v;
			--targetFuel;
		}
		--Data.Replay.MovesLeft;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Data.Replay.Populations[team] -= Population();
	}

	protected abstract int Population();

	protected abstract int Speed();

	protected override void Start()
	{
		base.Start();
		targetPos = transform.position;
		targetRot = transform.rotation;
	}

	protected override void Update()
	{
		base.Update();
		if ((targetPos - transform.position).magnitude > Settings.Tolerance)
			transform.position = Vector3.Lerp(transform.position, targetPos, Settings.TransitionRate * Time.smoothDeltaTime);
		if (Quaternion.Angle(transform.rotation, targetRot) > Settings.Tolerance)
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Settings.TransitionRate * Time.smoothDeltaTime);
	}
}