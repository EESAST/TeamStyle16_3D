#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

[ExecuteInEditMode, RequireComponent(typeof(WaterBase))]
public class PlanarReflection : MonoBehaviour
{
	// reflection
	public Color clearColor = Color.grey;
	// height
	public float clipPlaneOffset = 0.07F;
	private Dictionary<Camera, bool> helperCameras;
	private Vector3 oldpos = Vector3.zero;
	private Camera reflectionCamera;
	public LayerMask reflectionMask;
	public String reflectionSampler = "_ReflectionTex";
	public bool reflectSkybox = false;
	private Material sharedMaterial;

	private static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = (1.0F - 2.0F * plane[0] * plane[0]);
		reflectionMat.m01 = (-2.0F * plane[0] * plane[1]);
		reflectionMat.m02 = (-2.0F * plane[0] * plane[2]);
		reflectionMat.m03 = (-2.0F * plane[3] * plane[0]);

		reflectionMat.m10 = (-2.0F * plane[1] * plane[0]);
		reflectionMat.m11 = (1.0F - 2.0F * plane[1] * plane[1]);
		reflectionMat.m12 = (-2.0F * plane[1] * plane[2]);
		reflectionMat.m13 = (-2.0F * plane[3] * plane[1]);

		reflectionMat.m20 = (-2.0F * plane[2] * plane[0]);
		reflectionMat.m21 = (-2.0F * plane[2] * plane[1]);
		reflectionMat.m22 = (1.0F - 2.0F * plane[2] * plane[2]);
		reflectionMat.m23 = (-2.0F * plane[3] * plane[2]);

		reflectionMat.m30 = 0.0F;
		reflectionMat.m31 = 0.0F;
		reflectionMat.m32 = 0.0F;
		reflectionMat.m33 = 1.0F;

		return reflectionMat;
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		var offsetPos = pos + normal * clipPlaneOffset;
		var m = cam.worldToCameraMatrix;
		var cpos = m.MultiplyPoint(offsetPos);
		var cnormal = m.MultiplyVector(normal).normalized * sideSign;

		return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
	}

	private Camera CreateReflectionCameraFor(Camera cam)
	{
		var reflName = gameObject.name + "Reflection" + cam.name;
		var go = GameObject.Find(reflName);

		if (!go)
			go = new GameObject(reflName, typeof(Camera));
		if (!go.GetComponent(typeof(Camera)))
			go.AddComponent(typeof(Camera));
		var reflectCamera = go.camera;

		reflectCamera.backgroundColor = clearColor;
		reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;

		SetStandardCameraParameter(reflectCamera, reflectionMask);

		if (!reflectCamera.targetTexture)
			reflectCamera.targetTexture = CreateTextureFor(cam);

		return reflectCamera;
	}

	private RenderTexture CreateTextureFor(Camera cam)
	{
		var rt = new RenderTexture(Mathf.FloorToInt(cam.pixelWidth * 0.5F), Mathf.FloorToInt(cam.pixelHeight * 0.5F), 24);
		rt.hideFlags = HideFlags.DontSave;
		return rt;
	}

	public void LateUpdate()
	{
		if (null != helperCameras)
			helperCameras.Clear();
	}

	public void OnDisable()
	{
		Shader.EnableKeyword("WATER_SIMPLE");
		Shader.DisableKeyword("WATER_REFLECTIVE");
	}

	public void OnEnable()
	{
		Shader.EnableKeyword("WATER_REFLECTIVE");
		Shader.DisableKeyword("WATER_SIMPLE");
	}

	public void RenderHelpCameras(Camera currentCam)
	{
		if (null == helperCameras)
			helperCameras = new Dictionary<Camera, bool>();

		if (!helperCameras.ContainsKey(currentCam))
			helperCameras.Add(currentCam, false);
		if (helperCameras[currentCam])
			return;

		if (!reflectionCamera)
			reflectionCamera = CreateReflectionCameraFor(currentCam);

		RenderReflectionFor(currentCam, reflectionCamera);

		helperCameras[currentCam] = true;
	}

	private void RenderReflectionFor(Camera cam, Camera reflectCamera)
	{
		if (!reflectCamera)
			return;

		if (sharedMaterial && !sharedMaterial.HasProperty(reflectionSampler))
			return;

		reflectCamera.cullingMask = reflectionMask & ~(1 << LayerMask.NameToLayer("Water"));

		SaneCameraSettings(reflectCamera);

		reflectCamera.backgroundColor = clearColor;
		reflectCamera.clearFlags = reflectSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
		if (reflectSkybox)
			if (cam.gameObject.GetComponent(typeof(Skybox)))
			{
				var sb = (Skybox)reflectCamera.gameObject.GetComponent(typeof(Skybox));
				if (!sb)
					sb = (Skybox)reflectCamera.gameObject.AddComponent(typeof(Skybox));
				sb.material = ((Skybox)cam.GetComponent(typeof(Skybox))).material;
			}

		GL.SetRevertBackfacing(true);

		var reflectiveSurface = transform; //waterHeight;

		var eulerA = cam.transform.eulerAngles;

		reflectCamera.transform.eulerAngles = new Vector3(-eulerA.x, eulerA.y, eulerA.z);
		reflectCamera.transform.position = cam.transform.position;

		var pos = reflectiveSurface.transform.position;
		pos.y = reflectiveSurface.position.y;
		var normal = reflectiveSurface.transform.up;
		var d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
		var reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

		var reflection = Matrix4x4.zero;
		reflection = CalculateReflectionMatrix(reflection, reflectionPlane);
		oldpos = cam.transform.position;
		var newpos = reflection.MultiplyPoint(oldpos);

		reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

		var clipPlane = CameraSpacePlane(reflectCamera, pos, normal, 1.0f);

		reflectCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

		reflectCamera.transform.position = newpos;
		var euler = cam.transform.eulerAngles;
		reflectCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

		reflectCamera.Render();

		GL.SetRevertBackfacing(false);
	}

	private void SaneCameraSettings(Camera helperCam)
	{
		helperCam.depthTextureMode = DepthTextureMode.None;
		helperCam.backgroundColor = Color.black;
		helperCam.clearFlags = CameraClearFlags.SolidColor;
		helperCam.renderingPath = RenderingPath.Forward;
	}

	private void SetStandardCameraParameter(Camera cam, LayerMask mask)
	{
		cam.cullingMask = mask & ~(1 << LayerMask.NameToLayer("Water"));
		cam.backgroundColor = Color.black;
		cam.enabled = false;
	}

	private static float sgn(float a)
	{
		if (a > 0.0F)
			return 1.0F;
		if (a < 0.0F)
			return -1.0F;
		return 0.0F;
	}

	public void Start() { sharedMaterial = ((WaterBase)gameObject.GetComponent(typeof(WaterBase))).sharedMaterial; }

	public void WaterTileBeingRendered(Transform tr, Camera currentCam)
	{
		RenderHelpCameras(currentCam);

		if (reflectionCamera && sharedMaterial)
			sharedMaterial.SetTexture(reflectionSampler, reflectionCamera.targetTexture);
	}
}