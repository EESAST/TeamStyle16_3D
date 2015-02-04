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
		yield return new WaitForSeconds(0.04f);
		while (!rigidbody.IsSleeping() && transform.position.y > Settings.HeightOfLevel[1] && Time.time < spawnTime + maxLifeSpan)
			yield return new WaitForSeconds(0.04f);
		var attenuation = transform.position.y < Settings.HeightOfLevel[1] ? Settings.Fragment.FastAttenuation : Settings.Fragment.SlowAttenuation;
		while ((smokeTrail.maxEmission = smokeTrail.minEmission *= attenuation) > 3)
			yield return new WaitForSeconds(0.04f);
		GetComponent<MeshCollider>().enabled = false;
		Destroy(transform.parent.childCount == 1 ? transform.parent.gameObject : gameObject, 3);
	}

	private void Start()
	{
		spawnTime = Time.time;
		smokeTrail = transform.GetComponentInChildren<ParticleEmitter>();
		StartCoroutine(Extinguish());
	}
}