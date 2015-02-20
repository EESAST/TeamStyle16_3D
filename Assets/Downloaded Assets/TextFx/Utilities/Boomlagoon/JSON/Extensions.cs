#region

using System.Collections.Generic;

#endregion

namespace Boomlagoon.JSON
{
	public static class Extensions
	{
		public static T Pop<T>(this List<T> list)
		{
			var result = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return result;
		}
	}
}