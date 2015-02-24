#region

using System;
using UnityEngine;

#endregion

////////////////////////////////////////////////////////////////////////////////////////////////////
// Helper Classes

////////////////////////////////////////////////////////////////////////////////////////////////////
// Moba_Camera Class
public class Moba_Camera : MonoBehaviour
{
	private const float MAXROTATIONXAXIS = 89.9f;
	private const float MINROTATIONXAXIS = -89.9f;
	private Vector2 _currentCameraRotation;
	private float _currentZoomAmount;
	private Vector3 _forceDestination = Vector3.zero;
	private bool changeInCamera = true;
	public Moba_Camera_Inputs inputs = new Moba_Camera_Inputs();
	public bool isForcedMoving;
	private float mouseWheelDownTime;
	public Moba_Camera_Requirements requirements = new Moba_Camera_Requirements();
	public Moba_Camera_Settings settings = new Moba_Camera_Settings();
	private bool shallRevertZoom;
	private float targetZoomAmount;
	// the current Camera Rotation

	public Vector2 currentCameraRotation
	{
		get { return _currentCameraRotation; }
		set
		{
			_currentCameraRotation = value;
			changeInCamera = true;
		}
	}

	public float currentZoomAmount
	{
		get { return _currentZoomAmount; }
		set
		{
			_currentZoomAmount = value;
			changeInCamera = true;
		}
	}

	public Vector3 ForceDestination
	{
		private get
		{
			var tmp = _forceDestination == Vector3.zero ? requirements.pivot.position : _forceDestination;
			tmp.y = settings.movement.useDefaultHeight ? settings.movement.defaultHeight : requirements.pivot.position.y;
			return tmp;
		}
		set
		{
			_forceDestination = value;
			settings.cameraLocked = false;
		}
	}

	private void CalculateCameraBoundaries()
	{
		if (settings.useBoundaries) // check if the pivot is not in hbPos boundary
			if (!Moba_Camera_Boundaries.isPointInBoundary(requirements.pivot.position))
			{
				// Get the closet boundary to the pivot
				var boundary = Moba_Camera_Boundaries.GetClosestBoundary(requirements.pivot.position);
				if (boundary != null) // set the pivot's screenPoint to the closet point on the boundary
					requirements.pivot.position = Moba_Camera_Boundaries.GetClosestPointOnBoundary(boundary, requirements.pivot.position);
			}
	}

	private void CalculateCameraMovement()
	{
		if ((inputs.useKeyCodeInputs ? Input.GetKeyDown(inputs.keycodes.LockCamera) : Input.GetButtonDown(inputs.axis.button_lock_camera)) && settings.lockTargetTransform != null)
			settings.cameraLocked = !settings.cameraLocked;
		if (isForcedMoving)
		{
			if ((ForceDestination - requirements.pivot.position).magnitude > settings.tolerance)
				requirements.pivot.position = Vector3.Lerp(requirements.pivot.position, ForceDestination, settings.movement.transitionRate * Time.smoothDeltaTime);
		}
		else if (settings.lockTargetTransform != null && (settings.cameraLocked || (inputs.useKeyCodeInputs ? Input.GetKey(inputs.keycodes.characterFocus) : Input.GetButton(inputs.axis.button_char_focus))))
		{
			var target = settings.lockTargetTransform.WorldCenterOfElement();
			if (!settings.movement.useLockTargetHeight)
				target.y = settings.movement.useDefaultHeight ? settings.movement.defaultHeight : requirements.pivot.position.y;
			if ((target - requirements.pivot.position).magnitude > settings.tolerance)
				requirements.pivot.position = Vector3.Lerp(requirements.pivot.position, target, settings.movement.transitionRate * Time.smoothDeltaTime);
			_forceDestination = Vector3.zero;
		}
		else
		{
			var movementVector = Vector3.zero;
			if ((Input.mousePosition.x < settings.movement.edgeHoverOffset && settings.movement.edgeHoverMovement) || ((inputs.useKeyCodeInputs) ? (Input.GetKey(inputs.keycodes.CameraMoveLeft)) : (Input.GetButton(inputs.axis.button_camera_move_left))))
				movementVector += requirements.pivot.transform.right;
			if ((Input.mousePosition.x > Screen.width - settings.movement.edgeHoverOffset && settings.movement.edgeHoverMovement) || ((inputs.useKeyCodeInputs) ? (Input.GetKey(inputs.keycodes.CameraMoveRight)) : (Input.GetButton(inputs.axis.button_camera_move_right))))
				movementVector -= requirements.pivot.transform.right;
			if ((Input.mousePosition.y < settings.movement.edgeHoverOffset && settings.movement.edgeHoverMovement) || ((inputs.useKeyCodeInputs) ? (Input.GetKey(inputs.keycodes.CameraMoveBackward)) : (Input.GetButton(inputs.axis.button_camera_move_backward))))
				movementVector += requirements.pivot.transform.forward;
			if ((Input.mousePosition.y > Screen.height - settings.movement.edgeHoverOffset && settings.movement.edgeHoverMovement) || ((inputs.useKeyCodeInputs) ? (Input.GetKey(inputs.keycodes.CameraMoveForward)) : (Input.GetButton(inputs.axis.button_camera_move_forward))))
				movementVector -= requirements.pivot.transform.forward;
			if (movementVector != Vector3.zero)
			{
				requirements.pivot.position += movementVector.normalized * settings.movement.cameraMovementRate * Time.smoothDeltaTime;
				_forceDestination = Vector3.zero;
			}
			if ((ForceDestination - requirements.pivot.position).magnitude > settings.tolerance)
				requirements.pivot.position = Vector3.Lerp(requirements.pivot.position, ForceDestination, settings.movement.transitionRate * Time.smoothDeltaTime);
		}
	}

	private void CalculateCameraRotation()
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////
		// Camera rotate
		if (inputs.useKeyCodeInputs ? Input.GetKeyDown(inputs.keycodes.RotateCamera) : Input.GetButtonDown(inputs.axis.button_rotate_camera))
			mouseWheelDownTime = Time.time;
		if ((inputs.useKeyCodeInputs ? Input.GetKey(inputs.keycodes.RotateCamera) : (Input.GetButton(inputs.axis.button_rotate_camera))) && Time.time - mouseWheelDownTime > settings.rotation.thresholdTime)
		{
			float changeInRotationX = 0;
			float changeInRotationY = 0;
			Screen.lockCursor = true;
			// Lock the cursor to the pivot of the screen and hide the cursor
			if (!settings.rotation.lockRotationX)
			{
				var deltaMouseVertical = Input.GetAxis(inputs.axis.DeltaMouseVertical);
				if (Mathf.Abs(deltaMouseVertical) > settings.tolerance)
					if (settings.rotation.constRotationRate)
						changeInRotationX = Mathf.Sign(deltaMouseVertical);
					else
						changeInRotationX = deltaMouseVertical;
			}
			if (!settings.rotation.lockRotationY)
			{
				var deltaMouseHorizontal = Input.GetAxis(inputs.axis.DeltaMouseHorizontal);
				if (Mathf.Abs(deltaMouseHorizontal) > settings.tolerance)
					if (settings.rotation.constRotationRate)
						changeInRotationY = Mathf.Sign(deltaMouseHorizontal);
					else
						changeInRotationY = deltaMouseHorizontal;
			}
			var deltaCameraRotation = Vector2.Scale(new Vector2(changeInRotationX, changeInRotationY), settings.rotation.cameraRotationRate) * Time.smoothDeltaTime;
			if (deltaCameraRotation.magnitude > settings.tolerance)
				currentCameraRotation += deltaCameraRotation;
		}
		else
		{
			Screen.lockCursor = false;
			if (settings.rotation.cameraRotationAutoRevert && (settings.rotation.defaultRotation - _currentCameraRotation).magnitude > settings.tolerance)
				currentCameraRotation = Vector2.Lerp(_currentCameraRotation, settings.rotation.defaultRotation, settings.rotation.transitionRate * Time.smoothDeltaTime);
		}
	}

	private void CalculateCameraUpdates()
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////
		// Update the cameraPivot screenPoint relative to the pivot if there was hbPos change in the cameraPivot transforms
		// if there is no change in the cameraPivot exit update
		if (!changeInCamera)
			return;

		// Check if the fMaxZoomVal is greater than the fMinZoomVal
		if (settings.zoom.maxZoom < settings.zoom.minZoom)
			settings.zoom.maxZoom = settings.zoom.minZoom + 1;

		// Check if Camera Zoom is between the min and max
		if (targetZoomAmount < settings.zoom.minZoom)
			targetZoomAmount = settings.zoom.minZoom;
		if (targetZoomAmount > settings.zoom.maxZoom)
			targetZoomAmount = settings.zoom.maxZoom;

		// Restrict rotationOffsetToLocal X value
		if (_currentCameraRotation.x > MAXROTATIONXAXIS)
			_currentCameraRotation.x = MAXROTATIONXAXIS;
		else if (_currentCameraRotation.x < MINROTATIONXAXIS)
			_currentCameraRotation.x = MINROTATIONXAXIS;

		// Calculate the new screenPoint of the cameraPivot
		// rotate pivot by the change int cameraPivot 
		var forwardRotation = Quaternion.AngleAxis(_currentCameraRotation.y, Vector3.up) * Vector3.forward;
		requirements.pivot.transform.rotation = Quaternion.LookRotation(forwardRotation);

		//requirements.pivot.transform.Rotation(Vector3.up, changeInRotationY);

		var CamVec = requirements.pivot.transform.TransformDirection(Vector3.forward);

		// Apply Camera Rotations
		CamVec = Quaternion.AngleAxis(_currentCameraRotation.x, requirements.pivot.transform.TransformDirection(Vector3.right)) * CamVec;
		//CamVec = Quaternion.AngleAxis(_currentCameraRotation.externalY, Vector3.up) * CamVec;

		// Move cameraPivot along CamVec by ZoomAmount
		requirements.offset.position = CamVec * _currentZoomAmount + requirements.pivot.position;

		// Make Camera look at the pivot
		requirements.offset.transform.LookAt(requirements.pivot);

		// reset the change in cameraPivot value to false
		changeInCamera = false;
	}

	private void CalculateCameraZoom()
	{
		////////////////////////////////////////////////////////////////////////////////////////////////////
		if ((inputs.useKeyCodeInputs ? Input.GetKeyUp(inputs.keycodes.RotateCamera) : Input.GetButtonUp(inputs.axis.button_rotate_camera)) && Time.time - mouseWheelDownTime < settings.zoom.thresholdTime)
			shallRevertZoom = true;
		// Camera Zoom In/Out
		var inverted = 1;

		var mouseScrollWheel = Input.GetAxis(inputs.axis.DeltaScrollWheel);
		if (Mathf.Abs(mouseScrollWheel) > settings.tolerance)
		{
			// Set the hbPos cameraPivot value has changed
			shallRevertZoom = false;
			float zoomChange;
			if (settings.zoom.constZoomRate)
				if (mouseScrollWheel > 0)
					zoomChange = 0.2f;
				else
					zoomChange = -0.2f;
			else
				zoomChange = mouseScrollWheel;
			// change the zoom amount based on if zoom is inverted
			if (!settings.zoom.invertZoom)
				inverted = -1;

			targetZoomAmount += zoomChange * settings.zoom.zoomRate * inverted * Time.smoothDeltaTime;
		}
		if (shallRevertZoom)
		{
			if (Math.Abs(settings.zoom.defaultZoom - _currentZoomAmount) > settings.tolerance)
				targetZoomAmount = currentZoomAmount = Mathf.Lerp(_currentZoomAmount, settings.zoom.defaultZoom, settings.zoom.transitionRate * Time.smoothDeltaTime);
		}
		else if (Mathf.Abs(targetZoomAmount - _currentZoomAmount) > settings.tolerance)
			currentZoomAmount = Mathf.Lerp(_currentZoomAmount, targetZoomAmount, settings.zoom.transitionRate * Time.smoothDeltaTime);
	}

	//////////////////////////////////////////////////////////////////////////////////////////
	// Get Variables from outside script
	public Camera GetCamera() { return requirements.camera; }

	private void LateUpdate()
	{
		CalculateCameraZoom();
		CalculateCameraRotation();
		CalculateCameraMovement();
		CalculateCameraUpdates();
		CalculateCameraBoundaries();
	}

	public void SetCameraRotation(Vector2 rotation) { currentCameraRotation = new Vector2(rotation.x, rotation.y); }

	public void SetCameraRotation(float x, float y) { currentCameraRotation = new Vector2(x, y); }

	public void SetCameraZoom(float amount) { currentZoomAmount = amount; }

	//////////////////////////////////////////////////////////////////////////////////////////
	// Class functions

	//////////////////////////////////////////////////////////////////////////////////////////
	// Set Variables from outside script
	public void SetTargetTransform(Transform t)
	{
		if (transform != null)
			settings.lockTargetTransform = t;
	}

	// True if either the zoom amount or the camera rotation value changed

	// Use this for initialization
	private void Start()
	{
		if (!requirements.pivot || !requirements.offset || !requirements.camera)
		{
			var missingRequirements = "";
			if (requirements.pivot == null)
			{
				missingRequirements += " / Pivot";
				enabled = false;
			}

			if (requirements.offset == null)
			{
				missingRequirements += " / Offset";
				enabled = false;
			}

			if (requirements.camera == null)
			{
				missingRequirements += " / Camera";
				enabled = false;
			}
			Debug.LogWarning("Moba_Camera Requirements Missing" + missingRequirements + ". Add missing objects to the requirement tab under the Moba_camera script in the Inspector.");
			Debug.LogWarning("Moba_Camera script requires two empty gameobjects, Pivot and Offset, and hbPos cameraPivot." + "Parent the Offset to the Pivot and the Camera to the Offset. See the Moba_Camera Readme for more information on setup.");
		}

		// set values to the default values
		targetZoomAmount = _currentZoomAmount = settings.zoom.defaultZoom;
		_currentCameraRotation = settings.rotation.defaultRotation;

		// if using the default height
		if (settings.movement.useDefaultHeight && enabled)
		{
			// set the pivots height to the default height
			var tmp = requirements.pivot.transform.position;
			tmp.y = settings.movement.defaultHeight;
			requirements.pivot.transform.position = tmp;
		}
	}
}