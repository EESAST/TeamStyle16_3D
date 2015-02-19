#region

using System.Collections;
using UnityEngine;

#endregion

public abstract class Plane : Unit
{
	public bool isFalling;
	public bool isHovering = true;

	protected override IEnumerator AimAtPosition(Vector3 targetPosition) { yield return StartCoroutine(AdjustOrientation(Vector3.Scale(targetPosition - transform.position, new Vector3(1, 0, 1)))); }

	private IEnumerator Fall()
	{
		isFalling = true;
		foreach (IIdleFX idleFX in GetComponentsInChildren(typeof(IIdleFX)))
			idleFX.Disable();
		rigidbody.isKinematic = false;
		rigidbody.WakeUp();
		while (--targetHP > 0)
			yield return new WaitForSeconds(Settings.DeltaTime);
	}

	protected override int Level() { return 3; }

	protected override void LoadMark() { markRect = (Instantiate(Resources.Load("Marks/Plane")) as GameObject).GetComponent<RectTransform>(); }

	private void OnCollisionEnter()
	{
		if (isFalling)
			targetHP -= 20;
	}

	protected override void Update()
	{
		base.Update();
		if (Mathf.RoundToInt(currentFuel) <= 0 && !isFalling)
			StartCoroutine(Fall());
	}
}