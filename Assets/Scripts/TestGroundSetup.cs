#region

using GameStatics;
using UnityEngine;

#endregion

public class TestGroundSetup : MonoBehaviour
{
	private void Awake()
	{
		QualitySettings.shadowDistance = Settings.ShadowDistance;

		var cameraRequirements = Camera.main.GetComponentInParent<Moba_Camera>().requirements;
		cameraRequirements.camera = Camera.main;
		cameraRequirements.offset = Camera.main.transform.parent;
		cameraRequirements.pivot = Camera.main.transform.root;

		var cameraSettings = Camera.main.GetComponentInParent<Moba_Camera>().settings;
		cameraSettings.movement.cameraMovementRate = Settings.Camera.Movement.Rate;
		cameraSettings.movement.defaultHeight = Settings.Camera.Movement.DefaultHeight;
		cameraSettings.rotation.lockRotationY = cameraSettings.rotation.lockRotationX = Settings.Camera.Rotation.Locked;
		cameraSettings.rotation.cameraRotationAutoRevert = Settings.Camera.Rotation.AutoRevert;
		cameraSettings.zoom.maxZoom = Settings.Camera.Zoom.Max;
		cameraSettings.zoom.minZoom = Settings.Camera.Zoom.Min;
		cameraSettings.zoom.defaultZoom = Settings.Camera.Zoom.Default;
		cameraSettings.zoom.zoomRate = Settings.Camera.Zoom.Rate;
	}
}