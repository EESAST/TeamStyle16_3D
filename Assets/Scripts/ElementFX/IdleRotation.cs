#region

using System.Collections;
using UnityEngine;

#endregion

public class IdleRotation : MonoBehaviour, IIdleFX
{
	private readonly float maxRestTime = 4;
	private readonly float minRestTime = 1;
	public bool alphaEnabled;
	public float alphaMax;
	public float alphaMin;
	public float angularSpeed;
	public bool betaEnabled;
	public float betaMax;
	public float betaMin;

	public void Disable() { enabled = false; }

	public void Enable() { enabled = true; }

	private IEnumerator Start()
	{
		while (true)
		{
			var targetRotation = Quaternion.Euler(alphaEnabled ? -Random.Range(alphaMin, alphaMax) : transform.localEulerAngles.x, betaEnabled ? Random.Range(betaMin, betaMax) : transform.localEulerAngles.y, transform.localEulerAngles.z);
			while (enabled && Quaternion.Angle(transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, angularSpeed * Time.smoothDeltaTime), targetRotation) > Settings.AngularTolerance)
				yield return null;
			yield return new WaitForSeconds(Random.Range(minRestTime, maxRestTime));
		}
	}
}