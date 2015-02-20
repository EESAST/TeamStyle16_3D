#region

using UnityEngine;

#endregion

public class CustomCharacterInfo
{
	public bool flipped;
	public Rect uv;
	public Rect vert;
	public float width;

	public void ScaleClone(float scale, ref CustomCharacterInfo char_info)
	{
		char_info.flipped = flipped;
		char_info.uv = new Rect(uv);
		char_info.vert = new Rect(vert);
		char_info.width = width;

		// Scale char_info values
		char_info.vert.x /= scale;
		char_info.vert.y /= scale;
		char_info.vert.width /= scale;
		char_info.vert.height /= scale;
		char_info.width /= scale;
	}
}