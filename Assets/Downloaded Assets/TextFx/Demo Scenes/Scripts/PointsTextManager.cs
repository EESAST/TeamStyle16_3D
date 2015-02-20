#region

using System.Collections;
using UnityEngine;

#endregion

public class PointsTextManager : MonoBehaviour
{
	public EffectManager m_points_textfx;
	public float m_text_change_delay = 0.55f;
	public int Points { get; set; }

	public void AddPoints(int points) { StartCoroutine(SetPointsAnimated(Points + points)); }

	public void SetPoints(int points)
	{
		Points = points;
		m_points_textfx.SetText("Points: " + Points);
	}

	private IEnumerator SetPointsAnimated(int points)
	{
		Points = points;

		m_points_textfx.PlayAnimation();

		yield return new WaitForSeconds(m_text_change_delay);

		m_points_textfx.SetText("Points: " + Points);
	}

	private void Start() { SetPoints(0); }
}