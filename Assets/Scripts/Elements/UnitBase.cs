#region

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#endregion

public abstract class UnitBase : Element
{
	private float currentAmmo;
	private float currentHP;
	private Canvas hbCanvas;
	private int hbHorizontalPixelNumber;
	private RawImage hbImage;
	private Color32[] hbPixels;
	private RectTransform hbRect;
	private Text hbText;
	private Texture2D hbTexture;
	private int lastHPIndex;
	public int targetAmmo;
	public int targetHP;

	protected override void Awake()
	{
		base.Awake();
		(hbCanvas = (Instantiate(Resources.Load("HealthBar")) as GameObject).GetComponent<Canvas>()).worldCamera = Camera.main;
		hbRect = hbCanvas.transform.FindChild("HBRect").GetComponent<RectTransform>();
		hbImage = hbRect.FindChild("HBImage").GetComponent<RawImage>();
		hbText = hbRect.FindChild("HBText").GetComponent<Text>();
		hbHorizontalPixelNumber = Mathf.RoundToInt(Mathf.Pow(MaxHP(), 0.25f) * 25);
	}

	protected override void Destruct()
	{
		base.Destruct();
		StartCoroutine(Explode());
	}

	private IEnumerator Explode()
	{
		var dummy = new GameObject(name);
		var meshFilters = GetComponentsInChildren<MeshFilter>();
		var threshold = 3 * RelativeSize / Mathf.Pow(meshFilters.Sum(meshFilter => meshFilter.mesh.triangles.Length), 0.6f);
		var count = 0;
		var thickness = Settings.Fragment.ThicknessPerUnitSize * RelativeSize * Settings.ScaleFactor;
		var desiredAverageMass = Mathf.Pow(RelativeSize * Settings.ScaleFactor * 0.12f, 3);
		var totalMass = 0f;
		foreach (var meshFilter in meshFilters)
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
					var fragmentedMesh = new Mesh { vertices = Methods.Array.Add(meshFilter.transform.TransformPoints(new[] { mesh.vertices[subMeshTriangles[j + 0]], mesh.vertices[subMeshTriangles[j + 1]], mesh.vertices[subMeshTriangles[j + 2]], mesh.vertices[subMeshTriangles[j + 0]] - mesh.normals[subMeshTriangles[j + 0]] * thickness, mesh.vertices[subMeshTriangles[j + 1]] - mesh.normals[subMeshTriangles[j + 1]] * thickness, mesh.vertices[subMeshTriangles[j + 2]] - mesh.normals[subMeshTriangles[j + 2]] * thickness }, out center), -center), uv = new[] { mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]], mesh.uv[subMeshTriangles[j + 0]], mesh.uv[subMeshTriangles[j + 1]], mesh.uv[subMeshTriangles[j + 2]] }, triangles = new[] { 0, 2, 3, 2, 5, 3, 0, 3, 1, 1, 3, 4, 1, 4, 2, 2, 4, 5, 2, 0, 1, 5, 4, 3 } };
					fragmentedMesh.RecalculateNormals();
					fragmentedMesh.CalculateTangents();
					var fragment = Instantiate(Resources.Load("Fragment"), center, Quaternion.identity) as GameObject;
					fragment.GetComponent<MeshCollider>().sharedMesh = fragment.GetComponent<MeshFilter>().sharedMesh = fragmentedMesh;
					fragment.GetComponent<MeshRenderer>().material = material;
					fragment.rigidbody.SetDensity(1000);
					totalMass += fragment.rigidbody.mass;
					fragment.transform.parent = dummy.transform;
					var smokeTrail = fragment.GetComponentInChildren<ParticleEmitter>();
					smokeTrail.maxSize = smokeTrail.minSize = thickness * 3;
					if (++count % 5 == 0)
						yield return null;
				}
			}
		}
		var ratio = desiredAverageMass * count / totalMass;
		foreach (var fragmentManager in dummy.GetComponentsInChildren<FragmentManager>())
		{
			fragmentManager.rigidbody.mass *= ratio;
			fragmentManager.enabled = true;
		}
		var detonator = Instantiate(Resources.Load("Detonator"), transform.TransformPoint(Center()), Quaternion.identity) as GameObject;
		detonator.GetComponent<Detonator>().size = RelativeSize * Settings.ScaleFactor;
		detonator.GetComponent<DetonatorForce>().power = Mathf.Pow(RelativeSize, 2.5f) * Mathf.Pow(Settings.ScaleFactor, 3);
		Destroy(gameObject);
	}

	protected override IEnumerator FadeOut()
	{
		var markImage = markRect.GetComponent<RawImage>();
		var c1 = markImage.color;
		var c2 = hbImage.color;
		var c3 = hbText.color;
		while ((c1.a *= 0.8f) + (c2.a *= 0.8f) + (c3.a *= 0.8f) > Mathf.Epsilon)
		{
			markImage.color = c1;
			hbImage.color = c2;
			hbText.color = c3;
			yield return new WaitForSeconds(0.04f);
		}
	}

	public void FireAtElement(UnitBase targetElement, int damage)
	{
		FireAtPosition(targetElement.transform.WorldCenterOfEntity());
		targetElement.targetHP -= damage;
	}

	public void FireAtPosition(Vector3 internalTargetPosition) { targetAmmo -= Constants.AmmoOnce[Kind()]; //TODO:run this when firing is actually animated; firing animation;
	}

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		team = Mathf.RoundToInt(info["team"].n);
		currentHP = targetHP = Mathf.RoundToInt(info["health"].n);
		currentFuel = targetFuel = Mathf.RoundToInt(info["fuel"].n);
		currentAmmo = targetAmmo = float.IsPositiveInfinity(info["ammo"].n) ? -1 : Mathf.RoundToInt(info["ammo"].n);
		currentMetal = targetMetal = Mathf.RoundToInt(info["metal"].n);
	}

	protected abstract int MaxHP();

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (hbRect)
			Destroy(hbRect.gameObject);
	}

	protected override void OnGUI()
	{
		base.OnGUI();
		if (!MouseOver || Screen.lockCursor)
			return;
		GUILayout.BeginArea(new Rect(Input.mousePosition.x - Screen.width * 0.06f, Screen.height - Input.mousePosition.y - Screen.height * 0.12f, Screen.width * 0.12f, Screen.height * 0.24f), GUI.skin.box);
		GUILayout.Label("燃料：" + Mathf.RoundToInt(currentFuel), Data.GUI.Label.SmallLeft);
		GUILayout.FlexibleSpace();
		if (float.IsInfinity(currentAmmo))
			print(Time.time);
		GUILayout.Label("弹药：" + (currentAmmo < 0 ? "无限" : Mathf.RoundToInt(currentAmmo).ToString()), Data.GUI.Label.SmallLeft);
		GUILayout.FlexibleSpace();
		GUILayout.Label("金属：" + Mathf.RoundToInt(currentMetal), Data.GUI.Label.SmallLeft);
		GUILayout.EndArea();
	}

	protected override void Start()
	{
		base.Start();
		var hbImageRect = hbImage.GetComponent<RectTransform>();
		hbImageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hbHorizontalPixelNumber);
		hbImageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 10);
		var hbTextRect = hbText.GetComponent<RectTransform>();
		hbTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Settings.TextGranularity * hbHorizontalPixelNumber);
		hbTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Settings.TextGranularity * 10);
		hbTextRect.localScale = Vector2.one / Settings.TextGranularity;
		hbImage.texture = hbTexture = new Texture2D(hbHorizontalPixelNumber, 4) { wrapMode = TextureWrapMode.Clamp };
		hbPixels = hbTexture.GetPixels32();
		for (var i = 0; i < hbHorizontalPixelNumber; i++)
			for (var j = 0; j < 4; j++)
				if (j != 1)
					hbPixels[i + hbHorizontalPixelNumber * j] = Settings.HealthBar.EmptyColor;
		hbTexture.SetPixels32(hbPixels);
		hbTexture.Apply();
	}

	protected override void Update()
	{
		base.Update();
		if (Mathf.Abs(targetHP - currentHP) > Settings.Tolerance)
			currentHP = Mathf.Lerp(currentHP, targetHP, 3 * Time.deltaTime);
		if (Mathf.Abs(targetAmmo - currentAmmo) > Settings.Tolerance)
			currentAmmo = Mathf.Lerp(currentAmmo, targetAmmo, 3 * Time.deltaTime);

		#region Update Health Bar

		var hpIndex = Mathf.RoundToInt(currentHP / MaxHP() * hbHorizontalPixelNumber);
		if (hpIndex != lastHPIndex)
		{
			for (var i = Mathf.Min(hpIndex, lastHPIndex); i < Mathf.Max(hpIndex, lastHPIndex); i++)
				for (var j = 0; j < 4; j++)
					if (j != 1)
						hbPixels[i + hbHorizontalPixelNumber * j] = hpIndex < lastHPIndex ? Settings.HealthBar.EmptyColor : Settings.HealthBar.FullColor;
			hbTexture.SetPixels32(hbPixels);
			hbTexture.Apply();
			lastHPIndex = hpIndex;
		}
		var hbPos = Camera.main.WorldToScreenPoint(transform.TransformPoint(Center()) + Vector3.up * (Dimensions().y / 2 + Settings.HealthBar.VerticalPositionOffset) * transform.lossyScale.y);
		hbCanvas.planeDistance = hbPos.z;
		hbRect.anchoredPosition = hbPos;
		hbRect.localScale = Vector2.one * Monitor.ScreenSize.x / 100 / Mathf.Clamp(hbPos.z / Settings.ScaleFactor, 3, 15);
		if (!isDead)
			hbText.color = new Color(1, 1, 1, Mathf.Clamp01(5 - hbPos.z / Settings.ScaleFactor / 2));
		hbText.text = Mathf.RoundToInt(currentHP) + "/" + MaxHP();

		#endregion

		if (currentHP <= 0 && !isDead)
			Destruct();
	}
}