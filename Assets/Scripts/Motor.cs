#region

using UnityEngine;

#endregion

public class Motor : MonoBehaviour
{
	private float deltaAngle;
	private Quaternion lastLookRotation;
	public float rotationTransitionTime = 1;
	public float speed = 1;
	private float startTime;
	private Quaternion targetLookRotation;
	public Vector3 Destination { get; set; }
	public Vector3 Position { get { return Methods.Coordinates.InternalToExternal(transform.position); } set { transform.position = Methods.Coordinates.ExternalToInternal(value); } }

	private void Awake()
	{
		rigidbody.drag = 0.1f;
		targetLookRotation = lastLookRotation = rigidbody.rotation;
	}

	private void Update()
	{
		rigidbody.AddForce(-Physics.gravity, ForceMode.Acceleration);
		var target = Methods.Coordinates.ExternalToInternal(Destination);
		var newLookRotation = rigidbody.velocity.magnitude < Vector3.kEpsilon ? targetLookRotation : Quaternion.LookRotation(rigidbody.velocity);
		if ((Quaternion.Angle(newLookRotation, targetLookRotation)) > Mathf.Epsilon)
		{
			startTime = Time.fixedTime;
			lastLookRotation = rigidbody.rotation;
			targetLookRotation = newLookRotation;
			deltaAngle = Quaternion.Angle(lastLookRotation, targetLookRotation);
		}
		var angleToRotate = Quaternion.Angle(rigidbody.rotation, targetLookRotation);
		if (angleToRotate > Time.fixedDeltaTime / rotationTransitionTime * deltaAngle)
			rigidbody.rotation = Quaternion.Slerp(lastLookRotation, targetLookRotation, (Time.fixedTime - startTime) / rotationTransitionTime);
		else
			lastLookRotation = rigidbody.rotation = targetLookRotation;
		if (rigidbody.position != target && angleToRotate < Mathf.Epsilon)
			rigidbody.AddForce((target - rigidbody.position), ForceMode.Acceleration);
		/*rigidbody.velocity = speed * (targetHeight - rigidbody.position).normalized * Settings.Map.ScaleFactor;
			if ((rigidbody.position - targetHeight).magnitude <= rigidbody.velocity.magnitude * Time.fixedDeltaTime)
			{
				rigidbody.MovePosition(targetHeight);
				rigidbody.velocity = Vector3.zero;
				rigidbody.angularVelocity = Vector3.zero;
			}*/
	}
}