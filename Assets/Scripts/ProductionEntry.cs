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
	public bool ready;
	private int targetPos;
	private int team;
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

	public void Initialize(int team, int kind, int roundsLeft, bool shallAnimate = false)
	{
		this.team = team;
		this.kind = kind;
		lifeSpan = Settings.Replay.MaxTimePerFrame * Constants.BuildRounds[kind];
		description.text = Constants.ChineseNames[kind];
		underlay.sprite = tintedIcon.sprite = Resources.Load<Sprite>("ProductionEntryIcons/" + Constants.BaseTypeNames[kind]);
		tintedIcon.fillAmount = 1 - (float)roundsLeft / Constants.BuildRounds[kind];
		foreach (var productionEntry in Data.Replay.ProductionLists[team])
		{
			++productionEntry.targetPos;
			if (!shallAnimate)
				productionEntry.currentPos = productionEntry.targetPos;
		}
		Data.Replay.ProductionLists[team].Add(this);
	}

	public void Initialize(int team, int kind)
	{
		Initialize(team, kind, Constants.BuildRounds[kind], true);
		currentPos = -1;
		Data.Replay.Bases[team].targetMetal -= Constants.Costs[kind];
	}

	private void OnDestroy()
	{
		Delegates.CurrentTeamColorChanged -= RefreshColor;
		Delegates.ScreenSizeChanged -= RefreshEntryRect;
	}

	private IEnumerator Progress()
	{
		while (Data.GamePaused || (tintedIcon.fillAmount += Time.deltaTime * Data.Replay.ProductionTimeScale / lifeSpan) < 1)
			yield return null;
		ready = true;
	}

	private void RefreshColor() { tintedIcon.color = description.color = Data.TeamColor.Current[team]; }

	private void RefreshEntryRect()
	{
		entry.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Data.GUI.ProductionEntrySize);
		entry.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Data.GUI.ProductionEntrySize);
	}

	private void Start()
	{
		StartCoroutine(Progress());
		RefreshColor();
		RefreshEntryRect();
	}

	private void Update()
	{
		if (!Data.GamePaused && Mathf.Abs(targetPos - currentPos) > Settings.Tolerance)
			currentPos = Mathf.Lerp(currentPos, targetPos, Settings.TransitionRate * Time.unscaledDeltaTime);
		var t = currentPos % Settings.GUI.MaxProductionEntryNumPerRow;
		entry.anchoredPosition = t < Settings.GUI.MaxProductionEntryNumPerRow - 1 ? Data.GUI.ProductionEntrySize * new Vector2(t, currentPos < 0 ? 0 : -Mathf.Floor(currentPos / Settings.GUI.MaxProductionEntryNumPerRow)) : Data.GUI.ProductionEntrySize * new Vector2((Settings.GUI.MaxProductionEntryNumPerRow - 1) * (Settings.GUI.MaxProductionEntryNumPerRow - t), -(Mathf.Floor(currentPos / Settings.GUI.MaxProductionEntryNumPerRow) + t + 1 - Settings.GUI.MaxProductionEntryNumPerRow));
		if (team == 1)
			entry.anchoredPosition -= Vector2.up * Data.GUI.ProductionEntrySize * Mathf.Ceil((float)Data.Replay.ProductionLists[0].Count / Settings.GUI.MaxProductionEntryNumPerRow);
	}
}