#region

using System.Collections;

#endregion

public abstract class Unit : UnitBase
{
	public IEnumerator Move(JSONObject nodes)
	{
		transform.position = Methods.Coordinates.JSONToInternal(nodes.list[nodes.Count - 1]); //TODO:animate movement according to nodes
		--Data.Game.MovesLeft;
		yield break;
	}
}