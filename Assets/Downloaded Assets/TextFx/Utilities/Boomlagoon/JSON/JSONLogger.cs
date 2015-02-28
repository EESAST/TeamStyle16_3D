#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WII || UNITY_PS3 || UNITY_XBOX360 || UNITY_FLASH
#define USE_UNITY_DEBUGGING
#endif

#region

#if USE_UNITY_DEBUGGING
using UnityEngine;

#else
using System.Diagnostics;

#endif

#endregion

namespace Boomlagoon.JSON
{
	static class JSONLogger
	{
#if USE_UNITY_DEBUGGING
		public static void Log(string str) { Debug.Log(str); }

		public static void Error(string str) { Debug.LogError(str); }
#else
		public static void Log(string str) { Debug.WriteLine(str); }

		public static void Error(string str) { Debug.WriteLine(str); }
#endif
	}
}