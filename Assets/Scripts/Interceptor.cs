#region

using System.Collections;
using UnityEngine;

#endregion

public class Interceptor : MonoBehaviour
{
	private Transform[] missiles;
	private Carrier owner;
	private Transform seat;
	private ParticleSystem[] trails;

	private IEnumerator AdjustOrientation(Vector3 targetOrientation)
	{
		var targetRotation = Quaternion.LookRotation(targetOrientation);
		while (Quaternion.Angle(transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Settings.Interceptor.AngularCorrectionRate * Time.smoothDeltaTime), targetRotation) > Settings.AngularTolerance)
			yield return null;
	}

	public IEnumerator AimAtPosition(Vector3 targetPosition)
	{
		++owner.movingInterceptorsLeft;
		var localTarget = Vector3.forward * Settings.DimensionScaleFactor / 3;
		foreach (var trail in trails)
			trail.Play();
		audio.Play();
		while (Vector3.Distance(transform.localPosition = Vector3.MoveTowards(transform.localPosition, localTarget, Settings.Interceptor.Speed * Time.smoothDeltaTime), localTarget) > Settings.DimensionalTolerance)
			yield return null;
		transform.parent.DetachChildren();
		var target = targetPosition + ((transform.position + Vector3.up * (Settings.Map.HeightOfLevel[3] - Settings.Map.HeightOfLevel[1]) * 0.8f - targetPosition).normalized * 2.5f + Random.insideUnitSphere) * Settings.DimensionScaleFactor;
		while ((target - transform.position).magnitude > Settings.DimensionalTolerancePerUnitSpeed * Settings.Interceptor.Speed)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target - transform.position), Time.smoothDeltaTime * Settings.Interceptor.AngularCorrectionRate);
			transform.Translate(Vector3.forward * Settings.Interceptor.Speed * Time.smoothDeltaTime);
			yield return null;
		}
		yield return StartCoroutine(AdjustOrientation(targetPosition - transform.position));
		--owner.movingInterceptorsLeft;
	}

	private void Awake()
	{
		owner = transform.GetComponentInParent<Carrier>();
		seat = transform.parent;
		missiles = new[] { transform.Find("Airframe/LSP"), transform.Find("Airframe/RSP") };
		trails = GetComponentsInChildren<ParticleSystem>();
		gameObject.AddComponent<AudioSource>();
		audio.clip = Resources.Load<AudioClip>("Sounds/Interceptor_Launching");
		audio.dopplerLevel = 0;
		audio.maxDistance = Settings.Audio.MaxAudioDistance;
		audio.rolloffMode = AudioRolloffMode.Linear;
		audio.volume = Settings.Audio.Volume.Unit;
	}

	public void FireAtPosition(Vector3 targetPosition)
	{
		for (var i = 0; i < 2; ++i)
			(Instantiate(Resources.Load("Bomb"), missiles[i].position, missiles[i].rotation) as GameObject).GetComponent<BombManager>().Initialize(owner, targetPosition, BombManager.Level.Small);
	}

	public void FireAtUnitBase(UnitBase targetUnitBase)
	{
		for (var i = 0; i < 2; ++i)
			(Instantiate(Resources.Load("Bomb"), missiles[i].position, missiles[i].rotation) as GameObject).GetComponent<BombManager>().Setup(owner, targetUnitBase, BombManager.Level.Small);
	}

	public IEnumerator Return()
	{
		++owner.movingInterceptorsLeft;
		audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Interceptor_Returning"));
		while ((seat.position - transform.position).magnitude > Settings.DimensionalTolerancePerUnitSpeed * Settings.Interceptor.Speed)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(seat.position - transform.position), Time.smoothDeltaTime * Settings.Interceptor.AngularCorrectionRate);
			transform.Translate(Vector3.forward * Settings.Interceptor.Speed * Time.smoothDeltaTime);
			yield return null;
		}
		transform.parent = seat;
		foreach (var trail in trails)
			trail.Stop();
		while (Quaternion.Angle(transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.identity, Settings.Interceptor.AngularCorrectionRate * Time.smoothDeltaTime), Quaternion.identity) > Settings.AngularTolerance || Vector3.Distance(transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Settings.Interceptor.Speed * Time.smoothDeltaTime), Vector3.zero) > Settings.DimensionalTolerance)
			yield return null;
		--owner.movingInterceptorsLeft;
	}
}