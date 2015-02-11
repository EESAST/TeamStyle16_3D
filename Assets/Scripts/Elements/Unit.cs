#region

using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

public abstract class Unit : UnitBase
{
	private IEnumerator AdjustOrientation(Vector3 dir)
	{
		//dir.y = 0;
		var p = transform.rotation;
		var q = Quaternion.LookRotation(dir);
		var angle = Quaternion.Angle(p, q);

		if (angle < 30)
			yield break;

		const float AngularSpeed = 120; // 120 degrees per sec

		var Interpolations = 1 + (int)angle / 3;
		var dt = 1.0f / Interpolations;

		var dtime = angle / AngularSpeed / Interpolations;
		for (float t = 0; t <= 1; t += dt)
		{
			transform.rotation = Quaternion.Slerp(p, q, t);
			yield return new WaitForSeconds(dtime);
		}
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
		//transform.position = Methods.Coordinates.JSONToInternal(nodes.list[nodes.Count - 1]);
		//TODO:animate movement according to nodes
		/*foreach(var p in nodes.list) {
			transform.position = Methods.Coordinates.JSONToInternal(p);
			yield return new WaitForSeconds(0.1f);
		}*/

		// use kinematics?
		//rigidbody.velocity = ;

		const int InterpolationsPerUnitMove = 10;
		const float TimerPerUnitMove = 0.2f; // time in seconds to go across a block

		const float dt = 1.0f / InterpolationsPerUnitMove;
		const float dtime = TimerPerUnitMove / InterpolationsPerUnitMove;
		int i;
		for (i = 0; i < nodes.list.Count - 2;)
		{
			var u = Methods.Coordinates.JSONToInternal(nodes.list[i]);
			var v = Methods.Coordinates.JSONToInternal(nodes.list[i + 1]);
			var w = Methods.Coordinates.JSONToInternal(nodes.list[i + 2]);
			Vector3 a = v - u, b = w - v;
			// wait for orientation adjusted
			yield return StartCoroutine(AdjustOrientation(a));
			if ((a - b).magnitude < 1)
			{
				for (float t = 0; t < 1; t += dt)
				{
					transform.position = //Methods.Coordinates.ExternalToInternal(
						Vector3.Lerp(u, v, t); //);
					yield return new WaitForSeconds(dtime);
				}
				i += 1;
			}
			else
			{
				var p = transform.rotation;
				var q = Quaternion.LookRotation(b);
				var o = u + b; // origin of the circle
				b = -b;
				for (float t = 0; t < 1; t += dt / 2)
				{
					transform.position = o + Vector3.Slerp(b, a, t);
					transform.rotation = Quaternion.Slerp(p, q, t);
					yield return new WaitForSeconds(dtime);
				}
				i += 2;
			}
		}
		if (i == nodes.list.Count - 2)
		{
			var u = Methods.Coordinates.JSONToInternal(nodes.list[i]);
			var v = Methods.Coordinates.JSONToInternal(nodes.list[i + 1]);
			for (float t = 0; t < 1; t += dt)
			{
				transform.position = Vector3.Lerp(u, v, t);
				yield return new WaitForSeconds(dtime);
			}
		}
		transform.position = Methods.Coordinates.JSONToInternal(nodes.list[nodes.Count - 1]);
		--Data.Replay.MovesLeft;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Data.Replay.Populations[team] -= Population();
	}

	protected abstract int Population();
}