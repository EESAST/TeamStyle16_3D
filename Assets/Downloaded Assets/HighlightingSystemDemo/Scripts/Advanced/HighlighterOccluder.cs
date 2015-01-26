#region

using HighlightingSystem;
using UnityEngine;

#endregion

public class HighlighterOccluder : MonoBehaviour
{
	private Highlighter h;
	// 
	private void Awake()
	{
		h = GetComponent<Highlighter>();
		if (h == null)
			h = gameObject.AddComponent<Highlighter>();
	}

	// 
	private void OnEnable() { h.OccluderOn(); }
}