#region

using System.Collections;
using GameStatics;
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
	private Highlighter highlighter;
	private int HP;
	private bool isDead;
	private int lastHPIndex;
	private RectTransform markRect;
	public int team;
	protected virtual Quaternion DefaultRotation { get { return Quaternion.Euler(0, Random.Range(-180, 180), 0); } }

	public JSONObject Info
	{
		set
		{
			_info = value;
			UpdateInfo();
		}
	}

	public Vector3 Position { get { return Methods.Coordinates.InternalToExternal(transform.position); } set { transform.position = Methods.Coordinates.ExternalToInternal(value); } }
	protected virtual int RelativeSize { get { return 1; } }

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
		hbHorizontalPixelNumber = Mathf.RoundToInt(Mathf.Pow(MaxHP(), 0.3f) * 20);
		highlighter = gameObject.AddComponent<Highlighter>();
		gameObject.AddComponent<Rigidbody>().isKinematic = true;
		gameObject.ChangeLayer(LayerMask.NameToLayer("Entity"));
	}

	public void Deselect() { highlighter.ConstantOff(); }

	protected virtual void Destruct()
	{
		isDead = true;
		foreach (IEntityFX entityFX in GetComponentsInChildren(typeof(IEntityFX)))
			entityFX.Disable();
		highlighter.Die();
		StartCoroutine(FadeOut());
	}

	protected abstract Vector3 Dimensions();

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

	protected virtual void OnDestroy()
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

	public void Select()
	{
		var cameraSettings = Camera.main.GetComponentInParent<Moba_Camera>().settings;
		cameraSettings.lockTargetTransform = transform;
		cameraSettings.cameraLocked = true;
		highlighter.ConstantOnImmediate(Data.TeamColor.Current[team]);
		//Destruct();
	}

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
		hbImage.texture = hbTexture = new Texture2D(hbHorizontalPixelNumber, 4)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		hbPixels = hbTexture.GetPixels32();
		for (var i = 0; i < hbHorizontalPixelNumber; i++)
			for (var j = 0; j < 4; j++)
				if (j != 1)
					hbPixels[i + hbHorizontalPixelNumber * j] = Settings.HealthBar.EmptyColor;
		hbTexture.SetPixels32(hbPixels);
		hbTexture.Apply();
	}

	private void Update()
	{
		markRect.anchoredPosition = Methods.Coordinates.InternalToMiniMapBasedScreen(transform.position);

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
		var hbPos = Camera.main.WorldToScreenPoint(rigidbody.worldCenterOfMass + Vector3.up * (Dimensions().y / 2 + Settings.HealthBar.VerticalPositionOffset) * transform.lossyScale.y);
		hbCanvas.planeDistance = hbPos.z;
		hbRect.anchoredPosition = hbPos;
		hbRect.localScale = Vector2.one * 10 / Mathf.Clamp(hbPos.z / Settings.ScaleFactor, 3, 15);
		if (!isDead)
			hbText.color = Color.Lerp(Color.white, Color.clear, Mathf.Clamp01((hbPos.z / Settings.ScaleFactor - 8) / 2));
		hbText.text = HP + "/" + MaxHP();

		#endregion

		HP = (HP + 1) % (MaxHP() + 1);
	}

	protected virtual void UpdateInfo()
	{
		var pos = _info["pos"];
		float posX, posY, posZ;
		if (pos["__class__"].str == "Rectangle")
		{
			posX = (pos["upper_left"]["x"].n + pos["lower_right"]["x"].n) / 2;
			posY = (pos["upper_left"]["y"].n + pos["lower_right"]["y"].n) / 2;
			posZ = (pos["upper_left"]["z"].n + pos["lower_right"]["z"].n) / 2;
		}
		else
		{
			posX = pos["x"].n;
			posY = pos["y"].n;
			posZ = pos["z"].n;
		}
		Position = new Vector3(posX, posY, posZ);
		var delta = Mathf.CeilToInt((RelativeSize - 1) / 2f);
		for (var x = Mathf.RoundToInt(posX - delta); x <= Mathf.RoundToInt(posX + delta); x++)
			for (var y = Mathf.RoundToInt(posY - delta); y <= Mathf.RoundToInt(posY + delta); y++)
				Data.IsOccupied[x, y] = true;
	}
}