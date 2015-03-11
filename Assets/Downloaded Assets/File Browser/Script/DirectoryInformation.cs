#region

using System.IO;
using UnityEngine;

#endregion

public class DirectoryInformation
{
	public readonly DirectoryInfo directoryInfo;
	private readonly GUIContent guiContent;

	public DirectoryInformation(DirectoryInfo directoryInfo, Texture directoryTexture)
	{
		this.directoryInfo = directoryInfo;
		guiContent = new GUIContent(directoryInfo.Name, directoryTexture);
	}

	public bool Button(GUIStyle guiStyle = null) { return GUILayout.Button(guiContent, guiStyle ?? new GUIStyle("button") { alignment = TextAnchor.MiddleLeft }); }
}