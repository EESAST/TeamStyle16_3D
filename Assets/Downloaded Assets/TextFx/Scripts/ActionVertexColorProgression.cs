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
public class ActionVertexColorProgression : ActionVariableProgression
{
	[SerializeField]
	private VertexColour m_from = new VertexColour();

	[SerializeField]
	private VertexColour m_to = new VertexColour();

	[SerializeField]
	private VertexColour m_to_to = new VertexColour();

	[SerializeField]
	private VertexColour[] m_values;

	public ActionVertexColorProgression(VertexColour start_colour)
	{
		m_from = start_colour.Clone();
		m_to = start_colour.Clone();
		m_to_to = start_colour.Clone();
	}

#if UNITY_EDITOR
	public int NumEditorLines { get { return Progression == (int)ValueProgression.Constant ? 3 : 4; } }
#endif
	public VertexColour ValueFrom { get { return m_from; } }
	public VertexColour[] Values { get { return m_values; } set { m_values = value; } }
	public VertexColour ValueThen { get { return m_to_to; } }
	public VertexColour ValueTo { get { return m_to; } }

	public void CalculateProgressions(int num_progressions, VertexColour[] offset_vert_colours, Color[] offset_colours)
	{
		if (Progression == (int)ValueProgression.Eased || Progression == (int)ValueProgression.EasedCustom || Progression == (int)ValueProgression.Random || (m_is_offset_from_last && ((offset_colours != null && offset_colours.Length > 1) || (offset_vert_colours != null && offset_vert_colours.Length > 1))))
		{
			var constant_offset = (offset_colours != null && offset_colours.Length == 1) || (offset_vert_colours != null && offset_vert_colours.Length == 1);
			m_values = new VertexColour[num_progressions];

			for (var idx = 0; idx < num_progressions; idx++)
				m_values[idx] = m_is_offset_from_last ? (offset_colours != null ? new VertexColour(offset_colours[constant_offset ? 0 : idx]) : offset_vert_colours[constant_offset ? 0 : idx].Clone()) : new VertexColour(new Color(0, 0, 0, 0));
		}
		else
			m_values = new VertexColour[1] { m_is_offset_from_last ? (offset_colours != null ? new VertexColour(offset_colours[0]) : offset_vert_colours[0].Clone()) : new VertexColour(new Color(0, 0, 0, 0)) };


		if (Progression == (int)ValueProgression.Random)
			for (var idx = 0; idx < num_progressions; idx++)
				m_values[idx] = m_values[idx].Add(m_from.Add(m_to.Sub(m_from).Multiply(Random.value)));
		else if (Progression == (int)ValueProgression.Eased)
		{
			float progression;

			for (var idx = 0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : idx / (num_progressions - 1f);

				if (m_to_to_bool)
					if (progression <= 0.5f)
						m_values[idx] = m_values[idx].Add(m_from.Add((m_to.Sub(m_from)).Multiply(EasingManager.GetEaseProgress(m_ease_type, progression / 0.5f))));
					else
					{
						progression -= 0.5f;
						m_values[idx] = m_values[idx].Add(m_to.Add((m_to_to.Sub(m_to)).Multiply(EasingManager.GetEaseProgress(m_ease_type, progression / 0.5f))));
					}
				else
					m_values[idx] = m_values[idx].Add(m_from.Add((m_to.Sub(m_from)).Multiply(EasingManager.GetEaseProgress(m_ease_type, progression))));
			}
		}
		else if (Progression == (int)ValueProgression.EasedCustom)
		{
			float progression;

			for (var idx = 0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : idx / (num_progressions - 1f);

				m_values[idx] = m_values[idx].Add(m_from.Add((m_to.Sub(m_from)).Multiply(m_custom_ease_curve.Evaluate(progression))));
			}
		}
		else if (Progression == (int)ValueProgression.Constant)
			for (var idx = 0; idx < m_values.Length; idx++)
				m_values[idx] = m_values[idx].Add(m_from);
	}

	public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, VertexColour[] offset_colours)
	{
		var progression_idx = GetProgressionIndex(progression_variables, animate_per);
		var constant_offset = offset_colours != null && offset_colours.Length == 1;

		m_values[progression_idx] = m_is_offset_from_last ? offset_colours[constant_offset ? 0 : progression_idx].Clone() : new VertexColour(new Color(0, 0, 0, 0));
		m_values[progression_idx] = m_values[progression_idx].Add(m_from.Add(m_to.Sub(m_from).Multiply(Random.value)));
	}

	public ActionVertexColorProgression Clone()
	{
		var color_progression = new ActionVertexColorProgression(new VertexColour());

		color_progression.m_progression_idx = Progression;
		color_progression.m_ease_type = m_ease_type;
		color_progression.m_from = m_from.Clone();
		color_progression.m_to = m_to.Clone();
		color_progression.m_to_to = m_to_to.Clone();
		color_progression.m_to_to_bool = m_to_to_bool;
		color_progression.m_is_offset_from_last = m_is_offset_from_last;
		color_progression.m_unique_randoms = m_unique_randoms;
		color_progression.m_override_animate_per_option = m_override_animate_per_option;
		color_progression.m_animate_per = m_animate_per;

		return color_progression;
	}

	public void ConvertFromFlatColourProg(ActionColorProgression flat_colour_progression)
	{
		m_progression_idx = flat_colour_progression.Progression;
		m_ease_type = flat_colour_progression.EaseType;
		m_from = new VertexColour(flat_colour_progression.ValueFrom);
		m_to = new VertexColour(flat_colour_progression.ValueTo);
		m_to_to = new VertexColour(flat_colour_progression.ValueThen);
		m_to_to_bool = flat_colour_progression.UsingThirdValue;
		m_is_offset_from_last = flat_colour_progression.IsOffsetFromLast;
		m_unique_randoms = flat_colour_progression.UniqueRandom;
	}

#if UNITY_EDITOR
	public float DrawEditorGUI(GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		var y_offset = DrawProgressionEditorHeader(label, position, offset_legal, unique_random_legal, bold_label, ProgressionExtraOptions, ProgressionExtraOptionIndexes);
		var x_offset = position.x + ACTION_INDENT_LEVEL_1;

		EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT * 2), Progression == (int)ValueProgression.Constant ? "Colours" : "Colours\nFrom", EditorStyles.miniBoldLabel);
		x_offset += 60;

		m_from.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT * 2, LINE_HEIGHT), m_from.top_left);
		m_from.bottom_left = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT * 2, LINE_HEIGHT), m_from.bottom_left);
		x_offset += 45;
		m_from.top_right = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT * 2, LINE_HEIGHT), m_from.top_right);
		m_from.bottom_right = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT * 2, LINE_HEIGHT), m_from.bottom_right);


		if (Progression != (int)ValueProgression.Constant)
		{
			x_offset += 65;

			EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT * 2), "Colours\nTo", EditorStyles.miniBoldLabel);
			x_offset += 60;

			m_to.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT * 2, LINE_HEIGHT), m_to.top_left);
			m_to.bottom_left = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT * 2, LINE_HEIGHT), m_to.bottom_left);
			x_offset += 45;
			m_to.top_right = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT * 2, LINE_HEIGHT), m_to.top_right);
			m_to.bottom_right = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT * 2, LINE_HEIGHT), m_to.bottom_right);


			if (Progression == (int)ValueProgression.Eased && m_to_to_bool)
			{
				x_offset += 65;

				EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT * 2), "Colours\nThen To", EditorStyles.miniBoldLabel);
				x_offset += 60;

				m_to_to.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT * 2, LINE_HEIGHT), m_to_to.top_left);
				m_to_to.bottom_left = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT * 2, LINE_HEIGHT), m_to_to.bottom_left);
				x_offset += 45;
				m_to_to.top_right = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT * 2, LINE_HEIGHT), m_to_to.top_right);
				m_to_to.bottom_right = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT * 2, LINE_HEIGHT), m_to_to.bottom_right);
			}

			if (Progression == (int)ValueProgression.EasedCustom)
			{
				m_custom_ease_curve = EditorGUI.CurveField(new Rect(position.x + ACTION_INDENT_LEVEL_1, y_offset + LINE_HEIGHT * 2 + 10, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve", m_custom_ease_curve);
				y_offset += LINE_HEIGHT * 1.2f;
			}
		}

		return (y_offset + LINE_HEIGHT * 2 + 10) - position.y;
	}
#endif

	public override JSONValue ExportData()
	{
		var json_data = new JSONObject();

		ExportBaseData(ref json_data);

		json_data["m_from"] = m_from.ExportData();
		json_data["m_to"] = m_to.ExportData();
		json_data["m_to_to"] = m_to_to.ExportData();

		return new JSONValue(json_data);
	}

	public VertexColour GetValue(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default) { return GetValue(GetProgressionIndex(progression_variables, animate_per_default)); }

	public VertexColour GetValue(int progression_idx)
	{
		var num_vals = m_values.Length;
		if (num_vals > 1 && progression_idx < num_vals)
			return m_values[progression_idx];
		if (num_vals == 1)
			return m_values[0];
		return new VertexColour(Color.white);
	}

	public override void ImportData(JSONObject json_data)
	{
		m_from = json_data["m_from"].Obj.JSONtoVertexColour();
		m_to = json_data["m_to"].Obj.JSONtoVertexColour();
		m_to_to = json_data["m_to_to"].Obj.JSONtoVertexColour();

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
					m_from = value_pair.Value.StringToVertexColor('|', '<', '^');
					break;
				case "m_to":
					m_to = value_pair.Value.StringToVertexColor('|', '<', '^');
					break;
				case "m_to_to":
					m_to_to = value_pair.Value.StringToVertexColor('|', '<', '^');
					break;

				default:
					ImportBaseLagacyData(value_pair);
					break;
			}
		}
	}

	public void SetConstant(VertexColour constant_value)
	{
		m_progression_idx = (int)ValueProgression.Constant;
		m_from = constant_value;
	}

	public void SetEased(EasingEquation easing_function, VertexColour eased_from, VertexColour eased_to)
	{
		m_progression_idx = (int)ValueProgression.Eased;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to_bool = false;
		m_ease_type = easing_function;
	}

	public void SetEased(EasingEquation easing_function, VertexColour eased_from, VertexColour eased_to, VertexColour eased_then)
	{
		m_progression_idx = (int)ValueProgression.Eased;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to = eased_then;
		m_to_to_bool = true;
		m_ease_type = easing_function;
	}

	public void SetEasedCustom(AnimationCurve easing_curve, VertexColour eased_from, VertexColour eased_to)
	{
		m_progression_idx = (int)ValueProgression.EasedCustom;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to_bool = false;

		m_custom_ease_curve = easing_curve;
	}

	public void SetRandom(VertexColour random_min, VertexColour random_max, bool unique_randoms = false)
	{
		m_progression_idx = (int)ValueProgression.Random;
		m_from = random_min;
		m_to = random_max;
		m_unique_randoms = unique_randoms;
	}
}