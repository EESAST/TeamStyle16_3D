#region

using UnityEngine;

#endregion

// Sets up transformation matrices to scale&scroll water waves
// for the case where graphics card does not support vertex programs.

[ExecuteInEditMode]
public class WaterSimple : MonoBehaviour
{
	private void Update()
	{
		if (!GetComponent<Renderer>())
			return;
		var mat = GetComponent<Renderer>().sharedMaterial;
		if (!mat)
			return;

		var waveSpeed = mat.GetVector("WaveSpeed");
		var waveScale = mat.GetFloat("_WaveScale");
		var t = Time.time / 20.0f;

		var offset4 = waveSpeed * (t * waveScale);
		var offsetClamped = new Vector4(Mathf.Repeat(offset4.x, 1.0f), Mathf.Repeat(offset4.y, 1.0f), Mathf.Repeat(offset4.z, 1.0f), Mathf.Repeat(offset4.w, 1.0f));
		mat.SetVector("_WaveOffset", offsetClamped);

		var scale = new Vector3(1.0f / waveScale, 1.0f / waveScale, 1);
		var scrollMatrix = Matrix4x4.TRS(new Vector3(offsetClamped.x, offsetClamped.y, 0), Quaternion.identity, scale);
		mat.SetMatrix("_WaveMatrix", scrollMatrix);

		scrollMatrix = Matrix4x4.TRS(new Vector3(offsetClamped.z, offsetClamped.w, 0), Quaternion.identity, scale * 0.45f);
		mat.SetMatrix("_WaveMatrix2", scrollMatrix);
	}
}