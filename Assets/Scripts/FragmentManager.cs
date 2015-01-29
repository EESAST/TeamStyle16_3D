#region

using System.Collections;
using GameStatics;
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
		yield return new WaitForSeconds(0.1f);
		while (!rigidbody.IsSleeping() && transform.position.y > Settings.HeightOfLayer[1] && Time.time < spawnTime + maxLifeSpan)
			yield return new WaitForSeconds(0.1f);
		var attenuation = transform.position.y < Settings.HeightOfLayer[1] ? Settings.Fragment.AttenuationFast : Settings.Fragment.AttenuationSlow;
		while ((smokeTrail.maxEmission = smokeTrail.minEmission *= attenuation) > 10)
			yield return new WaitForSeconds(0.1f);
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