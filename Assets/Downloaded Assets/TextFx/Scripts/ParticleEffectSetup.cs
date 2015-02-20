#region

using System;
using Boomlagoon.JSON;
using UnityEngine;

#endregion

[Serializable]
public class ParticleEffectSetup : EffectItemSetup
{
	public ActionFloatProgression m_duration = new ActionFloatProgression(0);
	public PARTICLE_EFFECT_TYPE m_effect_type;
	public bool m_follow_mesh;
	public ParticleEmitter m_legacy_particle_effect;
	public ActionVector3Progression m_position_offset = new ActionVector3Progression(Vector3.zero);
	public bool m_rotate_relative_to_letter = true;
	public ActionVector3Progression m_rotation_offset = new ActionVector3Progression(Vector3.zero);
	public ParticleSystem m_shuriken_particle_effect;

	public JSONValue ExportData()
	{
		var json_data = new JSONObject();

		ExportBaseData(ref json_data);

		json_data["m_effect_type"] = (int)m_effect_type;
		if (m_effect_type == PARTICLE_EFFECT_TYPE.LEGACY)
			json_data["m_legacy_particle_effect"] = m_legacy_particle_effect.ToPath();
		else
			json_data["m_shuriken_particle_effect"] = m_shuriken_particle_effect.ToPath();
		json_data["m_duration"] = m_duration.ExportData();
		json_data["m_follow_mesh"] = m_follow_mesh;
		json_data["m_position_offset"] = m_position_offset.ExportData();
		json_data["m_rotation_offset"] = m_rotation_offset.ExportData();
		json_data["m_rotate_relative_to_letter"] = m_rotate_relative_to_letter;

		return new JSONValue(json_data);
	}

	public void ImportData(JSONObject json_data)
	{
		m_effect_type = (PARTICLE_EFFECT_TYPE)(int)json_data["m_effect_type"].Number;
		if (m_effect_type == PARTICLE_EFFECT_TYPE.LEGACY)
			m_legacy_particle_effect = json_data["m_legacy_particle_effect"].Str.PathToParticleEmitter();
		else
			m_shuriken_particle_effect = json_data["m_shuriken_particle_effect"].Str.PathToParticleSystem();
		m_duration.ImportData(json_data["m_duration"].Obj);
		m_follow_mesh = json_data["m_follow_mesh"].Boolean;
		m_position_offset.ImportData(json_data["m_position_offset"].Obj);
		m_rotation_offset.ImportData(json_data["m_rotation_offset"].Obj);
		m_rotate_relative_to_letter = json_data["m_rotate_relative_to_letter"].Boolean;

		ImportBaseData(json_data);
	}
}