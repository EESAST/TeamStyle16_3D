#region

using System.Collections;
using GameStatics;
using UnityEngine;

#endregion

public class FragmentManager : MonoBehaviour
{
	private ParticleEmitter smokeTrail;

	private IEnumerator Extinguish()
	{
		rigidbody.isKinematic = false;
		yield return new WaitForSeconds(0.1f);
		while (!rigidbody.IsSleeping() && transform.position.y > Data.SeaLevel)
			yield return new WaitForSeconds(0.1f);
		var attenuation = transform.position.y < Data.SeaLevel ? 0.6f : 0.9f;
		while ((smokeTrail.maxEmission = smokeTrail.minEmission *= attenuation) > 10)
			yield return new WaitForSeconds(0.1f);
		GetComponent<MeshCollider>().enabled = false;
		Destroy(transform.parent.childCount == 1 ? transform.parent.gameObject : gameObject, 2);
	}

	private void Start()
	{
		smokeTrail = transform.GetComponentInChildren<ParticleEmitter>();
		StartCoroutine(Extinguish());
	}
}