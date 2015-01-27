#region

using System.IO;
using UnityEngine;

#endregion

public class LoadBattle : MonoBehaviour
{
	private JSONObject prev_info;

	private void Start()
	{
		prev_info = new JSONObject(File.OpenText("Assets\\Files\\test.battle").ReadToEnd().Replace("\"{", "{").Replace("}\"", "}").Replace("\\\"", "\""));

		//gamebody
		Debug.Log(prev_info);
		Debug.Log(prev_info["gamebody"]);

		Debug.Log(prev_info["gamebody"]["commands"]);
		Debug.Log(prev_info["gamebody"]["map_info"]);
		Debug.Log(prev_info["gamebody"]["populations"]);
		Debug.Log(prev_info["gamebody"]["production_lists"]);
		Debug.Log(prev_info["gamebody"]["round"]);
		Debug.Log(prev_info["gamebody"]["scores"]);

		//commands（与history的command相似）
		Debug.Log(prev_info["gamebody"]["commands"][0][0]["operand"]);
		Debug.Log(prev_info["gamebody"]["commands"][0][0]["pos"]["x"]);
		Debug.Log(prev_info["gamebody"]["commands"][0][0]["pos"]["y"]);
		Debug.Log(prev_info["gamebody"]["commands"][0][0]["pos"]["z"]);

		//map_info
		Debug.Log(prev_info["gamebody"]["map_info"]["elements"]); //与prev_info["key_frames"][0][0]相似，用循环+判断来获取元素
		Debug.Log(prev_info["gamebody"]["map_info"]["max_population"]);
		Debug.Log(prev_info["gamebody"]["map_info"]["max_round"]);
		Debug.Log(prev_info["gamebody"]["map_info"]["record_interval"]);
		Debug.Log(prev_info["gamebody"]["map_info"]["time_per_round"]);
		Debug.Log(prev_info["gamebody"]["map_info"]["types"]);
		Debug.Log(prev_info["gamebody"]["map_info"]["weather"]);
		Debug.Log(prev_info["gamebody"]["map_info"]["x_max"]);
		Debug.Log(prev_info["gamebody"]["map_info"]["y_max"]);

		//gamebody结束

		//history

		Debug.Log(prev_info["history"]["command"]);
		Debug.Log(prev_info["history"]["event"]);
		Debug.Log(prev_info["history"]["population"]);
		Debug.Log(prev_info["history"]["score"]);
		Debug.Log(prev_info["history"]["unit_num"]);

		//history中的维数
		Debug.Log(prev_info["history"]["command"][0][0][0]);
		Debug.Log(prev_info["history"]["event"][0][0]);
		Debug.Log(prev_info["history"]["population"][0][0]);
		Debug.Log(prev_info["history"]["score"][0][0]);
		Debug.Log(prev_info["history"]["unit_num"][0][0]);

		//command中的内容
		Debug.Log(prev_info["history"]["command"][0][0][0]["operand"]);
		Debug.Log(prev_info["history"]["command"][0][0][0]["pos"]["x"]);
		Debug.Log(prev_info["history"]["command"][0][0][0]["pos"]["y"]);
		Debug.Log(prev_info["history"]["command"][0][0][0]["pos"]["z"]);

		//event中的内容
		Debug.Log(prev_info["history"]["event"][0][0]["index"]);
		Debug.Log(prev_info["history"]["event"][0][0]["target_pos"]["x"]);
		Debug.Log(prev_info["history"]["event"][0][0]["target_pos"]["y"]);
		Debug.Log(prev_info["history"]["event"][0][0]["target_pos"]["z"]);

		//history结束

		//关键帧key_frames[]

		Debug.Log("[\"key_frames\"][0]:\n" + prev_info["key_frames"][0]);

		//在[0][0]帧中的第0个对象 理论上可行，但是实际上有引号，无法解析
		//JSONObject key_frames = prev_info ["key_frames"][0][0][0];
		//prev_info ["key_frames"][0][0].Count 的值为 -1

		//故稍作修改

		//		string pos = prev_info["key_frames"][0][0].ToString();
		//		string b = pos.Substring (1, pos.Length - 2);//去掉引号
		//		JSONObject key_frames = new JSONObject (b);//key_frames = prev_info["key_frames"][0][0]

		for (var i = 0; i < prev_info["key_frames"][0][0].Count; i++)
		{
			//JSONObject o = key_frames[layer];
			var o = prev_info["key_frames"][0][0][i];
			Debug.Log(o);
			switch (o["__class__"].ToString())
			{
				case "\"Fort\"":
				case "\"Base\"":
					Debug.Log("类型：Fort或Base");
					Debug.Log("index:" + o["index"]);
					Debug.Log("ammo:" + o["ammo"]);
					Debug.Log("ammo_max:" + o["ammo_max"]);
					Debug.Log("ammo_once:" + o["ammo_once"]);
					Debug.Log("attacks:" + o["attacks"]);
					Debug.Log("build_round:" + o["build_round"]);
					Debug.Log("cost:" + o["cost"]);
					Debug.Log("defences:" + o["defences"]);
					Debug.Log("fire_ranges:" + o["fire_ranges"]);
					Debug.Log("fuel:" + o["fuel"]);
					Debug.Log("fuel_max:" + o["fuel_max"]);
					Debug.Log("health:" + o["health"]);
					Debug.Log("health_max:" + o["health_max"]);
					Debug.Log("metal:" + o["metal"]);
					Debug.Log("metal_max:" + o["metal_max"]);
					Debug.Log("population:" + o["population"]);
					Debug.Log("jsonPos:" + o["pos"]);
					Debug.Log("sight_ranges:" + o["sight_ranges"]);
					Debug.Log("speed:" + o["speed"]);
					Debug.Log("team:" + o["team"]);
					break;
				case "\"Oilfield\"":
					Debug.Log("类型：Oilfield");
					Debug.Log("index:" + o["index"]);
					Debug.Log("fuel:" + o["fuel"]);
					Debug.Log("jsonPos:" + o["pos"]);
					break;
				case "\"Mine\"":
					Debug.Log("类型：Mine");
					Debug.Log("index:" + o["index"]);
					Debug.Log("metal:" + o["metal"]);
					Debug.Log("jsonPos:" + o["pos"]);
					break;
			}
		}

		//keyframes结束
	}
}