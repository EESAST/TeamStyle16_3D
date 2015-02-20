#define PARSE_ESCAPED_UNICODE

#region

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
#if PARSE_ESCAPED_UNICODE
using System.Text.RegularExpressions;

#endif

#endregion

namespace Boomlagoon.JSON
{
	public class JSONObject : IEnumerable<KeyValuePair<string, JSONValue>>
	{
		private readonly IDictionary<string, JSONValue> values = new Dictionary<string, JSONValue>();

		public JSONObject() { }

		/// <summary>
		///     Construct a copy of the given JSONObject.
		/// </summary>
		/// <param name="other"></param>
		public JSONObject(JSONObject other)
		{
			values = new Dictionary<string, JSONValue>();

			if (other != null)
				foreach (var keyValuePair in other.values)
					values[keyValuePair.Key] = new JSONValue(keyValuePair.Value);
		}

		public JSONValue this[string key] { get { return GetValue(key); } set { values[key] = value; } }

		IEnumerator IEnumerable.GetEnumerator() { return values.GetEnumerator(); }

		public IEnumerator<KeyValuePair<string, JSONValue>> GetEnumerator() { return values.GetEnumerator(); }

		public void Add(string key, JSONValue value) { values[key] = value; }

		public void Add(KeyValuePair<string, JSONValue> pair) { values[pair.Key] = pair.Value; }

		/// <summary>
		///     Empty this JSONObject of all values.
		/// </summary>
		public void Clear() { values.Clear(); }

		/// <param name="key"></param>
		/// <returns>Does 'key' exist in this object.</returns>
		public bool ContainsKey(string key) { return values.ContainsKey(key); }

		private static JSONObject Fail(char expected, int position, bool force_hide_errors = false) { return Fail(new string(expected, 1), position, force_hide_errors); }

		private static JSONObject Fail(string expected, int position, bool force_hide_errors = false)
		{
			if (!force_hide_errors)
				JSONLogger.Error("Invalid json string, expecting " + expected + " at " + position);
			return null;
		}

		public JSONArray GetArray(string key)
		{
			var value = GetValue(key);
			if (value == null)
			{
				JSONLogger.Error(key + " == null");
				return null;
			}
			return value.Array;
		}

		public bool GetBoolean(string key)
		{
			var value = GetValue(key);
			if (value == null)
			{
				JSONLogger.Error(key + " == null");
				return false;
			}
			return value.Boolean;
		}

		public double GetNumber(string key)
		{
			var value = GetValue(key);
			if (value == null)
			{
				JSONLogger.Error(key + " == null");
				return double.NaN;
			}
			return value.Number;
		}

		public JSONObject GetObject(string key)
		{
			var value = GetValue(key);
			if (value == null)
			{
				JSONLogger.Error(key + " == null");
				return null;
			}
			return value.Obj;
		}

		public string GetString(string key)
		{
			var value = GetValue(key);
			if (value == null)
			{
				JSONLogger.Error(key + "(string) == null");
				return string.Empty;
			}
			return value.Str;
		}

		public JSONValue GetValue(string key)
		{
			JSONValue value;
			values.TryGetValue(key, out value);
			return value;
		}

		/// <summary>
		///     Attempt to parse a string into a JSONObject.
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns>A new JSONObject or null if parsing fails.</returns>
		public static JSONObject Parse(string jsonString, bool force_hide_errors = false)
		{
			if (string.IsNullOrEmpty(jsonString))
				return null;

			JSONValue currentValue = null;

			var keyList = new List<string>();

			var state = JSONParsingState.Object;

			for (var startPosition = 0; startPosition < jsonString.Length; ++startPosition)
			{
				startPosition = SkipWhitespace(jsonString, startPosition);

				switch (state)
				{
					case JSONParsingState.Object:
						if (jsonString[startPosition] != '{')
							return Fail('{', startPosition, force_hide_errors);

						JSONValue newObj = new JSONObject();
						if (currentValue != null)
							newObj.Parent = currentValue;
						currentValue = newObj;

						state = JSONParsingState.Key;
						break;

					case JSONParsingState.EndObject:
						if (jsonString[startPosition] != '}')
							return Fail('}', startPosition, force_hide_errors);

						if (currentValue.Parent == null)
							return currentValue.Obj;

						switch (currentValue.Parent.Type)
						{
							case JSONValueType.Object:
								currentValue.Parent.Obj.values[keyList.Pop()] = new JSONValue(currentValue.Obj);
								break;

							case JSONValueType.Array:
								currentValue.Parent.Array.Add(new JSONValue(currentValue.Obj));
								break;

							default:
								return Fail("valid object", startPosition, force_hide_errors);
						}
						currentValue = currentValue.Parent;

						state = JSONParsingState.ValueSeparator;
						break;

					case JSONParsingState.Key:
						if (jsonString[startPosition] == '}')
						{
							--startPosition;
							state = JSONParsingState.EndObject;
							break;
						}

						var key = ParseString(jsonString, ref startPosition, force_hide_errors);
						if (key == null)
							return Fail("key string", startPosition, force_hide_errors);
						keyList.Add(key);
						state = JSONParsingState.KeyValueSeparator;
						break;

					case JSONParsingState.KeyValueSeparator:
						if (jsonString[startPosition] != ':')
							return Fail(':', startPosition, force_hide_errors);
						state = JSONParsingState.Value;
						break;

					case JSONParsingState.ValueSeparator:
						switch (jsonString[startPosition])
						{
							case ',':
								state = currentValue.Type == JSONValueType.Object ? JSONParsingState.Key : JSONParsingState.Value;
								break;

							case '}':
								state = JSONParsingState.EndObject;
								--startPosition;
								break;

							case ']':
								state = JSONParsingState.EndArray;
								--startPosition;
								break;

							default:
								return Fail(", } ]", startPosition, force_hide_errors);
						}
						break;

					case JSONParsingState.Value:
						{
							var c = jsonString[startPosition];
							if (c == '"')
								state = JSONParsingState.String;
							else if (char.IsDigit(c) || c == '-')
								state = JSONParsingState.Number;
							else
								switch (c)
								{
									case '{':
										state = JSONParsingState.Object;
										break;

									case '[':
										state = JSONParsingState.Array;
										break;

									case ']':
										if (currentValue.Type == JSONValueType.Array)
											state = JSONParsingState.EndArray;
										else
											return Fail("valid array", startPosition, force_hide_errors);
										break;

									case 'f':
									case 't':
										state = JSONParsingState.Boolean;
										break;


									case 'n':
										state = JSONParsingState.Null;
										break;

									default:
										return Fail("beginning of value", startPosition, force_hide_errors);
								}

							--startPosition; //To re-evaluate this char in the newly selected state
							break;
						}

					case JSONParsingState.String:
						var str = ParseString(jsonString, ref startPosition, force_hide_errors);
						if (str == null)
							return Fail("string value", startPosition, force_hide_errors);

						switch (currentValue.Type)
						{
							case JSONValueType.Object:
								currentValue.Obj.values[keyList.Pop()] = new JSONValue(str);
								break;

							case JSONValueType.Array:
								currentValue.Array.Add(str);
								break;

							default:
								if (!force_hide_errors)
									JSONLogger.Error("Fatal error, current JSON value not valid");
								return null;
						}

						state = JSONParsingState.ValueSeparator;
						break;

					case JSONParsingState.Number:
						var number = ParseNumber(jsonString, ref startPosition);
						if (double.IsNaN(number))
							return Fail("valid number", startPosition, force_hide_errors);

						switch (currentValue.Type)
						{
							case JSONValueType.Object:
								currentValue.Obj.values[keyList.Pop()] = new JSONValue(number);
								break;

							case JSONValueType.Array:
								currentValue.Array.Add(number);
								break;

							default:
								if (!force_hide_errors)
									JSONLogger.Error("Fatal error, current JSON value not valid");
								return null;
						}

						state = JSONParsingState.ValueSeparator;

						break;

					case JSONParsingState.Boolean:
						if (jsonString[startPosition] == 't')
						{
							if (jsonString.Length < startPosition + 4 || jsonString[startPosition + 1] != 'r' || jsonString[startPosition + 2] != 'u' || jsonString[startPosition + 3] != 'e')
								return Fail("true", startPosition, force_hide_errors);

							switch (currentValue.Type)
							{
								case JSONValueType.Object:
									currentValue.Obj.values[keyList.Pop()] = new JSONValue(true);
									break;

								case JSONValueType.Array:
									currentValue.Array.Add(new JSONValue(true));
									break;

								default:
									if (!force_hide_errors)
										JSONLogger.Error("Fatal error, current JSON value not valid");
									return null;
							}

							startPosition += 3;
						}
						else
						{
							if (jsonString.Length < startPosition + 5 || jsonString[startPosition + 1] != 'a' || jsonString[startPosition + 2] != 'l' || jsonString[startPosition + 3] != 's' || jsonString[startPosition + 4] != 'e')
								return Fail("false", startPosition, force_hide_errors);

							switch (currentValue.Type)
							{
								case JSONValueType.Object:
									currentValue.Obj.values[keyList.Pop()] = new JSONValue(false);
									break;

								case JSONValueType.Array:
									currentValue.Array.Add(new JSONValue(false));
									break;

								default:
									if (!force_hide_errors)
										JSONLogger.Error("Fatal error, current JSON value not valid");
									return null;
							}

							startPosition += 4;
						}

						state = JSONParsingState.ValueSeparator;
						break;

					case JSONParsingState.Array:
						if (jsonString[startPosition] != '[')
							return Fail('[', startPosition, force_hide_errors);

						JSONValue newArray = new JSONArray();
						if (currentValue != null)
							newArray.Parent = currentValue;
						currentValue = newArray;

						state = JSONParsingState.Value;
						break;

					case JSONParsingState.EndArray:
						if (jsonString[startPosition] != ']')
							return Fail(']', startPosition, force_hide_errors);

						if (currentValue.Parent == null)
							return currentValue.Obj;

						switch (currentValue.Parent.Type)
						{
							case JSONValueType.Object:
								currentValue.Parent.Obj.values[keyList.Pop()] = new JSONValue(currentValue.Array);
								break;

							case JSONValueType.Array:
								currentValue.Parent.Array.Add(new JSONValue(currentValue.Array));
								break;

							default:
								return Fail("valid object", startPosition, force_hide_errors);
						}
						currentValue = currentValue.Parent;

						state = JSONParsingState.ValueSeparator;
						break;

					case JSONParsingState.Null:
						if (jsonString[startPosition] == 'n')
						{
							if (jsonString.Length < startPosition + 4 || jsonString[startPosition + 1] != 'u' || jsonString[startPosition + 2] != 'l' || jsonString[startPosition + 3] != 'l')
								return Fail("null", startPosition, force_hide_errors);

							switch (currentValue.Type)
							{
								case JSONValueType.Object:
									currentValue.Obj.values[keyList.Pop()] = new JSONValue(JSONValueType.Null);
									break;

								case JSONValueType.Array:
									currentValue.Array.Add(new JSONValue(JSONValueType.Null));
									break;

								default:
									if (!force_hide_errors)
										JSONLogger.Error("Fatal error, current JSON value not valid");
									return null;
							}

							startPosition += 3;
						}
						state = JSONParsingState.ValueSeparator;
						break;
				}
			}
			if (!force_hide_errors)
				JSONLogger.Error("Unexpected end of string");
			return null;
		}

		private static double ParseNumber(string str, ref int startPosition)
		{
			if (startPosition >= str.Length || (!char.IsDigit(str[startPosition]) && str[startPosition] != '-'))
				return double.NaN;

			var endPosition = startPosition + 1;

			for (; endPosition < str.Length && str[endPosition] != ',' && str[endPosition] != ']' && str[endPosition] != '}'; ++endPosition)
				;

			double result;
			if (!double.TryParse(str.Substring(startPosition, endPosition - startPosition), NumberStyles.Float, CultureInfo.InvariantCulture, out result))
				return double.NaN;
			startPosition = endPosition - 1;
			return result;
		}

		private static string ParseString(string str, ref int startPosition, bool force_hide_errors = false)
		{
			if (str[startPosition] != '"' || startPosition + 1 >= str.Length)
			{
				Fail('"', startPosition, force_hide_errors);
				return null;
			}

			var endPosition = str.IndexOf('"', startPosition + 1);
			if (endPosition <= startPosition)
			{
				Fail('"', startPosition + 1, force_hide_errors);
				return null;
			}

			while (str[endPosition - 1] == '\\')
			{
				endPosition = str.IndexOf('"', endPosition + 1);
				if (endPosition <= startPosition)
				{
					Fail('"', startPosition + 1, force_hide_errors);
					return null;
				}
			}

			var result = string.Empty;

			if (endPosition > startPosition + 1)
				result = str.Substring(startPosition + 1, endPosition - startPosition - 1);

			startPosition = endPosition;

#if PARSE_ESCAPED_UNICODE
			// Parse Unicode characters that are escaped as \uXXXX
			do
			{
				var m = unicodeRegex.Match(result);
				if (!m.Success)
					break;

				var s = m.Groups[1].Captures[0].Value;
				unicodeBytes[1] = byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
				unicodeBytes[0] = byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
				s = Encoding.Unicode.GetString(unicodeBytes);

				result = result.Replace(m.Value, s);
			}
			while (true);
#endif

			return result;
		}

		/// <summary>
		///     Remove the JSONValue attached to the given key.
		/// </summary>
		/// <param name="key"></param>
		public void Remove(string key)
		{
			if (values.ContainsKey(key))
				values.Remove(key);
		}

		private static int SkipWhitespace(string str, int pos)
		{
			for (; pos < str.Length && char.IsWhiteSpace(str[pos]); ++pos)
				;
			return pos;
		}

		/// <returns>String representation of this JSONObject</returns>
		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append('{');

			foreach (var pair in values)
			{
				stringBuilder.Append("\"" + pair.Key + "\"");
				stringBuilder.Append(':');
				stringBuilder.Append(pair.Value);
				stringBuilder.Append(',');
			}
			if (values.Count > 0)
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		private enum JSONParsingState
		{
			Object,
			Array,
			EndObject,
			EndArray,
			Key,
			Value,
			KeyValueSeparator,
			ValueSeparator,
			String,
			Number,
			Boolean,
			Null
		}

#if PARSE_ESCAPED_UNICODE
		private static readonly Regex unicodeRegex = new Regex(@"\\u([0-9a-fA-F]{4})");
		private static readonly byte[] unicodeBytes = new byte[2];
#endif
	}
}