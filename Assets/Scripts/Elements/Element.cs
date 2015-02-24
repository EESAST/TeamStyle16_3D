#region

using System.Collections;
using System.Linq;
using HighlightingSystem;
using JSON;
using UnityEngine;
using UnityEngine.UI;

#endregion

public abstract class Element : MonoBehaviour
{
	protected float currentFuel;
	protected float currentMetal;
	private bool guiInitialized;
	protected Highlighter highlighter;
	public int index;
	protected RectTransform markRect;
	private Texture markTexture;
	public bool MouseOver;
	public int targetFuel;
	public int targetMetal;
	public int team;
	protected virtual Transform Beamer { get { return transform; } }
	protected virtual Quaternion DefaultRotation { get { return Quaternion.Euler(0, Random.Range(-180f, 180), 0); } }
	protected virtual int RelativeSize { get { return 1; } }

	protected virtual void Awake()
	{
		Delegates.MarkPatternChanged += RefreshMarkPattern;
		Delegates.MarkSizeChanged += RefreshMarkSize;
		Delegates.ScreenSizeChanged += RefreshMarkSize;
		Delegates.CurrentTeamColorChanged += RefreshColor;
		LoadMark();
		markRect.name = Level().ToString();
		markTexture = markRect.GetComponent<RawImage>().texture;
		markRect.SetParent(GameObject.Find("Map").transform);
		for (var i = markRect.GetSiblingIndex() - 1; i >= 0; i--)
		{
			int level;
			if (i > 0 && int.TryParse(markRect.parent.GetChild(i - 1).name, out level) && level > Level())
				continue;
			markRect.SetSiblingIndex(i);
			break;
		}
		highlighter = gameObject.AddComponent<Highlighter>();
		gameObject.AddComponent<Rigidbody>().isKinematic = true;
		foreach (var childCollider in GetComponentsInChildren<Collider>())
			childCollider.gameObject.layer = LayerMask.NameToLayer("Element");
		gameObject.AddComponent<AudioSource>();
		audio.dopplerLevel = 0;
		audio.maxDistance = Settings.Audio.MaxAudioDistance;
		audio.rolloffMode = AudioRolloffMode.Linear;
		transform.rotation = DefaultRotation;
		transform.localScale = Vector3.one * RelativeSize * Settings.DimensionScaleFactor * 2 / ((Dimensions().x + Dimensions().z));
	}

	public IEnumerator Beam(Component target, float elapsedTime, BeamType beamType)
	{
		var beamFX = (Instantiate(Resources.Load("Beam")) as GameObject).particleSystem;
		beamFX.transform.parent = Beamer;
		beamFX.transform.position = Beamer.GetComponent<Element>() ? Beamer.WorldCenterOfElement() : Beamer.position;
		beamFX.startSpeed = Settings.Replay.BeamSpeed;
		var beamAudio = beamFX.audio;
		beamAudio.clip = Resources.Load<AudioClip>("Sounds/Beam_" + beamType);
		beamAudio.dopplerLevel = 0;
		beamAudio.maxDistance = Settings.Audio.MaxAudioDistance;
		beamAudio.volume = Settings.Audio.Volume.Beam;
		beamAudio.Play();
		beamFX.Play();
		var element = Beamer.GetComponentInParent<Element>();
		for (var startTime = Time.time; ((Time.time - startTime) / elapsedTime) < 1;)
		{
			var v = target.transform.WorldCenterOfElement() - beamFX.transform.position;
			beamFX.transform.rotation = Quaternion.LookRotation(v);
			beamFX.startLifetime = v.magnitude / beamFX.startSpeed;
			if (element is Mine)
				beamFX.startColor = new Color32(245, 245, 220, 255);
			else if (element is Oilfield)
				beamFX.startColor = Color.green;
			else
				beamFX.startColor = Data.TeamColor.Current[element.team];
			yield return null;
		}
		beamFX.Stop();
		Destroy(beamFX.gameObject, beamFX.startLifetime);
		var time = Time.time;
		beamAudio.loop = false;
		while (beamAudio && beamAudio.isPlaying)
			yield return null;
		if (!beamAudio)
			yield break;
		var loopsLeft = Mathf.FloorToInt((beamFX.startLifetime - Time.time + time) / beamAudio.clip.length);
		if (loopsLeft <= 0)
			yield break;
		beamAudio.loop = true;
		beamAudio.Play();
		yield return new WaitForSeconds(loopsLeft * beamAudio.clip.length);
		if (beamAudio)
			beamAudio.Stop();
	}

	public abstract Vector3 Center();

	public virtual void Deselect()
	{
		Camera.main.GetComponentInParent<Moba_Camera>().settings.lockTargetTransform = null;
		highlighter.ConstantOff();
	}

	protected virtual void Destruct()
	{
		Data.Replay.Elements.Remove(index);
		tag = "Doodad";
		foreach (var elementFX in GetComponentsInChildren(typeof(IElementFX)).Cast<IElementFX>())
			elementFX.Disable();
		highlighter.Die();
		StartCoroutine(FadeOut());
	}

	protected abstract Vector3 Dimensions();

	protected abstract IEnumerator FadeOut();

	public virtual void Initialize(JSONObject info)
	{
		index = Mathf.RoundToInt(info["index"].n);
		Data.Replay.Elements.Add(index, this);
		float posX, posY;
		transform.position = Methods.Coordinates.ExternalToInternal(Methods.Coordinates.JSONToExternal(info["pos"], out posX, out posY));
		if (Level() != 2)
			return;
		var delta = Mathf.CeilToInt((RelativeSize - 1) / 2f);
		for (var x = Mathf.FloorToInt(posX - delta); x <= Mathf.CeilToInt(posX + delta); x++)
			for (var y = Mathf.FloorToInt(posY - delta); y <= Mathf.CeilToInt(posY + delta); y++)
				Data.IsOccupied[x, y] = true;
	}

	private void InitializeGUI()
	{
		Methods.GUI.InitializeStyles();
		guiInitialized = true;
	}

	protected abstract int Kind();

	protected abstract int Level();

	protected abstract void LoadMark();

	protected virtual void OnDestroy()
	{
		Delegates.MarkPatternChanged -= RefreshMarkPattern;
		Delegates.MarkSizeChanged -= RefreshMarkSize;
		Delegates.ScreenSizeChanged -= RefreshMarkSize;
		Delegates.CurrentTeamColorChanged -= RefreshColor;
		if (markRect)
			Destroy(markRect.gameObject);
	}

	protected virtual void OnGUI()
	{
		GUI.depth = -1;
		if (!guiInitialized)
			InitializeGUI();
	}

	protected virtual void RefreshColor() { highlighter.ConstantParams(markRect.GetComponent<RawImage>().color = Data.TeamColor.Current[team]); }

	private void RefreshMarkPattern() { markRect.GetComponent<RawImage>().texture = Data.MiniMap.MarkPatternIndex == 0 ? markTexture : null; }

	private void RefreshMarkSize()
	{
		markRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, RelativeSize * Data.MiniMap.MarkScaleFactor * Data.MiniMap.ScaleFactor);
		markRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, RelativeSize * Data.MiniMap.MarkScaleFactor * Data.MiniMap.ScaleFactor);
	}

	public virtual void Select()
	{
		var cameraSettings = Camera.main.GetComponentInParent<Moba_Camera>().settings;
		cameraSettings.lockTargetTransform = transform;
		cameraSettings.cameraLocked = true;
		highlighter.ConstantOnImmediate(Data.TeamColor.Current[team]);
	}

	protected virtual void Start()
	{
		RefreshColor();
		RefreshMarkPattern();
		RefreshMarkSize();
	}

	public Vector3 TopCenter() { return transform.TransformPoint(Center()) + Dimensions().y * transform.lossyScale.y / 2 * Vector3.up; }

	protected virtual void Update()
	{
		if (MouseOver)
			highlighter.On(Data.TeamColor.Current[team]);
		markRect.anchoredPosition = Vector2.Scale(new Vector2(Data.MapSize.y, Data.MapSize.x) * Data.MiniMap.ScaleFactor, Methods.Coordinates.InternalToMiniMapRatios(transform.position));
		if (Mathf.Abs(targetFuel - currentFuel) > Settings.Tolerance)
			currentFuel = Mathf.Lerp(currentFuel, targetFuel, Settings.TransitionRate * Time.smoothDeltaTime);
		if (Mathf.Abs(targetMetal - currentMetal) > Settings.Tolerance)
			currentMetal = Mathf.Lerp(currentMetal, targetMetal, Settings.TransitionRate * Time.smoothDeltaTime);
	}
}