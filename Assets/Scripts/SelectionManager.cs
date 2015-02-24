#region

using System.Linq;
using UnityEngine;

#endregion

public class SelectionManager : MonoBehaviour
{
	private Element lastDownElement;
	private Element lastOverElement;
	private Element lastSelectedElement;

	private void LateUpdate()
	{
		Revert();
		if (Input.GetMouseButtonUp(1) && lastSelectedElement)
		{
			lastSelectedElement.Deselect();
			lastSelectedElement = null;
			Camera.main.audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Element_Deselect"));
		}
		if (Data.GUI.OccupiedRects.Any(rect => rect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) || Screen.lockCursor)
			lastOverElement = null;
		else
		{
			Element target = null;
			RaycastHit hitInfo;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, LayerMask.GetMask("Element")))
				target = hitInfo.transform.GetComponentInParent<Element>();
			if (target)
			{
				if (lastOverElement != target)
					Camera.main.audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Element_Over"));
				target.MouseOver = true;
			}
			lastOverElement = target;
			if (Input.GetMouseButtonDown(0))
				lastDownElement = target;
			if (Input.GetMouseButtonUp(0) && target && target == lastDownElement)
			{
				if (lastSelectedElement)
					lastSelectedElement.Deselect();
				(lastSelectedElement = target).Select();
				Camera.main.audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Element_Select"));
			}
		}
		Data.GUI.OccupiedRects.Clear();
	}

	private void OnDisable() { Revert(); }

	private void Revert()
	{
		if (lastOverElement)
			lastOverElement.MouseOver = false;
	}
}