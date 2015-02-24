#region

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion

public class MusicManager : MonoBehaviour
{
	private readonly List<AudioClip> musicList = new List<AudioClip>();
	private int lastIndex;
	public List<AudioClip> musics;

	private void Awake()
	{
		if (Data.MusicManager)
		{
			Destroy(gameObject);
			return;
		}
		Data.MusicManager = this;
		DontDestroyOnLoad(gameObject);
		audio.volume = Settings.Audio.Volume.Background;
	}

	private IEnumerator PlayMusic(int index)
	{
		audio.clip = musicList[index];
		audio.Play();
		while (audio.isPlaying)
			yield return null;
	}

	private IEnumerator Start()
	{
		musicList.Add(musics[0]);
		var musicCount = musics.Count;
		for (var i = 1; i < musicCount; ++i)
		{
			var index = Random.Range(1, musics.Count);
			var clip = musics[index];
			musicList.Add(clip);
			musics.Remove(clip);
		}
		yield return StartCoroutine(PlayMusic(0));
		while (true)
			yield return StartCoroutine(PlayMusic(lastIndex = (lastIndex + 1) % musicList.Count));
	}
}