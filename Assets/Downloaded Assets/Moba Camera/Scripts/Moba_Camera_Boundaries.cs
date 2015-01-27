#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class Moba_Camera_Boundaries
{
	// Layer for boundaries
	public enum BoundaryType
	{
		cube,
		sphere,
		none
	};

	public static string boundaryLayer = "mobaCameraBoundaryLayer";
	private static bool boundaryLayerExists = true;
	// List containing the boundaries in the scene
	private static readonly List<Moba_Camera_Boundary> cube_boundaries = new List<Moba_Camera_Boundary>();
	private static readonly List<Moba_Camera_Boundary> sphere_boundaries = new List<Moba_Camera_Boundary>();
	// add boundary to list 
	public static bool AddBoundary(Moba_Camera_Boundary boundary, BoundaryType type)
	{
		if (boundary == null)
		{
			Debug.LogWarning("Name: " + boundary.name + "; Error: AddBoundary() - null boundary passed");
			return false;
		}

		if (type == BoundaryType.cube)
		{
			cube_boundaries.Add(boundary);
			return true;
		}
		if (type == BoundaryType.sphere)
		{
			sphere_boundaries.Add(boundary);
			return true;
		}
		Debug.LogWarning("Name: " + boundary.name + "; Error: AddBoundary() - Incorrect BoundaryType, boundary will not be used");
		return false;
	}

	// returns the distance from the pivot of the box collider based on the box rotations
	private static Vector3 calBoxRelations(BoxCollider box, Vector3 point, bool containedToBox, out bool isPointInBox)
	{
		var center = box.transform.position + box.center;

		// Cube Size Information
		var hWidth = box.size.x / 2.0f * box.transform.localScale.x;
		var hHeight = box.size.y / 2.0f * box.transform.localScale.y;
		var hDepth = box.size.z / 2.0f * box.transform.localScale.z;

		var yt = Vector3.Dot((point - center), box.transform.up);
		var pointOffsetY = point + yt * -box.transform.up;

		var xt = Vector3.Dot((pointOffsetY - center), box.transform.right);
		var pointOffsetX = pointOffsetY + xt * -box.transform.right;

		var zVec = pointOffsetX - center;
		var yVec = point - pointOffsetY;
		var xVec = pointOffsetY - pointOffsetX;

		var zDist = zVec.magnitude;
		var yDist = yVec.magnitude;
		var xDist = xVec.magnitude;

		isPointInBox = true;
		if (zDist > hDepth)
		{
			if (containedToBox)
				zDist = hDepth;
			isPointInBox = false;
		}
		if (yDist > hHeight)
		{
			if (containedToBox)
				yDist = hHeight;
			isPointInBox = false;
		}
		if (xDist > hWidth)
		{
			if (containedToBox)
				xDist = hWidth;
			isPointInBox = false;
		}

		zDist *= ((Vector3.Dot(box.transform.forward, zVec) >= 0.0f) ? (1.0f) : (-1.0f));
		yDist *= ((Vector3.Dot(box.transform.up, yVec) >= 0.0f) ? (1.0f) : (-1.0f));
		xDist *= ((Vector3.Dot(box.transform.right, xVec) >= 0.0f) ? (1.0f) : (-1.0f));

		return new Vector3(xDist, yDist, zDist);
	}

	public static Moba_Camera_Boundary GetClosestBoundary(Vector3 point)
	{
		// Contains the info for the closest boundary
		Moba_Camera_Boundary closestBoundary = null;
		var closestDistance = 999999.0f;

		// if pivot is outside the boundries find the closest cube
		foreach (var boundary in cube_boundaries)
		{
			if (boundary == null)
				continue;
			if (boundary.isActive == false)
				continue;

			var boxCollider = boundary.GetComponent<BoxCollider>();
			var pointOnSurface = getClosestPointOnSurfaceBox(boxCollider, point);

			var distance = (point - pointOnSurface).magnitude;

			// if the distance is closer calculate the point and set
			if (distance < closestDistance)
			{
				closestBoundary = boundary;
				closestDistance = distance;
			}
		}

		foreach (var boundary in sphere_boundaries)
		{
			if (boundary.isActive == false)
				continue;

			var sphereCollider = boundary.GetComponent<SphereCollider>();

			var center = boundary.transform.position + sphereCollider.center;
			var radius = sphereCollider.radius;

			var centerToPoint = point - center;

			var pointOnSurface = center + (centerToPoint.normalized * radius);

			// the distance from the pivot of the sphere to the posiiton
			var distance = (point - pointOnSurface).magnitude;

			// check if it's the closest point
			if (distance < closestDistance)
			{
				closestBoundary = boundary;
				closestDistance = distance;
			}
		}

		return closestBoundary;
	}

	public static Vector3 GetClosestPointOnBoundary(Moba_Camera_Boundary boundary, Vector3 point)
	{
		var pointOnBoundary = point;

		// Find the closest point on the boundary depending on type of boundary
		if (boundary.type == BoundaryType.cube)
		{
			var boxCollider = boundary.GetComponent<BoxCollider>();

			pointOnBoundary = getClosestPointOnSurfaceBox(boxCollider, point);
		}
		else if (boundary.type == BoundaryType.sphere)
		{
			var sphereCollider = boundary.GetComponent<SphereCollider>();

			var center = boundary.transform.position + sphereCollider.center;
			var radius = sphereCollider.radius;

			var centerToPosition = point - center;

			// Get point on surface of the sphere
			pointOnBoundary = center + (centerToPosition.normalized * radius);
		}

		return pointOnBoundary;
	}

	private static Vector3 getClosestPointOnSurfaceBox(BoxCollider box, Vector3 point)
	{
		bool isIn;
		var dists = calBoxRelations(box, point, true, out isIn);
		return box.transform.position + box.transform.forward * dists.z + box.transform.right * dists.x + box.transform.up * dists.y;
	}

	// returns number of boundaries in the lists
	public static int GetNumberOfBoundaries() { return cube_boundaries.Count + sphere_boundaries.Count; }

	// Check if hbPos give point in within any boundary contained in the list
	public static bool isPointInBoundary(Vector3 point)
	{
		var pointIsInBoundary = false;
		// loop through each cube boundary
		foreach (var boundary in cube_boundaries)
		{
			// check if the boundary is not active. if true, skip it.
			if (boundary.isActive == false)
				continue;
			var boxCollider = boundary.GetComponent<BoxCollider>();
			if (boxCollider == null)
			{
				Debug.LogWarning("Boundary: " + boundary.name + "; Error: BoundaryType and Collider mismatch.");
				continue;
			}
			bool pointIsIn;
			calBoxRelations(boxCollider, point, false, out pointIsIn);

			if (pointIsIn)
				pointIsInBoundary = true;
		}

		// loop through each sphere boundary
		foreach (var boundary in sphere_boundaries)
		{
			// check if the boundary is not active. if true, skip it.
			if (boundary.isActive == false)
				continue;

			var sphereCollider = boundary.GetComponent<SphereCollider>();
			if (sphereCollider == null)
			{
				Debug.LogWarning("Boundary: " + boundary.name + "; Error: BoundaryType and Collider mismatch.");
				continue;
			}
			// check if the distance from the pivot of the boundary to the point is less then the radius
			if ((boundary.transform.position + sphereCollider.center - point).magnitude < sphereCollider.radius)
				pointIsInBoundary = true;
		}
		return pointIsInBoundary;
	}

	// Remove hbPos boundary from the list
	public static bool RemoveBoundary(Moba_Camera_Boundary boundary, BoundaryType type)
	{
		if (type == BoundaryType.cube)
			return cube_boundaries.Remove(boundary);
		if (type == BoundaryType.sphere)
			return cube_boundaries.Remove(boundary);
		return false;
	}

	public static void SetBoundaryLayerExist(bool value)
	{
		if (boundaryLayerExists)
		{
			boundaryLayerExists = false;
			Debug.LogWarning("LayerMask not set for Moba_Camera_Boundaries. Add new Layer named " + boundaryLayer + ". Check Read me for more information on recommended settings.");
		}
	}
}