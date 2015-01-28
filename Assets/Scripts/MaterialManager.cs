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
		CargoShip.LoadMaterial();
		Carrier.LoadMaterial();
		Destroyer.LoadMaterial();
		Fighter.LoadMaterial();
		Fort.LoadMaterial();
		Scout.LoadMaterial();
		Submarine.LoadMaterial();
	}

	private void OnDestroy() { Delegates.TeamColorChanged -= RefreshMaterialColor; }

	private void RefreshMaterialColor()
	{
		Base.RefreshMaterialColor();
		CargoShip.RefreshMaterialColor();
		Carrier.RefreshMaterialColor();
		Destroyer.RefreshMaterialColor();
		Fighter.RefreshMaterialColor();
		Fort.RefreshMaterialColor();
		Scout.RefreshMaterialColor();
		Submarine.RefreshMaterialColor();
	}

	private void Update()
	{
		Fort.RefreshRibbonTextureOffset();
		Scout.RefreshRibbonTextureOffset();
	}
}