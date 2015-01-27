#region

using UnityEngine;

#endregion

[RequireComponent(typeof(Detonator))]
[AddComponentMenu("Detonator/Light")]
public class DetonatorLight : DetonatorComponent
{
	private readonly Color _baseColor = Color.white;
	private readonly float _baseIntensity = 1f;
	private float _explodeTime = -1000f;
	private GameObject _light;
	private Light _lightComponent;
	private float _reduceAmount;
	private float _scaledDuration;
	public float intensity;

	public override void Explode()
	{
		if (detailThreshold > detail)
			return;

		_lightComponent.color = color;
		_lightComponent.range = size * 50f;
		_scaledDuration = (duration * timeScale);
		_lightComponent.enabled = true;
		_lightComponent.intensity = intensity;
		_explodeTime = Time.time;
	}

	public override void Init()
	{
		_light = new GameObject("Light");
		_light.transform.parent = transform;
		_light.transform.localPosition = localPosition;
		_lightComponent = (Light)_light.AddComponent("Light");
		_lightComponent.type = LightType.Point;
		_lightComponent.enabled = false;
	}

	public void Reset()
	{
		color = _baseColor;
		intensity = _baseIntensity;
	}

	private void Update()
	{
		if ((_explodeTime + _scaledDuration > Time.time) && _lightComponent.intensity > 0f)
		{
			_reduceAmount = intensity * (Time.deltaTime / _scaledDuration);
			_lightComponent.intensity -= _reduceAmount;
		}
		else if (_lightComponent)
			_lightComponent.enabled = false;
	}
}