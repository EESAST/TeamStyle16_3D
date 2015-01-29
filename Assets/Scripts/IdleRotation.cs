#region

using UnityEngine;

#endregion

public class IdleRotation : MonoBehaviour, IEntityFX
{
	//Currently implements alpha rotation in self space combined with beta rotation in parent sapce. Additionally, the pivots in both spaces should be the same, thus are only calculated once using transform.TransformPoint(translationOffsetInSelfSpace).
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
	public Vector3 rotationOffsetInParentSpace;
	public Vector3 rotationOffsetInSelfSpace;
	public float rotationSpeed;
	private float transitionTime;
	public Vector3 translationOffsetInSelfSpace;

	public void Disable() { enabled = false; }

	private void Update()
	{
		if (!alphaRotationEnabled && !betaRotationEnabled)
			return;
		if (Time.time < lastTime + transitionTime)
		{
			var pivot = transform.TransformPoint(translationOffsetInSelfSpace);
			transform.RotateAround(pivot, transform.TransformDirection(Quaternion.Euler(rotationOffsetInSelfSpace) * Vector3.left), alphaSpeed * Time.deltaTime);
			transform.RotateAround(pivot, transform.parent.TransformDirection(Quaternion.Euler(rotationOffsetInParentSpace) * Vector3.up), betaSpeed * Time.deltaTime);
		}
		else if (Time.time > nextTime)
		{
			lastTime = Time.time;
			float deltaAlpha = 0, deltaBeta = 0;
			if (alphaRotationEnabled)
			{
				lastAlpha = nextAlpha;
				if (alphaMax - alphaMin < 360)
					nextAlpha = Random.Range(alphaMin, alphaMax);
				else
					nextAlpha = lastAlpha + Random.Range(-180f, 180f);
				deltaAlpha = nextAlpha - lastAlpha;
			}
			if (betaRotationEnabled)
			{
				lastBeta = nextBeta;
				if (betaMax - betaMin < 360)
					nextBeta = Random.Range(betaMin, betaMax);
				else
					nextBeta = lastBeta + Random.Range(-180f, 180);
				deltaBeta = nextBeta - lastBeta;
			}
			transitionTime = (Mathf.Abs(deltaAlpha) + Mathf.Abs(deltaBeta)) / rotationSpeed;
			nextTime = Time.time + transitionTime + Random.Range(1f, 4);
			alphaSpeed = deltaAlpha / transitionTime;
			betaSpeed = deltaBeta / transitionTime;
		}
	}
}