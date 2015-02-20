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
public abstract class ActionVariableProgression
{
	[SerializeField]
	protected AnimatePerOptions m_animate_per;

	[SerializeField]
	protected AnimationCurve m_custom_ease_curve = new AnimationCurve();

	[SerializeField]
	protected EasingEquation m_ease_type = EasingEquation.Linear;

	[SerializeField]
	protected bool m_is_offset_from_last;

	[SerializeField]
	protected bool m_override_animate_per_option;

	[SerializeField]
	protected ValueProgression m_progression = ValueProgression.Constant; // Legacy field

	[SerializeField]
	protected int m_progression_idx = -1;

	[SerializeField]
	protected bool m_to_to_bool;

	[SerializeField]
	protected bool m_unique_randoms;

	public AnimatePerOptions AnimatePer { get { return m_animate_per; } set { m_animate_per = value; } }
	public AnimationCurve CustomEaseCurve { get { return m_custom_ease_curve; } }
	public EasingEquation EaseType { get { return m_ease_type; } }
	public bool IsOffsetFromLast { get { return m_is_offset_from_last; } set { m_is_offset_from_last = value; } }
	public bool OverrideAnimatePerOption { get { return m_override_animate_per_option; } set { m_override_animate_per_option = value; } }

	public int Progression
	{
		get
		{
			if (m_progression_idx == -1)
				m_progression_idx = (int)m_progression;
			return m_progression_idx;
		}
	}

	public virtual int[] ProgressionExtraOptionIndexes { get { return null; } }
	public virtual string[] ProgressionExtraOptions { get { return null; } }
	public bool UniqueRandom { get { return Progression == (int)ValueProgression.Random && m_unique_randoms; } }
	public bool UsingThirdValue { get { return m_to_to_bool; } }
#if UNITY_EDITOR
	public float DrawProgressionEditorHeader(GUIContent label, Rect position, bool offset_legal, bool unique_randoms_legal, bool bold_label = true, string[] extra_options = null, int[] extra_option_indexes = null)
	{
		var x_offset = position.x;
		var y_offset = position.y;
		if (bold_label)
			EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), label, EditorStyles.boldLabel);
		else
			EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), label);
		x_offset += PROGRESSION_HEADER_LABEL_WIDTH;

		var options = Enum.GetNames(typeof(ValueProgression));
		var option_indexes = PROGRESSION_ENUM_VALUES;

		if (extra_options != null && extra_option_indexes != null && extra_options.Length > 0 && extra_options.Length == extra_option_indexes.Length)
		{
			var original_length = options.Length;
			Array.Resize(ref options, options.Length + extra_options.Length);
			Array.Copy(extra_options, 0, options, original_length, extra_options.Length);

			original_length = option_indexes.Length;
			Array.Resize(ref option_indexes, option_indexes.Length + extra_option_indexes.Length);
			Array.Copy(extra_option_indexes, 0, option_indexes, original_length, extra_option_indexes.Length);
		}

		m_progression_idx = EditorGUI.IntPopup(new Rect(x_offset, y_offset, ENUM_SELECTOR_WIDTH_SMALL + 18, LINE_HEIGHT), Progression, options, option_indexes);
		x_offset += ENUM_SELECTOR_WIDTH_SMALL + 25;

		if (m_progression_idx == (int)ValueProgression.Eased)
		{
			EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), new GUIContent("Function :", "Easing function used to lerp values between 'from' and 'to'."));
			x_offset += 65;
			m_ease_type = (EasingEquation)EditorGUI.EnumPopup(new Rect(x_offset, y_offset, ENUM_SELECTOR_WIDTH_MEDIUM, LINE_HEIGHT), m_ease_type);
			x_offset += ENUM_SELECTOR_WIDTH_MEDIUM + 10;

			EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), new GUIContent("3rd?", "Option to add a third state to lerp values between."));
			x_offset += 35;
			m_to_to_bool = EditorGUI.Toggle(new Rect(x_offset, y_offset, ENUM_SELECTOR_WIDTH_MEDIUM, LINE_HEIGHT), m_to_to_bool);
		}
		else if (m_progression_idx == (int)ValueProgression.Random && unique_randoms_legal)
			m_unique_randoms = EditorGUI.Toggle(new Rect(x_offset, y_offset, 200, LINE_HEIGHT), new GUIContent("Unique Randoms?", "Denotes whether a new random value will be picked each time this action is repeated (like when in a loop)."), m_unique_randoms);
		y_offset += LINE_HEIGHT;

		if (offset_legal)
		{
			m_is_offset_from_last = EditorGUI.Toggle(new Rect(position.x + ACTION_INDENT_LEVEL_1, y_offset, 200, LINE_HEIGHT), new GUIContent("Offset From Last?", "Denotes whether this value will offset from whatever value it had in the last state. End states offset the start state. Start states offset the previous actions end state."), m_is_offset_from_last);
			y_offset += LINE_HEIGHT;
		}

		if ((m_progression_idx == (int)ValueProgression.Eased || m_progression_idx == (int)ValueProgression.Random))
		{
			m_override_animate_per_option = EditorGUI.Toggle(new Rect(position.x + ACTION_INDENT_LEVEL_1, y_offset, 200, LINE_HEIGHT), new GUIContent("Override AnimatePer?", "Denotes whether this state value progression will use the global 'Animate Per' setting, or define its own."), m_override_animate_per_option);
			if (m_override_animate_per_option)
				m_animate_per = (AnimatePerOptions)EditorGUI.EnumPopup(new Rect(position.x + ACTION_INDENT_LEVEL_1 + 200, y_offset, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), m_animate_per);

			y_offset += LINE_HEIGHT;
		}
		else
			m_override_animate_per_option = false;

		return position.y + (y_offset - position.y);
	}
#endif

	protected void ExportBaseData(ref JSONObject json_data)
	{
		json_data["m_progression"] = Progression;
		json_data["m_ease_type"] = (int)m_ease_type;
		json_data["m_is_offset_from_last"] = m_is_offset_from_last;
		json_data["m_to_to_bool"] = m_to_to_bool;
		json_data["m_unique_randoms"] = m_unique_randoms;
		json_data["m_animate_per"] = (int)m_animate_per;
		json_data["m_override_animate_per_option"] = m_override_animate_per_option;

		if (Progression == (int)ValueProgression.EasedCustom)
			json_data["m_custom_ease_curve"] = m_custom_ease_curve.ExportData();
	}

	public abstract JSONValue ExportData();

	public int GetProgressionIndex(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default) { return progression_variables.GetValue(m_override_animate_per_option ? m_animate_per : animate_per_default); }

	protected void ImportBaseData(JSONObject json_data)
	{
		m_progression_idx = (int)json_data["m_progression"].Number;
		m_ease_type = (EasingEquation)(int)json_data["m_ease_type"].Number;
		m_is_offset_from_last = json_data["m_is_offset_from_last"].Boolean;
		m_to_to_bool = json_data["m_to_to_bool"].Boolean;
		m_unique_randoms = json_data["m_unique_randoms"].Boolean;
		m_animate_per = (AnimatePerOptions)(int)json_data["m_animate_per"].Number;
		m_override_animate_per_option = json_data["m_override_animate_per_option"].Boolean;
		if (json_data.ContainsKey("m_custom_ease_curve"))
			m_custom_ease_curve = json_data["m_custom_ease_curve"].Array.JSONtoAnimationCurve();
	}

	public void ImportBaseLagacyData(KeyValuePair<string, string> value_pair)
	{
		switch (value_pair.Key)
		{
			case "m_progression":
				m_progression_idx = int.Parse(value_pair.Value);
				break;
			case "m_ease_type":
				m_ease_type = (EasingEquation)int.Parse(value_pair.Value);
				break;
			case "m_is_offset_from_last":
				m_is_offset_from_last = bool.Parse(value_pair.Value);
				break;
			case "m_to_to_bool":
				m_to_to_bool = bool.Parse(value_pair.Value);
				break;
			case "m_unique_randoms":
				m_unique_randoms = bool.Parse(value_pair.Value);
				break;
			case "m_animate_per":
				m_animate_per = (AnimatePerOptions)int.Parse(value_pair.Value);
				break;
			case "m_override_animate_per_option":
				m_override_animate_per_option = bool.Parse(value_pair.Value);
				break;
			case "m_custom_ease_curve":
				m_custom_ease_curve = value_pair.Value.ToAnimationCurve();
				break;
		}
	}

	public abstract void ImportData(JSONObject json_data);

#if UNITY_EDITOR
	protected const float LINE_HEIGHT = 20;
	protected const float VECTOR_3_WIDTH = 300;
	protected const float PROGRESSION_HEADER_LABEL_WIDTH = 150;
	protected const float ACTION_INDENT_LEVEL_1 = 10;
	protected const float ENUM_SELECTOR_WIDTH = 300;
	protected const float ENUM_SELECTOR_WIDTH_MEDIUM = 120;
	protected const float ENUM_SELECTOR_WIDTH_SMALL = 70;

	protected static int[] PROGRESSION_ENUM_VALUES = { 0, 1, 2, 3 };
#endif
}