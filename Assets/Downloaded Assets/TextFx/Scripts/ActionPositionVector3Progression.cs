#region

using System;
using System.Collections.Generic;
using Boomlagoon.JSON;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

#endregion

[Serializable]
public class ActionPositionVector3Progression : ActionVector3Progression
{
	public const int CURVE_OPTION_INDEX = 4;
	public const string CURVE_OPTION_STRING = "Curve";
	[SerializeField] private TextFxBezierCurve m_bezier_curve = new TextFxBezierCurve();
	[SerializeField] private bool m_force_position_override;

	public ActionPositionVector3Progression(Vector3 start_vec)
	{
		m_from = start_vec;
		m_to = start_vec;
		m_to_to = start_vec;
	}

	public TextFxBezierCurve BezierCurve { get { return m_bezier_curve; } }
	public bool ForcePositionOverride { get { return m_force_position_override; } }
#if UNITY_EDITOR
	public override int NumEditorLines
	{
		get
		{
			if (Progression == (int)ValueProgression.Constant)
				return 4;
			if (Progression == (int)ValueProgression.Random || Progression == (int)ValueProgression.EasedCustom)
				return 7;
			if (Progression == CURVE_OPTION_INDEX)
				return 2 + (m_bezier_curve.EditorVisible ? 1 + ((m_bezier_curve.m_anchor_points != null ? m_bezier_curve.m_anchor_points.Count : 0) * 2) : 0);
			return m_to_to_bool ? 8 : 6;
		}
	}
#endif
	public override int[] ProgressionExtraOptionIndexes { get { return EXTRA_OPTION_INDEXS; } }
	public override string[] ProgressionExtraOptions { get { return EXTRA_OPTION_STRINGS; } }

	public void CalculatePositionProgressions(ref float[] letter_progressions, int num_progressions, Vector3[] offset_vecs, bool force_offset_from_last = false)
	{
		if (Progression == CURVE_OPTION_INDEX)
		{
			var constant_offset = offset_vecs != null && offset_vecs.Length == 1;
			m_values = new Vector3[num_progressions];

			for (var idx = 0; idx < num_progressions; idx++)
				m_values[idx] = m_is_offset_from_last ? offset_vecs[constant_offset ? 0 : idx] : Vector3.zero;

			for (var idx = 0; idx < letter_progressions.Length; idx++)
				m_values[idx] += m_bezier_curve.GetCurvePoint(letter_progressions[idx]);
		}
		else
			CalculateProgressions(num_progressions, offset_vecs);
	}

	public ActionPositionVector3Progression CloneThis()
	{
		var progression = new ActionPositionVector3Progression(Vector3.zero);

		progression.m_progression_idx = Progression;
		progression.m_ease_type = m_ease_type;
		progression.m_from = m_from;
		progression.m_to = m_to;
		progression.m_to_to = m_to_to;
		progression.m_to_to_bool = m_to_to_bool;
		progression.m_is_offset_from_last = m_is_offset_from_last;
		progression.m_unique_randoms = m_unique_randoms;
		progression.m_force_position_override = m_force_position_override;
		progression.m_override_animate_per_option = m_override_animate_per_option;
		progression.m_animate_per = m_animate_per;
		progression.m_ease_curve_per_axis = m_ease_curve_per_axis;
		progression.m_custom_ease_curve = new AnimationCurve(m_custom_ease_curve.keys);
		progression.m_custom_ease_curve_y = new AnimationCurve(m_custom_ease_curve_y.keys);
		progression.m_custom_ease_curve_z = new AnimationCurve(m_custom_ease_curve_z.keys);
		progression.m_bezier_curve = new TextFxBezierCurve(progression.m_bezier_curve);

		return progression;
	}

#if UNITY_EDITOR
	public float DrawPositionEditorGUI(GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
	{
		var x_offset = position.x + ACTION_INDENT_LEVEL_1;
		var y_offset = DrawProgressionEditorHeader(label, position, offset_legal, unique_random_legal, bold_label, ProgressionExtraOptions, ProgressionExtraOptionIndexes);

		if (Progression == CURVE_OPTION_INDEX)
		{
			// Handle displaying Bezier Curve position setup options
			var bezier_curve = m_bezier_curve;

			bezier_curve.EditorVisible = EditorGUI.Foldout(new Rect(x_offset, y_offset, 300, LINE_HEIGHT), bezier_curve.EditorVisible, new GUIContent("Anchor Points" + (bezier_curve.EditorVisible ? "  [Scene View Debug]" : "")), true);
			y_offset += LINE_HEIGHT;

			if (bezier_curve.EditorVisible)
			{
				x_offset += 15;

				if (GUI.Button(new Rect(x_offset, y_offset, 120, LINE_HEIGHT), "Add Point"))
					bezier_curve.AddNewAnchor();
				y_offset += LINE_HEIGHT;

				if (bezier_curve.m_anchor_points != null)
					for (var idx = 0; idx < bezier_curve.m_anchor_points.Count; idx++)
					{
						m_bezier_curve.m_anchor_points[idx].m_anchor_point = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, 200, LINE_HEIGHT), "Anchor #" + idx, m_bezier_curve.m_anchor_points[idx].m_anchor_point);
						m_bezier_curve.m_anchor_points[idx].m_handle_point = EditorGUI.Vector3Field(new Rect(x_offset + 210, y_offset, 200, LINE_HEIGHT), "Handle #" + idx, m_bezier_curve.m_anchor_points[idx].m_handle_point);

						if (GUI.Button(new Rect(x_offset - 23, y_offset + 12, 20, LINE_HEIGHT), "X"))
						{
							m_bezier_curve.m_anchor_points.RemoveAt(idx);
							idx--;
							break;
						}

						y_offset += LINE_HEIGHT * 2;
					}
			}

			return (y_offset) - position.y;
		}


		if (Progression != (int)ValueProgression.Eased)
		{
			var toggle_pos = new Rect();
			if (offset_legal)
				toggle_pos = new Rect(x_offset + 190, y_offset - LINE_HEIGHT, 200, LINE_HEIGHT);
			else
			{
				toggle_pos = new Rect(x_offset, y_offset, 200, LINE_HEIGHT);

				y_offset += LINE_HEIGHT;
			}
			m_force_position_override = EditorGUI.Toggle(toggle_pos, "Force This Position?", m_force_position_override);
		}

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
#endif

	public override JSONValue ExportData()
	{
		var json_data = base.ExportData().Obj;

		json_data["m_force_position_override"] = m_force_position_override;
		if (Progression == CURVE_OPTION_INDEX)
			json_data["m_bezier_curve"] = m_bezier_curve.ExportData();

		return new JSONValue(json_data);
	}

	public override void ImportData(JSONObject json_data)
	{
		base.ImportData(json_data);

		m_force_position_override = json_data["m_force_position_override"].Boolean;

		if (json_data.ContainsKey("m_bezier_curve"))
			m_bezier_curve.ImportData(json_data["m_bezier_curve"].Obj);
	}

	public void SetBezierCurve(TextFxBezierCurve bezier_curve)
	{
		m_progression_idx = CURVE_OPTION_INDEX;
		m_bezier_curve = bezier_curve;
	}

	public void SetBezierCurve(params Vector3[] curve_points)
	{
		m_progression_idx = CURVE_OPTION_INDEX;

		var bezier_curve = new TextFxBezierCurve();
		bezier_curve.m_anchor_points = new List<BezierCurvePoint>();

		BezierCurvePoint curve_point = null;
		var idx = 0;
		foreach (var point in curve_points)
		{
			if (idx % 2 == 0)
			{
				curve_point = new BezierCurvePoint();
				curve_point.m_anchor_point = point;
			}
			else
			{
				curve_point.m_handle_point = point;
				bezier_curve.m_anchor_points.Add(curve_point);
			}

			idx++;
		}

		if (idx % 2 == 1)
		{
			curve_point.m_handle_point = curve_point.m_anchor_point;
			bezier_curve.m_anchor_points.Add(curve_point);
		}

		m_bezier_curve = bezier_curve;
	}

	public void SetConstant(Vector3 constant_value, bool force_this_position = false)
	{
		m_progression_idx = (int)ValueProgression.Constant;
		m_from = constant_value;
		m_force_position_override = force_this_position;
	}

	public static string[] EXTRA_OPTION_STRINGS = { CURVE_OPTION_STRING };
	public static int[] EXTRA_OPTION_INDEXS = { CURVE_OPTION_INDEX };
}