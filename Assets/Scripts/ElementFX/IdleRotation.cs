#region

using UnityEngine;

#endregion

public class IdleRotation : MonoBehaviour, IIdleFX
{
	public bool alphaEnabled;
	public float alphaMax;
	public float alphaMin;
	private float alphaSpeed;
	public float angularSpeed;
	public bool betaEnabled;
	public float betaMax;
	public float betaMin;
	private float betaSpeed;
	private bool interrupted;
	private float lastAlpha;
	private float lastBeta;
	private float lastTime;
	private float nextAlpha;
	private float nextBeta;
	private float nextTime;
	private float transitionTime;

	public void Disable() { enabled = false; }

	public void Enable()
	{
		RefreshAngle();
		enabled = true;
	}

	private void RefreshAngle()
	{
		interrupted = true;
		nextAlpha = transform.localEulerAngles.x;
		if (nextAlpha > 180)
			nextAlpha -= 360;
		nextBeta = transform.localEulerAngles.y;
		if (nextBeta > 180)
			nextBeta -= 360;
	}

	private void Start() { RefreshAngle(); }

	private void Update()
	{
		if (!alphaEnabled && !betaEnabled)
			return;
		if (!interrupted)
		{
			if (Time.time < lastTime + transitionTime)
			{
				transform.Rotate(Vector3.right, alphaSpeed * Time.deltaTime);
				transform.Rotate(transform.parent.up, betaSpeed * Time.deltaTime, Space.World);
			}
			if (Time.time < nextTime)
				return;
		}
		interrupted = false;
		lastTime = Time.time;
		float deltaAlpha = 0, deltaBeta = 0;
		if (alphaEnabled)
		{
			lastAlpha = nextAlpha;
			if (alphaMax - alphaMin < 360f)
				nextAlpha = Random.Range(alphaMin, alphaMax);
			else
				nextAlpha = lastAlpha + Random.Range(-180f, 180f);
			deltaAlpha = nextAlpha - lastAlpha;
		}
		if (betaEnabled)
		{
			lastBeta = nextBeta;
			if (betaMax - betaMin < 360f)
				nextBeta = Random.Range(betaMin, betaMax);
			else
				nextBeta = lastBeta + Random.Range(-180f, 180f);
			deltaBeta = nextBeta - lastBeta;
		}
		transitionTime = (Mathf.Abs(deltaAlpha) + Mathf.Abs(deltaBeta)) / angularSpeed;
		nextTime = Time.time + transitionTime + Random.Range(Settings.IdleRotation.MinRestTime, Settings.IdleRotation.MaxRestTime);
		alphaSpeed = deltaAlpha / transitionTime;
		betaSpeed = deltaBeta / transitionTime;
	}
}