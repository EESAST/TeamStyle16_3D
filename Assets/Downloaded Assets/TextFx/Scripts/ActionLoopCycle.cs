#region

using System;
using Boomlagoon.JSON;

#endregion

[Serializable]
public class ActionLoopCycle
{
	public bool m_delay_first_only;
	public int m_end_action_idx;
	private bool m_first_pass = true;
	public LOOP_TYPE m_loop_type = LOOP_TYPE.LOOP;
	public int m_number_of_loops;
	public int m_start_action_idx;

	public ActionLoopCycle() { }

	public ActionLoopCycle(int start, int end)
	{
		m_start_action_idx = start;
		m_end_action_idx = end;
	}

	public ActionLoopCycle(int start, int end, int num_loops, LOOP_TYPE loop_type)
	{
		m_start_action_idx = start;
		m_end_action_idx = end;
		m_number_of_loops = num_loops;
		m_loop_type = loop_type;
	}

	public bool FirstPass { get { return m_first_pass; } set { m_first_pass = value; } }
	public int SpanWidth { get { return m_end_action_idx - m_start_action_idx; } }

	public ActionLoopCycle Clone()
	{
		var action_loop = new ActionLoopCycle(m_start_action_idx, m_end_action_idx);

		action_loop.m_number_of_loops = m_number_of_loops;
		action_loop.m_loop_type = m_loop_type;
		action_loop.m_delay_first_only = m_delay_first_only;

		return action_loop;
	}

	public JSONValue ExportData()
	{
		var json_data = new JSONObject();

		json_data["m_delay_first_only"] = m_delay_first_only;
		json_data["m_end_action_idx"] = m_end_action_idx;
		json_data["m_loop_type"] = (int)m_loop_type;
		json_data["m_number_of_loops"] = m_number_of_loops;
		json_data["m_start_action_idx"] = m_start_action_idx;

		return new JSONValue(json_data);
	}

	public void ImportData(JSONObject json_data)
	{
		m_delay_first_only = json_data["m_delay_first_only"].Boolean;
		m_end_action_idx = (int)json_data["m_end_action_idx"].Number;
		m_loop_type = (LOOP_TYPE)(int)json_data["m_loop_type"].Number;
		m_number_of_loops = (int)json_data["m_number_of_loops"].Number;
		m_start_action_idx = (int)json_data["m_start_action_idx"].Number;
	}
}