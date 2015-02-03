#region

using System;
using UnityEditor;
using UnityEngine;

#endregion

internal class WaterEditorUtility
{
	public static void CurveGui(String name, SerializedObject serObj, Color color)
	{
		var curve = new AnimationCurve(new Keyframe(0, 0.0f, 1.0f, 1.0f), new Keyframe(1, 1.0f, 1.0f, 1.0f));
		curve = EditorGUILayout.CurveField(new GUIContent(name), curve, color, new Rect(0.0f, 0.0f, 1.0f, 1.0f));

		//if (GUI.changed) {
		//	AnimationCurveChanged(((WaterBase)serObj.targetObject).sharedMaterial, curve);
		//((WaterBase)serObj.targetObject).gameObject.SendMessage ("AnimationCurveChanged", SendMessageOptions.DontRequireReceiver);
		//}          
	}

	public static Color GetMaterialColor(String name, Material mat) { return mat.GetColor(name); }

	// helper functions to retrieve & set material values

	public static float GetMaterialFloat(String name, Material mat) { return mat.GetFloat(name); }

	public static Texture GetMaterialTexture(String theName, Material mat) { return mat.GetTexture(theName); }

	public static Vector4 GetMaterialVector(String name, Material mat) { return mat.GetVector(name); }

	public static Material LocateValidWaterMaterial(Transform parent)
	{
		if (parent.renderer && parent.renderer.sharedMaterial)
			return parent.renderer.sharedMaterial;
		foreach (Transform t in parent)
			if (t.renderer && t.renderer.sharedMaterial)
				return t.renderer.sharedMaterial;
		return null;
	}

	public static void SetMaterialColor(String name, Color color, Material mat) { mat.SetColor(name, color); }

	public static void SetMaterialFloat(String name, float f, Material mat) { mat.SetFloat(name, f); }

	public static void SetMaterialTexture(String theName, Texture parameter, Material mat) { mat.SetTexture(theName, parameter); }

	public static void SetMaterialVector(String name, Vector4 vector, Material mat) { mat.SetVector(name, vector); }
}