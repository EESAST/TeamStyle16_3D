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
		Cargo.LoadMaterial();
		Carrier.LoadMaterial();
		Destroyer.LoadMaterial();
		Fighter.LoadMaterial();
		Fort.LoadMaterial();
		Mine.LoadMaterial();
		Oilfield.LoadMaterial();
		Scout.LoadMaterial();
		Submarine.LoadMaterial();
		materialLoaded = true;
	}

	private void OnDestroy() { Delegates.CurrentTeamColorChanged -= RefreshMaterialColor; }

	private void RefreshMaterialColor()
	{
		Base.RefreshMaterialColor();
		Cargo.RefreshMaterialColor();
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
		Oilfield.RefreshTextureOffset();
		Scout.RefreshTextureOffset();
	}
}