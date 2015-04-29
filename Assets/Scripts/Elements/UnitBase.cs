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
	protected bool isAttacking;
	private int lastHPIndex;
	protected int targetAmmo;
	public int targetHP;

	protected abstract IEnumerator AimAtPosition(Vector3 targetPosition);

	private int AmmoOnce() { return Constants.AmmoOnce[Kind()]; }

	public IEnumerator AttackPosition(Vector3 targetPosition)
	{
		isAttacking = true;
		yield return StartCoroutine(AimAtPosition(targetPosition));
		targetAmmo -= AmmoOnce();
		yield return StartCoroutine(FireAtPosition(targetPosition));
		yield return StartCoroutine(Replayer.ShowMessageAt(targetPosition + Settings.MessagePositionOffset, "Miss..."));
	}

	public IEnumerator AttackUnitBase(UnitBase targetUnitBase, int damage)
	{
		isAttacking = true;
		var fort = targetUnitBase as Fort;
		if (fort)
		{
			var fortIndex = fort.index;
			var fortLife = fort.rebornsLeft;
			Element targetElement;
			while (Data.GamePaused || !Data.Replay.Elements.TryGetValue(fortIndex, out targetElement) || (targetElement as Fort).life != fortLife)
				yield return null;
			targetUnitBase = targetElement as UnitBase;
		}
		var targetPosition = targetUnitBase.transform.WorldCenterOfElement();
		yield return StartCoroutine(AimAtPosition(targetPosition));
		targetAmmo -= AmmoOnce();
		if (targetUnitBase)
		{
			yield return StartCoroutine(FireAtUnitBase(targetUnitBase));
			targetUnitBase.targetHP -= damage;
			Data.Replay.TargetScores[team] += Constants.Score.PerDamage * damage;
		}
		else
			yield return StartCoroutine(FireAtPosition(targetPosition));
	}

	protected override void Awake()
	{
		base.Awake();
		(hbCanvas = (Instantiate(Resources.Load("HealthBar")) as GameObject).GetComponent<Canvas>()).worldCamera = Camera.main;
		hbRect = hbCanvas.transform.Find("HBRect").GetComponent<RectTransform>();
		hbImage = hbRect.Find("HBImage").GetComponent<RawImage>();
		hbText = hbRect.Find("HBText").GetComponent<Text>();
		hbHorizontalPixelNumber = Mathf.RoundToInt(Mathf.Pow(MaxHP(), 0.25f) * 10);
		gameObject.AddComponent<Rigidbody>().isKinematic = true;
	}

	protected virtual void Destruct()
	{
		Data.Replay.Elements.Remove(index);
		tag = "Doodad";
		foreach (var elementFX in GetComponentsInChildren(typeof(IElementFX)).Cast<IElementFX>())
			elementFX.Disable();
		highlighter.Die();
		StartCoroutine(FadeOut());
		StartCoroutine(Explode());
	}

	private IEnumerator Explode()
	{
		if (this is Plane)
			rigidbody.isKinematic = true;
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
					while (Data.GamePaused)
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
		audio.volume = RelativeSize == 3 ? Settings.Audio.Volume.Death3 : Settings.Audio.Volume.Death1;
		audio.clip = Resources.Load<AudioClip>("Sounds/Death_" + RelativeSize);
		if (Data.GamePaused)
			shallResumeAudio = true;
		else
			audio.Play();
		var detonator = Instantiate(Resources.Load("Detonator_Death"), transform.TransformPoint(Center()), Quaternion.identity) as GameObject;
		detonator.GetComponent<Detonator>().size = RelativeSize * Settings.DimensionScaleFactor;
		detonator.GetComponent<DetonatorForce>().power = Mathf.Pow(RelativeSize, 2.5f) * Mathf.Pow(Settings.DimensionScaleFactor, 3);
		foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
			meshRenderer.collider.enabled = meshRenderer.enabled = false;
		Destroy(dummy, Settings.Fragment.MaxLifeSpan * 2);
		var thisFort = this as Fort;
		if (thisFort)
		{
			var fort = (Instantiate(Resources.Load("Fort/Fort")) as GameObject).GetComponent<Fort>();
			fort.StartCoroutine(fort.Reborn(transform.position, index, thisFort.targetTeams, targetFuel, targetAmmo, targetMetal, thisFort.life + 1));
		}
		while (explosionsLeft > 0)
			yield return null;
		var carrier = this as Carrier;
		if (carrier && carrier.movingInterceptorsLeft > 0)
			carrier.ForceDestructReturningInterceptors();
		while (Data.GamePaused || audio.isPlaying)
			yield return null;
		Destroy(gameObject);
	}

	private IEnumerator FadeOut()
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
		team = info["team"].i;
		if (team < 2)
			++Data.Replay.UnitNums[team];
		currentHP = targetHP = info["health"].i;
		currentFuel = targetFuel = info["fuel"].i;
		currentAmmo = targetAmmo = float.IsPositiveInfinity(info["ammo"].n) ? -1 : info["ammo"].i;
		currentMetal = targetMetal = info["metal"].i;
	}

	protected int MaxHP() { return Constants.MaxHP[Kind()]; }

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
		var heightRatio = 0.15f + (ShowMetalInfo() ? 0.05f : 0);
		GUILayout.BeginArea(new Rect(Input.mousePosition.x - Screen.width * 0.06f, Screen.height - Input.mousePosition.y - Screen.height * heightRatio / 2, Screen.width * 0.12f, Screen.height * heightRatio).FitScreen(), GUI.skin.box);
		GUILayout.FlexibleSpace();
		GUILayout.Label("燃料：" + Mathf.RoundToInt(currentFuel), Data.GUI.Label.MediumLeft);
		GUILayout.FlexibleSpace();
		GUILayout.Label("弹药：" + (currentAmmo < 0 ? "无限" : Mathf.RoundToInt(currentAmmo).ToString()), Data.GUI.Label.MediumLeft);
		GUILayout.FlexibleSpace();
		if (ShowMetalInfo())
		{
			GUILayout.Label("金属：" + Mathf.RoundToInt(currentMetal), Data.GUI.Label.MediumLeft);
			GUILayout.FlexibleSpace();
		}
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

	protected abstract bool ShowMetalInfo();

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
		var elapsedTime = Mathf.Max((fuel * Settings.Replay.FuelMultiplier + ammo * Settings.Replay.AmmoMultiplier + metal * Settings.Replay.MetalMultiplier) / Settings.Replay.SupplyRate, 0.1f);
		StartCoroutine(Beam(target, elapsedTime, BeamType.Supply));
		yield return new WaitForSeconds((target.transform.WorldCenterOfElement() - Beamer.position).magnitude / Settings.Replay.BeamSpeed);
		target.FlashingOn();
		var effectedFuel = 0;
		var effectedAmmo = 0;
		var effectedMetal = 0;
		for (float t, startTime = Time.time; (t = (Time.time - startTime) / elapsedTime) < 1;)
		{
			if (!Data.GamePaused)
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
		if (!Data.GamePaused)
		{
			if (Mathf.Abs(targetAmmo - currentAmmo) > Settings.Tolerance)
				currentAmmo = Mathf.Lerp(currentAmmo, targetAmmo, Settings.TransitionRate * Time.unscaledDeltaTime);
			if (Mathf.Abs(targetHP - currentHP) > Settings.Tolerance)
				currentHP = Mathf.Lerp(currentHP, targetHP, Settings.TransitionRate * Time.unscaledDeltaTime);
			if (Mathf.RoundToInt(currentHP) == 0 && tag != "Doodad" && !isAttacking)
				Destruct();
		}

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