#region

using System.Collections;
using System.Linq;
using GameStatics;
using HighlightingSystem;
using UnityEngine;
using UnityEngine.UI;

#endregion

public abstract class EntityBehaviour : MonoBehaviour
{
	private static readonly Quaternion _defaultRotation = Quaternion.identity;
	private static readonly float _relativeSize = 1;
	protected JSONObject _info;
	private Canvas hbCanvas;
	private RawImage hbImage;
	protected RectTransform hbRect;
	private Text hbText;
	private Texture2D hbTexture;
	private Highlighter highlighter;
	public int HP = 50;
	private int lastHP;
	private RectTransform markRect;
	public int team;
	protected virtual Quaternion DefaultRotation { get { return _defaultRotation; } }
	public Vector3 Destination { get; set; }

	public JSONObject Info
	{
		get { return _info; }
		set
		{
			_info = value;
			UpdateInfo();
		}
	}

	public Vector3 Position { get { return Methods.Coordinates.InternalToExternal(transform.position); } set { transform.position = Methods.Coordinates.ExternalToInternal(value); } }
	protected virtual float RelativeSize { get { return _relativeSize; } }

	protected virtual void Awake()
	{
		Delegates.ScreenSizeChanged += RefreshMarkRect;
		Delegates.TeamColorChanged += RefreshColor;
		(hbCanvas = (Instantiate(Resources.Load("HealthBar")) as GameObject).GetComponent<Canvas>()).worldCamera = Camera.main;
		hbRect = hbCanvas.transform.FindChild("HBRect").GetComponent<RectTransform>();
		hbImage = hbRect.FindChild("HBImage").GetComponent<RawImage>();
		hbText = hbRect.FindChild("HBText").GetComponent<Text>();
		markRect = (Instantiate(Resources.Load("Mark")) as GameObject).GetComponent<RectTransform>();
		markRect.SetParent(GameObject.Find("MiniMap").transform);
		markRect.SetSiblingIndex(markRect.GetSiblingIndex() - 1);
		highlighter = gameObject.AddComponent<Highlighter>();
		gameObject.AddComponent<Rigidbody>().isKinematic = true;
		gameObject.ChangeLayer(LayerMask.NameToLayer("Entity"));
	}

	public void Deselect() { highlighter.ConstantOff(); }

	private void Destruct()
	{
		foreach (IEntityFX entityFX in GetComponentsInChildren(typeof(IEntityFX)))
			entityFX.Disable();
		highlighter.Die();
		StartCoroutine(FadeOut());
		StartCoroutine(Explode());
	}

	protected abstract Vector3 Dimensions();

	private IEnumerator Explode()
	{
		var dummy = new GameObject(name);
		var fragments = new ArrayList();
		var count = GetComponentsInChildren<MeshFilter>().Sum(meshFilter => meshFilter.mesh.triangles.Length);
		var threshold = 300 * RelativeSize / count;
		count = 0;
		foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
		{
			var mesh = meshFilter.mesh;
			for (var i = 0; i < mesh.subMeshCount; i++)
			{
				var subMeshTriangles = mesh.GetTriangles(i);
				var material = meshFilter.GetComponent<MeshRenderer>().sharedMaterials[i];
				for (var j = 0; j < subMeshTriangles.Length; j += 3)
				{
					if (Random.Range(0, 1f) > threshold)
						continue;
					Vector3 center;
					var fragmentedMesh = new Mesh
					{
						vertices = Methods.Array.Add(meshFilter.transform.TransformPoints(new[] { mesh.vertices[subMeshTriangles[j + 0]], mesh.vertices[subMeshTriangles[j + 1]], mesh.vertices[subMeshTriangles[j + 2]], mesh.vertices[subMeshTriangles[j + 0]] - mesh.normals[subMeshTriangles[j + 0]] * 0.1f * RelativeSize, mesh.vertices[subMeshTriangles[j + 1]] - mesh.normals[subMeshTriangles[j + 1]] * 0.1f * RelativeSize, mesh.vertices[subMeshTriangles[j + 2]] - mesh.normals[subMeshTriangles[j + 2]] * 0.1f * RelativeSize }, out center), -center),
						uv = new[] { mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]], mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]] },
						triangles = new[] { 0, 2, 3, 2, 5, 3, 0, 3, 1, 1, 3, 4, 1, 4, 2, 2, 4, 5, 2, 0, 1, 5, 4, 3 }
					};
					fragmentedMesh.RecalculateNormals();
					fragmentedMesh.CalculateTangents();
					var fragment = new GameObject
					{
						layer = LayerMask.NameToLayer("Fragment")
					};
					fragment.transform.position = center;
					fragment.AddComponent<MeshRenderer>().material = material;
					fragment.AddComponent<MeshFilter>().sharedMesh = fragmentedMesh;
					var meshCollider = fragment.AddComponent<MeshCollider>();
					meshCollider.sharedMesh = fragmentedMesh;
					meshCollider.convex = true;
					var rigidBody = fragment.AddComponent<Rigidbody>();
					rigidBody.isKinematic = true;
					rigidBody.SetDensity(Mathf.Pow(8 / RelativeSize, 4));
					fragment.transform.parent = dummy.transform;
					var smokeTrail = Instantiate(Resources.Load("Smoke Trail")) as GameObject;
					smokeTrail.transform.SetParent(fragment.transform, false);
					fragments.Add(fragment);
					if (count++ % 5 == 0)
						yield return null;
				}
			}
		}
		foreach (GameObject fragment in fragments)
			fragment.AddComponent<FragmentManager>();
		Instantiate(Resources.Load("Detonator"), rigidbody.worldCenterOfMass, Quaternion.identity);
		Destroy(gameObject);
	}

	private IEnumerator FadeOut()
	{
		var markImage = markRect.GetComponent<RawImage>();
		var c1 = markImage.color;
		var c2 = hbImage.color;
		var c3 = hbText.color;
		while ((c1.a *= 0.9f) + (c2.a *= 0.9f) + (c3.a *= 0.9f) > Mathf.Epsilon)
		{
			markImage.color = c1;
			hbImage.color = c2;
			hbText.color = c3;
			yield return new WaitForSeconds(0.02f);
		}
	}

	protected abstract int MaxHP();

	public void MouseOver() { highlighter.On(Data.TeamColor.Current[team]); }

	protected void OnDestroy()
	{
		Delegates.ScreenSizeChanged -= RefreshMarkRect;
		Delegates.TeamColorChanged -= RefreshColor;
		if (hbRect)
			Destroy(hbRect.gameObject);
		if (markRect)
			Destroy(markRect.gameObject);
	}

	protected void OnGUI()
	{
		/*#region Draw Info Panel

		if (!mouseOver || Screen.lockCursor)
			return;
		var infoPanel = new Rect(Input.mousePosition.x - 110, Screen.height - Input.mousePosition.y - 110, 100, Info.Count * Settings.UI.FontSize);
		if (infoPanel.x < 0)
			infoPanel.x = 0;
		if (infoPanel.y < 0)
			infoPanel.y = 0;
		//infoPanel.x = Mathf.Min(Screen.width - infoPanel.width, infoPanel.x);
		infoPanel.y = Mathf.Min(Screen.height - infoPanel.height, infoPanel.y);
		GUI.BeginGroup(infoPanel);
		var r = new Rect(0, 0, infoPanel.width, infoPanel.height);
		GUI.Box(r, "");
		r.height = Settings.UI.FontSize;
		for (var i = 0; i < Info.Count; ++i)
		{
			GUI.Label(r, Info[i].ToString());
			r.y += Settings.UI.FontSize;
		}
		GUI.EndGroup();

		#endregion*/
	}

	private void RefreshColor() { highlighter.ConstantParams(markRect.GetComponent<RawImage>().color = Data.TeamColor.Current[team]); }

	private void RefreshMarkRect()
	{
		markRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, RelativeSize * Data.MiniMap.ScaleFactor);
		markRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, RelativeSize * Data.MiniMap.ScaleFactor);
	}

	public virtual void Select()
	{
		var cameraSettings = Camera.main.GetComponentInParent<Moba_Camera>().settings;
		cameraSettings.lockTargetTransform = transform;
		cameraSettings.cameraLocked = true;
		highlighter.ConstantOnImmediate(Data.TeamColor.Current[team]);
		Destruct();
	}

	protected virtual void Start()
	{
		transform.rotation = DefaultRotation;
		transform.localScale = Vector3.one * RelativeSize * Settings.ScaleFactor * 2 / ((Dimensions().x + Dimensions().z));
		var hbImageRect = hbImage.GetComponent<RectTransform>();
		hbImageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxHP());
		hbImageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 10);
		var hbTextRect = hbText.GetComponent<RectTransform>();
		hbTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Settings.UI.TextGranularity * MaxHP());
		hbTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Settings.UI.TextGranularity * 10);
		hbTextRect.localScale = Vector2.one / Settings.UI.TextGranularity;
		hbImage.texture = hbTexture = new Texture2D(MaxHP(), 4)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		for (var i = 0; i < MaxHP(); i++)
			for (var j = 0; j < hbTexture.height; j++)
				if (j != 1)
					hbTexture.SetPixel(i, j, Settings.HealthBar.EmptyColor);
	}

	private void Update()
	{
		markRect.anchoredPosition = Methods.Coordinates.InternalToMiniMapBasedScreen(transform.position);

		#region Update Health Bar

		if (HP != lastHP)
		{
			if (HP < lastHP)
			{
				for (var i = HP; i < lastHP; i++)
					for (var j = 0; j < hbTexture.height; j++)
						if (j != 1)
							hbTexture.SetPixel(i, j, Settings.HealthBar.EmptyColor);
			}
			else
				for (var i = lastHP; i < HP; i++)
					for (var j = 0; j < 4; j++)
						if (j != 1)
							hbTexture.SetPixel(i, j, Settings.HealthBar.FullColor);
			hbTexture.Apply();
			lastHP = HP;
		}
		var hbPos = Camera.main.WorldToScreenPoint(rigidbody.worldCenterOfMass + Vector3.up * (Dimensions().y / 2 + Settings.HealthBar.VerticalPositionOffset) * transform.lossyScale.y);
		hbCanvas.planeDistance = hbPos.z;
		hbRect.anchoredPosition = hbPos;
		hbRect.localScale = Vector2.one * 10 / Mathf.Clamp(hbPos.z / Settings.ScaleFactor, 3, 15);
		hbText.color = Color.Lerp(Color.white, Color.clear, Mathf.Clamp01((hbPos.z / Settings.ScaleFactor - 8) / 2));
		hbText.text = HP + "/" + MaxHP();

		#endregion
	}

	protected virtual void UpdateInfo()
	{
		if (_info["pos"]["__class__"].str == "Rectangle")
		{
			var lowerRight = Methods.Coordinates.JSONToExternal(_info["pos"]["lower_right"]);
			var upperLeft = Methods.Coordinates.JSONToExternal(_info["pos"]["upper_left"]);
			Position = (lowerRight + upperLeft) / 2;
		}
		else
			Position = Methods.Coordinates.JSONToExternal(_info["pos"]);
	}
}