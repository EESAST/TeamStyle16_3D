#region

using System.Collections;
using UnityEngine;

#endregion

public class Flashlight : MonoBehaviour, IElementFX
{
	public float amplitude;
	private LensFlare flare;
	public float maxOmega;
	public float minOmega;
	public float offset;
	private float omega;
	public Vector3 translationOffsetInParentSpace;

	public void Disable()
	{
		enabled = false;
		StartCoroutine(FadeOut());
	}

	private void Awake()
	{
		transform.localPosition = translationOffsetInParentSpace;
		omega = Random.Range(minOmega, maxOmega);
		light.range = Settings.Map.ScaleFactor;
		flare = GetComponent<LensFlare>();
	}

	private IEnumerator FadeOut()
	{
		while ((light.intensity *= Settings.FastAttenuation) + (flare.brightness *= Settings.FastAttenuation) > Mathf.Epsilon)
			yield return new WaitForSeconds(Settings.DeltaTime);
	}

	public void RefreshLightColor() { flare.color = light.color = Data.TeamColor.Current[GetComponentInParent<Element>().team]; }

	private void Update() { flare.brightness = light.intensity = amplitude * Mathf.Sin(omega * Time.time) + offset; }
}