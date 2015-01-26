#region

using GameStatics;
using UnityEngine;

#endregion

public class TeamColorManager : MonoBehaviour
{
	private void Awake() { Data.TeamColor.Desired = new[] { Color.magenta, Color.cyan, Color.white }; }

	public void Randomize()
	{
		for (var i = 0; i < 3; i++)
			Data.TeamColor.Desired[i] = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
	}
}