#region

using UnityEngine;

#endregion

/*
	Todo - set duration and color properly (actually, i'm not sure this is possible)
	calculate count based on detail
	inherit velocity
*/

[RequireComponent(typeof(Detonator)), AddComponentMenu("Detonator/Object Spray")]
public class DetonatorSpray : DetonatorComponent
{
	private bool _delayedExplosionStarted;
	private float _explodeDelay;
	private Vector3 _explosionPosition;
	private float _tmpScale;
	public int count = 10;
	public float maxScale = 1f;
	public float minScale = 1f;
	public GameObject sprayObject;
	public float startingRadius = 0f;

	public override void Explode()
	{
		if (!_delayedExplosionStarted)
			_explodeDelay = explodeDelayMin + (Random.value * (explodeDelayMax - explodeDelayMin));
		if (_explodeDelay <= 0) //if the delayTime is zero
		{
			var detailCount = (int)(detail * count);
			for (var i = 0; i < detailCount; i++)
			{
				var randVec = Random.onUnitSphere * (startingRadius * size);
				var velocityVec = new Vector3((velocity.x * size), (velocity.y * size), (velocity.z * size));
				var chunk = Instantiate(sprayObject, (transform.position + randVec), transform.rotation) as GameObject;
				chunk.transform.parent = transform;

				//calculate scale for this piece
				_tmpScale = (minScale + (Random.value * (maxScale - minScale)));
				_tmpScale = _tmpScale * size;

				chunk.transform.localScale = new Vector3(_tmpScale, _tmpScale, _tmpScale);

				if (MyDetonator().upwardsBiasFX > 0f)
					velocityVec = new Vector3((velocityVec.x / Mathf.Log(MyDetonator().upwardsBiasFX)), (velocityVec.y * Mathf.Log(MyDetonator().upwardsBiasFX)), (velocityVec.z / Mathf.Log(MyDetonator().upwardsBiasFX)));

				chunk.rigidbody.velocity = Vector3.Scale(randVec.normalized, velocityVec);
				chunk.rigidbody.velocity = Vector3.Scale(randVec.normalized, velocityVec);
				Destroy(chunk, (duration * timeScale));

				_delayedExplosionStarted = false;
				_explodeDelay = 0f;
			}
		}
		else
		//tell update to start reducing the start delay and call explode again when it's zero
			_delayedExplosionStarted = true;
	}

	public override void Init()
	{
		//unused
	}

	public void Reset() { velocity = new Vector3(15f, 15f, 15f); }

	private void Update()
	{
		if (_delayedExplosionStarted)
		{
			_explodeDelay = (_explodeDelay - Time.deltaTime);
			if (_explodeDelay <= 0f)
				Explode();
		}
	}
}