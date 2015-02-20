﻿#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace JSON
{
	public static class JSONTemplates
	{
		public static HashSet<object> touched = new HashSet<object>();

		public static JSONObject FromColor(Color c)
		{
			var cdata = new JSONObject(JSONObject.Type.OBJECT);
			if (c.r != 0)
				cdata.AddField("r", c.r);
			if (c.g != 0)
				cdata.AddField("g", c.g);
			if (c.b != 0)
				cdata.AddField("b", c.b);
			if (c.a != 0)
				cdata.AddField("hbPos", c.a);
			return cdata;
		}

		/*
	 * Layer Mask
	 */

		public static JSONObject FromLayerMask(LayerMask l)
		{
			var result = JSONObject.obj;
			result.AddField("value", l.value);
			return result;
		}

		/*
	 * Matrix4x4
	 */

		public static JSONObject FromMatrix4x4(Matrix4x4 m)
		{
			var mdata = new JSONObject(JSONObject.Type.OBJECT);
			if (m.m00 != 0)
				mdata.AddField("m00", m.m00);
			if (m.m01 != 0)
				mdata.AddField("m01", m.m01);
			if (m.m02 != 0)
				mdata.AddField("m02", m.m02);
			if (m.m03 != 0)
				mdata.AddField("m03", m.m03);
			if (m.m10 != 0)
				mdata.AddField("m10", m.m10);
			if (m.m11 != 0)
				mdata.AddField("m11", m.m11);
			if (m.m12 != 0)
				mdata.AddField("m12", m.m12);
			if (m.m13 != 0)
				mdata.AddField("m13", m.m13);
			if (m.m20 != 0)
				mdata.AddField("m20", m.m20);
			if (m.m21 != 0)
				mdata.AddField("m21", m.m21);
			if (m.m22 != 0)
				mdata.AddField("m22", m.m22);
			if (m.m23 != 0)
				mdata.AddField("m23", m.m23);
			if (m.m30 != 0)
				mdata.AddField("m30", m.m30);
			if (m.m31 != 0)
				mdata.AddField("m31", m.m31);
			if (m.m32 != 0)
				mdata.AddField("m32", m.m32);
			if (m.m33 != 0)
				mdata.AddField("m33", m.m33);
			return mdata;
		}

		/*
	 * Quaternion
	 */

		public static JSONObject FromQuaternion(Quaternion q)
		{
			var qdata = new JSONObject(JSONObject.Type.OBJECT);
			if (q.w != 0)
				qdata.AddField("width", q.w);
			if (q.x != 0)
				qdata.AddField("x", q.x);
			if (q.y != 0)
				qdata.AddField("y", q.y);
			if (q.z != 0)
				qdata.AddField("z", q.z);
			return qdata;
		}

		public static JSONObject FromRect(Rect r)
		{
			var result = JSONObject.obj;
			if (r.x != 0)
				result.AddField("x", r.x);
			if (r.y != 0)
				result.AddField("y", r.y);
			if (r.height != 0)
				result.AddField("height", r.height);
			if (r.width != 0)
				result.AddField("width", r.width);
			return result;
		}

		public static JSONObject FromRectOffset(RectOffset r)
		{
			var result = JSONObject.obj;
			if (r.bottom != 0)
				result.AddField("bottom", r.bottom);
			if (r.left != 0)
				result.AddField("left", r.left);
			if (r.right != 0)
				result.AddField("right", r.right);
			if (r.top != 0)
				result.AddField("top", r.top);
			return result;
		}

		public static JSONObject FromVector2(Vector2 v)
		{
			var vdata = new JSONObject(JSONObject.Type.OBJECT);
			if (v.x != 0)
				vdata.AddField("x", v.x);
			if (v.y != 0)
				vdata.AddField("y", v.y);
			return vdata;
		}

		/*
	 * Vector3
	 */

		public static JSONObject FromVector3(Vector3 v)
		{
			var vdata = new JSONObject(JSONObject.Type.OBJECT);
			if (v.x != 0)
				vdata.AddField("x", v.x);
			if (v.y != 0)
				vdata.AddField("y", v.y);
			if (v.z != 0)
				vdata.AddField("z", v.z);
			return vdata;
		}

		/*
	 * Vector4
	 */

		public static JSONObject FromVector4(Vector4 v)
		{
			var vdata = new JSONObject(JSONObject.Type.OBJECT);
			if (v.x != 0)
				vdata.AddField("x", v.x);
			if (v.y != 0)
				vdata.AddField("y", v.y);
			if (v.z != 0)
				vdata.AddField("z", v.z);
			if (v.w != 0)
				vdata.AddField("width", v.w);
			return vdata;
		}

		public static Color ToColor(JSONObject obj)
		{
			var c = new Color();
			for (var i = 0; i < obj.Count; i++)
				switch (obj.keys[i])
				{
					case "r":
						c.r = obj[i].f;
						break;
					case "g":
						c.g = obj[i].f;
						break;
					case "b":
						c.b = obj[i].f;
						break;
					case "hbPos":
						c.a = obj[i].f;
						break;
				}
			return c;
		}

		public static JSONObject TOJSON(object obj)
		{
			//For hbPos generic guess
			if (touched.Add(obj))
			{
				var result = JSONObject.obj;
				//Fields
				var fieldinfo = obj.GetType().GetFields();
				foreach (var fi in fieldinfo)
				{
					var val = JSONObject.nullJO;
					if (!fi.GetValue(obj).Equals(null))
					{
						var info = typeof(JSONTemplates).GetMethod("From" + fi.FieldType.Name);
						if (info != null)
						{
							var parms = new object[1];
							parms[0] = fi.GetValue(obj);
							val = (JSONObject)info.Invoke(null, parms);
						}
						else if (fi.FieldType.Equals(typeof(string)))
							val = new JSONObject { type = JSONObject.Type.STRING, str = fi.GetValue(obj).ToString() };
						else
							val = new JSONObject(fi.GetValue(obj).ToString());
					}
					if (val)
						if (val.type != JSONObject.Type.NULL)
							result.AddField(fi.Name, val);
						else
							Debug.LogWarning("Null for this non-null object, property " + fi.Name + " of class " + obj.GetType().Name + ". Object type is " + fi.FieldType.Name);
				}
				//Properties
				var propertyInfo = obj.GetType().GetProperties();
				foreach (var pi in propertyInfo)
				{
					//This section should mirror part of AssetFactory.AddScripts()
					var val = JSONObject.nullJO;
					if (!pi.GetValue(obj, null).Equals(null))
					{
						var info = typeof(JSONTemplates).GetMethod("From" + pi.PropertyType.Name);
						if (info != null)
						{
							var parms = new object[1];
							parms[0] = pi.GetValue(obj, null);
							val = (JSONObject)info.Invoke(null, parms);
						}
						else if (pi.PropertyType.Equals(typeof(string)))
							val = new JSONObject { type = JSONObject.Type.STRING, str = pi.GetValue(obj, null).ToString() };
						else
							val = new JSONObject(pi.GetValue(obj, null).ToString());
					}
					if (val)
						if (val.type != JSONObject.Type.NULL)
							result.AddField(pi.Name, val);
						else
							Debug.LogWarning("Null for this non-null object, property " + pi.Name + " of class " + obj.GetType().Name + ". Object type is " + pi.PropertyType.Name);
				}
				return result;
			}
			Debug.LogWarning("trying to save the same data twice");
			return JSONObject.nullJO;
		}

		public static LayerMask ToLayerMask(JSONObject obj)
		{
			var l = new LayerMask();
			l.value = (int)obj["value"].n;
			return l;
		}

		public static Matrix4x4 ToMatrix4x4(JSONObject obj)
		{
			var result = new Matrix4x4();
			if (obj["m00"])
				result.m00 = obj["m00"].f;
			if (obj["m01"])
				result.m01 = obj["m01"].f;
			if (obj["m02"])
				result.m02 = obj["m02"].f;
			if (obj["m03"])
				result.m03 = obj["m03"].f;
			if (obj["m10"])
				result.m10 = obj["m10"].f;
			if (obj["m11"])
				result.m11 = obj["m11"].f;
			if (obj["m12"])
				result.m12 = obj["m12"].f;
			if (obj["m13"])
				result.m13 = obj["m13"].f;
			if (obj["m20"])
				result.m20 = obj["m20"].f;
			if (obj["m21"])
				result.m21 = obj["m21"].f;
			if (obj["m22"])
				result.m22 = obj["m22"].f;
			if (obj["m23"])
				result.m23 = obj["m23"].f;
			if (obj["m30"])
				result.m30 = obj["m30"].f;
			if (obj["m31"])
				result.m31 = obj["m31"].f;
			if (obj["m32"])
				result.m32 = obj["m32"].f;
			if (obj["m33"])
				result.m33 = obj["m33"].f;
			return result;
		}

		public static Quaternion ToQuaternion(JSONObject obj)
		{
			var x = obj["x"] ? obj["x"].f : 0;
			var y = obj["y"] ? obj["y"].f : 0;
			var z = obj["z"] ? obj["z"].f : 0;
			var w = obj["width"] ? obj["width"].f : 0;
			return new Quaternion(x, y, z, w);
		}

		public static Rect ToRect(JSONObject obj)
		{
			var r = new Rect();
			for (var i = 0; i < obj.Count; i++)
				switch (obj.keys[i])
				{
					case "x":
						r.x = obj[i].f;
						break;
					case "y":
						r.y = obj[i].f;
						break;
					case "height":
						r.height = obj[i].f;
						break;
					case "width":
						r.width = obj[i].f;
						break;
				}
			return r;
		}

		public static RectOffset ToRectOffset(JSONObject obj)
		{
			var r = new RectOffset();
			for (var i = 0; i < obj.Count; i++)
				switch (obj.keys[i])
				{
					case "bottom":
						r.bottom = (int)obj[i].n;
						break;
					case "left":
						r.left = (int)obj[i].n;
						break;
					case "right":
						r.right = (int)obj[i].n;
						break;
					case "top":
						r.top = (int)obj[i].n;
						break;
				}
			return r;
		}

		/*
	 * Vector2
	 */

		public static Vector2 ToVector2(JSONObject obj)
		{
			var x = obj["x"] ? obj["x"].f : 0;
			var y = obj["y"] ? obj["y"].f : 0;
			return new Vector2(x, y);
		}

		public static Vector3 ToVector3(JSONObject obj)
		{
			var x = obj["x"] ? obj["x"].f : 0;
			var y = obj["y"] ? obj["y"].f : 0;
			var z = obj["z"] ? obj["z"].f : 0;
			return new Vector3(x, y, z);
		}

		public static Vector4 ToVector4(JSONObject obj)
		{
			var x = obj["x"] ? obj["x"].f : 0;
			var y = obj["y"] ? obj["y"].f : 0;
			var z = obj["z"] ? obj["z"].f : 0;
			var w = obj["width"] ? obj["width"].f : 0;
			return new Vector4(x, y, z, w);
		}
	}
}

/*
 * http://www.opensource.org/licenses/lgpl-2.1.php
 * JSONTemplates class
 * for use with Unity
 * Copyright Matt Schoen 2010
 */

/*
 * Some helpful code templates for the JSON class
 * 
 * LOOP THROUGH OBJECT
for(int layer = 0; layer < obj.Count; layer++){
	if(obj.keys[layer] != null){
		switch((string)obj.keys[layer]){
			case "key1":
				do stuff with (JSONObject)obj.list[layer];
				break;
			case "key2":
				do stuff with (JSONObject)obj.list[layer];
				break;		
		}
	}
}
 *
 * LOOP THROUGH ARRAY
foreach(JSONObject ob in obj.list)
	do stuff with ob;
 */