#region

using System.Collections;
using UnityEngine;

#endregion

public class Interceptor : MonoBehaviour
{
	private readonly float _speed = 1;
	private readonly float angularCorrectionRate = 180;
	private Transform[] missiles;
	private Carrier owner;
	private Transform seat;
	private float Speed { get { return _speed * Settings.DimensionScaleFactor; } }

	private IEnumerator AdjustOrientation(Vector3 targetOrientation)
	{
		var targetRotation = Quaternion.LookRotation(targetOrientation);
		while (Quaternion.Angle(transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angularCorrectionRate * Time.smoothDeltaTime), targetRotation) > Settings.AngularTolerance)
			yield return null;
	}

	public IEnumerator AimAtPosition(Vector3 targetPosition)
	{
		++owner.movingInterceptorsLeft;
		var localTarget = Vector3.forward * Settings.DimensionScaleFactor / 3;
		while (Vector3.Distance(transform.localPosition = Vector3.MoveTowards(transform.localPosition, localTarget, Speed * Time.smoothDeltaTime), localTarget) > Settings.DimensionalTolerance)
			yield return null;
		transform.parent.DetachChildren();
		var target = targetPosition + ((transform.position + Vector3.up * (Settings.Map.HeightOfLevel[3] - Settings.Map.HeightOfLevel[1]) * 0.8f - targetPosition).normalized * 2.5f + Random.insideUnitSphere) * Settings.DimensionScaleFactor;
		while ((target - transform.position).magnitude > Settings.DimensionalTolerancePerUnitSpeed * Speed)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target - transform.position), Time.smoothDeltaTime * angularCorrectionRate);
			transform.Translate(Vector3.forward * Speed * Time.smoothDeltaTime);
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
	}

	public void FireAtPosition(Vector3 targetPosition)
	{
		for (var i = 0; i < 2; ++i)
			(Instantiate(Resources.Load("Bomb"), missiles[i].position, missiles[i].rotation) as GameObject).GetComponent<BombManager>().Setup(owner, targetPosition, BombManager.BombLevel.Small);
	}

	public void FireAtUnitBase(UnitBase targetUnitBase)
	{
		for (var i = 0; i < 2; ++i)
			(Instantiate(Resources.Load("Bomb"), missiles[i].position, missiles[i].rotation) as GameObject).GetComponent<BombManager>().Setup(owner, targetUnitBase, BombManager.BombLevel.Small);
	}

	public IEnumerator Return()
	{
		++owner.movingInterceptorsLeft;
		while ((seat.position - transform.position).magnitude > Settings.DimensionalTolerancePerUnitSpeed * Speed)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(seat.position - transform.position), Time.smoothDeltaTime * angularCorrectionRate);
			transform.Translate(Vector3.forward * Speed * Time.smoothDeltaTime);
			yield return null;
		}
		transform.parent = seat;
		while (Quaternion.Angle(transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.identity, angularCorrectionRate * Time.smoothDeltaTime), Quaternion.identity) > Settings.AngularTolerance || Vector3.Distance(transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Speed * Time.smoothDeltaTime), Vector3.zero) > Settings.DimensionalTolerance)
			yield return null;
		--owner.movingInterceptorsLeft;
	}
}