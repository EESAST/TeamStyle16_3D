#region

using UnityEngine;

#endregion

public class MaterialManager : MonoBehaviour
{
	private static bool materialLoaded;

	private void Awake()
	{
		Delegates.CurrentTeamColorChanged += RefreshMaterialColor;
		if (!materialLoaded)
			LoadMaterial();
		RefreshMaterialColor();
	}

	private static void LoadMaterial()
	{
		Base.LoadMaterial();
		CargoShip.LoadMaterial();
		Carrier.LoadMaterial();
		Destroyer.LoadMaterial();
		Fighter.LoadMaterial();
		Fort.LoadMaterial();
		Mine.LoadMaterial();
		OilField.LoadMaterial();
		Scout.LoadMaterial();
		Submarine.LoadMaterial();
		materialLoaded = true;
	}

	private void OnDestroy() { Delegates.CurrentTeamColorChanged -= RefreshMaterialColor; }

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
		Fort.RefreshTextureOffset();
		OilField.RefreshTextureOffset();
		Scout.RefreshTextureOffset();
	}
}