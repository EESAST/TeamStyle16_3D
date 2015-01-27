#region

using GameStatics;
using UnityEngine;

#endregion

public class Setup : MonoBehaviour
{
	public Material[] skyBoxes;

	private void Awake()
	{
		QualitySettings.shadowDistance = Settings.ShadowDistance;
		RenderSettings.skybox = skyBoxes[Random.Range(0, skyBoxes.Length)];
		Physics.gravity = Vector3.down * Settings.ScaleFactor;

		var xMax = Mathf.RoundToInt(Data.BattleData["gamebody"]["map_info"]["x_max"].n);
		var yMax = Mathf.RoundToInt(Data.BattleData["gamebody"]["map_info"]["y_max"].n);
		Data.MapSize = new Vector2(xMax, yMax);
		Data.IsOccupied = new bool[xMax, yMax];
		Data.SeaLevel = Settings.HeightOfLayer[0] + Settings.Sea.VerticalPositionOffset;
		Data.TeamColor.Current = new Color[4];

		var cameraBoundary = GameObject.Find("CameraBoundary").GetComponent<BoxCollider>();
		cameraBoundary.size = new Vector3(Data.MapSize.y - 1, 0, Data.MapSize.x - 1) * Settings.ScaleFactor + Vector3.up * Settings.DeltaHeight * 2;
		Camera.main.farClipPlane = Settings.Camera.FarClipPlane;
		Camera.main.transform.root.position = cameraBoundary.transform.position = Methods.Coordinates.ExternalToInternal((Data.MapSize - Vector2.one) / 2) + Vector3.up * Settings.DeltaHeight;

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