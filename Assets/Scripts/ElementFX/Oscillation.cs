#region

using UnityEngine;

#endregion

public class Oscillation : MonoBehaviour, IElementFX
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
	public Vector3 rotationOffsetInParentSpace;
	public Vector3 rotationOffsetInSelfSpace;
	private float spawnTime;

	public void Disable() { enabled = false; }

	private void Awake()
	{
		amplitude = Random.Range(minAmplitude, maxAmplitude);
		angularAmplitude = Random.Range(minAngularAmplitude, maxAngularAmplitude);
		angularOmega = Random.Range(minAngularOmega, maxAngularOmega);
		omega = Random.Range(minOmega, maxOmega);
		spawnTime = Time.time;
	}

	private void Update()
	{
		transform.Translate((transform.parent ? transform.parent.TransformDirection(Quaternion.Euler(rotationOffsetInParentSpace) * Vector3.up) : Vector3.up) * omega * amplitude * Settings.Map.ScaleFactor * Mathf.Cos(omega * (Time.time - spawnTime)) * Time.smoothDeltaTime, Space.World);
		transform.Rotate(Quaternion.Euler(rotationOffsetInSelfSpace) * Vector3.forward, angularOmega * angularAmplitude * Mathf.Cos(angularOmega * (Time.time - spawnTime)) * Time.smoothDeltaTime);
	}
}