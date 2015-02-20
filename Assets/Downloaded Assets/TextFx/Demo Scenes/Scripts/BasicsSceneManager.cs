#region

using UnityEngine;

#endregion

public class BasicsSceneManager : MonoBehaviour
{
	private readonly Vector3 m_local_position = new Vector3(0, 0, -2f);
	private EffectManager m_current_active_effect;
	private int m_effect_index;
	private string[] m_effect_names;
	public BasicEffectData[] m_effects;
	private bool m_sync_toggle = true;

	private void OnGUI()
	{
		m_effect_index = GUI.SelectionGrid(new Rect((Screen.width / 2f) - (Screen.width / 4f), 2f * (Screen.height / 3f), Screen.width / 2f, 7 * (Screen.height / 24f)), m_effect_index, m_effect_names, 3);

		if (GUI.Button(new Rect(4.6f * (Screen.width / 6f), 10.7f * (Screen.height / 12f), Screen.width / 6.6f, Screen.height / 13f), m_sync_toggle ? "In Sync" : "Random"))
			m_sync_toggle = !m_sync_toggle;

		if (GUI.changed)
		{
			// Effect change requested
			// Stop/Hide current effect
#if !UNITY_3_5
			m_current_active_effect.gameObject.SetActive(false);
#else
			m_current_active_effect.gameObject.SetActiveRecursively(false);
#endif

			m_current_active_effect = m_sync_toggle ? m_effects[m_effect_index].m_effect_sync : m_effects[m_effect_index].m_effect_random;

#if !UNITY_3_5
			m_current_active_effect.gameObject.SetActive(true);
#else			
			m_current_active_effect.gameObject.SetActiveRecursively(true);
#endif
			m_current_active_effect.transform.localPosition = m_local_position;
			m_current_active_effect.PlayAnimation();
		}

#if !UNITY_EDITOR || USE_EDITOR_GUI_NAVIGATION
		if(GUI.Button(new Rect((Screen.width/28f), 10.5f * (Screen.height/12f), Screen.width/7f, (Screen.height/13f)), "Back"))
		{
			PlayerPrefs.SetInt("TextFx_Skip_Intro_Anim", 1);
			Application.LoadLevel("TitleScene");
		}
#endif
	}

	private void Start()
	{
		m_effect_names = new string[m_effects.Length];

		var idx = 0;
		foreach (var effect_data in m_effects)
		{
			m_effect_names[idx] = effect_data.m_name;

			// Set effect to loop infinitely
			if (effect_data.m_effect_sync.GetAnimation(0).NumLoops > 0)
				effect_data.m_effect_sync.GetAnimation(0).GetLoop(0).m_number_of_loops = 0;
			if (effect_data.m_effect_random.GetAnimation(0).NumLoops > 0)
				effect_data.m_effect_random.GetAnimation(0).GetLoop(0).m_number_of_loops = 0;

			idx ++;
		}

		m_current_active_effect = m_effects[0].m_effect_sync;
		m_current_active_effect.ResetAnimation();
		m_current_active_effect.transform.localPosition = m_local_position;
		m_current_active_effect.PlayAnimation(0.5f);
	}
}