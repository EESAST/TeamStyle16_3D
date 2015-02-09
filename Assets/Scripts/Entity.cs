#region

using System.Collections;
using HighlightingSystem;
using UnityEngine;
using UnityEngine.UI;

#endregion

public abstract class Entity : MonoBehaviour
{
	protected JSONObject _info;
	private Canvas hbCanvas;
	private int hbHorizontalPixelNumber;
	private RawImage hbImage;
	private Color32[] hbPixels;
	private RectTransform hbRect;
	private Text hbText;
	private Texture2D hbTexture;
	protected Highlighter highlighter;
	protected int HP;
	private bool isDead;
	private int lastHPIndex;
	protected RectTransform markRect;
	protected Texture markTexture;

	[HideInInspector]
	public int team;

	protected virtual Quaternion DefaultRotation { get { return Quaternion.Euler(0, Random.Range(-180f, 180), 0); } }

	public JSONObject Info
	{
		set
		{
			_info = value;
			UpdateInfo();
		}
		get { return _info; }
	}

	protected virtual int RelativeSize { get { return 1; } }

	protected virtual void Awake()
	{
		Delegates.MarkPatternChanged += RefreshMarkPattern;
		Delegates.MarkSizeChanged += RefreshMarkSize;
		Delegates.ScreenSizeChanged += RefreshMarkSize;
		Delegates.CurrentTeamColorChanged += RefreshColor;
		(hbCanvas = (Instantiate(Resources.Load("HealthBar")) as GameObject).GetComponent<Canvas>()).worldCamera = Camera.main;
		hbRect = hbCanvas.transform.FindChild("HBRect").GetComponent<RectTransform>();
		hbImage = hbRect.FindChild("HBImage").GetComponent<RawImage>();
		hbText = hbRect.FindChild("HBText").GetComponent<Text>();
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
		hbHorizontalPixelNumber = Mathf.RoundToInt(Mathf.Pow(MaxHP(), 0.25f) * 25);
		highlighter = gameObject.AddComponent<Highlighter>();
		gameObject.AddComponent<Rigidbody>().isKinematic = true;
		foreach (var childCollider in GetComponentsInChildren<Collider>())
			childCollider.gameObject.layer = LayerMask.NameToLayer("Entity");
	}

	public abstract Vector3 Center();

	public virtual void Deselect()
	{
		Camera.main.GetComponentInParent<Moba_Camera>().settings.lockTargetTransform = null;
		highlighter.ConstantOff();
	}

	public virtual void Destruct()
	{
		Data.Entities.Remove(Mathf.RoundToInt(Info["index"].n));
		isDead = true;
		foreach (IEntityFX entityFX in GetComponentsInChildren(typeof(IEntityFX)))
			entityFX.Disable();
		highlighter.Die();
		StartCoroutine(FadeOut());
	}

	protected abstract Vector3 Dimensions();

	public IEnumerator FaceTarget(Vector3 target)
	{
		var dir = Methods.Coordinates.ExternalToInternal(target) - transform.position;
		dir.y = 0;
		var p = transform.rotation;
		var q = Quaternion.FromToRotation(Vector3.forward, //this.transform.TransformDirection (Vector3.forward),
			dir);
		for (float t = 0; t <= 1; t += 0.1f)
		{
			transform.rotation = Quaternion.Slerp(p, q, t);
			yield return new WaitForSeconds(0.01f);
		}
		//this.transform.rotation = q;

		//var angles0 = this.transform.rotation.eulerAngles;
		//var angles1 = Quaternion.LookRotation();
		//var angles = new Vector3(angles0);
		//angles
		//this.transform.eulerAngles

		//this.rigidbody.
		//this.rigidbody.velocity = new Vector3(posX - this.rigidbody.position
	}

	private IEnumerator FadeOut()
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

	protected abstract int Level();

	protected abstract void LoadMark();

	protected abstract int MaxHP();

	public void MouseOver() { highlighter.On(Data.TeamColor.Current[team]); }

	protected void OnDestroy()
	{
		Delegates.MarkPatternChanged -= RefreshMarkPattern;
		Delegates.MarkSizeChanged -= RefreshMarkSize;
		Delegates.ScreenSizeChanged -= RefreshMarkSize;
		Delegates.CurrentTeamColorChanged -= RefreshColor;
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

	protected virtual void RefreshColor() { highlighter.ConstantParams(markRect.GetComponent<RawImage>().color = Data.TeamColor.Current[team]); }

	private void RefreshMarkPattern() { markRect.GetComponent<RawImage>().texture = Data.MarkPatternIndex == 0 ? markTexture : null; }

	private void RefreshMarkSize()
	{
		markRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, RelativeSize * Data.MarkScaleFactor * Data.MiniMap.ScaleFactor);
		markRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, RelativeSize * Data.MarkScaleFactor * Data.MiniMap.ScaleFactor);
	}

	public virtual void Select()
	{
		var cameraSettings = Camera.main.GetComponentInParent<Moba_Camera>().settings;
		cameraSettings.lockTargetTransform = transform;
		cameraSettings.cameraLocked = true;
		highlighter.ConstantOnImmediate(Data.TeamColor.Current[team]);
	}

	public void SetPosition(float externalX, float externalY) { transform.position = Methods.Coordinates.ExternalToInternal(externalX, externalY, Level()); }

	protected virtual void Start()
	{
		transform.rotation = DefaultRotation;
		transform.localScale = Vector3.one * RelativeSize * Settings.ScaleFactor * 2 / ((Dimensions().x + Dimensions().z));
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
		RefreshColor();
		RefreshMarkPattern();
		RefreshMarkSize();
	}

	protected virtual void Update()
	{
		//HP = (HP + 1) % (MaxHP() + 1);

		markRect.anchoredPosition = Vector2.Scale(new Vector2(Data.MapSize.y, Data.MapSize.x) * Data.MiniMap.ScaleFactor, Methods.Coordinates.InternalToMiniMapRatios(transform.position));

		#region Update Health Bar

		var hpIndex = Mathf.RoundToInt((float)HP / MaxHP() * hbHorizontalPixelNumber);
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
		hbText.text = HP + "/" + MaxHP();

		#endregion
	}

	protected virtual void UpdateInfo()
	{
		float posX, posY;
		Methods.Coordinates.JSONToExternal(_info["pos"], out posX, out posY);
		SetPosition(posX, posY);
		var delta = Mathf.CeilToInt((RelativeSize - 1) / 2f);
		for (var x = Mathf.FloorToInt(posX - delta); x <= Mathf.CeilToInt(posX + delta); x++)
			for (var y = Mathf.FloorToInt(posY - delta); y <= Mathf.CeilToInt(posY + delta); y++)
				Data.IsOccupied[x, y] = true;
	}

	public void UpdateInfo_(JSONObject info)
	{
		//_info = info;	//not really necessary
		HP = Mathf.RoundToInt(info["health"].n);
		float posX, posY;
		Methods.Coordinates.JSONToExternal(info["pos"], out posX, out posY);
		SetPosition(posX, posY);
	}
}