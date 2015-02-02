#region

using System.IO;
using UnityEngine;

#endregion

public class FileInformation
{
	private readonly GUIContent guiContent;
	public FileInfo fileInfo;

	public FileInformation(FileInfo fileInfo, Texture fileTexture)
	{
		this.fileInfo = fileInfo;
		guiContent = new GUIContent(this.fileInfo.Name, fileTexture);
	}

	public bool Button() { return GUILayout.Button(guiContent, new GUIStyle("button") { alignment = TextAnchor.MiddleLeft }); }
}