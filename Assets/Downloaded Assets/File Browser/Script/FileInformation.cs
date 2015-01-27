#region

using System.IO;
using UnityEngine;

#endregion

public class FileInformation
{
	public FileInfo fi;
	public GUIContent gc;

	public FileInformation(FileInfo f)
	{
		fi = f;
		gc = new GUIContent(fi.Name);
	}

	public FileInformation(FileInfo f, Texture2D img)
	{
		fi = f;
		gc = new GUIContent(fi.Name, img);
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