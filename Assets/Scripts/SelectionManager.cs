#region

using UnityEngine;

#endregion

public class SelectionManager : MonoBehaviour
{
<<<<<<< HEAD
	private EntityBehaviour lastDownEntity;
	private EntityBehaviour lastSelectedEntity;
=======
	private Entity lastDownEntity;
	private Entity lastSelectedEntity;
>>>>>>> initial commit on another computer
	public LayerMask layerMask = -1;

	private void Update()
	{
		if (!Screen.lockCursor)
		{
<<<<<<< HEAD
			EntityBehaviour target = null;
			RaycastHit hitInfo;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, layerMask.value))
				target = hitInfo.transform.GetComponentInParent<EntityBehaviour>();
=======
			Entity target = null;
			RaycastHit hitInfo;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, layerMask.value))
				target = hitInfo.transform.GetComponentInParent<Entity>();
>>>>>>> initial commit on another computer
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