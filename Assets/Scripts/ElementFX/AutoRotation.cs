#region

using UnityEngine;

#endregion

public class AutoRotation : MonoBehaviour, IElementFX
{
	public bool enableInParentSpace;
	public bool enableInSelfSpace;
	public Vector3 omega;

	public void Disable() { enabled = false; }

	private void Update()
	{
		if (enableInSelfSpace)
		{
			transform.Rotate(Vector3.forward, omega.z * Time.deltaTime);
			transform.Rotate(Vector3.left, omega.x * Time.deltaTime);
			transform.Rotate(Vector3.up, omega.y * Time.deltaTime);
		}
		if (enableInParentSpace)
		{
			transform.Rotate(transform.parent.forward, omega.z * Time.deltaTime, Space.World);
			transform.Rotate(-transform.parent.right, omega.x * Time.deltaTime, Space.World);
			transform.Rotate(transform.parent.up, omega.y * Time.deltaTime, Space.World);
		}
	}
}