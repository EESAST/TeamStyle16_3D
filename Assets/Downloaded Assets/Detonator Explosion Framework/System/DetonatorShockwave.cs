#region

using UnityEngine;

#endregion

[RequireComponent(typeof(Detonator))]
[AddComponentMenu("Detonator/Shockwave")]
public class DetonatorShockwave : DetonatorComponent
{
	private readonly Color _baseColor = Color.white;
	private readonly float _baseDuration = .25f;
	private readonly float _baseSize = 1f;
	private readonly Vector3 _baseVelocity = new Vector3(0f, 0f, 0f);
	private GameObject _shockwave;
	private DetonatorBurstEmitter _shockwaveEmitter;
	public ParticleRenderMode renderMode;
	public Material shockwaveMaterial;
	//Build these to look correct at the stock Detonator size of 10m... then let the size parameter
	//cascade through to the emitters and let them do the scaling work... keep these absolute.
	public void BuildShockwave()
	{
		_shockwave = new GameObject("Shockwave");
		_shockwaveEmitter = (DetonatorBurstEmitter)_shockwave.AddComponent("DetonatorBurstEmitter");
		_shockwave.transform.parent = transform;
		_shockwave.transform.localRotation = Quaternion.identity;
		_shockwave.transform.localPosition = localPosition;
		_shockwaveEmitter.material = shockwaveMaterial;
		_shockwaveEmitter.exponentialGrowth = false;
		_shockwaveEmitter.useWorldSpace = MyDetonator().useWorldSpace;
	}

	public override void Explode()
	{
		if (on)
		{
			UpdateShockwave();
			_shockwaveEmitter.Explode();
		}
	}

	//if materials are empty fill them with defaults
	public void FillMaterials(bool wipe)
	{
		if (!shockwaveMaterial || wipe)
			shockwaveMaterial = MyDetonator().shockwaveMaterial;
	}

	public override void Init()
	{
		//make sure there are materials at all
		FillMaterials(false);
		BuildShockwave();
	}

	public void Reset()
	{
		FillMaterials(true);
		on = true;
		size = _baseSize;
		duration = _baseDuration;
		explodeDelayMin = 0f;
		explodeDelayMax = 0f;
		color = _baseColor;
		velocity = _baseVelocity;
	}

	public void UpdateShockwave()
	{
		_shockwave.transform.localPosition = Vector3.Scale(localPosition, (new Vector3(size, size, size)));
		_shockwaveEmitter.color = color;
		_shockwaveEmitter.duration = duration;
		_shockwaveEmitter.durationVariation = duration * 0.1f;
		_shockwaveEmitter.count = 1;
		_shockwaveEmitter.detail = 1;
		_shockwaveEmitter.particleSize = 25f;
		_shockwaveEmitter.sizeVariation = 0f;
		_shockwaveEmitter.velocity = new Vector3(0f, 0f, 0f);
		_shockwaveEmitter.startRadius = 0f;
		_shockwaveEmitter.sizeGrow = 202f;
		_shockwaveEmitter.size = size;
		_shockwaveEmitter.explodeDelayMin = explodeDelayMin;
		_shockwaveEmitter.explodeDelayMax = explodeDelayMax;
		_shockwaveEmitter.renderMode = renderMode;
	}
}