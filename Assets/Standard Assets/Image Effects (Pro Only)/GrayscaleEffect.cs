#region

using UnityEngine;

#endregion

[ExecuteInEditMode, AddComponentMenu("Image Effects/Color Adjustments/Grayscale")]
public class GrayscaleEffect : ImageEffectBase
{
	public float rampOffset;
	public Texture textureRamp;
	// Called by camera to apply image effect
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetTexture("_RampTex", textureRamp);
		material.SetFloat("_RampOffset", rampOffset);
		Graphics.Blit(source, destination, material);
	}
}