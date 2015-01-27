#region

using System.Collections;
using GameStatics;
using UnityEngine;

#endregion

public class FlashLight : MonoBehaviour, IEntityFX
{
	public float amplitude = 1;
	public float offset = 0;
	public float omega = Mathf.PI;
	public Vector3 translationOffsetToLocal;

	public void Disable()
	{
		enabled = false;
		StartCoroutine(FadeOut());
	}

	private void Awake()
	{
		Delegates.TeamColorChanged += SetLightColor;
		transform.localPosition = translationOffsetToLocal;
		light.range = Settings.ScaleFactor;
	}

	private IEnumerator FadeOut()
	{
		while ((light.intensity *= 0.8f) > Mathf.Epsilon)
			yield return new WaitForSeconds(0.1f);
	}

	private void OnDestroy() { Delegates.TeamColorChanged -= SetLightColor; }

	private void SetLightColor() { light.color = Data.TeamColor.Current[GetComponentInParent<Base>().team]; }

	private void Update() { light.intensity = amplitude * Mathf.Sin(omega * Time.time) + offset; }
}