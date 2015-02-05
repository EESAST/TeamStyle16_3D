#region

using UnityEngine;

#endregion

public class Underwater : MonoBehaviour
{
	private LayerMask defaultCullingMask;
	private bool defaultFog;
	private Color defaultFogColor;
	private float defaultFogDensity;
	private Material defaultSkybox;
	private bool isUnderwater;

	private void Start()
	{
		defaultCullingMask = camera.cullingMask;
		defaultFog = RenderSettings.fog;
		defaultFogColor = RenderSettings.fogColor;
		defaultFogDensity = RenderSettings.fogDensity;
		defaultSkybox = RenderSettings.skybox;
	}

	private void Update()
	{
		var newIsUnderwater = camera.transform.position.y < Settings.HeightOfLevel[1];
		if (isUnderwater == newIsUnderwater)
			return;
		isUnderwater = newIsUnderwater;
		if (isUnderwater)
		{
			camera.cullingMask = Settings.Ocean.UnderwaterCullingMask + 1;
			RenderSettings.fog = true;
			RenderSettings.fogColor = Settings.Ocean.FogColor;
			RenderSettings.fogDensity = Settings.Ocean.FogDensity;
			RenderSettings.skybox = null;
		}
		else
		{
			camera.cullingMask = defaultCullingMask;
			RenderSettings.fog = defaultFog;
			RenderSettings.fogColor = defaultFogColor;
			RenderSettings.fogDensity = defaultFogDensity;
			RenderSettings.skybox = defaultSkybox;
		}
	}
}