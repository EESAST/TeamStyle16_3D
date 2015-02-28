#define BACKWARDS_COMPATIBLE

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Boomlagoon.JSON;
using UnityEngine;
#if !UNITY_WINRT
using System.Xml;

#endif

#endregion

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer)), ExecuteInEditMode, AddComponentMenu("TextFx/EffectManager")]
public class EffectManager : MonoBehaviour
{
	public delegate void OnAnimationFinish();

	private const float BASE_LINE_HEIGHT = 1.05f;
	private const float FONT_SCALE_FACTOR = 10f;
	private const int JSON_EXPORTER_VERSION = 1;
	public static string m_version = "v2.8";
	public AnimatePerOptions m_animate_per = AnimatePerOptions.LETTER;
	private OnAnimationFinish m_animation_callback; // Callback called after animation has finished
	public float m_animation_speed_factor = 1;
	private float m_animation_timer;
	[SerializeField] private List<AudioSource> m_audio_sources; // List of AudioSources used for sound effects
	public float m_begin_delay;
	public bool m_begin_on_start;
	public float m_character_size = 1;
	[SerializeField] private string m_current_font_data_file_name = "";
	[SerializeField] private string m_current_font_name = "";
	[SerializeField] private CustomFontCharacterData m_custom_font_data;
	public TextDisplayAxis m_display_axis = TextDisplayAxis.HORIZONTAL;
	[SerializeField] private List<ParticleEffectInstanceManager> m_effect_managers;
	[SerializeField] private float m_font_baseline;
	public float m_font_baseline_override;
	public TextAsset m_font_data_file;
	public Material m_font_material;
	private float m_last_time;
	[SerializeField] private LetterSetup[] m_letters;
	public float m_line_height_factor = 1;
	private int m_lowest_action_progress;
	[SerializeField] private List<LetterAnimation> m_master_animations;
	public float m_max_width;
	private Mesh m_mesh;
	private CombineInstance[] m_mesh_combine_instance;
	private MeshFilter m_mesh_filter;
	[SerializeField] private int m_number_of_lines = -1;
	[SerializeField] private int m_number_of_words = -1;
	public ON_FINISH_ACTION m_on_finish_action = ON_FINISH_ACTION.NONE;
	public bool m_override_font_baseline = false;
	[SerializeField] private List<ParticleEmitter> m_particle_emitters;
	[SerializeField] private List<ParticleSystem> m_particle_systems;
	private bool m_paused;
	public Vector2 m_px_offset = new Vector2(0, 0);
	private Renderer m_renderer;
	private bool m_running;
	public string m_text = "";
	public TextAlignment m_text_alignment = TextAlignment.Left;
	public TextAnchor m_text_anchor = TextAnchor.MiddleCenter;
	[SerializeField] private List<TextSizeData> m_text_datas;
	public AnimationTime m_time_type = AnimationTime.GAME_TIME;
	private float m_total_text_width, m_total_text_height;
	public float FontBaseLine { get { return m_override_font_baseline ? m_font_baseline_override : m_font_baseline / FontScale; } }
	private float FontScale { get { return FONT_SCALE_FACTOR / m_character_size; } }
	public bool HasAudioParticleChildInstances { get { return (m_audio_sources != null && m_particle_emitters != null && m_particle_systems != null) && (m_audio_sources.Count > 0 || m_particle_emitters.Count > 0 || m_particle_systems.Count > 0); } }
	public bool IsFontBaseLineSet { get { return m_override_font_baseline || m_font_baseline != 0; } }

	public bool IsFontDataAssigned
	{
		get
		{
#if !UNITY_3_5
			if (m_font != null)
				return true;
#endif

			if (m_font_data_file != null && m_font_material != null)
				return true;

			return false;
		}
	}

	public List<LetterAnimation> LetterAnimations
	{
		get
		{
			if (m_master_animations == null)
				m_master_animations = new List<LetterAnimation>();
			return m_master_animations;
		}
	}

	public float LineHeight { get; private set; }
	// Getter/Setters
	private float LineHeightFactor { get { return m_line_height_factor * BASE_LINE_HEIGHT; } }
	public Transform m_transform { get; private set; }
	public int NumAnimations { get { return m_master_animations == null ? 0 : m_master_animations.Count; } }
	public List<ParticleEffectInstanceManager> ParticleEffectManagers { get { return m_effect_managers; } }

	public bool Paused
	{
		get { return m_paused; }
		set
		{
			m_paused = value;

			if (!m_paused && m_time_type == AnimationTime.REAL_TIME)
				m_last_time = Time.realtimeSinceStartup;

			PauseAllParticleEffects(m_paused);
		}
	}

	public bool Playing { get { return m_running && !m_paused; } }
	public Vector3 Position { get { return m_transform != null ? m_transform.position : transform.position; } }
	public Quaternion Rotation { get { return m_transform != null ? m_transform.rotation : transform.rotation; } }
	public Vector3 Scale { get { return m_transform != null ? m_transform.localScale : transform.localScale; } }
	public string Text { get { return m_text; } set { SetText(value); } }
	public List<TextSizeData> TextDimensions { get { return m_text_datas; } }

	public void AddAnimation()
	{
		if (m_master_animations == null)
			m_master_animations = new List<LetterAnimation>();

		m_master_animations.Add(new LetterAnimation());
	}

	private AudioSource AddNewAudioChild()
	{
		var new_audio_source = new GameObject("TextFx_AudioSource");
		new_audio_source.transform.parent = transform;

		var a_source = new_audio_source.AddComponent<AudioSource>();

		a_source.playOnAwake = false;

		if (m_audio_sources == null)
			m_audio_sources = new List<AudioSource>();

		m_audio_sources.Add(a_source);

		return a_source;
	}

	public void ClearCachedAudioParticleInstances(bool refresh_latest = false)
	{
		if (refresh_latest)
		{
			m_audio_sources = new List<AudioSource>(gameObject.GetComponentsInChildren<AudioSource>());
			m_particle_emitters = new List<ParticleEmitter>(gameObject.GetComponentsInChildren<ParticleEmitter>());
			m_particle_systems = new List<ParticleSystem>(gameObject.GetComponentsInChildren<ParticleSystem>());
		}

		foreach (var a_source in m_audio_sources)
			DestroyImmediate(a_source.gameObject);
		m_audio_sources = new List<AudioSource>();

		foreach (var p_emitter in m_particle_emitters)
			DestroyImmediate(p_emitter.gameObject);
		m_particle_emitters = new List<ParticleEmitter>();

		foreach (var p_system in m_particle_systems)
			DestroyImmediate(p_system.gameObject);
		m_particle_systems = new List<ParticleSystem>();

		m_effect_managers = new List<ParticleEffectInstanceManager>();
	}

	public void ClearFontCharacterData()
	{
		if (m_custom_font_data != null)
		{
			m_custom_font_data.m_character_infos.Clear();
			m_custom_font_data = null;
		}
	}

	// Continue all animations
	public void ContinueAnimation() { ContinueAnimation(-1); }

	// Continue specific animation with given index
	public void ContinueAnimation(int animation_index)
	{
		if (m_master_animations == null)
			return;

		if (animation_index >= 0)
			// Animation index specified
			ContinueAnimationState(animation_index);
		else
		// Continue all animations
			for (var anim_idx = 0; anim_idx < m_master_animations.Count; anim_idx++)
				ContinueAnimationState(anim_idx);
	}

	private void ContinueAnimationState(int animation_index)
	{
		var animation = m_master_animations[animation_index];
		animation.CurrentAnimationState = LETTER_ANIMATION_STATE.PLAYING;

		foreach (var letter_idx in animation.m_letters_to_animate)
			if (m_letters[letter_idx].WaitingToSync)
				// letter is in a waiting state. Continue it beyond this wait state.
				m_letters[letter_idx].ContinueAction(m_animation_timer, animation, m_animate_per);
			else if (m_letters[letter_idx].ActiveLoopCycles.Count > 0)
				// Letter is in a loop. Make the current loop cycle it's last, so that it progresses
				m_letters[letter_idx].ActiveLoopCycles[0].m_number_of_loops = 1;
	}

	public string ExportData(bool hard_copy = false)
	{
		var json_data = new JSONObject();

		json_data["TEXTFX_EXPORTER_VERSION"] = JSON_EXPORTER_VERSION;
		json_data["m_animate_per"] = (int)m_animate_per;
		json_data["m_display_axis"] = (int)m_display_axis;

		if (hard_copy)
		{
			json_data["m_begin_delay"] = m_begin_delay;
			json_data["m_begin_on_start"] = m_begin_on_start;
			json_data["m_character_size"] = m_character_size;
			json_data["m_line_height"] = m_line_height_factor;
			json_data["m_max_width"] = m_max_width;
			json_data["m_on_finish_action"] = (int)m_on_finish_action;
			json_data["m_px_offset"] = m_px_offset.Vector2ToJSON();
//			json_data["m_text"] = m_text;
			json_data["m_text_alignment"] = (int)m_text_alignment;
			json_data["m_text_anchor"] = (int)m_text_anchor;
			json_data["m_time_type"] = (int)m_time_type;
		}

		var letter_animations_data = new JSONArray();
		if (m_master_animations != null)
			foreach (var anim in m_master_animations)
				letter_animations_data.Add(anim.ExportData());
		json_data["LETTER_ANIMATIONS_DATA"] = letter_animations_data;

		return json_data.ToString();
	}

	public static float FloatLerp(float from_val, float to_val, float progress)
	{
		if (progress <= 1 && progress >= 0)
			return Mathf.Lerp(from_val, to_val, progress);
		return from_val + ((to_val - from_val) * progress);
	}

#if !UNITY_3_5
	// Called by TextfxFontChangeListener when a Font OnPostprocessAllAssets() call is triggered.
	// Checks if imported font is same as one being used.
	public void FontImportDetected(string font_name)
	{
		if (m_font == null)
			return;

		if (font_name.Equals(m_font.name.ToLower()))
		{
			m_current_font_name = "";

			SetText(m_text, true);

			m_font.textureRebuildCallback += FontTextureRebuilt;
		}
	}
#endif

	private void FontTextureRebuilt()
	{
		//Debug.Log("FontTextureRebuilt");

		SetText(m_text, true);
	}

	public LetterAnimation GetAnimation(int index)
	{
		if (m_master_animations != null && m_master_animations.Count > index && index >= 0)
			return m_master_animations[index];
		return null;
	}

	private bool GetCharacterInfo(char m_character, ref CustomCharacterInfo char_info)
	{
		if (m_character.Equals('\n') || m_character.Equals('\r'))
			return true;

#if !UNITY_3_5
		if (m_font != null)
		{
			if (!m_current_font_name.Equals(m_font.name))
			{
				// Recalculate font's baseline value
				// Checks through all available alpha characters and uses the most common bottom y_axis value as the baseline for the font.

				var baseline_values = new Dictionary<float, int>();
				float baseline;
				foreach (var character in m_font.characterInfo)
					// only check alpha characters (a-z, A-Z)
					if ((character.index >= 97 && character.index < 123) || (character.index >= 65 && character.index < 91))
					{
						baseline = -character.vert.y - character.vert.height;
						if (baseline_values.ContainsKey(baseline))
							baseline_values[baseline] ++;
						else
							baseline_values[baseline] = 1;
					}

				// Find most common baseline value used by the letters
				var idx = 0;
				int highest_num = 0, highest_idx = -1;
				float most_common_baseline = -1;
				foreach (var num in baseline_values.Values)
				{
					if (highest_idx == -1 || num > highest_num)
					{
						highest_idx = idx;
						highest_num = num;
					}
					idx++;
				}

				// Retrieve the most common value and use as baseline value
				idx = 0;
				foreach (var baseline_key in baseline_values.Keys)
				{
					if (idx == highest_idx)
					{
						most_common_baseline = baseline_key;
						break;
					}
					idx++;
				}

				m_font_baseline = most_common_baseline;

				// Set font name to current, to ensure this check doesn't happen each time
				m_current_font_name = m_font.name;
			}

			var font_char_info = new CharacterInfo();
			m_font.GetCharacterInfo(m_character, out font_char_info);

			char_info.flipped = font_char_info.flipped;
			char_info.uv = font_char_info.uv;
			char_info.vert = font_char_info.vert;
			char_info.width = font_char_info.width;

			// Scale char_info values
			char_info.vert.x /= FontScale;
			char_info.vert.y /= FontScale;
			char_info.vert.width /= FontScale;
			char_info.vert.height /= FontScale;
			char_info.width /= FontScale;

			if (font_char_info.width == 0)
				// Invisible character info returned because character is not contained within the font
				Debug.LogWarning("Character '" + GetHumanReadableCharacterString(m_character) + "' not found. Check that font '" + m_font.name + "' supports this character.");

			return true;
		}
#endif

		if (m_font_data_file != null)
		{
			if (m_custom_font_data == null || !m_font_data_file.name.Equals(m_current_font_data_file_name))
				// Setup m_custom_font_data for the custom font.
#if !UNITY_WINRT
				if (m_font_data_file.text.Substring(0, 5).Equals("<?xml"))
				{
					// Text file is in xml format

					m_current_font_data_file_name = m_font_data_file.name;
					m_custom_font_data = new CustomFontCharacterData();

					var reader = new XmlTextReader(new StringReader(m_font_data_file.text));

					var texture_width = 0;
					var texture_height = 0;
					int uv_x, uv_y;
					float width, height, xoffset, yoffset, xadvance;
					CustomCharacterInfo character_info;

					while (reader.Read())
						if (reader.IsStartElement())
							if (reader.Name.Equals("common"))
							{
								texture_width = int.Parse(reader.GetAttribute("scaleW"));
								texture_height = int.Parse(reader.GetAttribute("scaleH"));

								m_font_baseline = int.Parse(reader.GetAttribute("base"));
							}
							else if (reader.Name.Equals("char"))
							{
								uv_x = int.Parse(reader.GetAttribute("x"));
								uv_y = int.Parse(reader.GetAttribute("y"));
								width = float.Parse(reader.GetAttribute("width"));
								height = float.Parse(reader.GetAttribute("height"));
								xoffset = float.Parse(reader.GetAttribute("xoffset"));
								yoffset = float.Parse(reader.GetAttribute("yoffset"));
								xadvance = float.Parse(reader.GetAttribute("xadvance"));

								character_info = new CustomCharacterInfo();
								character_info.flipped = false;
								character_info.uv = new Rect(uv_x / (float)texture_width, 1 - (uv_y / (float)texture_height) - height / texture_height, width / texture_width, height / texture_height);
								character_info.vert = new Rect(xoffset, -yoffset, width, -height);
								character_info.width = xadvance;

								m_custom_font_data.m_character_infos.Add(int.Parse(reader.GetAttribute("id")), character_info);
							}
				}
				else
#endif
					if (m_font_data_file.text.Substring(0, 4).Equals("info"))
					{
						// Plain txt format
						m_current_font_data_file_name = m_font_data_file.name;
						m_custom_font_data = new CustomFontCharacterData();

						var texture_width = 0;
						var texture_height = 0;
						int uv_x, uv_y;
						float width, height, xoffset, yoffset, xadvance;
						CustomCharacterInfo character_info;
						string[] data_fields;

						var text_lines = m_font_data_file.text.Split('\n');

						foreach (var font_data in text_lines)
							if (font_data.Length >= 5 && font_data.Substring(0, 5).Equals("char "))
							{
								// character data line
								data_fields = ParseFieldData(font_data, new[] { "id=", "x=", "y=", "width=", "height=", "xoffset=", "yoffset=", "xadvance=" });
								uv_x = int.Parse(data_fields[1]);
								uv_y = int.Parse(data_fields[2]);
								width = float.Parse(data_fields[3]);
								height = float.Parse(data_fields[4]);
								xoffset = float.Parse(data_fields[5]);
								yoffset = float.Parse(data_fields[6]);
								xadvance = float.Parse(data_fields[7]);

								character_info = new CustomCharacterInfo();
								character_info.flipped = false;
								character_info.uv = new Rect(uv_x / (float)texture_width, 1 - (uv_y / (float)texture_height) - height / texture_height, width / texture_width, height / texture_height);
								character_info.vert = new Rect(xoffset, -yoffset + 1, width, -height);
								character_info.width = xadvance;

								m_custom_font_data.m_character_infos.Add(int.Parse(data_fields[0]), character_info);
							}
							else if (font_data.Length >= 6 && font_data.Substring(0, 6).Equals("common"))
							{
								data_fields = ParseFieldData(font_data, new[] { "scaleW=", "scaleH=", "base=" });
								texture_width = int.Parse(data_fields[0]);
								texture_height = int.Parse(data_fields[1]);

								m_font_baseline = int.Parse(data_fields[2]);
							}
					}

			if (m_custom_font_data.m_character_infos.ContainsKey(m_character))
			{
				m_custom_font_data.m_character_infos[m_character].ScaleClone(FontScale, ref char_info);

				return true;
			}
		}

		return false;
	}

	private string GetHumanReadableCharacterString(char character)
	{
		if (character.Equals('\n'))
			return "[NEW LINE]";
		if (character.Equals(' '))
			return "[SPACE]";
		if (character.Equals('\r'))
			return "[CARRIAGE RETURN]";
		if (character.Equals('\t'))
			return "[TAB]";
		return "" + character;
	}

	public Vector3 GetLetterPosition(int object_idx, OBJ_POS position_requested = OBJ_POS.CENTER, TRANSFORM_SPACE transform_space = TRANSFORM_SPACE.WORLD)
	{
		if (m_letters == null || m_letters.Length == 0)
			return Vector3.zero;

		object_idx = Mathf.Clamp(object_idx, 0, m_letters.Length - 1);

		switch (position_requested)
		{
			case OBJ_POS.CENTER:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[object_idx].Center : m_letters[object_idx].CenterLocal;
			case OBJ_POS.BOTTOM_LEFT:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[object_idx].BottomLeft : m_letters[object_idx].BottomLeftLocal;
			case OBJ_POS.BOTTOM_RIGHT:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[object_idx].BottomRight : m_letters[object_idx].BottomRightLocal;
			case OBJ_POS.TOP_LEFT:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[object_idx].TopLeft : m_letters[object_idx].TopLeftLocal;
			case OBJ_POS.TOP_RIGHT:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[object_idx].TopRight : m_letters[object_idx].TopRightLocal;

			default:
				return Vector3.zero;
		}
	}

	public Quaternion GetLetterRotation(int object_idx, TRANSFORM_SPACE transform_space = TRANSFORM_SPACE.WORLD)
	{
		if (m_letters == null || m_letters.Length == 0)
			return Quaternion.identity;

		object_idx = Mathf.Clamp(object_idx, 0, m_letters.Length - 1);

		return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[object_idx].Rotation : m_letters[object_idx].RotationLocal;
	}

	public Vector3 GetLetterScale(int object_idx, TRANSFORM_SPACE transform_space = TRANSFORM_SPACE.WORLD)
	{
		if (m_letters == null || m_letters.Length == 0)
			return Vector3.one;

		object_idx = Mathf.Clamp(object_idx, 0, m_letters.Length - 1);

		return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[object_idx].Scale : m_letters[object_idx].ScaleLocal;
	}

	public void ImportData(string data, bool force_clear_old_audio_particles = false)
	{
		if (force_clear_old_audio_particles)
			ClearCachedAudioParticleInstances(true);

		var json_data = JSONObject.Parse(data, true);

		if (json_data != null)
		{
			m_animate_per = (AnimatePerOptions)(int)json_data["m_animate_per"].Number;
			m_display_axis = (TextDisplayAxis)(int)json_data["m_display_axis"].Number;

			if (json_data.ContainsKey("m_begin_delay"))
				m_begin_delay = (float)json_data["m_begin_delay"].Number;
			if (json_data.ContainsKey("m_begin_on_start"))
				m_begin_on_start = json_data["m_begin_on_start"].Boolean;
			if (json_data.ContainsKey("m_character_size"))
				m_character_size = (float)json_data["m_character_size"].Number;
			if (json_data.ContainsKey("m_line_height"))
				m_line_height_factor = (float)json_data["m_line_height"].Number;
			if (json_data.ContainsKey("m_max_width"))
				m_max_width = (float)json_data["m_max_width"].Number;
			if (json_data.ContainsKey("m_on_finish_action"))
				m_on_finish_action = (ON_FINISH_ACTION)(int)json_data["m_on_finish_action"].Number;
			if (json_data.ContainsKey("m_px_offset"))
				m_px_offset = json_data["m_px_offset"].Obj.JSONtoVector2();
//			if(json_data.ContainsKey("m_text")) m_text = json_data["m_text"].Str;
			if (json_data.ContainsKey("m_text_alignment"))
				m_text_alignment = (TextAlignment)(int)json_data["m_text_alignment"].Number;
			if (json_data.ContainsKey("m_text_anchor"))
				m_text_anchor = (TextAnchor)(int)json_data["m_text_anchor"].Number;
			if (json_data.ContainsKey("m_time_type"))
				m_time_type = (AnimationTime)(int)json_data["m_time_type"].Number;

			m_master_animations = new List<LetterAnimation>();
			LetterAnimation letter_anim;
			foreach (var animation_data in json_data["LETTER_ANIMATIONS_DATA"].Array)
			{
				letter_anim = new LetterAnimation();
				letter_anim.ImportData(animation_data.Obj);
				m_master_animations.Add(letter_anim);
			}
		}
		else
		// Import string is not valid JSON, therefore assuming it is in the legacy data import format.
			this.ImportLegacyData(data);

		if (!Application.isPlaying && m_text.Equals(""))
			m_text = "TextFx";

		if (!m_text.Equals(""))
			SetText(m_text);

		ResetAnimation();
	}

	private void OnDestroy()
	{
		// Destroy all letter mesh instances
		if (m_letters != null)
			foreach (var letter in m_letters)
				if (Application.isPlaying)
					Destroy(letter.m_mesh);
				else
					DestroyImmediate(letter.m_mesh);

		// Destroy shared mesh instance.
		if (Application.isPlaying)
			Destroy(m_mesh);
		else
			DestroyImmediate(m_mesh);
	}

	private void OnDisable()
	{
#if !UNITY_3_5
		if (m_font != null)
			m_font.textureRebuildCallback -= FontTextureRebuilt;
#endif
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (m_max_width > 0)
		{
			Gizmos.color = Color.red;

			var position_offset = Vector3.zero;
			if (m_text_anchor == TextAnchor.LowerLeft || m_text_anchor == TextAnchor.MiddleLeft || m_text_anchor == TextAnchor.UpperLeft)
				position_offset += new Vector3((m_max_width > 0 ? m_max_width : m_total_text_width) / 2, 0, 0);
			else if (m_text_anchor == TextAnchor.LowerRight || m_text_anchor == TextAnchor.MiddleRight || m_text_anchor == TextAnchor.UpperRight)
				position_offset -= new Vector3((m_max_width > 0 ? m_max_width : m_total_text_width) / 2, 0, 0);

			if (m_text_anchor == TextAnchor.LowerCenter || m_text_anchor == TextAnchor.LowerLeft || m_text_anchor == TextAnchor.LowerRight)
				position_offset += new Vector3(0, m_total_text_height / 2, 0);
			else if (m_text_anchor == TextAnchor.UpperLeft || m_text_anchor == TextAnchor.UpperCenter || m_text_anchor == TextAnchor.UpperRight)
				position_offset -= new Vector3(0, m_total_text_height / 2, 0);

			if (m_max_width > 0)
			{
				// Left edge limit
				Gizmos.DrawWireCube(transform.position + position_offset - new Vector3(m_max_width / 2, 0, 0), new Vector3(0.01f, m_total_text_height, 0));
				// Right edge limit
				Gizmos.DrawWireCube(transform.position + position_offset + new Vector3(m_max_width / 2, 0, 0), new Vector3(0.01f, m_total_text_height, 0));
			}
		}
	}
#endif

	private void OnEnable()
	{
		if (m_mesh != null)
			return;

		// Set component variable references
		m_mesh_filter = gameObject.GetComponent<MeshFilter>();
		m_transform = transform;

		if (m_mesh_filter.sharedMesh != null)
		{
			// Check for two effects sharing the same SharedMesh instance (occurs when a MeshFilter component is duplicated)
			var objects = FindObjectsOfType(typeof(EffectManager)) as EffectManager[];

			foreach (var effect_manager in objects)
			{
				var otherMeshFilter = effect_manager.m_mesh_filter;
				if (otherMeshFilter != null)
					if (otherMeshFilter.sharedMesh == m_mesh_filter.sharedMesh && otherMeshFilter != m_mesh_filter)
					{
						// Found shared SharedMesh instance; initialising a new one
						m_mesh_filter.mesh = new Mesh();

						// Can't have effects sharing same individual letter meshes either, so flush the letter array
						m_letters = new LetterSetup[0];

						m_mesh = m_mesh_filter.sharedMesh;

						// Reset Text with new meshes
						SetText(m_text);
					}
			}

			m_mesh = m_mesh_filter.sharedMesh;
		}
		else
		{
			m_mesh = new Mesh();
			m_mesh_filter.mesh = m_mesh;

			if (IsFontDataAssigned)
				// Reset Text with new meshes
				SetText(m_text, true);
		}

#if BACKWARDS_COMPATIBLE
		if (m_master_animations != null)
		{
			var imported_legacy_effect_setup = false;

			// Check for:
			// - old particle/audio effect setups to port to new style
			// - old letter anchor definitions to port over
			foreach (var animation in m_master_animations)
				for (var action_idx = 0; action_idx < animation.NumActions; action_idx++)
					imported_legacy_effect_setup = animation.GetAction(action_idx).CheckForLegacySetups();

			if (imported_legacy_effect_setup)
				// Effects were automatically setup for legacy versions.
				// Force calculate action progression values
				PrepareAnimationData();
		}
#endif

#if !UNITY_3_5
		if (m_font != null)
		{
			m_font.textureRebuildCallback += FontTextureRebuilt;

			// Make sure dynamic fonts add all the letters required for this animation
			m_font.RequestCharactersInTexture(m_text);

#if UNITY_EDITOR
			if (!Application.isPlaying)
				// Force Texture Rebuild to avoid dynamic font texture size changes since playing. 
				FontTextureRebuilt();
#endif
		}
#endif
	}

	private string[] ParseFieldData(string data_string, string[] fields)
	{
		var data_values = new string[fields.Length];
		int count = 0, data_start_idx, data_end_idx;

		foreach (var field_name in fields)
		{
			data_start_idx = data_string.IndexOf(field_name) + field_name.Length;
			data_end_idx = data_string.IndexOf(" ", data_start_idx);

			data_values[count] = data_string.Substring(data_start_idx, data_end_idx - data_start_idx);

			count++;
		}

		return data_values;
	}

	private void PauseAllParticleEffects(bool paused)
	{
		if (m_effect_managers != null)
			foreach (var particle_effect in m_effect_managers)
				particle_effect.Pause(paused);
	}

	public void PlayAnimation(OnAnimationFinish animation_callback)
	{
		m_animation_callback = animation_callback;

		PlayAnimation();
	}

	public void PlayAnimation(float delay, OnAnimationFinish animation_callback)
	{
		m_animation_callback = animation_callback;

		PlayAnimation(delay);
	}

	public void PlayAnimation(float delay = 0)
	{
		if (m_master_animations == null || m_master_animations.Count == 0)
		{
			Debug.LogWarning("Unable to execute PlayAnimation(). No animations defined on this EffectManager instance");
			return;
		}

		var num_letters = m_letters.Length;

		m_audio_sources = new List<AudioSource>(gameObject.GetComponentsInChildren<AudioSource>());
		m_particle_emitters = new List<ParticleEmitter>(gameObject.GetComponentsInChildren<ParticleEmitter>());
		m_particle_systems = new List<ParticleSystem>(gameObject.GetComponentsInChildren<ParticleSystem>());
		m_effect_managers = new List<ParticleEffectInstanceManager>();

		// Stop all audio sources and particle effects
		foreach (var a_source in m_audio_sources)
			a_source.Stop();

		foreach (var p_emitter in m_particle_emitters)
		{
			p_emitter.emit = false;
			p_emitter.particles = null;
			p_emitter.enabled = false;
		}

		foreach (var p_system in m_particle_systems)
		{
			p_system.Stop();
			p_system.Clear();
		}

		// Prepare Master Animations data and check for particle effect onstart reset
		var reset_mesh = false;
		foreach (var animation in m_master_animations)
		{
			animation.CurrentAnimationState = LETTER_ANIMATION_STATE.PLAYING;

			foreach (var letter_idx in animation.m_letters_to_animate)
				if (letter_idx < num_letters)
				{
					m_letters[letter_idx].Reset(animation);
					m_letters[letter_idx].Active = true;
				}

			// Force letter start positions reset before playing, to avoid onStart particle effect positioning errors
			if (!reset_mesh && animation.NumActions > 0 && animation.GetAction(0).NumParticleEffectSetups > 0)
				foreach (var effect_setup in animation.GetAction(0).ParticleEffectSetups)
					if (effect_setup.m_play_when == PLAY_ITEM_EVENTS.ON_START)
					{
						UpdateMesh(false, true, 0, 0);

						reset_mesh = true;
					}
		}

		m_mesh_combine_instance = new CombineInstance[m_letters.Length];

		m_lowest_action_progress = 0;
		m_animation_timer = 0;


		if (delay > 0)
			StartCoroutine(PlayAnimationAfterDelay(delay));
		else
		{
			if (m_time_type == AnimationTime.REAL_TIME)
				m_last_time = Time.realtimeSinceStartup;

			m_running = true;
			m_paused = false;
		}
	}

	private IEnumerator PlayAnimationAfterDelay(float delay)
	{
		yield return StartCoroutine(TimeDelay(delay, m_time_type));

		if (m_time_type == AnimationTime.REAL_TIME)
			m_last_time = Time.realtimeSinceStartup;

		m_running = true;
		m_paused = false;
	}

	public void PlayAudioClip(AudioEffectSetup effect_setup, AnimationProgressionVariables progression_vars, AnimatePerOptions animate_per)
	{
		var sound_played = false;
		AudioSource source = null;

		if (m_audio_sources != null)
		{
			foreach (var a_source in m_audio_sources)
				if (!a_source.isPlaying)
				{
					// audio source free to play a sound
					source = a_source;

					sound_played = true;
					break;
				}

			if (!sound_played)
				source = AddNewAudioChild();
		}
		else
			source = AddNewAudioChild();

		PlayClip(source, effect_setup.m_audio_clip, effect_setup.m_delay.GetValue(progression_vars, animate_per), effect_setup.m_offset_time.GetValue(progression_vars, animate_per), effect_setup.m_volume.GetValue(progression_vars, animate_per), effect_setup.m_pitch.GetValue(progression_vars, animate_per));
	}

	private void PlayClip(AudioSource a_source, AudioClip clip, float delay, float start_time, float volume, float pitch)
	{
		a_source.clip = clip;
		a_source.time = start_time;
		a_source.volume = volume;
		a_source.pitch = pitch;

#if !UNITY_3_5 && !UNITY_4_0
		a_source.PlayDelayed(delay);
#else
		a_source.Play((ulong)( delay * 44100));
#endif
	}

	public void PlayParticleEffect(Mesh character_mesh, bool m_letter_flipped, ParticleEffectSetup effect_setup, AnimationProgressionVariables progression_vars, AnimatePerOptions animate_per)
	{
		var effect_played = false;

		if (effect_setup.m_legacy_particle_effect != null)
		{
			if (m_particle_emitters == null)
				m_particle_emitters = new List<ParticleEmitter>();

			foreach (var p_emitter in m_particle_emitters)
				if (!p_emitter.emit && p_emitter.particleCount == 0 && p_emitter.name.Equals(effect_setup.m_legacy_particle_effect.name + "(Clone)"))
				{
					m_effect_managers.Add(new ParticleEffectInstanceManager(this, character_mesh, m_letter_flipped, effect_setup, progression_vars, animate_per, p_emitter));

					effect_played = true;
					break;
				}

			if (!effect_played)
			{
				var p_emitter = Instantiate(effect_setup.m_legacy_particle_effect) as ParticleEmitter;
				m_particle_emitters.Add(p_emitter);
#if !UNITY_3_5
				p_emitter.gameObject.SetActive(true);
#else
				p_emitter.gameObject.SetActiveRecursively(true);
#endif
				p_emitter.emit = false;
				p_emitter.transform.parent = transform;

				m_effect_managers.Add(new ParticleEffectInstanceManager(this, character_mesh, m_letter_flipped, effect_setup, progression_vars, animate_per, p_emitter));
			}
		}
		else if (effect_setup.m_shuriken_particle_effect != null)
		{
			if (m_particle_systems == null)
				m_particle_systems = new List<ParticleSystem>();

			foreach (var p_system in m_particle_systems)
				// check if particle system instance is currently not being used, and if it's the same type of effect that we're looking for.
				if (!p_system.isPlaying && p_system.particleCount == 0 && p_system.name.Equals(effect_setup.m_shuriken_particle_effect.name + "(Clone)"))
				{
					m_effect_managers.Add(new ParticleEffectInstanceManager(this, character_mesh, m_letter_flipped, effect_setup, progression_vars, animate_per, particle_system: p_system));

					effect_played = true;
					break;
				}

			if (!effect_played)
			{
				// Make a new instance of the particleSystem effect and add to pool
				var p_system = Instantiate(effect_setup.m_shuriken_particle_effect) as ParticleSystem;
				m_particle_systems.Add(p_system);
#if !UNITY_3_5
				p_system.gameObject.SetActive(true);
#else
				p_system.gameObject.SetActiveRecursively(true);
#endif
				p_system.playOnAwake = false;
				p_system.Stop();
				p_system.transform.parent = transform;

				m_effect_managers.Add(new ParticleEffectInstanceManager(this, character_mesh, m_letter_flipped, effect_setup, progression_vars, animate_per, particle_system: p_system));
			}
		}
	}

	// Calculates values for all animation state progressions using current field values.
	public void PrepareAnimationData()
	{
		if (m_master_animations != null)
			foreach (var animation in m_master_animations)
				animation.PrepareData(m_letters, m_number_of_words, m_number_of_lines, m_animate_per);
	}

	public void RemoveAnimation(int index)
	{
		if (m_master_animations != null && index >= 0 && index < NumAnimations)
			m_master_animations.RemoveAt(index);
	}

	// Reset animation to starting state
	public void ResetAnimation()
	{
		UpdateMesh(false, true, 0, 0);

		foreach (var letter in m_letters)
			letter.AnimStateVars.Reset();

		m_running = false;
		m_paused = false;
		m_lowest_action_progress = 0;
		m_animation_timer = 0;

		StopAllParticleEffects(true);
	}

	public void SetAnimationState(int action_idx, float action_progress, bool update_action_values = false)
	{
		if (update_action_values)
			// Calculate action progression values
			PrepareAnimationData();

		UpdateMesh(false, true, action_idx, action_progress);
	}

	// Set Text Effect to its end state
	public void SetEndState()
	{
		m_running = false;
		m_paused = false;

		if (m_master_animations == null)
			return;

		var longest_action_list = 0;

		foreach (var animation in m_master_animations)
			if (animation.NumActions > longest_action_list)
				longest_action_list = animation.NumActions;

		SetAnimationState(longest_action_list - 1, 1);
	}

#if !UNITY_3_5
	public void SetFont(Font font)
	{
		m_font_data_file = null;
		m_font = font;
		m_font_material = null;

		m_font.textureRebuildCallback += FontTextureRebuilt;

		SetText(m_text, true);
	}
#endif

	public void SetFont(TextAsset font_data, Material font_material)
	{
#if !UNITY_3_5
		m_font = null;
#endif
		m_font_data_file = font_data;
		m_font_material = font_material;

		SetText(m_text, true);
	}

	public void SetText(string new_text, bool force_all_new = false)
	{
		if (m_renderer == null)
			m_renderer = renderer;

		var setup_correctly = false;

		// Automatically assign the font material to the renderer if its not already set
		if ((m_renderer.sharedMaterial == null || m_renderer.sharedMaterial != m_font_material) && m_font_material != null)
			m_renderer.sharedMaterial = m_font_material;
#if !UNITY_3_5
		else if (m_font != null)
		{
			if (m_renderer.sharedMaterial == null || m_renderer.sharedMaterial != m_font_material)
			{
				m_font_material = m_font.material;
				m_renderer.sharedMaterial = m_font_material;
			}

			if (m_renderer.sharedMaterial != null)
				setup_correctly = true;
		}

#endif

		if (!setup_correctly && (m_renderer.sharedMaterial == null || m_font_data_file == null))
		{
			// Incorrectly setup font information
			m_font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
			m_font_material = m_font.material;
			m_renderer.sharedMaterial = m_font_material;
			m_font_data_file = null;
		}

		m_text = new_text;

		// Remove all carriage return char's from new_text
		new_text = new_text.Replace("\r", "");

		var raw_chars = m_text.Replace(" ", "");
		raw_chars = raw_chars.Replace("\n", "");
		raw_chars = raw_chars.Replace("\r", "");
		raw_chars = raw_chars.Replace("\t", "");

		var text_length = new_text.Length;

		var prev_letters = m_letters;

		m_letters = new LetterSetup[raw_chars.Length];

		var char_info = new CustomCharacterInfo();
		CustomCharacterInfo last_char_info = null;

		m_text_datas = new List<TextSizeData>();

#if !UNITY_3_5
		if (m_font != null)
		{
			// Make sure font contains all characters required
			m_font.RequestCharactersInTexture(m_text);

			if (m_font_material.mainTexture.width != m_font_texture_width || m_font_material.mainTexture.height != m_font_texture_height)
			{
				// Font texture size has changed
				m_font_texture_width = m_font_material.mainTexture.width;
				m_font_texture_height = m_font_material.mainTexture.height;
				SetText(m_text, true);
				return;
			}
		}
#endif

		// Calculate bounds of text mesh
		char character;
		float y_max = 0, y_min = 0, x_max = 0, x_min = 0;
		float text_width = 0, text_height = 0;
		var line_letter_idx = 0;
		float line_height_offset = 0;
		float total_text_width = 0, total_text_height = 0;
		float line_width_at_last_space = 0;
		float space_char_offset = 0;
		var last_letter_setup_idx = -1;
		float last_space_y_max = 0;
		float last_space_y_min = 0;
		Rect uv_data;
		LetterSetup last_letter = null;

		float letter_offset = 0;
		var letter_count = 0;
		var line_idx = 0;
		var word_idx = 0;

		LineHeight = 0;

		Action AddNewLineData = () =>
		{
			if (m_display_axis == TextDisplayAxis.HORIZONTAL)
			{
				var height = Mathf.Abs(y_max - y_min) * LineHeightFactor;

				// Check if line is the tallest so far
				if (height > LineHeight)
					LineHeight = height;

				if (last_char_info != null)
					// Re-adjust width of last letter since its the end of the text line
					text_width += -last_char_info.width + last_char_info.vert.width + last_char_info.vert.x;

				m_text_datas.Add(new TextSizeData(text_width, height, line_height_offset, y_max));
				line_height_offset += height;

				if (text_width > total_text_width)
					total_text_width = text_width;
				total_text_height += height;
			}
			else
			{
				var width = Mathf.Abs(x_max - x_min) * LineHeightFactor;

				// Check if line is the tallest so far
				if (width > LineHeight)
					LineHeight = width;

				m_text_datas.Add(new TextSizeData(width, text_height * -1, line_height_offset, 0));
				line_height_offset += width;

				total_text_width += width;
				if (text_height < total_text_height)
					total_text_height = text_height;
			}

			line_letter_idx = 0;
			text_width = 0;
			line_width_at_last_space = 0;
			space_char_offset = 0;
			last_space_y_max = 0;
			last_space_y_min = 0;
			last_letter_setup_idx = -1;
			text_height = 0;
			last_char_info = null;
		};

		for (var letter_idx = 0; letter_idx < text_length; letter_idx++)
		{
			character = new_text[letter_idx];

			if (GetCharacterInfo(character, ref char_info))
			{
				if (character.Equals('\t'))
					continue;
				if (character.Equals(' '))
				{
					if (m_display_axis == TextDisplayAxis.HORIZONTAL)
					{
						// Record the state of the line dims at this point incase the next word is forced onto next line by bound box
						line_width_at_last_space = text_width;
						space_char_offset = char_info.width;
						last_space_y_max = y_max;
						last_space_y_min = y_min;

						last_letter_setup_idx = letter_count;
						text_width += char_info.width;
					}
					else
						char_info.vert.height = -char_info.width;

					// Add space width to offset value
					letter_offset += m_display_axis == TextDisplayAxis.HORIZONTAL ? char_info.width : -char_info.width;

					//Increment word count
					word_idx++;
				}
				else if (character.Equals('\n'))
				{
					AddNewLineData.Invoke();

					letter_offset = 0;
					line_idx++;

					//Increment word count
					word_idx++;
				}
				else
				{
					if (m_display_axis == TextDisplayAxis.HORIZONTAL)
					{
						if (line_letter_idx == 0 || char_info.vert.y > y_max)
							y_max = char_info.vert.y;
						if (line_letter_idx == 0 || char_info.vert.y + char_info.vert.height < y_min)
							y_min = char_info.vert.y + char_info.vert.height;

						// increment the text width by the letter progress width, and then full mesh width for last letter or end of line.
						text_width += (letter_idx == text_length - 1) ? char_info.vert.width + char_info.vert.x : char_info.width;

						// Handle bounding box if setup
						if (m_max_width > 0 && last_letter_setup_idx >= 0)
						{
							var actual_line_width = (letter_idx == text_length - 1) ? text_width : text_width - char_info.width + char_info.vert.width + char_info.vert.x;

							if (actual_line_width > m_max_width)
							{
								// Line exceeds bounding box width
								var new_line_text_width = text_width - line_width_at_last_space - space_char_offset;
								var new_line_y_min = last_space_y_min;
								var new_line_y_max = last_space_y_max;

								// Set line width to what it was at the last space (which is now the end of this line)
								text_width = line_width_at_last_space;
								y_max = last_space_y_max;
								y_min = last_space_y_min;


								letter_offset = 0;
								line_idx++;

								// Need to change the associated line number and positional offset of the letters now on a new line
								for (var past_letter_idx = last_letter_setup_idx; past_letter_idx < letter_count; past_letter_idx++)
								{
									m_letters[past_letter_idx].m_progression_variables.m_line_value = line_idx;

									m_letters[past_letter_idx].m_base_offset = m_display_axis == TextDisplayAxis.HORIZONTAL ? new Vector3(letter_offset, 0, 0) : new Vector3(0, letter_offset, 0);

									letter_offset += m_display_axis == TextDisplayAxis.HORIZONTAL ? m_letters[past_letter_idx].m_offset_width + (m_px_offset.x / FontScale) : m_letters[past_letter_idx].m_height + (-m_px_offset.y / FontScale);
								}

								AddNewLineData.Invoke();

								// Setup current values
								text_width = new_line_text_width;
								y_min = new_line_y_min;
								y_max = new_line_y_max;
							}
						}
					}
					else
					{
						if (line_letter_idx == 0 || char_info.vert.x + char_info.vert.width > x_max)
							x_max = char_info.vert.x + char_info.vert.width;
						if (line_letter_idx == 0 || char_info.vert.x < x_min)
							x_min = char_info.vert.x;

						text_height += char_info.vert.height;
					}


					// Get letterSetup reference
					if (letter_count < prev_letters.Length && !force_all_new)
						last_letter = prev_letters[letter_count];

					// Either reuse the same previous instance of LetterSetup or create a new one for this character.

					if (!force_all_new && prev_letters != null && letter_count < prev_letters.Length && last_letter.m_character.Equals(new_text[letter_idx].ToString()) && last_letter.m_progression_variables.m_letter_value == letter_idx && last_letter.m_mesh != null)
					{
						// Use same LetterSetup from previous configuration
						m_letters[letter_count] = last_letter;

						// Remove instance from previous letters list
						prev_letters[letter_count] = null;

						// position the letter offset again, incase it has changed from previous letters changing.
						last_letter.m_base_offset = m_display_axis == TextDisplayAxis.HORIZONTAL ? new Vector3(letter_offset, 0, 0) : new Vector3(0, letter_offset, 0);
						last_letter.SetupLetterMesh(ref char_info);
						last_letter.m_progression_variables.m_line_value = line_idx;
						last_letter.m_progression_variables.m_word_value = word_idx;
						last_letter.m_base_offsets_setup = false;
					}
					else
					{
						uv_data = char_info.uv;

						if (letter_count < prev_letters.Length && !force_all_new)
						{
							// Recycle last letter instance.
							m_letters[letter_count] = last_letter;

							// Setup Mesh UV co-ords and triangles (and fill in placeholder vertices)
							last_letter.m_mesh.vertices = new[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
							last_letter.m_mesh.uv = new[] { new Vector2(uv_data.x + uv_data.width, uv_data.y + uv_data.height), new Vector2(uv_data.x, uv_data.y + uv_data.height), new Vector2(uv_data.x, uv_data.y), new Vector2(uv_data.x + uv_data.width, uv_data.y) };
							last_letter.m_mesh.triangles = new[] { 2, 1, 0, 3, 2, 0 };
							last_letter.m_mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };

							last_letter.Recycle("" + character, letter_count, last_letter.m_mesh, m_display_axis == TextDisplayAxis.HORIZONTAL ? new Vector3(letter_offset, 0, 0) : new Vector3(0, letter_offset, 0), // base_offset
								ref char_info, line_idx, word_idx, this);

							last_letter.m_base_offsets_setup = false;

							// Remove instance from previous letters list
							prev_letters[letter_count] = null;
						}
						else
						{
							var mesh = new Mesh();
							// Setup Mesh UV co-ords and triangles (and fill in placeholder vertices)
							mesh.vertices = new[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
							mesh.uv = new[] { new Vector2(uv_data.x + uv_data.width, uv_data.y + uv_data.height), new Vector2(uv_data.x, uv_data.y + uv_data.height), new Vector2(uv_data.x, uv_data.y), new Vector2(uv_data.x + uv_data.width, uv_data.y) };
							mesh.triangles = new[] { 2, 1, 0, 3, 2, 0 };
							mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };

							m_letters[letter_count] = new LetterSetup("" + character, letter_count, mesh, m_display_axis == TextDisplayAxis.HORIZONTAL ? new Vector3(letter_offset, 0, 0) : new Vector3(0, letter_offset, 0), // base_offset
								ref char_info, line_idx, word_idx, this);

							if (last_letter != null)
								m_letters[letter_count].SetAnimationVars(last_letter);
						}
					}

					letter_count ++;

					letter_offset += m_display_axis == TextDisplayAxis.HORIZONTAL ? char_info.width + (m_px_offset.x / FontScale) : char_info.vert.height + (-m_px_offset.y / FontScale);


					last_char_info = char_info;
				}
			}

			line_letter_idx++;
		}

		// Save line and word info for later
		m_number_of_words = word_idx + 1;
		m_number_of_lines = line_idx + 1;

		if (m_display_axis == TextDisplayAxis.HORIZONTAL)
		{
			var height = Mathf.Abs(y_max - y_min);
			m_text_datas.Add(new TextSizeData(text_width, height, line_height_offset, y_max));

			if (text_width > total_text_width)
				total_text_width = text_width;
			total_text_height += height;
		}
		else
		{
			var width = Mathf.Abs(x_max - x_min);
			m_text_datas.Add(new TextSizeData(width, text_height * -1, line_height_offset, 0));

			total_text_width += width;

			if (text_height < total_text_height)
				total_text_height = text_height;
		}

		m_total_text_width = total_text_width;
		m_total_text_height = total_text_height;

		for (var idx = 0; idx < m_text_datas.Count; idx++)
		{
			m_text_datas[idx].m_total_text_height = total_text_height * (m_display_axis == TextDisplayAxis.HORIZONTAL ? 1 : -1);

			if (m_max_width > 0)
				m_text_datas[idx].m_total_text_width = m_max_width;
			else
				m_text_datas[idx].m_total_text_width = total_text_width;
		}

		// Destroy any left over unused meshes
		if (prev_letters != null)
			foreach (var old_letter in prev_letters)
				if (old_letter != null)
					// Letter wasn't used in new text setup; delete it's mesh instance.
					if (Application.isPlaying)
						Destroy(old_letter.m_mesh);
					else
						DestroyImmediate(old_letter.m_mesh);


		// Set letter base offsets where needed
		var all_offsets_set = true;
		do
		{
			all_offsets_set = true;

			foreach (var letter in m_letters)
				if (!letter.m_base_offsets_setup)
				{
					if (m_text_datas.Count == 0)
					{
						all_offsets_set = false;
						break;
					}
					letter.SetBaseOffset(m_text_anchor, m_display_axis, m_text_alignment, m_text_datas);
				}

			if (!all_offsets_set)
			{
				// If text_datas has been lost or if legacy effect and hasn't been created, re-set text to recalculate it.
				Debug.LogError("If text_datas has been lost or if legacy effect and hasn't been created, reset text.");
				SetText(m_text);
			}
		}
		while (!all_offsets_set);

		// Calculate action progression values
		PrepareAnimationData();

		// Render state of newly set text
		UpdateMesh(true, true, 0, 0);
	}

	private void Start()
	{
		// Initialise letters
		if (m_letters != null)
		{
			foreach (var letter in m_letters)
				letter.Init(this);

			if (Application.isPlaying && m_begin_on_start)
				PlayAnimation(m_begin_delay);
		}
	}

	private void StopAllParticleEffects(bool force_stop = false)
	{
		if (m_effect_managers != null)
		{
			foreach (var particle_effect in m_effect_managers)
				particle_effect.Stop(force_stop);

			m_effect_managers = new List<ParticleEffectInstanceManager>();
		}

		if (m_particle_systems != null)
			foreach (var p_system in m_particle_systems)
			{
				if (p_system == null)
					continue;

				p_system.Stop();
				p_system.Clear();
			}
		if (m_particle_emitters != null)
			foreach (var p_emit in m_particle_emitters)
			{
				if (p_emit == null)
					continue;

				p_emit.emit = false;
				p_emit.ClearParticles();
			}
	}

	private IEnumerator TimeDelay(float delay, AnimationTime time_type)
	{
		if (time_type == AnimationTime.GAME_TIME)
			yield return new WaitForSeconds(delay);
		else
		{
			float timer = 0;
			var last_time = Time.realtimeSinceStartup;
			float delta_time;
			while (timer < delay)
			{
				delta_time = Time.realtimeSinceStartup - last_time;
				if (delta_time > 0.1f)
					delta_time = 0.1f;
				timer += delta_time;
				last_time = Time.realtimeSinceStartup;
				yield return false;
			}
		}
	}

	private void Update()
	{
		if (!Application.isPlaying)
			return;

		if (m_running && !m_paused)
		{
			UpdateAnimation(m_time_type == AnimationTime.GAME_TIME ? Time.deltaTime : Time.realtimeSinceStartup - m_last_time);

			if (m_time_type == AnimationTime.REAL_TIME)
				m_last_time = Time.realtimeSinceStartup;
		}
	}

	public bool UpdateAnimation(float delta_time)
	{
		delta_time *= m_animation_speed_factor;

		m_animation_timer += delta_time;

		if (m_running && UpdateMesh(true, false, 0, 0, delta_time))
		{
			m_running = false;

			// Call to the animation-complete callback if assigned
			if (m_animation_callback != null)
				m_animation_callback();

			// Execute on finish action requested
			if (Application.isPlaying)
				if (m_on_finish_action == ON_FINISH_ACTION.DESTROY_OBJECT)
					Destroy(gameObject);
				else if (m_on_finish_action == ON_FINISH_ACTION.DISABLE_OBJECT)
#if !UNITY_3_5
					gameObject.SetActive(false);
#else
					gameObject.SetActiveRecursively(false);
#endif
				else if (m_on_finish_action == ON_FINISH_ACTION.RESET_ANIMATION)
					ResetAnimation();
		}

		if (m_effect_managers.Count > 0)
			for (var idx = 0; idx < m_effect_managers.Count; idx++)
				if (m_effect_managers[idx].Update(delta_time))
				{
					// particle effect instance is complete
					// Remove from list

					m_effect_managers.RemoveAt(idx);
					idx --;
				}

		return m_running;
	}

	private bool UpdateMesh(bool use_timer, bool force_render, int action_idx, float action_progress, float delta_time = 0)
	{
		if (m_letters == null)
			return false;

		var all_letter_anims_finished = true;
		bool all_letter_anims_waiting;
		var lowest_action_progress = -1;

		if (m_mesh_combine_instance == null || m_letters.Length != m_mesh_combine_instance.Length)
			m_mesh_combine_instance = new CombineInstance[m_letters.Length];

		LetterSetup letter_setup;
		int last_letter_idx;
		int anim_action_idx;
		var letters_calculated = new bool[m_letters.Length];

		if (m_master_animations != null)
			foreach (var animation in m_master_animations)
			{
				anim_action_idx = Mathf.Clamp(action_idx, 0, animation.NumActions - 1);

				last_letter_idx = -1;

				all_letter_anims_waiting = true;

				foreach (var letter_idx in animation.m_letters_to_animate)
				{
					// two of the same letter index next to each other. Or idx out of bounds.
					if (letter_idx == last_letter_idx || letter_idx >= m_letters.Length)
						continue;

					letter_setup = m_letters[letter_idx];

					if (lowest_action_progress == -1 || letter_setup.ActionProgress < lowest_action_progress)
						lowest_action_progress = letter_setup.ActionProgress;

					if (use_timer)
					{
						var anim_state = letter_setup.AnimateMesh(force_render, m_animation_timer, m_text_anchor, m_lowest_action_progress, animation, m_animate_per, delta_time, this);

						if (anim_state == LETTER_ANIMATION_STATE.STOPPED)
							lowest_action_progress = letter_setup.ActionProgress; //++;
						if (anim_state == LETTER_ANIMATION_STATE.PLAYING || anim_state == LETTER_ANIMATION_STATE.WAITING)
							all_letter_anims_finished = false;
						if (anim_state == LETTER_ANIMATION_STATE.PLAYING || anim_state == LETTER_ANIMATION_STATE.STOPPED)
							all_letter_anims_waiting = false;
					}
					else
						letter_setup.SetMeshState(anim_action_idx, action_progress, animation, m_animate_per, this);

					m_mesh_combine_instance[letter_idx].mesh = letter_setup.m_mesh;
					letters_calculated[letter_idx] = true;

					last_letter_idx = letter_idx;
				}

				// Set animation state
				if (animation.m_letters_to_animate.Count > 0)
					animation.CurrentAnimationState = all_letter_anims_waiting && use_timer ? LETTER_ANIMATION_STATE.WAITING : LETTER_ANIMATION_STATE.PLAYING;
				else
				// No letters in this animation, so mark as STOPPED
					animation.CurrentAnimationState = LETTER_ANIMATION_STATE.STOPPED;

				if (lowest_action_progress > m_lowest_action_progress)
					m_lowest_action_progress = lowest_action_progress;
			}

		for (var letter_index = 0; letter_index < letters_calculated.Length; letter_index++)
			if (!letters_calculated[letter_index])
			{
				// this letter hasn't been included in any animations, so it's not yet been added to the mesh
				letter_setup = m_letters[letter_index];
				letter_setup.SetMeshState(-1, 0, null, AnimatePerOptions.LETTER, this);
				m_mesh_combine_instance[letter_index].mesh = letter_setup.m_mesh;
			}

		if (m_mesh == null)
			OnEnable();

		m_mesh.CombineMeshes(m_mesh_combine_instance, true, false);

		return all_letter_anims_finished;
	}

	// Lerp function that handles progress value going over 1
	public static Vector3 Vector3Lerp(Vector3 from_vec, Vector3 to_vec, float progress)
	{
		if (progress <= 1 && progress >= 0)
			return Vector3.Lerp(from_vec, to_vec, progress);
		return from_vec + Vector3.Scale((to_vec - from_vec), Vector3.one * progress);
	}

#if !UNITY_3_5
	public Font m_font;
	private int m_font_texture_width;
	private int m_font_texture_height;
#endif
#if UNITY_EDITOR
	// Editor only variables

	public EffectManager()
	{
		EditorActionProgress = 0;
		EditorActionIdx = 0;
		LineHeight = 0;
	}

	public int EditorActionIdx { get; private set; }
	public float EditorActionProgress { get; private set; }
#endif
}