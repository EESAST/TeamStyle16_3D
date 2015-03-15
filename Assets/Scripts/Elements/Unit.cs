#region

using System;
using System.Collections;
using System.Linq;
using JSON;
using UnityEngine;

#endregion

public abstract class Unit : UnitBase
{
	private bool isActive;
	private bool isOrienting;
	private int movementNum;
	private Vector3 targetPosition;
	private float AngularSpeed { get { return Speed() * 30; } }

	protected virtual void Activate()
	{
		audio.Play();
		isActive = true;
	}

	protected IEnumerator AdjustOrientation(Vector3 targetOrientation)
	{
		if (targetOrientation == Vector3.zero)
			yield break;
		isOrienting = true;
		var ship = this as Ship;
		if (!ship)
			++movementNum;
		var targetRotation = Quaternion.LookRotation(targetOrientation);
		while (Quaternion.Angle(transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, AngularSpeed * Time.deltaTime), targetRotation) > Settings.AngularTolerance)
			yield return null;
		if (!ship)
			--movementNum;
		isOrienting = false;
	}

	protected override void Awake()
	{
		base.Awake();
		audio.clip = Resources.Load<AudioClip>("Sounds/" + Constants.TypeNames[Kind()] + "_Launching");
		audio.volume = Settings.Audio.Volume.Unit;
	}

	public IEnumerator Create(JSONObject info)
	{
		Initialize(info);
		StartCoroutine(ShowCreateFX());
		audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Create_" + team));
		yield return StartCoroutine(Data.Replay.Instance.ShowMessageAt(this, "Created!"));
		--Data.Replay.CreatesLeft;
	}

	protected virtual void Deactivate()
	{
		audio.Stop();
		isActive = false;
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
		if (plane)
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
				++movementNum;
				for (float t, startTime = Time.time; (t = (Time.time - startTime + Time.deltaTime) / time / 2) < 1 - Time.deltaTime / time / 4;)
				{
					targetPosition = o + Vector3.Slerp(-b, a, t);
					transform.rotation = Quaternion.Slerp(p, q, t);
					yield return null;
				}
				targetPosition = w;
				transform.rotation = q;
				--movementNum;
				i += 2;
				targetFuel -= 2;
			}
			else
			{
				++movementNum;
				for (float t, startTime = Time.time; (t = (Time.time - startTime + Time.deltaTime) / time) < 1 - Time.deltaTime / time / 2;)
				{
					targetPosition = Vector3.Lerp(u, v, t);
					yield return null;
				}
				targetPosition = v;
				--movementNum;
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
			++movementNum;
			for (float t, startTime = Time.time; (t = (Time.time - startTime + Time.deltaTime) / time) < 1 - Time.deltaTime / time / 2;)
			{
				targetPosition = Vector3.Lerp(u, v, t);
				yield return null;
			}
			targetPosition = v;
			--movementNum;
			--targetFuel;
		}
		--Data.Replay.MovesLeft;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Data.Replay.Populations[team] -= Population();
	}

	private int Population() { return Constants.Population[Kind()]; }

	private IEnumerator ShowCreateFX()
	{
		var radius = RelativeSize * Settings.DimensionScaleFactor / 2;
		var center = transform.TransformPoint(Center()) + Vector3.down * radius;
		var createFX = Instantiate(Resources.Load("CreateFX"), center + Vector3.right * radius, Quaternion.identity) as GameObject;
		var particleEmitters = createFX.GetComponentsInChildren<ParticleEmitter>();
		var maxEnergy = particleEmitters.Max(emitter => emitter.maxEnergy);
		for (float t, startTime = Time.time; (t = (Time.time - startTime) / (Settings.Replay.CreateTime - maxEnergy)) < 1;)
		{
			var theta = 3 * Mathf.PI * t;
			createFX.transform.position = center + radius * new Vector3(Mathf.Cos(theta), 2 * t, Mathf.Sin(theta));
			yield return null;
		}
		foreach (var emitter in particleEmitters)
			emitter.emit = false;
		Destroy(createFX, maxEnergy);
	}

	private int Speed() { return Constants.Speed[Kind()]; }

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
		if ((targetPosition - transform.position).magnitude > Settings.DimensionalTolerance)
			transform.position = Vector3.Lerp(transform.position, targetPosition, Settings.TransitionRate * Time.deltaTime);
		if (movementNum > 0 && !isActive)
			Activate();
		if (movementNum == 0 && isActive)
			Deactivate();
	}
}