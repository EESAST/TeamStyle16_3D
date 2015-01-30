#region

using GameStatics;
using UnityEngine;

#endregion

public abstract class Resource : Entity
{
	private float[] initialMaxEmission;
	private float[] initialMinEmission;
	private ParticleEmitter[] particleEmitters;

	protected override void Awake()
	{
		base.Awake();
		team = 3;
		particleEmitters = GetComponentsInChildren<ParticleEmitter>();
		initialMaxEmission = new float[particleEmitters.Length];
		initialMinEmission = new float[particleEmitters.Length];
		for (var i = 0; i < particleEmitters.Length; i++)
		{
			initialMaxEmission[i] = particleEmitters[i].maxEmission;
			initialMinEmission[i] = particleEmitters[i].minEmission;
		}
	}

	protected override void SetPosition(float externalX, float externalY) { transform.position = Methods.Coordinates.ExternalToInternal(externalX, externalY, 2); }

	protected override void Update()
	{
		base.Update();
		var ratio = (float)HP / MaxHP();
		for (var i = 0; i < particleEmitters.Length; i++)
		{
			particleEmitters[i].maxEmission = initialMaxEmission[i] * ratio;
			particleEmitters[i].minEmission = initialMinEmission[i] * ratio;
		}
	}
}