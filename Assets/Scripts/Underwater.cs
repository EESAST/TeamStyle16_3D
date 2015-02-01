#region

using GameStatics;
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
		defaultCullingMask = Camera.main.cullingMask;
		defaultFog = RenderSettings.fog;
		defaultFogColor = RenderSettings.fogColor;
		defaultFogDensity = RenderSettings.fogDensity;
		defaultSkybox = RenderSettings.skybox;
	}

	private void Update()
	{
		var newIsUnderwater = Camera.main.transform.position.y < Settings.HeightOfLevel[1];
		if (isUnderwater != newIsUnderwater)
		{
			isUnderwater = newIsUnderwater;
			if (isUnderwater)
			{
				Camera.main.cullingMask = Settings.Ocean.UnderwaterCullingMask + 1;
				RenderSettings.fog = true;
				RenderSettings.fogColor = Settings.Ocean.FogColor;
				RenderSettings.fogDensity = Settings.Ocean.FogDensity;
				RenderSettings.skybox = null;
			}
			else
			{
				Camera.main.cullingMask = defaultCullingMask;
				RenderSettings.fog = defaultFog;
				RenderSettings.fogColor = defaultFogColor;
				RenderSettings.fogDensity = defaultFogDensity;
				RenderSettings.skybox = defaultSkybox;
			}
		}
	}
}