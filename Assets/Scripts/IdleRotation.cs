#region

using UnityEngine;

#endregion

public class IdleRotation : MonoBehaviour, IEntityFX
{
	public float alphaMax;
	public float alphaMin;
	public bool alphaRotationEnabled;
	private float alphaSpeed;
	public float betaMax;
	public float betaMin;
	public bool betaRotationEnabled;
	private float betaSpeed;
	private float lastAlpha;
	private float lastBeta;
	private float lastTime;
	private float nextAlpha;
	private float nextBeta;
	private float nextTime;
	public Vector3 rotationOffsetToLocal;
	public Vector3 rotationOffsetToParent;
	public float rotationSpeed;
	private float transitionTime;
	public Vector3 translationOffsetToLocal;

	public void Disable() { enabled = false; }

	private void Update()
	{
		if (!alphaRotationEnabled && !betaRotationEnabled)
			return;
		if (Time.time < lastTime + transitionTime)
		{
			var pivot = transform.TransformPoint(translationOffsetToLocal);
			transform.RotateAround(pivot, transform.TransformDirection(Quaternion.Euler(rotationOffsetToLocal) * Vector3.left), alphaSpeed * Time.deltaTime);
			transform.RotateAround(pivot, transform.parent.TransformDirection(Quaternion.Euler(rotationOffsetToParent) * Vector3.up), betaSpeed * Time.deltaTime);
		}
		else if (Time.time > nextTime)
		{
			lastTime = Time.time;
			float deltaAlpha = 0, deltaBeta = 0;
			if (alphaRotationEnabled)
			{
				lastAlpha = nextAlpha;
				if (alphaMax - alphaMin + Mathf.Epsilon < 360f)
					nextAlpha = Random.Range(alphaMin, alphaMax);
				else
					nextAlpha = lastAlpha + Random.Range(-180f, 180f);
				deltaAlpha = nextAlpha - lastAlpha;
			}
			if (betaRotationEnabled)
			{
				lastBeta = nextBeta;
				if (betaMax - betaMin + Mathf.Epsilon < 360f)
					nextBeta = Random.Range(betaMin, betaMax);
				else
					nextBeta = lastBeta + Random.Range(-180f, 180f);
				deltaBeta = nextBeta - lastBeta;
			}
			transitionTime = (Mathf.Abs(deltaAlpha) + Mathf.Abs(deltaBeta)) / rotationSpeed;
			nextTime = Time.time + transitionTime + Random.Range(1f, 4);
			alphaSpeed = deltaAlpha / transitionTime;
			betaSpeed = deltaBeta / transitionTime;
		}
	}
}