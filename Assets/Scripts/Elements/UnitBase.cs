#region

using System.Collections;
using System.Linq;
using JSON;
using UnityEngine;
using UnityEngine.UI;

#endregion

public abstract class UnitBase : Element
{
	private float currentAmmo;
	private float currentHP;
	public int explosionsLeft;
	private Canvas hbCanvas;
	private int hbHorizontalPixelNumber;
	private RawImage hbImage;
	private Color32[] hbPixels;
	private RectTransform hbRect;
	private Text hbText;
	private Texture2D hbTexture;
	protected bool isAiming;
	private int lastHPIndex;
	protected int targetAmmo;
	public int targetHP;

	protected abstract IEnumerator AimAtPosition(Vector3 targetPosition);

	protected abstract int AmmoOnce();

	public IEnumerator AttackPosition(Vector3 targetPosition)
	{
		isAiming = true;
		yield return StartCoroutine(AimAtPosition(targetPosition));
		targetAmmo -= AmmoOnce();
		yield return StartCoroutine(FireAtPosition(targetPosition));
		yield return StartCoroutine(Data.Replay.Instance.ShowMessageAt(targetPosition + Settings.MessagePositionOffset, "Miss..."));
	}

	public IEnumerator AttackUnitBase(UnitBase targetUnitBase, int damage)
	{
		isAiming = true;
		Vector3 targetPosition;
		yield return StartCoroutine(AimAtPosition(targetPosition = targetUnitBase.transform.WorldCenterOfElement()));
		targetAmmo -= AmmoOnce();
		if (targetUnitBase)
		{
			yield return StartCoroutine(FireAtUnitBase(targetUnitBase));
			targetUnitBase.targetHP -= damage;
		}
		else
			yield return StartCoroutine(FireAtPosition(targetPosition));
		Data.Replay.TargetScores[team] += Constants.Score.PerDamage * damage;
	}

	protected override void Awake()
	{
		base.Awake();
		(hbCanvas = (Instantiate(Resources.Load("HealthBar")) as GameObject).GetComponent<Canvas>()).worldCamera = Camera.main;
		hbRect = hbCanvas.transform.Find("HBRect").GetComponent<RectTransform>();
		hbImage = hbRect.Find("HBImage").GetComponent<RawImage>();
		hbText = hbRect.Find("HBText").GetComponent<Text>();
		hbHorizontalPixelNumber = Mathf.RoundToInt(Mathf.Pow(MaxHP(), 0.25f) * 10);
	}

	protected override void Destruct()
	{
		base.Destruct();
		StartCoroutine(Explode());
	}

	private IEnumerator Explode()
	{
		var dummy = Instantiate(Resources.Load("Dummy"), transform.TransformPoint(Center()), Quaternion.identity) as GameObject;
		var meshFilters = GetComponentsInChildren<MeshFilter>();
		var threshold = 5 * RelativeSize / Mathf.Pow(meshFilters.Sum(meshFilter => meshFilter.mesh.triangles.Length), 0.6f);
		var count = 0;
		var thickness = Settings.Fragment.ThicknessPerUnitSize * RelativeSize;
		var desiredAverageMass = Mathf.Pow(RelativeSize * Settings.DimensionScaleFactor * 0.12f, 3);
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
					if (++count % 10 == 0)
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
		dummy.audio.maxDistance = Settings.Audio.MaxAudioDistance;
		dummy.audio.volume = RelativeSize == 3 ? Settings.Audio.Volume.Death3 : Settings.Audio.Volume.Death1;
		dummy.audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/Death_" + RelativeSize));
		var detonator = Instantiate(Resources.Load("Detonator_Death"), transform.TransformPoint(Center()), Quaternion.identity) as GameObject;
		detonator.GetComponent<Detonator>().size = RelativeSize * Settings.DimensionScaleFactor;
		detonator.GetComponent<DetonatorForce>().power = Mathf.Pow(RelativeSize, 2.5f) * Mathf.Pow(Settings.DimensionScaleFactor, 3);
		foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
			meshRenderer.collider.enabled = meshRenderer.enabled = false;
		Destroy(dummy, Settings.Fragment.MaxLifeSpan * 2);
		while (explosionsLeft > 0)
			yield return null;
		var carrier = this as Carrier;
		if (carrier && carrier.movingInterceptorsLeft > 0)
			carrier.ForceDestructInterceptors();
		Destroy(gameObject, Settings.DeltaTime);
	}

	protected override IEnumerator FadeOut()
	{
		var markImage = markRect.GetComponent<RawImage>();
		var c1 = markImage.color;
		var c2 = hbImage.color;
		var c3 = hbText.color;
		while ((c1.a *= Settings.FastAttenuation) + (c2.a *= Settings.FastAttenuation) + (c3.a *= Settings.FastAttenuation) > Mathf.Epsilon)
		{
			markImage.color = c1;
			hbImage.color = c2;
			hbText.color = c3;
			yield return new WaitForSeconds(Settings.DeltaTime);
		}
	}

	protected abstract IEnumerator FireAtPosition(Vector3 targetPosition);

	protected abstract IEnumerator FireAtUnitBase(UnitBase targetUnitBase);

	public override void Initialize(JSONObject info)
	{
		base.Initialize(info);
		team = Mathf.RoundToInt(info["team"].n);
		if (team < 2)
			++Data.Replay.UnitNums[team];
		currentHP = targetHP = Mathf.RoundToInt(info["health"].n);
		currentFuel = targetFuel = Mathf.RoundToInt(info["fuel"].n);
		currentAmmo = targetAmmo = float.IsPositiveInfinity(info["ammo"].n) ? -1 : Mathf.RoundToInt(info["ammo"].n);
		currentMetal = targetMetal = Mathf.RoundToInt(info["metal"].n);
	}

	protected abstract int MaxHP();

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (team < 2)
			--Data.Replay.UnitNums[team];
		if (hbRect)
			Destroy(hbRect.gameObject);
	}

	protected override void OnGUI()
	{
		base.OnGUI();
		if (!MouseOver || Screen.lockCursor)
			return;
		GUILayout.BeginArea(new Rect(Input.mousePosition.x - Screen.width * 0.075f, Screen.height - Input.mousePosition.y - Screen.height * 0.1f, Screen.width * 0.15f, Screen.height * 0.2f).FitScreen(), GUI.skin.box);
		GUILayout.Label("燃料：" + Mathf.RoundToInt(currentFuel), Data.GUI.Label.SmallLeft);
		GUILayout.FlexibleSpace();
		if (float.IsInfinity(currentAmmo))
			print(Time.time);
		GUILayout.Label("弹药：" + (currentAmmo < 0 ? "无限" : Mathf.RoundToInt(currentAmmo).ToString()), Data.GUI.Label.SmallLeft);
		GUILayout.FlexibleSpace();
		GUILayout.Label("金属：" + Mathf.RoundToInt(currentMetal), Data.GUI.Label.SmallLeft);
		GUILayout.EndArea();
	}

	private void RefreshHealthBar()
	{
		var hbPos = Camera.main.WorldToScreenPoint(TopCenter() + Settings.HealthBar.PositionOffset);
		hbCanvas.planeDistance = hbPos.z;
		hbRect.anchoredPosition = hbPos;
		hbRect.localScale = Vector2.one * Screen.width / 100 / Mathf.Clamp(hbPos.z / Settings.DimensionScaleFactor, 3, 15);
		if (tag != "Doodad")
			hbText.color = new Color(1, 1, 1, Mathf.Clamp01(4 - hbPos.z / Settings.DimensionScaleFactor / 2));
		hbText.text = Mathf.RoundToInt(currentHP) + "/" + MaxHP();
	}

	protected override void Start()
	{
		base.Start();
		var hbImageRect = hbImage.GetComponent<RectTransform>();
		hbImageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, hbHorizontalPixelNumber);
		hbImageRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 4);
		var hbTextRect = hbText.GetComponent<RectTransform>();
		hbTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Settings.GUI.TextGranularity * hbHorizontalPixelNumber);
		hbTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Settings.GUI.TextGranularity * 4);
		hbTextRect.localScale = Vector2.one / Settings.GUI.TextGranularity;
		hbImage.texture = hbTexture = new Texture2D(hbHorizontalPixelNumber, 4) { wrapMode = TextureWrapMode.Clamp };
		hbPixels = hbTexture.GetPixels32();
		lastHPIndex = Mathf.RoundToInt(currentHP / MaxHP() * hbHorizontalPixelNumber);
		for (var i = 0; i < hbHorizontalPixelNumber; i++)
			for (var j = 0; j < 4; j++)
				if (j != 1)
					hbPixels[i + hbHorizontalPixelNumber * j] = i < lastHPIndex ? Settings.HealthBar.FullColor : Settings.HealthBar.EmptyColor;
		hbTexture.SetPixels32(hbPixels);
		hbTexture.Apply();
		RefreshHealthBar();
		hbCanvas.gameObject.SetActive(true);
	}

	public IEnumerator Supply(UnitBase target, int fuel, int ammo, int metal)
	{
		var elapsedTime = Mathf.Max((fuel + ammo + metal) / Settings.Replay.SupplyRate, 0.1f);
		StartCoroutine(Beam(target, elapsedTime, BeamType.Supply));
		yield return new WaitForSeconds((target.transform.WorldCenterOfElement() - Beamer.position).magnitude / Settings.Replay.BeamSpeed);
		target.FlashingOn();
		var effectedFuel = 0;
		var effectedAmmo = 0;
		var effectedMetal = 0;
		for (float t, startTime = Time.time; (t = (Time.time - startTime) / elapsedTime) < 1;)
		{
			var deltaFuel = Mathf.RoundToInt(fuel * t - effectedFuel);
			if (deltaFuel > 0)
			{
				targetFuel -= deltaFuel;
				target.targetFuel += deltaFuel;
				effectedFuel += deltaFuel;
			}
			var deltaAmmo = Mathf.RoundToInt(ammo * t - effectedAmmo);
			if (deltaAmmo > 0)
			{
				targetAmmo -= deltaAmmo;
				target.targetAmmo += deltaAmmo;
				effectedAmmo += deltaAmmo;
			}
			var deltaMetal = Mathf.RoundToInt(metal * t - effectedMetal);
			if (deltaMetal > 0)
			{
				targetMetal -= deltaMetal;
				target.targetMetal += deltaMetal;
				effectedMetal += deltaMetal;
			}
			yield return null;
		}
		targetFuel -= fuel - effectedFuel;
		target.targetFuel += fuel - effectedFuel;
		targetAmmo -= ammo - effectedAmmo;
		target.targetAmmo += ammo - effectedAmmo;
		targetMetal -= metal - effectedMetal;
		target.targetMetal += metal - effectedMetal;
		target.FlashingOff();
		string message;
		if (metal == 0)
			if (ammo == 0)
				message = fuel > 0 ? "F: +" + fuel : "0";
			else
				message = (fuel > 0 ? "F: +" + fuel + "  " : "") + "A: +" + ammo;
		else
			message = (fuel > 0 ? "F: +" + fuel + "  " : "") + (ammo > 0 ? "A: +" + ammo + "  " : "") + "M: +" + metal;
		yield return StartCoroutine(Data.Replay.Instance.ShowMessageAt(target, message));
		--Data.Replay.SuppliesLeft;
	}

	protected override void Update()
	{
		base.Update();
		if (Mathf.Abs(targetAmmo - currentAmmo) > Settings.Tolerance)
			currentAmmo = Mathf.Lerp(currentAmmo, targetAmmo, Settings.TransitionRate * Time.deltaTime);
		if (Mathf.Abs(targetHP - currentHP) > Settings.Tolerance)
			currentHP = Mathf.Lerp(currentHP, targetHP, Settings.TransitionRate * Time.deltaTime);
		if (currentHP < 0)
			currentHP = targetHP = 0;
		if (Mathf.RoundToInt(currentHP) <= 0 && tag != "Doodad" && !isAiming)
			Destruct();

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
		RefreshHealthBar();

		#endregion
	}
}