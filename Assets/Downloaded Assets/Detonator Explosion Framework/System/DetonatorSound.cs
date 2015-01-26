#region

using UnityEngine;

#endregion

[RequireComponent(typeof(Detonator))]
[AddComponentMenu("Detonator/Sound")]
public class DetonatorSound : DetonatorComponent
{
	private bool _delayedExplosionStarted;
	private float _explodeDelay;
	private int _idx;
	private AudioSource _soundComponent;
	public float distanceThreshold = 50f; //threshold in m between playing nearSound and farSound
	public AudioClip[] farSounds;
	public float maxVolume = 1f;
	public float minVolume = .4f;
	public AudioClip[] nearSounds;
	public float rolloffFactor = 0.5f;

	public override void Explode()
	{
		if (detailThreshold > detail)
			return;

		if (!_delayedExplosionStarted)
			_explodeDelay = explodeDelayMin + (Random.value * (explodeDelayMax - explodeDelayMin));
		if (_explodeDelay <= 0)
		{
			//		_soundComponent.minVolume = minVolume;
			//		_soundComponent.maxVolume = maxVolume;
			//		_soundComponent.rolloffFactor = rolloffFactor;

			if (Vector3.Distance(Camera.main.transform.position, transform.position) < distanceThreshold)
			{
				_idx = (int)(Random.value * nearSounds.Length);
				_soundComponent.PlayOneShot(nearSounds[_idx]);
			}
			else
			{
				_idx = (int)(Random.value * farSounds.Length);
				_soundComponent.PlayOneShot(farSounds[_idx]);
			}
			_delayedExplosionStarted = false;
			_explodeDelay = 0f;
		}
		else
			_delayedExplosionStarted = true;
	}

	public override void Init() { _soundComponent = (AudioSource)gameObject.AddComponent("AudioSource"); }

	public void Reset() { }

	private void Update()
	{
		if (_soundComponent == null)
			return;

		_soundComponent.pitch = Time.timeScale;

		if (_delayedExplosionStarted)
		{
			_explodeDelay = (_explodeDelay - Time.deltaTime);
			if (_explodeDelay <= 0f)
				Explode();
		}
	}
}