#region

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class ProductionEntry : MonoBehaviour
{
	private float currentPos;
	private Text description;
	private RectTransform entry;
	public int kind;
	private float lifeSpan;
	private int progress;
	public bool ready;
	private float spawnTime;
	private int targetPos;
	public int team;
	private Image tintedIcon;
	private Image underlay;

	private void Awake()
	{
		Delegates.CurrentTeamColorChanged += RefreshColor;
		Delegates.ScreenSizeChanged += RefreshEntryRect;
		(entry = GetComponent<RectTransform>()).SetParent(GameObject.Find("ProductionLists").transform);
		description = entry.Find("Description").GetComponent<Text>();
		tintedIcon = entry.Find("TintedIcon").GetComponent<Image>();
		underlay = entry.Find("Underlay").GetComponent<Image>();
	}

	public IEnumerator Done()
	{
		Data.Replay.ProductionLists[team].Remove(this);
		for (var i = 0; i < Data.Replay.ProductionLists[team].Count - targetPos; i++)
			Data.Replay.ProductionLists[team][i].targetPos--;
		underlay.color = Color.clear;
		var c1 = description.color;
		var c2 = tintedIcon.color;
		Destroy(gameObject, 3);
		while ((c1.a *= Settings.FastAttenuation) + (c2.a *= Settings.FastAttenuation) > Mathf.Epsilon)
		{
			description.color = c1;
			tintedIcon.color = c2;
			yield return new WaitForSeconds(Settings.DeltaTime);
		}
	}

	private void OnDestroy()
	{
		Delegates.CurrentTeamColorChanged -= RefreshColor;
		Delegates.ScreenSizeChanged -= RefreshEntryRect;
	}

	private IEnumerator Progress()
	{
		spawnTime = Data.Replay.ProductionTimer;
		while ((tintedIcon.fillAmount = (Data.Replay.ProductionTimer - spawnTime) / lifeSpan) < 1)
			yield return null;
		ready = true;
	}

	public void RefreshColor() { tintedIcon.color = description.color = Data.TeamColor.Current[team]; }

	public void RefreshEntryRect()
	{
		entry.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Data.Replay.ProductionEntrySize);
		entry.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Data.Replay.ProductionEntrySize);
	}

	public void Setup(int team, int kind)
	{
		this.team = team;
		this.kind = kind;
		lifeSpan = Settings.MaxTimePerFrame * Constants.BuildRounds[kind];
		currentPos = -1;
		description.text = Constants.ChineseNames[kind];
		underlay.sprite = tintedIcon.sprite = Resources.Load<Sprite>("ProductionEntryIcons/" + Constants.BaseTypeNames[kind]);
		foreach (var productionEntry in Data.Replay.ProductionLists[team])
			productionEntry.targetPos++;
		Data.Replay.Bases[team].targetMetal -= Constants.Costs[kind];
		Data.Replay.ProductionLists[team].Add(this);
	}

	private void Start()
	{
		StartCoroutine(Progress());
		RefreshColor();
		RefreshEntryRect();
	}

	private void Update()
	{
		if (Mathf.Abs(targetPos - currentPos) > Settings.Tolerance)
			currentPos = Mathf.Lerp(currentPos, targetPos, Settings.TransitionRate * Time.smoothDeltaTime);
		var t = currentPos % Settings.MaxProductionEntryNumPerRow;
		entry.anchoredPosition = t < Settings.MaxProductionEntryNumPerRow - 1 ? Data.Replay.ProductionEntrySize * new Vector2(t, currentPos < 0 ? 0 : -Mathf.Floor(currentPos / Settings.MaxProductionEntryNumPerRow)) : Data.Replay.ProductionEntrySize * new Vector2((Settings.MaxProductionEntryNumPerRow - 1) * (Settings.MaxProductionEntryNumPerRow - t), -(Mathf.Floor(currentPos / Settings.MaxProductionEntryNumPerRow) + t + 1 - Settings.MaxProductionEntryNumPerRow));
		if (team == 1)
			entry.anchoredPosition -= Vector2.up * Data.Replay.ProductionEntrySize * Mathf.Ceil((float)Data.Replay.ProductionLists[0].Count / Settings.MaxProductionEntryNumPerRow);
	}
}