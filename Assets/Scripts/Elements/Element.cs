#region

using System.Collections;
using HighlightingSystem;
using JSON;
using UnityEngine;
using UnityEngine.UI;

#endregion

public abstract class Element : MonoBehaviour
{
	private AudioSource beamAudio;
	private ParticleSystem beamFX;
	private int beamsLeft;
	protected float currentFuel;
	protected float currentMetal;
	private bool firstBeam = true;
	private bool guiInitialized;
	protected Highlighter highlighter;
	public int index;
	protected bool isFlashing;
	protected RectTransform markRect;
	private Texture markTexture;
	public bool MouseOver;
	public bool shallResumeAudio;
	private bool shallResumeBeamAudio;
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
		Delegates.GameStateChanged += OnGameStateChanged;
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
		if (firstBeam)
		{
			beamFX = (Instantiate(Resources.Load("Beam")) as GameObject).particleSystem;
			beamFX.transform.parent = Beamer;
			beamFX.transform.position = Beamer.GetComponent<Element>() ? Beamer.WorldCenterOfElement() : Beamer.position;
			beamFX.startSpeed = Settings.Replay.BeamSpeed;
			beamAudio = beamFX.audio;
			beamAudio.maxDistance = Settings.Audio.MaxAudioDistance;
			beamAudio.volume = Settings.Audio.Volume.Beam;
			firstBeam = false;
		}
		else
			beamFX.gameObject.SetActive(true);
		beamAudio.clip = Resources.Load<AudioClip>("Sounds/Beam_" + beamType);
		if (Data.GamePaused)
			shallResumeBeamAudio = true;
		else
			beamAudio.Play();
		beamFX.Play();
		for (var startTime = Time.time; ((Time.time - startTime) / elapsedTime) < 1;)
		{
			if (!Data.GamePaused)
			{
				var v = target.transform.WorldCenterOfElement() - beamFX.transform.position;
				beamFX.transform.rotation = Quaternion.LookRotation(v);
				beamFX.startLifetime = v.magnitude / beamFX.startSpeed;
				if (this is Mine)
					beamFX.startColor = new Color32(245, 245, 220, 255);
				else if (this is Oilfield)
					beamFX.startColor = Color.green;
				else
					beamFX.startColor = Data.TeamColor.Current[team];
			}
			yield return null;
		}
		beamFX.Stop();
		var time = Time.time;
		beamAudio.loop = false;
		while (Data.GamePaused || beamAudio.isPlaying)
			yield return null;
		var loopsLeft = Mathf.RoundToInt((beamFX.startLifetime - Time.time + time) / beamAudio.clip.length);
		if (loopsLeft <= 0)
			yield break;
		beamAudio.loop = true;
		if (Data.GamePaused)
			shallResumeBeamAudio = true;
		else
			beamAudio.Play();
		yield return new WaitForSeconds(loopsLeft * beamAudio.clip.length);
		beamAudio.Stop();
		beamFX.gameObject.SetActive(false);
	}

	public abstract Vector3 Center();

	public void Deselect()
	{
		Camera.main.GetComponentInParent<Moba_Camera>().settings.lockTargetTransform = null;
		highlighter.ConstantOff();
		if (beamsLeft > 0 || this is Submarine)
			highlighter.FlashingOn();
	}

	protected abstract Vector3 Dimensions();

	public void FlashingOff()
	{
		if (--beamsLeft > 0)
			return;
		if (this is Submarine)
			highlighter.FlashingParams(Data.TeamColor.Current[team], Color.clear, Settings.Highlighter.SubmarineFlashingRate);
		else
		{
			highlighter.FlashingOff();
			isFlashing = false;
		}
	}

	public void FlashingOn()
	{
		++beamsLeft;
		if (this is Submarine)
			highlighter.FlashingParams(Data.TeamColor.Current[team], Color.clear, Settings.Highlighter.BeamFlashingRate);
		else if (SelectionManager.LastSelectedElement != this)
		{
			highlighter.FlashingOn();
			isFlashing = true;
		}
	}

	public virtual void Initialize(JSONObject info)
	{
		index = info["index"].i;
		Data.Replay.Elements.Add(index, this);
		float posX, posY;
		transform.position = Methods.Coordinates.ExternalToInternal(Methods.Coordinates.JSONToExternal(info["pos"], out posX, out posY));
		if (Level() != 2)
			return;
		var delta = Mathf.CeilToInt((RelativeSize - 1) / 2f);
		for (var x = Mathf.Max(Mathf.FloorToInt(posX - delta), 0); x <= Mathf.Min(Mathf.CeilToInt(posX + delta), Data.MapSize.x - 1); ++x)
			for (var y = Mathf.Max(Mathf.FloorToInt(posY - delta), 0); y <= Mathf.Min(Mathf.CeilToInt(posY + delta), Data.MapSize.y - 1); ++y)
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
		Delegates.GameStateChanged -= OnGameStateChanged;
		if (markRect)
			Destroy(markRect.gameObject);
	}

	private void OnGameStateChanged()
	{
		if (Data.GamePaused)
		{
			if (audio.isPlaying)
			{
				audio.Pause();
				shallResumeAudio = true;
			}
			if (beamAudio && beamAudio.isPlaying)
			{
				beamAudio.Pause();
				shallResumeBeamAudio = true;
			}
			if (isFlashing)
			{
				highlighter.FlashingOff();
				highlighter.ConstantOnImmediate();
			}
		}
		else
		{
			if (shallResumeAudio)
			{
				audio.Play();
				shallResumeAudio = false;
			}
			if (shallResumeBeamAudio)
			{
				beamAudio.Play();
				shallResumeBeamAudio = false;
			}
			if (isFlashing)
			{
				highlighter.ConstantOffImmediate();
				highlighter.FlashingOn();
			}
		}
	}

	protected virtual void OnGUI()
	{
		GUI.depth = -1;
		if (!guiInitialized)
			InitializeGUI();
	}

	protected virtual void RefreshColor()
	{
		var color = Data.TeamColor.Current[team];
		markRect.GetComponent<RawImage>().color = color;
		highlighter.ConstantParams(color);
		highlighter.FlashingParams(color, Color.clear, this is Submarine && beamsLeft == 0 ? Settings.Highlighter.SubmarineFlashingRate : Settings.Highlighter.BeamFlashingRate);
	}

	private void RefreshMarkPattern() { markRect.GetComponent<RawImage>().texture = Data.MiniMap.MarkPatternIndex == 0 ? markTexture : null; }

	private void RefreshMarkSize()
	{
		markRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, RelativeSize * Data.MiniMap.MarkScaleFactor * Data.MiniMap.ScaleFactor);
		markRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, RelativeSize * Data.MiniMap.MarkScaleFactor * Data.MiniMap.ScaleFactor);
	}

	public void Select()
	{
		var cameraSettings = Camera.main.GetComponentInParent<Moba_Camera>().settings;
		cameraSettings.lockTargetTransform = transform;
		cameraSettings.cameraLocked = true;
		highlighter.FlashingOff();
		highlighter.ConstantOnImmediate();
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
			currentFuel = Mathf.Lerp(currentFuel, targetFuel, Settings.TransitionRate * Time.deltaTime);
		if (Mathf.Abs(targetMetal - currentMetal) > Settings.Tolerance)
			currentMetal = Mathf.Lerp(currentMetal, targetMetal, Settings.TransitionRate * Time.deltaTime);
	}
}