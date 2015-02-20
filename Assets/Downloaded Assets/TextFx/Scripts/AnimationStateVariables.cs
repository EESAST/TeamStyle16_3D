#region

using System;
using System.Collections.Generic;

#endregion

[Serializable]
public class AnimationStateVariables
{
	public int m_action_index;
	public int m_action_index_progress; // Used to track progress through a loop cycle
	public float m_action_progress;
	public bool m_active;
	public List<ActionLoopCycle> m_active_loop_cycles;
	public float m_break_delay;
	public float m_linear_progress;
	public int m_prev_action_index;
	public bool m_reverse;
	public bool m_started_action; // triggered when action starts (after initial delay)
	public float m_timer_offset;
	public bool m_waiting_to_sync;

	public AnimationStateVariables Clone() { return new AnimationStateVariables { m_active = m_active, m_waiting_to_sync = m_waiting_to_sync, m_started_action = m_started_action, m_break_delay = m_break_delay, m_timer_offset = m_timer_offset, m_action_index = m_action_index, m_reverse = m_reverse, m_action_index_progress = m_action_index_progress, m_prev_action_index = m_prev_action_index, m_linear_progress = m_linear_progress, m_action_progress = m_action_progress, m_active_loop_cycles = m_active_loop_cycles }; }

	public void Reset()
	{
		m_active = false;
		m_waiting_to_sync = false;
		m_started_action = false;
		m_break_delay = 0;
		m_timer_offset = 0;
		m_action_index = 0;
		m_reverse = false;
		m_action_index_progress = 0;
		m_prev_action_index = -1;
		m_linear_progress = 0;
		m_action_progress = 0;
		m_active_loop_cycles.Clear();
	}
}