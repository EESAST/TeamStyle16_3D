#region

using UnityEngine;

#endregion

public class AutoRotation : MonoBehaviour, IEntityFX
{
	public float alphaSpeed;
	public float betaSpeed;
	public Vector3 rotationOffsetToLocal;
	public Vector3 rotationOffsetToParent;
	public Vector3 translationOffsetToLocal;

	public void Disable() { enabled = false; }

	private void Update()
	{
		var pivot = transform.TransformPoint(translationOffsetToLocal);
		transform.RotateAround(pivot, transform.TransformDirection(Quaternion.Euler(rotationOffsetToLocal) * Vector3.left), alphaSpeed * Time.deltaTime);
		transform.RotateAround(pivot, transform.parent.TransformDirection(Quaternion.Euler(rotationOffsetToParent) * Vector3.up), betaSpeed * Time.deltaTime);
	}
}