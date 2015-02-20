#region

using UnityEngine;

#endregion

public class ShootTarget : MonoBehaviour
{
	private readonly Color m_blue_colour = new Color(0, 0, 1, 1);
	private readonly Color m_green_colour = new Color(0, 1, 0, 1);
	private readonly Color m_red_colour = new Color(1, 0, 0, 1);
	private readonly Color m_tint_colour = new Color(1, 1, 1, 0.2f);
	private bool m_activated;
	private Color m_active_colour;
	private TARGET_COLOUR m_colour;
	private Mesh m_mesh;
	private Transform m_transform;

	private void OnMouseDown()
	{
		if (!m_activated)
		{
			m_mesh.colors = new[] { m_active_colour, m_active_colour, m_active_colour, m_active_colour };

			RuntimeDynamicSceneManager.TargetHit(m_transform.position, m_colour);

			m_activated = true;
		}
	}

	private void OnMouseExit()
	{
		if (!m_activated)
			m_mesh.colors = new[] { Color.clear, Color.clear, Color.clear, Color.clear };
	}

	private void OnMouseOver()
	{
		if (!m_activated)
			m_mesh.colors = new[] { m_tint_colour, m_tint_colour, m_tint_colour, m_tint_colour };
	}

	private void Reset()
	{
		m_activated = false;
		m_mesh.colors = new[] { Color.clear, Color.clear, Color.clear, Color.clear };

		SetupRandomColour();
	}

	private void SetupRandomColour()
	{
		m_colour = (TARGET_COLOUR)Random.Range(0, 3);

		switch (m_colour)
		{
			case TARGET_COLOUR.BLUE:
				m_active_colour = m_blue_colour;
				break;
			case TARGET_COLOUR.GREEN:
				m_active_colour = m_green_colour;
				break;
			case TARGET_COLOUR.RED:
				m_active_colour = m_red_colour;
				break;
		}
	}

	private void Start()
	{
		m_mesh = GetComponent<MeshFilter>().mesh;

		// Initialise mesh vertex colours to clear, to hide the mesh texture on screen
		m_mesh.colors = new[] { Color.clear, Color.clear, Color.clear, Color.clear };

		// register reset event callback
		RuntimeDynamicSceneManager.m_reset_event += Reset;

		// Cache transform component reference
		m_transform = transform;

		SetupRandomColour();
	}
}