#region

using System.IO;
using UnityEngine;

#endregion

public class DirectoryInformation
{
	public DirectoryInfo di;
	public GUIContent gc;

	public DirectoryInformation(DirectoryInfo d)
	{
		di = d;
		gc = new GUIContent(d.Name);
	}

	public DirectoryInformation(DirectoryInfo d, Texture2D img)
	{
		di = d;
		gc = new GUIContent(d.Name, img);
	}

	public bool button()
	{
		return GUILayout.Button(gc, new GUIStyle("button")
		{
			alignment = TextAnchor.MiddleLeft
		});
	}

	public bool button(GUIStyle gs) { return GUILayout.Button(gc, gs); }

	public void label() { GUILayout.Label(gc); }

	public void label(GUIStyle gs) { GUILayout.Label(gc, gs); }
}