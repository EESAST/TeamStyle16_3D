#region

using System.Collections;
using UnityEngine;

#endregion

public class BombManager : MonoBehaviour
{
	public enum BombLevel
	{
		Small,
		Medium,
		Large
	}

	private readonly float angularCorrectionRate = 360;
	private readonly float noise = 1;
	private readonly float speed = 4;
	private UnitBase attacker;
	private bool exploded;
	private BombLevel level;
	private Vector3 targetPosition;
	private UnitBase targetUnitBase;

	private void Awake()
	{
		foreach (var childCollider in GetComponentsInChildren<Collider>())
			childCollider.gameObject.layer = LayerMask.NameToLayer("Shell");
	}

	private static Vector3 Dimensions() { return new Vector3(0.12f, 0.12f, 0.74f); }

	private void Explode()
	{
		var detonator = string.Empty;
		switch (level)
		{
			case BombLevel.Small:
				detonator = "Detonator_Small";
				break;
			case BombLevel.Medium:
				detonator = "Detonator_Medium";
				break;
			case BombLevel.Large:
				detonator = "Detonator_Large";
				break;
		}
		(Instantiate(Resources.Load(detonator), rigidbody.position, Quaternion.identity) as GameObject).GetComponent<Detonator>().size = ((float)level + 1) / 2 * Settings.Map.ScaleFactor;
		--attacker.explosionsLeft;
		exploded = true;
		StartCoroutine(FadeOut());
	}

	private IEnumerator FadeOut()
	{
		rigidbody.Sleep();
		GetComponent<MeshRenderer>().enabled = false;
		var trail = transform.Find("Trail").particleSystem;
		while ((trail.emissionRate *= Settings.FastAttenuation) > 10)
			yield return new WaitForSeconds(Settings.DeltaTime);
		Destroy(gameObject);
	}

	private void FixedUpdate()
	{
		if (exploded)
			return;
		rigidbody.velocity = (transform.forward + Random.insideUnitSphere * noise / ((float)level + 1)) * speed * Settings.Map.ScaleFactor;
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetPosition - rigidbody.position), Time.fixedDeltaTime * angularCorrectionRate);
		if ((targetPosition - rigidbody.position).magnitude < Settings.Map.ScaleFactor * 0.4f)
			Explode();
	}

	private void OnTriggerEnter(Component collider)
	{
		if (exploded)
			return;
		var unitBase = collider.GetComponentInParent(typeof(UnitBase));
		if (!unitBase || unitBase != targetUnitBase)
			return;
		Explode();
	}

	public void Setup(UnitBase attacker, UnitBase targetUnitBase, BombLevel bombLevel = BombLevel.Medium)
	{
		this.attacker = attacker;
		this.targetUnitBase = targetUnitBase;
		targetPosition = targetUnitBase.transform.WorldCenterOfElement();
		level = bombLevel;
	}

	public void Setup(UnitBase attacker, Vector3 targetPosition, BombLevel bombLevel = BombLevel.Medium)
	{
		this.attacker = attacker;
		targetUnitBase = null;
		this.targetPosition = targetPosition;
		level = bombLevel;
	}

	private void Start() { transform.localScale = Vector3.one * ((int)level + 2) * Settings.Map.ScaleFactor * 0.1f / ((Dimensions().x + Dimensions().z)); }
}