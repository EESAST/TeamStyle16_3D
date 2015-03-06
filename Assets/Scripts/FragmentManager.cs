#region

using System.Collections;
using UnityEngine;

#endregion

public class FragmentManager : MonoBehaviour
{
	private readonly float maxLifeSpan = Random.Range(Settings.Fragment.MinLifeSpan, Settings.Fragment.MaxLifeSpan);
	private ParticleEmitter smokeTrail;
	private float spawnTime;

	private IEnumerator Extinguish()
	{
		rigidbody.isKinematic = false;
		rigidbody.WakeUp();
		while (!rigidbody.IsSleeping() && transform.position.y > Settings.Map.HeightOfLevel[1] && Time.time < spawnTime + maxLifeSpan)
			yield return null;
		var attenuation = transform.position.y < Settings.Map.HeightOfLevel[1] ? Settings.FastAttenuation : Settings.SlowAttenuation;
		while ((smokeTrail.maxEmission = smokeTrail.minEmission *= attenuation) > 3)
			yield return new WaitForSeconds(Settings.DeltaTime);
		GetComponent<MeshCollider>().enabled = false;
		Destroy(gameObject, 3);
	}

	private void Start()
	{
		spawnTime = Time.time;
		smokeTrail = transform.GetComponentInChildren<ParticleEmitter>();
		StartCoroutine(Extinguish());
	}
}