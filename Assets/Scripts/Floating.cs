#region

using UnityEngine;

#endregion

public class Floating : MonoBehaviour, IEntityFX
{
	public float amplitude = 1;
	public float omega = Mathf.PI;
	private Vector3 origin;

	public void Disable() { enabled = false; }

	private void Start() { origin = transform.position; }

	private void Update() { transform.position = origin + amplitude * Mathf.Sin(omega * Time.time) * Vector3.up; }
}