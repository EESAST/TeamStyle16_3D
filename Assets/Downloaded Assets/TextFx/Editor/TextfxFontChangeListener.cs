#region

using UnityEditor;
using UnityEngine;

#endregion

/* 	Class to listen for reimported Font files (caused by font size change, font type change and other setting changes).
	Calls to all EffectManager instances in scene to let them know the font that's changed. */

internal class TextfxFontChangeListener : AssetPostprocessor
{
#if !UNITY_3_5
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		string asset_path;
		foreach (var str in importedAssets)
		{
			asset_path = str.ToLower();

			var parts = asset_path.Split('.');
			var file_extension = parts[parts.Length - 1];

			if (file_extension.Equals("ttf") || file_extension.Equals("dfont") || file_extension.Equals("otf"))
			{
				// Imported a font file. Tell all EffectManager instances, to update text accordingly
				parts = asset_path.Split('/');
				var font_name = parts[parts.Length - 1];
				font_name = font_name.Replace(".ttf", "");
				font_name = font_name.Replace(".dfont", "");
				font_name = font_name.Replace(".otf", "");

				var effects = Object.FindObjectsOfType(typeof(EffectManager)) as EffectManager[];

				foreach (var effect in effects)
					effect.FontImportDetected(font_name);
			}
		}
	}
#endif
}