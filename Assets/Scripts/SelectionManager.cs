#region

using UnityEngine;
using UnityEngine.EventSystems;

#endregion

public class SelectionManager : MonoBehaviour
{
	private Element lastDownElement;
	private Element lastOverElement;
	private Element lastSelectedElement;

	private void OnDisable() { Revert(); }

	private void Revert()
	{
		if (lastOverElement)
			lastOverElement.MouseOver = false;
	}

	private void Update()
	{
		Revert();
		if (Input.GetMouseButtonUp(1) && lastSelectedElement)
		{
			lastSelectedElement.Deselect();
			lastSelectedElement = null;
		}
		if (EventSystem.current.IsPointerOverGameObject() || Screen.lockCursor)
			return;
		Element target = null;
		RaycastHit hitInfo;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, LayerMask.GetMask("Element")))
			target = hitInfo.transform.GetComponentInParent<Element>();
		if (lastOverElement = target)
			lastOverElement.MouseOver = true;
		if (Input.GetMouseButtonDown(0))
			lastDownElement = target;
		if (Input.GetMouseButtonUp(0) && target && target == lastDownElement)
		{
			if (lastSelectedElement)
				lastSelectedElement.Deselect();
			(lastSelectedElement = target).Select();
		}
	}
}