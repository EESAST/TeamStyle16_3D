#region

using System;
using UnityEngine;

#endregion

[Serializable]
public class ParticleEffectInstanceManager
{
	private readonly EffectManager m_effect_manager_handle;
	private readonly bool m_follow_mesh;
	private readonly bool m_letter_flipped;
	private readonly Mesh m_letter_mesh;
	private readonly ParticleEmitter m_particle_emitter;
	private readonly ParticleSystem m_particle_system;
	private readonly Vector3 m_position_offset;
	private readonly bool m_rotate_with_letter = true;
	private readonly Quaternion m_rotation_offset;
	private readonly Transform m_transform;
	private bool m_active;
	private float m_delay;
	private float m_duration;
	private Quaternion rotation;

	public ParticleEffectInstanceManager(EffectManager effect_manager, Mesh character_mesh, bool letter_flipped, ParticleEffectSetup effect_setup, AnimationProgressionVariables progression_vars, AnimatePerOptions animate_per, ParticleEmitter particle_emitter = null, ParticleSystem particle_system = null)
	{
		m_particle_emitter = particle_emitter;
		m_particle_system = particle_system;
		m_letter_mesh = character_mesh;
		m_letter_flipped = letter_flipped;
		m_follow_mesh = effect_setup.m_follow_mesh;
		m_duration = effect_setup.m_duration.GetValue(progression_vars, animate_per);
		m_delay = effect_setup.m_delay.GetValue(progression_vars, animate_per);
		m_position_offset = effect_setup.m_position_offset.GetValue(progression_vars, animate_per);
		m_rotation_offset = Quaternion.Euler(effect_setup.m_rotation_offset.GetValue(progression_vars, animate_per));
		m_rotate_with_letter = effect_setup.m_rotate_relative_to_letter;
		m_effect_manager_handle = effect_manager;
		m_active = false;

		if (m_particle_emitter != null)
		{
			m_transform = m_particle_emitter.transform;

			m_particle_emitter.emit = true;
			m_particle_emitter.enabled = false;
		}
		else if (m_particle_system != null)
		{
			m_transform = m_particle_system.transform;

			m_particle_system.playOnAwake = false;
			m_particle_system.Play();
#if !UNITY_3_5 && UNITY_EDITOR
			p_system_timer = 0;
#endif
		}
	}

	private void OrientateEffectToMesh()
	{
		// Position effect relative to letter mesh, according to offset and rotation settings
		m_letter_mesh.RecalculateNormals();
		if (!m_letter_mesh.normals[0].Equals(Vector3.zero))
		{
			rotation = m_rotate_with_letter ? Quaternion.LookRotation(m_letter_mesh.normals[0] * -1, m_letter_flipped ? m_letter_mesh.vertices[0] - m_letter_mesh.vertices[1] : m_letter_mesh.vertices[1] - m_letter_mesh.vertices[2]) : Quaternion.identity;

			m_transform.position = m_effect_manager_handle.Position + (m_effect_manager_handle.Rotation * Vector3.Scale((rotation * m_position_offset) + (m_letter_mesh.vertices[0] + m_letter_mesh.vertices[1] + m_letter_mesh.vertices[2] + m_letter_mesh.vertices[3]) / 4, m_effect_manager_handle.Scale));

			rotation *= m_rotation_offset;

			m_transform.rotation = rotation;
		}
		else
			m_transform.position = m_effect_manager_handle.m_transform.position + m_position_offset + (m_letter_mesh.vertices[0] + m_letter_mesh.vertices[1] + m_letter_mesh.vertices[2] + m_letter_mesh.vertices[3]) / 4;
	}

	public void Pause(bool state)
	{
		// Pause/unpause particle effects 
		if (m_particle_emitter != null)
		{
			if (state && m_particle_emitter.enabled)
				m_particle_emitter.enabled = false;
			else if (!state && !m_particle_emitter.enabled)
				m_particle_emitter.enabled = true;
		}
		else if (m_particle_system != null)
			if (state && !m_particle_system.isPaused)
				m_particle_system.Pause(true);
			else if (!state && m_particle_system.isPaused)
				m_particle_system.Play(true);
	}

	public void Stop(bool force_stop)
	{
		if (m_particle_emitter != null)
		{
			m_particle_emitter.emit = false;

			if (force_stop)
				m_particle_emitter.ClearParticles();
		}
		else if (m_particle_system != null)
		{
			m_particle_system.Stop(true);

			if (force_stop)
				m_particle_system.Clear(true);
		}
	}

	// Updates particle effect. Returns true when effect is completely finished and ready to be reused.
	public bool Update(float delta_time)
	{
		if (!m_active)
		{
			if (m_delay > 0)
			{
				m_delay -= delta_time;
				if (m_delay < 0)
					m_delay = 0;

				return false;
			}

			m_active = true;
#if !UNITY_3_5 && UNITY_EDITOR
			m_stopped_effect_particle_count = -1;
#endif

			// Position effect to current mesh position/orientation
			OrientateEffectToMesh();


			if (m_particle_emitter != null)
			{
				m_particle_emitter.emit = false;
				m_particle_emitter.enabled = true;

				if (m_duration > 0)
					m_particle_emitter.emit = true;
				else
					m_particle_emitter.Emit();
			}
			else
			{
				if (m_duration <= 0)
				{
					m_duration = m_particle_system.duration + m_particle_system.startLifetime;
					m_particle_system.loop = false;
				}

				m_particle_system.Play(true);
			}
		}

		if (m_follow_mesh)
			OrientateEffectToMesh();

		m_duration -= delta_time;

		if (m_duration > 0)
		{
#if !UNITY_3_5 && UNITY_EDITOR
			// Handle manually calling to simulate the ParticleSystem effect in the editor.
			if (!Application.isPlaying && m_particle_system != null)
			{
				p_system_timer += delta_time;

				m_particle_system.Simulate(p_system_timer, true, true);
			}
#endif

			return false;
		}

		if (m_particle_emitter != null)
		{
			m_particle_emitter.emit = false;

			if (m_particle_emitter.particleCount > 0)
				return false;
		}
		else if (m_particle_system != null)
		{
			if (Application.isPlaying)
				m_particle_system.Stop(true);
#if !UNITY_3_5 && UNITY_EDITOR
			// Handle manually calling to simulate the ParticleSystem effect in the editor until all particles dead.
			else
			{
				p_system_timer += delta_time;

				m_particle_system.Simulate(p_system_timer, true, true);

				if (m_stopped_effect_particle_count == -1)
					// Initialise particle array to current particle num
					m_particles_array = new ParticleSystem.Particle[m_particle_system.particleCount];

				m_particle_system.GetParticles(m_particles_array);

				if (m_stopped_effect_particle_count != -1)
				{
					old_num_particles = m_stopped_effect_particle_count;

					m_stopped_effect_particle_count = 0;
					for (var idx = 0; idx < old_num_particles; idx++)
						if (m_particles_array[idx].lifetime > 0.05f)
						{
							temp_array[m_stopped_effect_particle_count] = m_particles_array[idx];
							m_stopped_effect_particle_count++;
						}

//					 Remove any extra particles created by Simulate
					m_particle_system.SetParticles(temp_array, m_stopped_effect_particle_count);
				}
				else
				{
					m_stopped_effect_particle_count = m_particle_system.particleCount;
					temp_array = new ParticleSystem.Particle[m_stopped_effect_particle_count];
				}
			}
#endif

			if (m_particle_system.particleCount > 0)
				return false;
		}

		return true;
	}

#if !UNITY_3_5 && UNITY_EDITOR
	private int m_stopped_effect_particle_count, old_num_particles;
	private float p_system_timer;
	private ParticleSystem.Particle[] m_particles_array;
	private ParticleSystem.Particle[] temp_array;
#endif
}