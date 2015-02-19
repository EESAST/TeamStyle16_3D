#region

using JSON;
using UnityEditor;
using UnityEngine;

#endregion

public class JSONChecker : EditorWindow
{
	private JSONObject j;
	private string JSON = @"{
	""TestObject"": {
		""SomeText"": ""Blah"",
		""SomeObject"": {
			""SomeNumber"": 42,
			""SomeBool"": true,
			""SomeNull"": null
		},
		""SomeEmptyObject"": { },
		""SomeEmptyArray"": [ ]
	}
}";

	[MenuItem("Window/JSONChecker")]
	private static void Init() { GetWindow(typeof(JSONChecker)); }

	private void OnGUI()
	{
		JSON = EditorGUILayout.TextArea(JSON);
		GUI.enabled = JSON != "";
		if (GUILayout.Button("Check JSON"))
		{
			j = new JSONObject(JSON);
			Debug.Log(j.ToString(true));
		}
		if (j)
			if (j.type == JSONObject.Type.NULL)
				GUILayout.Label("JSON fail:\n" + j.ToString(true));
			else
				GUILayout.Label("JSON success:\n" + j.ToString(true));
	}
}