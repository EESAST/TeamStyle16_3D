#region

using UnityEngine;

#endregion

public class SelectionManager : MonoBehaviour
{
	private EntityBehaviour lastDownEntity;
	private EntityBehaviour lastSelectedEntity;
	public LayerMask layerMask = -1;

	private void Update()
	{
		if (!Screen.lockCursor)
		{
			EntityBehaviour target = null;
			RaycastHit hitInfo;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, layerMask.value))
				target = hitInfo.transform.GetComponentInParent<EntityBehaviour>();
			if (target != null)
			{
				if (Input.GetMouseButtonDown(0))
					lastDownEntity = target;
				if (Input.GetMouseButtonUp(0) && target == lastDownEntity)
				{
					if (lastSelectedEntity)
						lastSelectedEntity.Deselect();
					(lastSelectedEntity = target).Select();
				}
				target.MouseOver();
			}
		}
		if (Input.GetMouseButtonUp(1) && lastSelectedEntity)
		{
			lastSelectedEntity.Deselect();
			lastSelectedEntity = null;
		}
	}
}