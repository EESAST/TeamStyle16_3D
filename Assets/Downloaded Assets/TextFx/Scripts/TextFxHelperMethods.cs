#region

using System.Collections.Generic;
using Boomlagoon.JSON;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

#endregion

public static class TextFxHelperMethods
{
	public static JSONArray ExportData(this List<int> list)
	{
		var json_array = new JSONArray();

		foreach (var num in list)
			json_array.Add(num);

		return json_array;
	}

	public static JSONValue ExportData(this Vector3 vec)
	{
		var json_data = new JSONObject();
		json_data["x"] = vec.x;
		json_data["y"] = vec.y;
		json_data["z"] = vec.z;
		return new JSONValue(json_data);
	}

	public static JSONValue ExportData(this Color color)
	{
		var json_data = new JSONObject();

		json_data["r"] = color.r;
		json_data["g"] = color.g;
		json_data["b"] = color.b;
		json_data["a"] = color.a;

		return new JSONValue(json_data);
	}

	public static JSONValue ExportData(this VertexColour vert_color)
	{
		var json_data = new JSONObject();

		json_data["bottom_left"] = vert_color.bottom_left.ExportData();
		json_data["bottom_right"] = vert_color.bottom_right.ExportData();
		json_data["top_left"] = vert_color.top_left.ExportData();
		json_data["top_right"] = vert_color.top_right.ExportData();

		return new JSONValue(json_data);
	}

	public static JSONValue ExportData(this Keyframe frame)
	{
		var json_data = new JSONObject();
		json_data["inTangent"] = frame.inTangent;
		json_data["outTangent"] = frame.outTangent;
		json_data["tangentMode"] = frame.tangentMode;
		json_data["time"] = frame.time;
		json_data["value"] = frame.value;
		return new JSONValue(json_data);
	}

	public static JSONValue ExportData(this AnimationCurve curve)
	{
		var key_frame_data = new JSONArray();

		foreach (var key_frame in curve.keys)
			key_frame_data.Add(key_frame.ExportData());

		return key_frame_data;
	}

	public static string[] GetArrayOfFirstEntries(this string[,] two_d_array)
	{
		var result_array = new string[two_d_array.GetLength(0)];

		for (var idx = 0; idx < two_d_array.GetLength(0); idx++)
			result_array[idx] = two_d_array[idx, 0];

		return result_array;
	}

	public static void ImportLegacyData(this EffectManager effect_manager, string data)
	{
		var data_list = data.StringToList();

		KeyValuePair<string, string> value_pair;
		string key, value;
		var anim_idx = 0;

		for (var idx = 0; idx < data_list.Count; idx++)
		{
			value_pair = (KeyValuePair<string, string>)data_list[idx];
			key = value_pair.Key;
			value = value_pair.Value;

			switch (key)
			{
				case "m_animate_per":
					effect_manager.m_animate_per = (AnimatePerOptions)int.Parse(value);
					break;
				case "m_begin_delay":
					effect_manager.m_begin_delay = float.Parse(value);
					break;
				case "m_begin_on_start":
					effect_manager.m_begin_on_start = bool.Parse(value);
					break;
				case "m_character_size":
					effect_manager.m_character_size = float.Parse(value);
					break;
				case "m_display_axis":
					effect_manager.m_display_axis = (TextDisplayAxis)int.Parse(value);
					break;
				case "m_line_height":
					effect_manager.m_line_height_factor = float.Parse(value);
					break;
				case "m_max_width":
					effect_manager.m_max_width = float.Parse(value);
					break;
				case "m_on_finish_action":
					effect_manager.m_on_finish_action = (ON_FINISH_ACTION)int.Parse(value);
					break;
				case "m_px_offset":
					effect_manager.m_px_offset = value.StringToVector2();
					break;
//				case "m_text":
//					effect_manager.m_text = value; break;
				case "m_text_alignment":
					effect_manager.m_text_alignment = (TextAlignment)int.Parse(value);
					break;
				case "m_text_anchor":
					effect_manager.m_text_anchor = (TextAnchor)int.Parse(value);
					break;
				case "m_time_type":
					effect_manager.m_time_type = (AnimationTime)int.Parse(value);
					break;

				case "ANIM_DATA_START":
					if (anim_idx == effect_manager.NumAnimations)
						effect_manager.AddAnimation();
					idx = effect_manager.GetAnimation(anim_idx).ImportLegacyData(data_list, idx + 1);
					anim_idx ++;
					break;
			}
		}
	}

	public static int ImportLegacyData(this LetterAnimation letter_anim, List<object> data_list, int index_offset = 0)
	{
		KeyValuePair<string, string> value_pair;
		string key, value;
		int idx;
		int loop_idx = 0, action_idx = 0;

		for (idx = index_offset; idx < data_list.Count; idx++)
		{
			value_pair = (KeyValuePair<string, string>)data_list[idx];
			key = value_pair.Key;
			value = value_pair.Value;

			if (key.Equals("ANIM_DATA_END"))
				// reached end of this animations import data
				break;

			switch (key)
			{
				case "m_letters_to_animate":
					var letter_list = value.StringToList(';');
					letter_anim.m_letters_to_animate = new List<int>();
					if (letter_list != null)
						foreach (var obj in letter_list)
							letter_anim.m_letters_to_animate.Add(int.Parse(obj.ToString()));
					break;
				case "m_letters_to_animate_custom_idx":
					letter_anim.m_letters_to_animate_custom_idx = int.Parse(value);
					break;
				case "m_letters_to_animate_option":
					letter_anim.m_letters_to_animate_option = (LETTERS_TO_ANIMATE)int.Parse(value);
					break;


				// LOOP DATA IMPORT
				case "LOOP_DATA_START":
					if (loop_idx == letter_anim.NumLoops)
						letter_anim.AddLoop();
					break;
				case "LOOP_DATA_END":
					loop_idx++;
					break;
				case "m_delay_first_only":
					letter_anim.GetLoop(loop_idx).m_delay_first_only = bool.Parse(value);
					break;
				case "m_end_action_idx":
					letter_anim.GetLoop(loop_idx).m_end_action_idx = int.Parse(value);
					break;
				case "m_loop_type":
					letter_anim.GetLoop(loop_idx).m_loop_type = (LOOP_TYPE)int.Parse(value);
					break;
				case "m_number_of_loops":
					letter_anim.GetLoop(loop_idx).m_number_of_loops = int.Parse(value);
					break;
				case "m_start_action_idx":
					letter_anim.GetLoop(loop_idx).m_start_action_idx = int.Parse(value);
					break;


				// ACTION DATA IMPORT
				case "ACTION_DATA_START":
					if (action_idx == letter_anim.NumActions)
						letter_anim.AddAction();
					idx = letter_anim.GetAction(action_idx).ImportLegacyData(data_list, idx + 1);
					action_idx ++;
					break;
			}
		}

		// Remove any extra LoopData or LetterAction instances that existed prior to importing
		if (letter_anim.NumLoops > loop_idx)
			letter_anim.RemoveLoops(loop_idx, letter_anim.NumLoops - loop_idx);

		if (letter_anim.NumActions > action_idx)
			letter_anim.RemoveActions(action_idx, letter_anim.NumActions - action_idx);

		return idx;
	}

	public static int ImportLegacyData(this LetterAction letter_action, List<object> data_list, int index_offset = 0)
	{
		KeyValuePair<string, string> value_pair;
		string key, value;
		int idx;

		letter_action.ClearAudioEffectSetups();
		letter_action.ClearParticleEffectSetups();

		AudioEffectSetup audio_setup = null;
		ParticleEffectSetup effect_setup = null;

		for (idx = index_offset; idx < data_list.Count; idx++)
		{
			value_pair = (KeyValuePair<string, string>)data_list[idx];
			key = value_pair.Key;
			value = value_pair.Value;

			if (key.Equals("ACTION_DATA_END"))
				// reached end of this Actions import data
				break;

			switch (key)
			{
				case "m_action_type":
					letter_action.m_action_type = (ACTION_TYPE)int.Parse(value);
					break;
				case "m_ease_type":
					letter_action.m_ease_type = (EasingEquation)int.Parse(value);
					break;
				case "m_use_gradient_start":
					letter_action.m_use_gradient_start = bool.Parse(value);
					break;
				case "m_use_gradient_end":
					letter_action.m_use_gradient_end = bool.Parse(value);
					break;
				case "m_force_same_start_time":
					letter_action.m_force_same_start_time = bool.Parse(value);
					break;
				// Legacy letter anchor import support
				case "m_letter_anchor":
					letter_action.m_letter_anchor_start = int.Parse(value);
					letter_action.m_letter_anchor_2_way = false;
					break;

				// New letter anchor import support
				case "m_letter_anchor_start":
					letter_action.m_letter_anchor_start = int.Parse(value);
					break;
				case "m_letter_anchor_end":
					letter_action.m_letter_anchor_end = int.Parse(value);
					break;
				case "m_letter_anchor_2_way":
					letter_action.m_letter_anchor_2_way = bool.Parse(value);
					break;


				case "m_offset_from_last":
					letter_action.m_offset_from_last = bool.Parse(value);
					break;
				case "m_position_axis_ease_data":
					letter_action.m_position_axis_ease_data.ImportLegacyData(value);
					break;
				case "m_rotation_axis_ease_data":
					letter_action.m_rotation_axis_ease_data.ImportLegacyData(value);
					break;
				case "m_scale_axis_ease_data":
					letter_action.m_scale_axis_ease_data.ImportLegacyData(value);
					break;


				case "m_start_colour":
					letter_action.m_start_colour.ImportLegacyData(value);
					break;
				case "m_end_colour":
					letter_action.m_end_colour.ImportLegacyData(value);
					break;
				case "m_start_vertex_colour":
					letter_action.m_start_vertex_colour.ImportLegacyData(value);
					break;
				case "m_end_vertex_colour":
					letter_action.m_end_vertex_colour.ImportLegacyData(value);
					break;
				case "m_start_euler_rotation":
					letter_action.m_start_euler_rotation.ImportLegacyData(value);
					break;
				case "m_end_euler_rotation":
					letter_action.m_end_euler_rotation.ImportLegacyData(value);
					break;
				case "m_start_pos":
					letter_action.m_start_pos.ImportLegacyData(value);
					break;
				case "m_end_pos":
					letter_action.m_end_pos.ImportLegacyData(value);
					break;
				case "m_start_scale":
					letter_action.m_start_scale.ImportLegacyData(value);
					break;
				case "m_end_scale":
					letter_action.m_end_scale.ImportLegacyData(value);
					break;
				case "m_delay_progression":
					letter_action.m_delay_progression.ImportLegacyData(value);
					break;
				case "m_duration_progression":
					letter_action.m_duration_progression.ImportLegacyData(value);
					break;


				case "m_audio_on_start":
					if (value.PathToAudioClip() != null)
						audio_setup = new AudioEffectSetup { m_audio_clip = value.PathToAudioClip(), m_play_when = PLAY_ITEM_EVENTS.ON_START, m_effect_assignment = PLAY_ITEM_ASSIGNMENT.PER_LETTER, m_loop_play_once = false };
					break;
				case "m_audio_on_start_delay":
					if (audio_setup != null)
						audio_setup.m_delay.ImportLegacyData(value);
					break;
				case "m_audio_on_start_offset":
					if (audio_setup != null)
						audio_setup.m_offset_time.ImportLegacyData(value);
					break;
				case "m_audio_on_start_pitch":
					if (audio_setup != null)
						audio_setup.m_pitch.ImportLegacyData(value);
					break;
				case "m_audio_on_start_volume":
					if (audio_setup != null)
					{
						audio_setup.m_volume.ImportLegacyData(value);
						letter_action.AddAudioEffectSetup(audio_setup);
						audio_setup = null;
					}
					break;

				case "m_audio_on_finish":
					if (value.PathToAudioClip() != null)
						audio_setup = new AudioEffectSetup { m_audio_clip = value.PathToAudioClip(), m_play_when = PLAY_ITEM_EVENTS.ON_FINISH, m_effect_assignment = PLAY_ITEM_ASSIGNMENT.PER_LETTER, m_loop_play_once = false };
					break;
				case "m_audio_on_finish_delay":
					if (audio_setup != null)
						audio_setup.m_delay.ImportLegacyData(value);
					break;
				case "m_audio_on_finish_offset":
					if (audio_setup != null)
						audio_setup.m_offset_time.ImportLegacyData(value);
					break;
				case "m_audio_on_finish_pitch":
					if (audio_setup != null)
						audio_setup.m_pitch.ImportLegacyData(value);
					break;
				case "m_audio_on_finish_volume":
					if (audio_setup != null)
					{
						audio_setup.m_volume.ImportLegacyData(value);
						letter_action.AddAudioEffectSetup(audio_setup);
						audio_setup = null;
					}
					break;


				// BACKWARDS COMPATIBILITY PARTICLE IMPORT
				case "m_emitter_on_start":
					if (value.PathToParticleEmitter() != null)
						effect_setup = new ParticleEffectSetup { m_legacy_particle_effect = value.PathToParticleEmitter(), m_play_when = PLAY_ITEM_EVENTS.ON_START, m_loop_play_once = false, m_rotation_offset = new ActionVector3Progression(new Vector3(0, 180, 0)), m_rotate_relative_to_letter = true, m_effect_type = PARTICLE_EFFECT_TYPE.LEGACY };
					break;
				case "m_emitter_on_start_delay":
					if (effect_setup != null)
						effect_setup.m_delay.ImportLegacyData(value);
					break;
				case "m_emitter_on_start_duration":
					if (effect_setup != null)
						effect_setup.m_duration.ImportLegacyData(value);
					break;
				case "m_emitter_on_start_follow_mesh":
					if (effect_setup != null)
						effect_setup.m_follow_mesh = bool.Parse(value);
					break;
				case "m_emitter_on_start_offset":
					if (effect_setup != null)
						effect_setup.m_position_offset.ImportLegacyData(value);
					break;
				case "m_emitter_on_start_per_letter":
					if (effect_setup != null)
					{
						effect_setup.m_effect_assignment = bool.Parse(value) ? PLAY_ITEM_ASSIGNMENT.PER_LETTER : PLAY_ITEM_ASSIGNMENT.CUSTOM;
						if (effect_setup.m_effect_assignment == PLAY_ITEM_ASSIGNMENT.CUSTOM)
							effect_setup.m_effect_assignment_custom_letters = new List<int> { 0 };

						letter_action.AddParticleEffectSetup(effect_setup);
						effect_setup = null;
					}
					break;

				case "m_emitter_on_finish":
					if (value.PathToParticleEmitter() != null)
						effect_setup = new ParticleEffectSetup { m_legacy_particle_effect = value.PathToParticleEmitter(), m_play_when = PLAY_ITEM_EVENTS.ON_FINISH, m_loop_play_once = false, m_rotation_offset = new ActionVector3Progression(new Vector3(0, 180, 0)), m_rotate_relative_to_letter = true, m_effect_type = PARTICLE_EFFECT_TYPE.LEGACY };
					break;
				case "m_emitter_on_finish_delay":
					if (effect_setup != null)
						effect_setup.m_delay.ImportLegacyData(value);
					break;
				case "m_emitter_on_finish_duration":
					if (effect_setup != null)
						effect_setup.m_duration.ImportLegacyData(value);
					break;
				case "m_emitter_on_finish_follow_mesh":
					if (effect_setup != null)
						effect_setup.m_follow_mesh = bool.Parse(value);
					break;
				case "m_emitter_on_finish_offset":
					if (effect_setup != null)
						effect_setup.m_position_offset.ImportLegacyData(value);
					break;
				case "m_emitter_on_finish_per_letter":
					if (effect_setup != null)
					{
						effect_setup.m_effect_assignment = bool.Parse(value) ? PLAY_ITEM_ASSIGNMENT.PER_LETTER : PLAY_ITEM_ASSIGNMENT.CUSTOM;
						if (effect_setup.m_effect_assignment == PLAY_ITEM_ASSIGNMENT.CUSTOM)
							effect_setup.m_effect_assignment_custom_letters = new List<int> { 0 };

						letter_action.AddParticleEffectSetup(effect_setup);
						effect_setup = null;
					}
					break;
			}
		}

		return idx;
	}

	public static void ImportLegacyData(this AxisEasingOverrideData axis_data, string data_string)
	{
		var data_parts = data_string.Split('|');
		if (int.Parse(data_parts[0]) == 1)
		{
			axis_data.m_override_default = true;
			axis_data.m_x_ease = (EasingEquation)int.Parse(data_parts[1]);
			axis_data.m_y_ease = (EasingEquation)int.Parse(data_parts[2]);
			axis_data.m_z_ease = (EasingEquation)int.Parse(data_parts[3]);
		}
		else
			axis_data.m_override_default = false;
	}

	public static AnimationCurve JSONtoAnimationCurve(this JSONArray json_data)
	{
		var anim_curve = new AnimationCurve();
		anim_curve.keys = new Keyframe[0];

		foreach (var frame_data in json_data)
			anim_curve.AddKey(frame_data.Obj.JSONtoKeyframe());

		return anim_curve;
	}

	public static Color JSONtoColor(this JSONObject json_data) { return new Color { r = (float)json_data["r"].Number, g = (float)json_data["g"].Number, b = (float)json_data["b"].Number, a = (float)json_data["a"].Number }; }

	public static Keyframe JSONtoKeyframe(this JSONObject json_data) { return new Keyframe { inTangent = (float)json_data["inTangent"].Number, outTangent = (float)json_data["outTangent"].Number, tangentMode = (int)json_data["tangentMode"].Number, time = (float)json_data["time"].Number, value = (float)json_data["value"].Number }; }

	public static List<int> JSONtoListInt(this JSONArray json_array)
	{
		var int_list = new List<int>();

		foreach (var int_val in json_array)
			int_list.Add((int)int_val.Number);

		return int_list;
	}

	public static Vector2 JSONtoVector2(this JSONObject json_data) { return new Vector2 { x = (float)json_data["x"].Number, y = (float)json_data["y"].Number }; }

	public static Vector3 JSONtoVector3(this JSONObject json_data) { return new Vector3 { x = (float)json_data["x"].Number, y = (float)json_data["y"].Number, z = (float)json_data["z"].Number }; }

	public static VertexColour JSONtoVertexColour(this JSONObject json_data) { return new VertexColour { bottom_left = json_data["bottom_left"].Obj.JSONtoColor(), bottom_right = json_data["bottom_right"].Obj.JSONtoColor(), top_left = json_data["top_left"].Obj.JSONtoColor(), top_right = json_data["top_right"].Obj.JSONtoColor() }; }

	public static AudioClip PathToAudioClip(this string path)
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;
#else
		return null;
#endif
	}

	public static ParticleEmitter PathToParticleEmitter(this string path)
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadAssetAtPath(path, typeof(ParticleEmitter)) as ParticleEmitter;
#else
		return null;
#endif
	}

	public static ParticleSystem PathToParticleSystem(this string path)
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadAssetAtPath(path, typeof(ParticleSystem)) as ParticleSystem;
#else
		return null;
#endif
	}

	private static Color StringDataToColor(string data_string, char delimiter = ';', char seperator = ':')
	{
		var value_pairs = data_string.Split(delimiter);
		string[] data_parts;
		var color = new Color();

		foreach (var value_pair in value_pairs)
		{
			data_parts = value_pair.Split(seperator);

			if (data_parts[0].Equals("r"))
				color.r = float.Parse(data_parts[1]);
			else if (data_parts[0].Equals("g"))
				color.g = float.Parse(data_parts[1]);
			else if (data_parts[0].Equals("b"))
				color.b = float.Parse(data_parts[1]);
			else if (data_parts[0].Equals("a"))
				color.a = float.Parse(data_parts[1]);
		}

		return color;
	}

	public static Color StringToColor(this string data_string, char delimiter = ';', char seperator = ':') { return StringDataToColor(data_string, delimiter, seperator); }

	public static List<object> StringToList(this string data_string, char delimiter = ',', char seperator = '=')
	{
		// Assuming the data string is book-ended with brackets
		data_string = data_string.Substring(1, data_string.Length - 2);

		if (data_string.Equals(""))
			return null;

		var list = new List<object>();

		if (data_string.Contains("" + seperator))
		{
			// List of keyvaluepairs

			var value_pairs = data_string.Split(delimiter);
			string[] data_parts;

			foreach (var value_pair in value_pairs)
			{
				data_parts = value_pair.Split(seperator);

				list.Add(new KeyValuePair<string, string>(data_parts[0], data_parts[1]));
			}
		}
		else
			list = new List<object>(data_string.Split(delimiter));

		return list;
	}

	public static Vector2 StringToVector2(this string data_string, char delimiter = ';', char seperator = ':')
	{
		var value_pairs = data_string.Split(delimiter);
		string[] data_parts;
		var vec = new Vector2();

		foreach (var value_pair in value_pairs)
		{
			data_parts = value_pair.Split(seperator);

			if (data_parts[0].Equals("x"))
				vec.x = float.Parse(data_parts[1]);
			else if (data_parts[0].Equals("y"))
				vec.y = float.Parse(data_parts[1]);
		}

		return vec;
	}

	public static Vector3 StringToVector3(this string data_string, char delimiter = ';', char seperator = ':')
	{
		var value_pairs = data_string.Split(delimiter);
		string[] data_parts;
		var vec = new Vector3();

		foreach (var value_pair in value_pairs)
		{
			data_parts = value_pair.Split(seperator);

			if (data_parts[0].Equals("x"))
				vec.x = float.Parse(data_parts[1]);
			else if (data_parts[0].Equals("y"))
				vec.y = float.Parse(data_parts[1]);
			else if (data_parts[0].Equals("z"))
				vec.z = float.Parse(data_parts[1]);
		}

		return vec;
	}

	public static VertexColour StringToVertexColor(this string data_string, char delimiter = ';', char seperator = ':', char color_seperator = '|')
	{
		var color_string_datas = data_string.Split(color_seperator);

		return new VertexColour { bottom_left = StringDataToColor(color_string_datas[0], delimiter, seperator), bottom_right = StringDataToColor(color_string_datas[1], delimiter, seperator), top_left = StringDataToColor(color_string_datas[2], delimiter, seperator), top_right = StringDataToColor(color_string_datas[3], delimiter, seperator) };
	}

	public static AnimationCurve ToAnimationCurve(this string curve_data)
	{
		var curve = new AnimationCurve();
		var key_frame_data_parts = curve_data.Split('#');

		if (key_frame_data_parts.Length % 5 != 0)
			return curve;

		var idx = 0;

		var key_frame = new Keyframe();

		for (idx = 0; idx < key_frame_data_parts.Length; idx++)
		{
			if (idx % 5 == 0)
			{
				if (idx > 0)
					curve.AddKey(key_frame);

				key_frame = new Keyframe();
				key_frame.time = float.Parse(key_frame_data_parts[idx]);
			}
			if (idx % 5 == 1)
				key_frame.value = float.Parse(key_frame_data_parts[idx]);
			if (idx % 5 == 2)
				key_frame.inTangent = float.Parse(key_frame_data_parts[idx]);
			if (idx % 5 == 3)
				key_frame.outTangent = float.Parse(key_frame_data_parts[idx]);
			if (idx % 5 == 4)
				key_frame.tangentMode = int.Parse(key_frame_data_parts[idx]);
		}

		if (idx > 0)
			curve.AddKey(key_frame);

		return curve;
	}

	public static string ToPath(this AudioClip clip)
	{
#if UNITY_EDITOR
		return AssetDatabase.GetAssetPath(clip);
#else
		return "";
#endif
	}

	public static string ToPath(this ParticleEmitter emitter)
	{
#if UNITY_EDITOR
		return AssetDatabase.GetAssetPath(emitter);
#else
		return "";
#endif
	}

	public static string ToPath(this ParticleSystem p_system)
	{
#if UNITY_EDITOR
		return AssetDatabase.GetAssetPath(p_system);
#else
		return "";
#endif
	}

	public static JSONValue Vector2ToJSON(this Vector2 vec)
	{
		var json_data = new JSONObject();
		json_data["x"] = vec.x;
		json_data["y"] = vec.y;

		return new JSONValue(json_data);
	}

	public static string Vector2ToString(this Vector2 vec, char delimiter = ';', char seperator = ':') { return "x" + seperator + vec.x + delimiter + "y" + seperator + vec.y; }
}