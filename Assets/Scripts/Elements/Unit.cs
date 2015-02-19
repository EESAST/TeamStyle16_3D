#region

using System;
using System.Collections;
using System.Linq;
using JSON;
using UnityEngine;

#endregion

public abstract class Unit : UnitBase
{
	private bool isOrienting;
	private Vector3 targetPosition;
	private float AngularSpeed { get { return Speed() * 30; } }

	protected IEnumerator AdjustOrientation(Vector3 targetOrientation)
	{
		isOrienting = true;
		var targetRotation = Quaternion.LookRotation(targetOrientation);
		while (Quaternion.Angle(transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, AngularSpeed * Time.smoothDeltaTime), targetRotation) > Settings.AngularTolerance)
			yield return null;
		isOrienting = false;
	}

	public IEnumerator Create(JSONObject info)
	{
		Initialize(info);
		StartCoroutine(ShowCreateFX());
		yield return new WaitForSeconds(Settings.CreateTime * 0.6f);
		yield return StartCoroutine(Replayer.ShowMessageAt(transform.WorldCenterOfElement() + Vector3.up * RelativeSize * Settings.Map.ScaleFactor / 2, "Created!", Settings.CreateTime * 0.4f));
		--Data.Replay.CreatesLeft;
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
		var plane = this as Plane;
		if (plane != null)
			plane.isHovering = false;
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
					targetPosition = o + Vector3.Slerp(-b, a, t);
					transform.rotation = Quaternion.Slerp(p, q, t);
					yield return null;
				}
				targetPosition = w;
				transform.rotation = q;
				i += 2;
				targetFuel -= 2;
			}
			else
			{
				for (float t, startTime = Time.time; (t = (Time.time - startTime + Time.smoothDeltaTime) / time) < 1 - Time.smoothDeltaTime / time / 2;)
				{
					targetPosition = Vector3.Lerp(u, v, t);
					yield return null;
				}
				targetPosition = v;
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
				targetPosition = Vector3.Lerp(u, v, t);
				yield return null;
			}
			targetPosition = v;
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

	private IEnumerator ShowCreateFX()
	{
		var radius = RelativeSize * Settings.Map.ScaleFactor / 2;
		var center = transform.TransformPoint(Center()) + Vector3.down * radius;
		var createFX = Instantiate(Resources.Load("CreateFX"), center + Vector3.right * radius, Quaternion.identity) as GameObject;
		var particleEmitters = createFX.GetComponentsInChildren<ParticleEmitter>();
		var maxEnergy = particleEmitters.Max(emitter => emitter.maxEnergy);
		for (float t, startTime = Time.time; (t = (Time.time - startTime) / (Settings.CreateTime - maxEnergy)) < 1;)
		{
			var theta = 3 * Mathf.PI * t;
			createFX.transform.position = center + radius * new Vector3(Mathf.Cos(theta), 2 * t, Mathf.Sin(theta));
			yield return null;
		}
		foreach (var emitter in particleEmitters)
			emitter.emit = false;
		Destroy(createFX, maxEnergy);
		--Data.Replay.CreatesLeft;
	}

	protected abstract int Speed();

	protected override void Start()
	{
		base.Start();
		targetPosition = transform.position;
	}

	protected override void Update()
	{
		base.Update();
		var plane = this as Plane;
		if (plane && plane.isFalling)
			return;
		if ((targetPosition - transform.position).magnitude > Settings.Tolerance)
			transform.position = Vector3.Lerp(transform.position, targetPosition, Settings.TransitionRate * Time.smoothDeltaTime);
	}
}