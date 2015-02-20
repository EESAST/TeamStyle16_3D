#region

using System;
using Boomlagoon.JSON;
using UnityEngine;

#endregion

[Serializable]
public class AudioEffectSetup : EffectItemSetup
{
	public AudioClip m_audio_clip;
	public ActionFloatProgression m_offset_time = new ActionFloatProgression(0);
	public ActionFloatProgression m_pitch = new ActionFloatProgression(1);
	public ActionFloatProgression m_volume = new ActionFloatProgression(1);

	public JSONValue ExportData()
	{
		var json_data = new JSONObject();

		ExportBaseData(ref json_data);

		json_data["m_audio_clip"] = m_audio_clip.ToPath();
		json_data["m_offset_time"] = m_offset_time.ExportData();
		json_data["m_volume"] = m_volume.ExportData();
		json_data["m_pitch"] = m_pitch.ExportData();

		return new JSONValue(json_data);
	}

	public void ImportData(JSONObject json_data)
	{
		m_audio_clip = json_data["m_audio_clip"].Str.PathToAudioClip();
		m_offset_time.ImportData(json_data["m_offset_time"].Obj);
		m_volume.ImportData(json_data["m_volume"].Obj);
		m_pitch.ImportData(json_data["m_pitch"].Obj);

		ImportBaseData(json_data);
	}
}