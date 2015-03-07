#region

using UnityEngine;

#endregion

public class SelectionManager : MonoBehaviour
{
	public static Element LastSelectedElement;
	private Element lastDownElement;
	private Element lastOverElement;

	private void LateUpdate()
	{
		Revert();
		if (Input.GetMouseButtonUp(1) && LastSelectedElement)
		{
			LastSelectedElement.Deselect();
			LastSelectedElement = null;
			Camera.main.audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Element_Deselect"));
		}
		if (Methods.GUI.MouseOver() || Data.MiniMap.FrameRect.Contains(Input.mousePosition) || Screen.lockCursor)
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
				if (LastSelectedElement)
					LastSelectedElement.Deselect();
				(LastSelectedElement = target).Select();
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