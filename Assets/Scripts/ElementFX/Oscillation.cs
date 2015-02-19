#region

using UnityEngine;

#endregion

public class Oscillation : MonoBehaviour, IIdleFX
{
	private float amplitude;
	private float angularAmplitude;
	private float angularOmega;
	public float maxAmplitude;
	public float maxAngularAmplitude;
	public float maxAngularOmega;
	public float maxOmega;
	public float minAmplitude;
	public float minAngularAmplitude;
	public float minAngularOmega;
	public float minOmega;
	private float omega;
	private Vector3 originalEulerAngles;
	private Vector3 originalPosition;
	private float spawnTime;
	private float stopTime;

	public void Disable()
	{
		stopTime = Time.time;
		enabled = false;
	}

	public void Enable()
	{
		spawnTime += Time.time - stopTime;
		enabled = true;
	}

	private void Awake()
	{
		amplitude = Random.Range(minAmplitude, maxAmplitude);
		angularAmplitude = Random.Range(minAngularAmplitude, maxAngularAmplitude);
		angularOmega = Random.Range(minAngularOmega, maxAngularOmega);
		omega = Random.Range(minOmega, maxOmega);
		spawnTime = Time.time;
		originalEulerAngles = transform.localEulerAngles;
		originalPosition = transform.localPosition;
	}

	private void Update()
	{
		if (Mathf.Abs(angularAmplitude) > Mathf.Epsilon)
			transform.localEulerAngles = originalEulerAngles + Vector3.forward * angularAmplitude * Mathf.Sin(angularOmega * (Time.time - spawnTime));
		if (Mathf.Abs(amplitude) > Mathf.Epsilon)
			transform.localPosition = originalPosition + Vector3.up * amplitude * Mathf.Sin(omega * (Time.time - spawnTime));
	}
}