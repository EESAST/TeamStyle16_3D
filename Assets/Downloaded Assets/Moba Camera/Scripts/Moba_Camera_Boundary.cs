#region

using UnityEngine;

#endregion

/*
 * On Start adds self to the list of boundaries in the scene.
 * - Remove having to drag and drop each indiviual boundary.
 */

public class Moba_Camera_Boundary : MonoBehaviour
{
	// what type of boundary, set in the inspector
	public bool isActive = true;
	public Moba_Camera_Boundaries.BoundaryType type = Moba_Camera_Boundaries.BoundaryType.none;

	private void OnDestroy() { Moba_Camera_Boundaries.RemoveBoundary(this, type); }

	// Use this for initialization
	private void Start()
	{
		// Adds this boundary to the list of boundaries
		Moba_Camera_Boundaries.AddBoundary(this, type);
		if (LayerMask.NameToLayer(Moba_Camera_Boundaries.boundaryLayer) != -1)
			gameObject.layer = LayerMask.NameToLayer(Moba_Camera_Boundaries.boundaryLayer);
		else
		{
			Moba_Camera_Boundaries.SetBoundaryLayerExist(false);
			collider.isTrigger = true;
		}
	}
}