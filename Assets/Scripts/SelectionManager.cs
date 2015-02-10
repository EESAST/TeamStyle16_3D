#region

using UnityEngine;
using UnityEngine.EventSystems;

#endregion

public class SelectionManager : MonoBehaviour
{
	private Element _lastDownElement;
	private Element _lastOverElement;
	private Element _lastSelectedElement;

	private void Update()
	{
		if (_lastOverElement)
			_lastOverElement.MouseOver = false;
		if (EventSystem.current.IsPointerOverGameObject())
			return;
		if (Input.GetMouseButtonUp(1) && _lastSelectedElement)
		{
			_lastSelectedElement.Deselect();
			_lastSelectedElement = null;
		}
		if (Screen.lockCursor)
			return;
		Element target = null;
		RaycastHit hitInfo;
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, LayerMask.GetMask("Element")))
			target = hitInfo.transform.GetComponentInParent<Element>();
		if (target)
			(_lastOverElement = target).MouseOver = true;
		if (Input.GetMouseButtonDown(0))
			_lastDownElement = target;
		if (Input.GetMouseButtonUp(0) && target && target == _lastDownElement)
		{
			if (_lastSelectedElement)
				_lastSelectedElement.Deselect();
			(_lastSelectedElement = target).Select();
		}
	}
}