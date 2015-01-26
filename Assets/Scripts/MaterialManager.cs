#region

using GameStatics;
using UnityEngine;

#endregion

public class MaterialManager : MonoBehaviour
{
	private void Awake()
	{
		Delegates.TeamColorChanged += RefreshMaterialColor;
		Base.LoadMaterial();
		Fort.LoadMaterial();
	}

	private void OnDestroy() { Delegates.TeamColorChanged -= RefreshMaterialColor; }

	private void RefreshMaterialColor()
	{
		Base.RefreshMaterialColor();
		Fort.RefreshMaterialColor();
	}

	private void Update() { Fort.RefreshRibbonTextureOffset(); }
}