#region

using System;
using System.Collections.Generic;
using Boomlagoon.JSON;
using UnityEngine;

#endregion

[Serializable]
public class EffectItemSetup
{
	public Vector2 CUSTOM_LETTERS_LIST_POS = Vector2.zero;
	public ActionFloatProgression m_delay = new ActionFloatProgression(0);
	public bool m_editor_display;
	public PLAY_ITEM_ASSIGNMENT m_effect_assignment;
	public List<int> m_effect_assignment_custom_letters;
	public bool m_loop_play_once;
	public PLAY_ITEM_EVENTS m_play_when;

	public void ExportBaseData(ref JSONObject json_data)
	{
		json_data["m_play_when"] = (int)m_play_when;
		json_data["m_effect_assignment"] = (int)m_effect_assignment;
		json_data["m_loop_play_once"] = m_loop_play_once;
		json_data["m_effect_assignment_custom_letters"] = m_effect_assignment_custom_letters.ExportData();
		json_data["m_delay"] = m_delay.ExportData();
	}

	public void ImportBaseData(JSONObject json_data)
	{
		m_play_when = (PLAY_ITEM_EVENTS)(int)json_data["m_play_when"].Number;
		m_effect_assignment = (PLAY_ITEM_ASSIGNMENT)(int)json_data["m_effect_assignment"].Number;
		m_loop_play_once = json_data["m_loop_play_once"].Boolean;
		m_delay.ImportData(json_data["m_delay"].Obj);

		m_effect_assignment_custom_letters = json_data["m_effect_assignment_custom_letters"].Array.JSONtoListInt();
		m_loop_play_once = json_data["m_loop_play_once"].Boolean;
	}
}