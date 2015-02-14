#region

using UnityEngine;

#endregion

public class AutoRotation : MonoBehaviour, IElementFX
{
	public bool enableInParentSpace;
	public bool enableInSelfSpace;
	public Vector3 omega;
	public Vector3 rotationOffsetInParentSpace;
	public Vector3 rotationOffsetInSelfSpace;
	public Vector3 translationOffsetInParentSpace;
	public Vector3 translationOffsetInSelfSpace;

	public void Disable() { enabled = false; }

	private void Update()
	{
		if (enableInSelfSpace)
		{
			var pivot = transform.TransformPoint(translationOffsetInSelfSpace);
			var rotationOffest = Quaternion.Euler(rotationOffsetInSelfSpace);
			transform.RotateAround(pivot, transform.TransformDirection(rotationOffest * Vector3.forward), omega.z * Time.smoothDeltaTime);
			transform.RotateAround(pivot, transform.TransformDirection(rotationOffest * Vector3.left), omega.x * Time.smoothDeltaTime);
			transform.RotateAround(pivot, transform.TransformDirection(rotationOffest * Vector3.up), omega.y * Time.smoothDeltaTime);
		}
		if (enableInParentSpace)
		{
			var pivot = transform.parent.TransformPoint(translationOffsetInParentSpace);
			var rotationOffest = Quaternion.Euler(rotationOffsetInParentSpace);
			transform.RotateAround(pivot, transform.parent.TransformDirection(rotationOffest * Vector3.forward), omega.z * Time.smoothDeltaTime);
			transform.RotateAround(pivot, transform.parent.TransformDirection(rotationOffest * Vector3.left), omega.x * Time.smoothDeltaTime);
			transform.RotateAround(pivot, transform.parent.TransformDirection(rotationOffest * Vector3.up), omega.y * Time.smoothDeltaTime);
		}
	}
}