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
public class ActionVector3Progression : ActionVariableProgression
{
	[SerializeField]
	protected AnimationCurve m_custom_ease_curve_y = new AnimationCurve();

	[SerializeField]
	protected AnimationCurve m_custom_ease_curve_z = new AnimationCurve();

	[SerializeField]
	protected bool m_ease_curve_per_axis;

	[SerializeField]
	protected Vector3 m_from = Vector3.zero;

	[SerializeField]
	protected Vector3 m_to = Vector3.zero;

	[SerializeField]
	protected Vector3 m_to_to = Vector3.zero;

	[SerializeField]
	protected Vector3[] m_values;

	public ActionVector3Progression() { }

	public ActionVector3Progression(Vector3 start_vec)
	{
		m_from = start_vec;
		m_to = start_vec;
		m_to_to = start_vec;
	}

	public AnimationCurve CustomEaseCurveY { get { return m_custom_ease_curve_y; } }
	public AnimationCurve CustomEaseCurveZ { get { return m_custom_ease_curve_z; } }
	public bool EaseCurvePerAxis { get { return m_ease_curve_per_axis; } }
#if UNITY_EDITOR
	public virtual int NumEditorLines
	{
		get
		{
			if (Progression == (int)ValueProgression.Constant)
				return 3;
			if (Progression == (int)ValueProgression.Random || Progression == (int)ValueProgression.EasedCustom || (Progression == (int)ValueProgression.Eased && !m_to_to_bool))
				return 6;
			return 8;
		}
	}
#endif
	public Vector3 ValueFrom { get { return m_from; } }
	public Vector3[] Values { get { return m_values; } set { m_values = value; } }
	public Vector3 ValueThen { get { return m_to_to; } }
	public Vector3 ValueTo { get { return m_to; } }

	public virtual void CalculateProgressions(int num_progressions, Vector3[] offset_vecs, bool force_offset_from_last = false)
	{
		var offset_from_last = force_offset_from_last ? true : m_is_offset_from_last;

		// Initialise the array of values. Array of only one if all progressions share the same constant value.
		if (Progression == (int)ValueProgression.Eased || Progression == (int)ValueProgression.EasedCustom || Progression == (int)ValueProgression.Random || (offset_from_last && offset_vecs.Length > 1))
		{
			var constant_offset = offset_vecs != null && offset_vecs.Length == 1;
			m_values = new Vector3[num_progressions];

			for (var idx = 0; idx < num_progressions; idx++)
				m_values[idx] = offset_from_last ? offset_vecs[constant_offset ? 0 : idx] : Vector3.zero;
		}
		else
			m_values = new Vector3[1] { offset_from_last && offset_vecs.Length >= 1 ? offset_vecs[0] : Vector3.zero };

		if (Progression == (int)ValueProgression.Random)
			for (var idx = 0; idx < num_progressions; idx++)
				m_values[idx] += new Vector3(m_from.x + (m_to.x - m_from.x) * Random.value, m_from.y + (m_to.y - m_from.y) * Random.value, m_from.z + (m_to.z - m_from.z) * Random.value);
		else if (Progression == (int)ValueProgression.Eased)
		{
			float progression;

			for (var idx = 0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : idx / (num_progressions - 1f);

				if (m_to_to_bool)
					if (progression <= 0.5f)
						m_values[idx] += m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression / 0.5f);
					else
					{
						progression -= 0.5f;
						m_values[idx] += m_to + (m_to_to - m_to) * EasingManager.GetEaseProgress(EasingManager.GetEaseTypeOpposite(m_ease_type), progression / 0.5f);
					}
				else
					m_values[idx] += m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression);
			}
		}
		else if (Progression == (int)ValueProgression.EasedCustom)
		{
			float progression;

			for (var idx = 0; idx < num_progressions; idx++)
			{
				progression = num_progressions == 1 ? 0 : idx / (num_progressions - 1f);

				if (m_ease_curve_per_axis)
				{
					m_values[idx].x += m_from.x + (m_to.x - m_from.x) * m_custom_ease_curve.Evaluate(progression);
					m_values[idx].y += m_from.y + (m_to.y - m_from.y) * m_custom_ease_curve_y.Evaluate(progression);
					m_values[idx].z += m_from.z + (m_to.z - m_from.z) * m_custom_ease_curve_z.Evaluate(progression);
				}
				else
					m_values[idx] += m_from + (m_to - m_from) * m_custom_ease_curve.Evaluate(progression);
			}
		}
		else if (Progression == (int)ValueProgression.Constant)
			for (var idx = 0; idx < m_values.Length; idx++)
				m_values[idx] += m_from;
	}

	public void CalculateRotationProgressions(ref float[] letter_progressions, int num_progressions, Vector3[] offset_vecs, TextFxBezierCurve curve_override = null)
	{
		if (curve_override != null)
		{
			// Work out letter rotations based on the provided bezier curve setup

			var constant_offset = offset_vecs != null && offset_vecs.Length == 1;
			m_values = new Vector3[num_progressions];

			for (var idx = 0; idx < num_progressions; idx++)
				m_values[idx] = m_is_offset_from_last ? offset_vecs[constant_offset ? 0 : idx] : Vector3.zero;

			for (var idx = 0; idx < letter_progressions.Length; idx++)
				m_values[idx] += curve_override.GetCurvePointRotation(letter_progressions[idx]);
		}

		CalculateProgressions(num_progressions, curve_override == null ? offset_vecs : m_values, curve_override != null);
	}

	public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, Vector3[] offset_vec)
	{
		var progression_idx = GetProgressionIndex(progression_variables, animate_per);
		var constant_offset = offset_vec != null && offset_vec.Length == 1;

		m_values[progression_idx] = m_is_offset_from_last ? offset_vec[constant_offset ? 0 : progression_idx] : Vector3.zero;
		m_values[progression_idx] += new Vector3(m_from.x + (m_to.x - m_from.x) * Random.value, m_from.y + (m_to.y - m_from.y) * Random.value, m_from.z + (m_to.z - m_from.z) * Random.value);
	}

	public ActionVector3Progression Clone()
	{
		var vector3_progression = new ActionVector3Progression(Vector3.zero);

		vector3_progression.m_progression_idx = Progression;
		vector3_progression.m_ease_type = m_ease_type;
		vector3_progression.m_from = m_from;
		vector3_progression.m_to = m_to;
		vector3_progression.m_to_to = m_to_to;
		vector3_progression.m_to_to_bool = m_to_to_bool;
		vector3_progression.m_is_offset_from_last = m_is_offset_from_last;
		vector3_progression.m_unique_randoms = m_unique_randoms;
		vector3_progression.m_override_animate_per_option = m_override_animate_per_option;
		vector3_progression.m_animate_per = m_animate_per;
		vector3_progression.m_ease_curve_per_axis = m_ease_curve_per_axis;
		vector3_progression.m_custom_ease_curve = new AnimationCurve(m_custom_ease_curve.keys);
		vector3_progression.m_custom_ease_curve_y = new AnimationCurve(m_custom_ease_curve_y.keys);
		vector3_progression.m_custom_ease_curve_z = new AnimationCurve(m_custom_ease_curve_z.keys);

		return vector3_progression;
	}

	public override JSONValue ExportData()
	{
		var json_data = new JSONObject();

		ExportBaseData(ref json_data);

		json_data["m_from"] = m_from.ExportData();
		json_data["m_to"] = m_to.ExportData();
		json_data["m_to_to"] = m_to_to.ExportData();
		json_data["m_ease_curve_per_axis"] = m_ease_curve_per_axis;

		if (Progression == (int)ValueProgression.EasedCustom && m_ease_curve_per_axis)
		{
			json_data["m_custom_ease_curve_y"] = m_custom_ease_curve_y.ExportData();
			json_data["m_custom_ease_curve_z"] = m_custom_ease_curve_z.ExportData();
		}

		return new JSONValue(json_data);
	}

	public Vector3 GetValue(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default) { return GetValue(GetProgressionIndex(progression_variables, animate_per_default)); }

	private Vector3 GetValue(int progression_idx)
	{
		var num_vals = m_values.Length;
		if (num_vals > 1 && progression_idx < num_vals)
			return m_values[progression_idx];
		if (num_vals == 1)
			return m_values[0];
		return Vector3.zero;
	}

	public override void ImportData(JSONObject json_data)
	{
		m_from = json_data["m_from"].Obj.JSONtoVector3();
		m_to = json_data["m_to"].Obj.JSONtoVector3();
		m_to_to = json_data["m_to_to"].Obj.JSONtoVector3();
		m_ease_curve_per_axis = json_data["m_ease_curve_per_axis"].Boolean;
		if (json_data.ContainsKey("m_custom_ease_curve_y"))
		{
			m_custom_ease_curve_y = json_data["m_custom_ease_curve_y"].Array.JSONtoAnimationCurve();
			m_custom_ease_curve_z = json_data["m_custom_ease_curve_z"].Array.JSONtoAnimationCurve();
		}

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
					m_from = value_pair.Value.StringToVector3('|', '<');
					break;
				case "m_to":
					m_to = value_pair.Value.StringToVector3('|', '<');
					break;
				case "m_to_to":
					m_to_to = value_pair.Value.StringToVector3('|', '<');
					break;
				case "m_ease_curve_per_axis":
					m_ease_curve_per_axis = bool.Parse(value_pair.Value);
					break;
				case "m_custom_ease_curve_y":
					m_custom_ease_curve_y = value_pair.Value.ToAnimationCurve();
					break;
				case "m_custom_ease_curve_z":
					m_custom_ease_curve_z = value_pair.Value.ToAnimationCurve();
					break;

				default:
					ImportBaseLagacyData(value_pair);
					break;
			}
		}
	}

	public void SetConstant(Vector3 constant_value)
	{
		m_progression_idx = (int)ValueProgression.Constant;
		m_from = constant_value;
	}

	public void SetEased(EasingEquation easing_function, Vector3 eased_from, Vector3 eased_to)
	{
		m_progression_idx = (int)ValueProgression.Eased;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to_bool = false;
		m_ease_type = easing_function;
	}

	public void SetEased(EasingEquation easing_function, Vector3 eased_from, Vector3 eased_to, Vector3 eased_then)
	{
		m_progression_idx = (int)ValueProgression.Eased;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to = eased_then;
		m_to_to_bool = true;
		m_ease_type = easing_function;
	}

	public void SetEasedCustom(AnimationCurve easing_curve, Vector3 eased_from, Vector3 eased_to)
	{
		m_progression_idx = (int)ValueProgression.EasedCustom;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to_bool = false;

		m_ease_curve_per_axis = false;
		m_custom_ease_curve = easing_curve;
	}

	public void SetEasedCustom(AnimationCurve easing_curve_x, AnimationCurve easing_curve_y, AnimationCurve easing_curve_z, Vector3 eased_from, Vector3 eased_to)
	{
		m_progression_idx = (int)ValueProgression.EasedCustom;
		m_from = eased_from;
		m_to = eased_to;
		m_to_to_bool = false;

		m_ease_curve_per_axis = true;
		m_custom_ease_curve = easing_curve_x;
		m_custom_ease_curve_y = easing_curve_y;
		m_custom_ease_curve_z = easing_curve_z;
	}

	public void SetRandom(Vector3 random_min, Vector3 random_max, bool unique_randoms = false)
	{
		m_progression_idx = (int)ValueProgression.Random;
		m_from = random_min;
		m_to = random_max;
		m_unique_randoms = unique_randoms;
	}

#if UNITY_EDITOR
	public float DrawEditorGUI(GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		var x_offset = position.x + ACTION_INDENT_LEVEL_1;
		var y_offset = DrawProgressionEditorHeader(label, position, offset_legal, unique_random_legal, bold_label, ProgressionExtraOptions, ProgressionExtraOptionIndexes);

		m_from = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), Progression == (int)ValueProgression.Constant ? "Vector" : "Vector From", m_from);
		y_offset += LINE_HEIGHT * 2;

		if (Progression != (int)ValueProgression.Constant)
		{
			m_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector To", m_to);
			y_offset += LINE_HEIGHT * 2;

			if (Progression == (int)ValueProgression.Eased && m_to_to_bool)
			{
				m_to_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector Then", m_to_to);
				y_offset += LINE_HEIGHT * 2;
			}

			y_offset = DrawVector3CustomEaseCurveSettings(x_offset, y_offset);
		}

		return (y_offset) - position.y;
	}

	protected float DrawVector3CustomEaseCurveSettings(float x_offset, float y_offset)
	{
		if (Progression == (int)ValueProgression.EasedCustom)
		{
			EditorGUI.LabelField(new Rect(x_offset + VECTOR_3_WIDTH + 5, y_offset + 1, 70, LINE_HEIGHT), new GUIContent("Per Axis?", "Enables the definition of a custom animation easing curve for each axis (x,y,z)."));
			m_ease_curve_per_axis = EditorGUI.Toggle(new Rect(x_offset + VECTOR_3_WIDTH + 75, y_offset, 20, LINE_HEIGHT), m_ease_curve_per_axis);
			m_custom_ease_curve = EditorGUI.CurveField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve" + (m_ease_curve_per_axis ? " (x)" : ""), m_custom_ease_curve);
			y_offset += LINE_HEIGHT * 1.2f;

			if (m_ease_curve_per_axis)
			{
				m_custom_ease_curve_y = EditorGUI.CurveField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve (y)", m_custom_ease_curve_y);
				y_offset += LINE_HEIGHT * 1.2f;
				m_custom_ease_curve_z = EditorGUI.CurveField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve (z)", m_custom_ease_curve_z);
				y_offset += LINE_HEIGHT * 1.2f;
			}
		}

		return y_offset;
	}
#endif
}