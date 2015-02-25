#region

using System;
using System.Collections.Generic;
using Boomlagoon.JSON;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;

#endif

#endregion

[Serializable]
public class ActionFloatProgression : ActionVariableProgression
{
	[SerializeField] private float m_from;
	[SerializeField] private float m_to;
	[SerializeField] private float m_to_to;
	[SerializeField] private float[] m_values;

	public ActionFloatProgression(float start_val)
	{
		m_from = start_val;
		m_to = start_val;
		m_to_to = start_val;
	}

#if UNITY_EDITOR
	public int NumEditorLines
	{
		get
		{
			if (Progression == (int)ValueProgression.Constant)
				return 2;
			if (Progression == (int)ValueProgression.Random || Progression == (int)ValueProgression.EasedCustom || (Progression == (int)ValueProgression.Eased && !m_to_to_bool))
				return 4;
			return 5;
		}
	}
#endif
	public float ValueFrom { get { return m_from; } }
	public float[] Values { get { return m_values; } set { m_values = value; } }
	public float ValueThen { get { return m_to_to; } }
	public float ValueTo { get { return m_to; } }

	public void CalculateProgressions(int num_progressions)
	{
		// Initialise array of values.
		m_values = new float[Progression == (int)ValueProgression.Eased || Progression == (int)ValueProgression.EasedCustom || Progression == (int)ValueProgression.Random ? num_progressions : 1];

		if (Progression == (int)ValueProgression.Random) //  && (progression >= 0 || m_unique_randoms))
			for (var idx = 0; idx < num_progressions; idx++)
				m_values[idx] = m_from + (m_to - m_from) * Random.value;
		else if (Progression == (int)ValueProgression.Eased)
		{
			float progression;
			for (var idx = 0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : idx / (num_progressions - 1f);

				if (m_to_to_bool)
					if (progression <= 0.5f)
						m_values[idx] = m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression / 0.5f);
					else
					{
						progression -= 0.5f;
						m_values[idx] = m_to + (m_to_to - m_to) * EasingManager.GetEaseProgress(EasingManager.GetEaseTypeOpposite(m_ease_type), progression / 0.5f);
					}
				else
					m_values[idx] = m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression);
			}
		}
		else if (Progression == (int)ValueProgression.EasedCustom)
		{
			float progression;

			for (var idx = 0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : idx / (num_progressions - 1f);

				m_values[idx] += m_from + (m_to - m_from) * m_custom_ease_curve.Evaluate(progression);
			}
		}
		else if (Progression == (int)ValueProgression.Constant)
			m_values[0] = m_from;
	}

	public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per) { m_values[GetProgressionIndex(progression_variables, animate_per)] = m_from + (m_to - m_from) * Random.value; }

	public ActionFloatProgression Clone()
	{
		var float_progression = new ActionFloatProgression(0);

		float_progression.m_progression_idx = Progression;
		float_progression.m_ease_type = m_ease_type;
		float_progression.m_from = m_from;
		float_progression.m_to = m_to;
		float_progression.m_to_to = m_to_to;
		float_progression.m_to_to_bool = m_to_to_bool;
		float_progression.m_unique_randoms = m_unique_randoms;
		float_progression.m_override_animate_per_option = m_override_animate_per_option;
		float_progression.m_animate_per = m_animate_per;

		return float_progression;
	}

#if UNITY_EDITOR
	public float DrawEditorGUI(GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		var x_offset = position.x + ACTION_INDENT_LEVEL_1;
		var y_offset = DrawProgressionEditorHeader(label, position, offset_legal, unique_random_legal, bold_label, ProgressionExtraOptions, ProgressionExtraOptionIndexes);

		m_from = EditorGUI.FloatField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), Progression == (int)ValueProgression.Constant ? "Value" : "Value From", m_from);
		y_offset += LINE_HEIGHT;

		if (Progression != (int)ValueProgression.Constant)
		{
			m_to = EditorGUI.FloatField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Value To", m_to);
			y_offset += LINE_HEIGHT;

			if (Progression == (int)ValueProgression.Eased && m_to_to_bool)
			{
				m_to_to = EditorGUI.FloatField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Value Then", m_to_to);
				y_offset += LINE_HEIGHT;
			}


			if (Progression == (int)ValueProgression.EasedCustom)
			{
				m_custom_ease_curve = EditorGUI.CurveField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve", m_custom_ease_curve);
				y_offset += LINE_HEIGHT * 1.2f;
			}
		}

		return (y_offset) - position.y;
	}
#endif

	public override JSONValue ExportData()
	{
		var json_data = new JSONObject();

		ExportBaseData(ref json_data);

		json_data["m_from"] = m_from;
		json_data["m_to"] = m_to;
		json_data["m_to_to"] = m_to_to;

		return new JSONValue(json_data);
	}

	public float GetValue(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default) { return GetValue(GetProgressionIndex(progression_variables, animate_per_default)); }

	public float GetValue(int progression_idx)
	{
		var num_vals = m_values.Length;
		if (num_vals > 1 && progression_idx < num_vals)
			return m_values[progression_idx];
		if (num_vals == 1)
			return m_values[0];
		return 0;
	}

	public override void ImportData(JSONObject json_data)
	{
		m_from = (float)json_data["m_from"].Number;
		m_to = (float)json_data["m_to"].Number;
		m_to_to = (float)json_data["m_to_to"].Number;

		ImportBaseData(json_data);
	}

	public void ImportLegacyData(string data_string)
	{
		KeyValuePair<string, string> value_pair;
		var obj_list = data_string.StringToList(';', ':');

		foreach (var obj in obj_list)
		{
			value_pair = (KeyValuePair<string, string>)obj;

			switch (value_pair.Key)
			{
				case "m_from":
					m_from = float.Parse(value_pair.Value);
					break;
				case "m_to":
					m_to = float.Parse(value_pair.Value);
					break;
				case "m_to_to":
					m_to_to = float.Parse(value_pair.Value);
					break;

				default:
					ImportBaseLagacyData(value_pair);
					break;
			}
		}
	}

	public void SetConstant(float constant_value)
	{
		m_progression_idx = (int)ValueProgression.Constant;
		m_from = constant_value;
	}

	public void SetEased(EasingEquation easing_function, float eased_from, float eased_to)
	{
		m_progression_idx = (int)ValueProgression.Eased;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to_bool = false;
		m_ease_type = easing_function;
	}

	public void SetEased(EasingEquation easing_function, float eased_from, float eased_to, float eased_then)
	{
		m_progression_idx = (int)ValueProgression.Eased;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to = eased_then;
		m_to_to_bool = true;
		m_ease_type = easing_function;
	}

	public void SetEasedCustom(AnimationCurve easing_curve, float eased_from, float eased_to)
	{
		m_progression_idx = (int)ValueProgression.EasedCustom;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to_bool = false;
		m_custom_ease_curve = easing_curve;
	}

	public void SetRandom(float random_min, float random_max, bool unique_randoms = false)
	{
		m_progression_idx = (int)ValueProgression.Random;
		m_from = random_min;
		m_to = random_max;
		m_unique_randoms = unique_randoms;
	}
}