#region

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class ProductionEntry : MonoBehaviour
{
	private float currentPos;
	private Text description;
	private int desiredPos;
	private RectTransform entry;
	private float lifeSpan;
	private int progress;
	private float spawnTime;
	private int team;
	private Image tintedIcon;
	private Image underlay;

	private void Awake()
	{
		Delegates.CurrentTeamColorChanged += RefreshColor;
		Delegates.ScreenSizeChanged += RefreshEntryRect;
		(entry = GetComponent<RectTransform>()).SetParent(GameObject.Find("ProductionLists").transform);
		description = entry.FindChild("Description").GetComponent<Text>();
		tintedIcon = entry.FindChild("TintedIcon").GetComponent<Image>();
		underlay = entry.FindChild("Underlay").GetComponent<Image>();
	}

	private void OnDestroy()
	{
		Delegates.CurrentTeamColorChanged -= RefreshColor;
		Delegates.ScreenSizeChanged -= RefreshEntryRect;
	}

	private IEnumerator Progress()
	{
		spawnTime = Time.time;
		while ((tintedIcon.fillAmount = (Time.time - spawnTime) / lifeSpan) < 1)
			yield return new WaitForSeconds(0.04f);
		for (var i = 0; i < Data.ProductionList[team].Count - desiredPos - 1; i++)
			Data.ProductionList[team][i].desiredPos--;
		Data.ProductionList[team].Remove(this);
		underlay.color = Color.clear;
		var c1 = description.color;
		var c2 = tintedIcon.color;
		Destroy(gameObject, 3);
		while ((c1.a *= 0.8f) + (c2.a *= 0.8f) > Mathf.Epsilon)
		{
			description.color = c1;
			tintedIcon.color = c2;
			yield return new WaitForSeconds(0.04f);
		}
	}

	public void RefreshColor() { tintedIcon.color = description.color = Data.TeamColor.Current[team]; }

	public void RefreshEntryRect()
	{
		entry.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Data.ProductionEntrySize);
		entry.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Data.ProductionEntrySize);
	}

	public void Setup(int team, int kind)
	{
		this.team = team;
		lifeSpan = Settings.TimePerFrame * Constants.BuildRounds[kind];
		currentPos = -1;
		description.text = Constants.ChineseNames[kind];
		underlay.sprite = tintedIcon.sprite = Resources.Load<Sprite>("ProductionEntryIcons/" + Constants.BaseTypeNames[kind]);
		foreach (var productionEntry in Data.ProductionList[team])
			productionEntry.desiredPos++;
		Data.ProductionList[team].Add(this);
	}

	private void Start()
	{
		StartCoroutine(Progress());
		RefreshColor();
		RefreshEntryRect();
	}

	private void Update()
	{
		if (Mathf.Abs(desiredPos - currentPos) > Settings.Tolerance)
			currentPos = Mathf.Lerp(currentPos, desiredPos, 3 * Time.deltaTime);
		var t = currentPos % Settings.MaxEntryNumPerRow;
		entry.anchoredPosition = t < Settings.MaxEntryNumPerRow - 1 ? Data.ProductionEntrySize * new Vector2(t, currentPos < 0 ? 0 : -Mathf.Floor(currentPos / Settings.MaxEntryNumPerRow)) : Data.ProductionEntrySize * new Vector2((Settings.MaxEntryNumPerRow - 1) * (Settings.MaxEntryNumPerRow - t), -(Mathf.Floor(currentPos / Settings.MaxEntryNumPerRow) + t + 1 - Settings.MaxEntryNumPerRow));
		if (team == 1)
			entry.anchoredPosition -= Vector2.up * Data.ProductionEntrySize * Mathf.Ceil((float)Data.ProductionList[0].Count / Settings.MaxEntryNumPerRow);
	}
}