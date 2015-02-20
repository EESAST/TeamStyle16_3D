#region

using System;
using Boomlagoon.JSON;

#endregion

[Serializable]
public class AxisEasingOverrideData
{
	public bool m_override_default;
	public EasingEquation m_x_ease = EasingEquation.Linear;
	public EasingEquation m_y_ease = EasingEquation.Linear;
	public EasingEquation m_z_ease = EasingEquation.Linear;

	public AxisEasingOverrideData Clone()
	{
		var axis_data = new AxisEasingOverrideData();
		axis_data.m_override_default = m_override_default;
		axis_data.m_x_ease = m_x_ease;
		axis_data.m_y_ease = m_y_ease;
		axis_data.m_z_ease = m_z_ease;
		return axis_data;
	}

	public JSONValue ExportData()
	{
		var json_data = new JSONObject();

		json_data["m_override_default"] = m_override_default;
		json_data["m_x_ease"] = (int)m_x_ease;
		json_data["m_y_ease"] = (int)m_y_ease;
		json_data["m_z_ease"] = (int)m_z_ease;

		return new JSONValue(json_data);
	}

	public void ImportData(JSONObject json_data)
	{
		m_override_default = json_data["m_override_default"].Boolean;
		m_x_ease = (EasingEquation)(int)json_data["m_x_ease"].Number;
		m_y_ease = (EasingEquation)(int)json_data["m_y_ease"].Number;
		m_z_ease = (EasingEquation)(int)json_data["m_z_ease"].Number;
	}
}