#region

using UnityEngine;

#endregion

[ExecuteInEditMode, AddComponentMenu("Image Effects/Blur/Blur")]
public class BlurEffect : MonoBehaviour
{
	//private static string blurMatString =

	private static Material m_Material;
	public Shader blurShader = null;

	/// Blur spread for each iteration. Lower values
	/// give better looking blur, but require more iterations to
	/// get large blurs. Value is usually between 0.5 and 1.0.
	public float blurSpread = 0.6f;

	/// Blur iterations - larger number means more blur.
	public int iterations = 3;

	protected Material material
	{
		get
		{
			if (m_Material == null)
			{
				m_Material = new Material(blurShader);
				m_Material.hideFlags = HideFlags.DontSave;
			}
			return m_Material;
		}
	}

	// Downsamples the texture to a quarter resolution.
	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		var off = 1.0f;
		Graphics.BlitMultiTap(source, dest, material, new Vector2(-off, -off), new Vector2(-off, off), new Vector2(off, off), new Vector2(off, -off));
	}

	// Performs one blur iteration.
	public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		var off = 0.5f + iteration * blurSpread;
		Graphics.BlitMultiTap(source, dest, material, new Vector2(-off, -off), new Vector2(-off, off), new Vector2(off, off), new Vector2(off, -off));
	}

	protected void OnDisable()
	{
		if (m_Material)
			DestroyImmediate(m_Material);
	}

	// Called by the camera to apply the image effect
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		var rtW = source.width / 4;
		var rtH = source.height / 4;
		var buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

		// Copy source to the 4x4 smaller texture.
		DownSample4x(source, buffer);

		// Blur the small texture
		for (var i = 0; i < iterations; i++)
		{
			var buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
			FourTapCone(buffer, buffer2, i);
			RenderTexture.ReleaseTemporary(buffer);
			buffer = buffer2;
		}
		Graphics.Blit(buffer, destination);

		RenderTexture.ReleaseTemporary(buffer);
	}

	// --------------------------------------------------------

	protected void Start()
	{
		// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
		// Disable if the shader can't run on the users graphics card
		if (!blurShader || !material.shader.isSupported)
			enabled = false;
	}
}