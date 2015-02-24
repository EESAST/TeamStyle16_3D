#region

using System.Collections;
using UnityEngine;

#endregion

public class BombManager : MonoBehaviour
{
	public enum Level
	{
		Small,
		Medium,
		Large
	}

	private UnitBase attacker;
	private bool exploded;
	private Level level;
	private Vector3 targetPosition;
	private UnitBase targetUnitBase;

	private void Awake()
	{
		foreach (var childCollider in GetComponentsInChildren<Collider>())
			childCollider.gameObject.layer = LayerMask.NameToLayer("Bomb");
		audio.dopplerLevel /= Settings.DimensionScaleFactor;
		audio.maxDistance = Settings.Audio.MaxAudioDistance;
		audio.volume = Settings.Audio.Volume.Bomb;
	}

	private static Vector3 Dimensions() { return new Vector3(0.12f, 0.12f, 0.74f); }

	private void Explode()
	{
		audio.clip = Resources.Load<AudioClip>("Sounds/Impact_" + level);
		audio.dopplerLevel = 0;
		audio.Play();
		(Instantiate(Resources.Load("Detonator_" + level), transform.position, Quaternion.identity) as GameObject).GetComponent<Detonator>().size = ((float)level + 1) / 2 * Settings.DimensionScaleFactor;
		--attacker.explosionsLeft;
		exploded = true;
		StartCoroutine(FadeOut());
	}

	private IEnumerator FadeOut()
	{
		GetComponent<MeshRenderer>().enabled = false;
		var trail = transform.Find("Trail").particleSystem;
		while ((trail.emissionRate *= Settings.FastAttenuation) > 3 || audio.isPlaying)
			yield return new WaitForSeconds(Settings.DeltaTime);
		Destroy(gameObject);
	}

	public void Initialize(UnitBase attacker, Vector3 targetPosition, Level bombLevel = Level.Medium)
	{
		this.attacker = attacker;
		targetUnitBase = null;
		this.targetPosition = targetPosition;
		level = bombLevel;
	}

	private void OnTriggerEnter(Component other)
	{
		if (exploded)
			return;
		var unitBase = other.GetComponentInParent(typeof(UnitBase));
		if (!unitBase || unitBase != targetUnitBase)
			return;
		Explode();
	}

	public void Setup(UnitBase attacker, UnitBase targetUnitBase, Level bombLevel = Level.Medium)
	{
		this.attacker = attacker;
		this.targetUnitBase = targetUnitBase;
		targetPosition = targetUnitBase.transform.WorldCenterOfElement();
		level = bombLevel;
	}

	private IEnumerator Start()
	{
		audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Launcher_" + level));
		transform.localScale = Vector3.one * ((int)level + 1) * 0.1f * Settings.DimensionScaleFactor / ((Dimensions().x + Dimensions().z));
		while (!exploded && (targetPosition - transform.position).magnitude > Settings.DimensionalTolerancePerUnitSpeed * Settings.Bomb.Speed)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), Time.smoothDeltaTime * Settings.Bomb.AngularCorrectionRate);
			transform.Translate((Vector3.forward + Random.insideUnitSphere * Settings.Bomb.Noise / ((float)level + 2)) * Settings.Bomb.Speed * Time.smoothDeltaTime);
			yield return null;
		}
		if (!exploded)
			Explode();
	}
}