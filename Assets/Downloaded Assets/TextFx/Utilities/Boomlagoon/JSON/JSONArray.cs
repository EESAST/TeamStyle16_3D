#region

using System.Collections;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Boomlagoon.JSON
{
	public class JSONArray : IEnumerable<JSONValue>
	{
		private readonly List<JSONValue> values = new List<JSONValue>();

		public JSONArray() { }

		/// <summary>
		///     Construct a new array and copy each value from the given array into the new one
		/// </summary>
		/// <param name="array"></param>
		public JSONArray(JSONArray array)
		{
			values = new List<JSONValue>();
			foreach (var v in array.values)
				values.Add(new JSONValue(v));
		}

		public JSONValue this[int index] { get { return values[index]; } set { values[index] = value; } }

		/// <returns>
		///     Return the length of the array
		/// </returns>
		public int Length { get { return values.Count; } }

		IEnumerator IEnumerable.GetEnumerator() { return values.GetEnumerator(); }

		public IEnumerator<JSONValue> GetEnumerator() { return values.GetEnumerator(); }

		/// <summary>
		///     Add a JSONValue to this array
		/// </summary>
		/// <param name="value"></param>
		public void Add(JSONValue value) { values.Add(value); }

		/// <summary>
		///     Empty the array of all values.
		/// </summary>
		public void Clear() { values.Clear(); }

		/// <summary>
		///     Concatenate two JSONArrays
		/// </summary>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <returns>
		///     A new JSONArray that is the result of adding all of the right-hand side array's values to the left-hand side
		///     array.
		/// </returns>
		public static JSONArray operator +(JSONArray lhs, JSONArray rhs)
		{
			var result = new JSONArray(lhs);
			foreach (var value in rhs.values)
				result.Add(value);
			return result;
		}

		/// <summary>
		///     Attempt to parse a string as a JSON array.
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns>A new JSONArray object if successful, null otherwise.</returns>
		public static JSONArray Parse(string jsonString)
		{
			var tempObject = JSONObject.Parse("{ \"array\" :" + jsonString + '}');
			return tempObject == null ? null : tempObject.GetValue("array").Array;
		}

		/// <summary>
		///     Remove the value at the given index, if it exists.
		/// </summary>
		/// <param name="index"></param>
		public void Remove(int index)
		{
			if (index >= 0 && index < values.Count)
				values.RemoveAt(index);
			else
				JSONLogger.Error("index out of range: " + index + " (Expected 0 <= index < " + values.Count + ")");
		}

		/// <returns>String representation of this JSONArray</returns>
		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append('[');
			foreach (var value in values)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(',');
			}
			if (values.Count > 0)
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			stringBuilder.Append(']');
			return stringBuilder.ToString();
		}
	}
}