#region

using UnityEngine;

#endregion

public class TouchToMouse : MonoBehaviour
{
	//Transform beingMoved;

	private void Update()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		foreach (var touch in Input.touches)
		{
			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit))
				continue;
			if (touch.phase == TouchPhase.Began)
				hit.transform.gameObject.SendMessage("OnMouseDown");
		}
	}
}