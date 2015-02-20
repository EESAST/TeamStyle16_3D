#region

using System.Collections;
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
	protected bool isDead;
	protected RectTransform markRect;
	protected Texture markTexture;
	public bool MouseOver;
	public int targetFuel;
	public int targetMetal;
	public int team;
	public virtual Transform Beamer { get { return transform; } }
	protected virtual Quaternion DefaultRotation { get { return Quaternion.Euler(0, Random.Range(-180f, 180), 0); } }
	public virtual int RelativeSize { get { return 1; } }

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
	}

	/*public virtual IEnumerator Beam(ParticleSystem beam, Component target, float elapsedTime)
	{
		beam.transform.parent = Beamer;
		beam.transform.position = Beamer.GetComponent<UnitBase>() ? Beamer.WorldCenterOfElement() : Beamer.position;
		beam.startSpeed = Settings.Map.ScaleFactor * 2;
		beam.Play();
		for (var startTime = Time.time; ((Time.time - startTime) / elapsedTime) < 1;)
		{
			var v = target.transform.WorldCenterOfElement() - beam.transform.position;
			beam.transform.rotation = Quaternion.LookRotation(v);
			beam.startLifetime = v.magnitude / beam.startSpeed;
			beam.startColor = Data.TeamColor.Current[team];
			yield return null;
		}
		beam.Stop();
	}*/

	public abstract Vector3 Center();

	public virtual void Deselect()
	{
		Camera.main.GetComponentInParent<Moba_Camera>().settings.lockTargetTransform = null;
		highlighter.ConstantOff();
	}

	protected virtual void Destruct()
	{
		isDead = true;
		Data.Replay.Elements.Remove(index);
		foreach (IElementFX elementFX in GetComponentsInChildren(typeof(IElementFX)))
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
		Methods.GUI.InitializeStyles();
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
		transform.rotation = DefaultRotation;
		transform.localScale = Vector3.one * RelativeSize * Settings.DimensionScaleFactor * 2 / ((Dimensions().x + Dimensions().z));
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