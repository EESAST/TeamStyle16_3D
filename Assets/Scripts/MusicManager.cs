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

	private void Awake() { audio.volume = Settings.Audio.Volume.Background; }

	private IEnumerator PlayMusic(int index)
	{
		audio.clip = musicList[index % musicList.Count];
		audio.Play();
		while (audio.isPlaying)
			yield return new WaitForSeconds(Settings.DeltaTime);
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
		while (true)
			yield return StartCoroutine(PlayMusic(lastIndex++));
	}
}