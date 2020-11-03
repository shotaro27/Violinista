using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Violin : MonoBehaviour, IPausable
{
	[SerializeField]
	private GameObject ViolinAudioPrefab;
	[SerializeField]
	private GameManager4 gameManager;
	internal List<ViolinAudio> ViolinAudios = new List<ViolinAudio>();
	internal float Volume { get; set; }
	(int pitch, float playTime, int channel) plays;
	Dictionary<int, float> pTime = new Dictionary<int, float>();

	private void Start()
	{
		plays.playTime = -1;
	}

	public void Play(int pitch, float playTime, int channel)
	{
		if (gameManager.GameMode == GameMode.ViolinGame) return;
		var ViolinAudioObj = Instantiate(ViolinAudioPrefab);
		ViolinAudioObj.GetComponent<AudioSource>().volume = Volume / 2;
		var ViolinAudio = ViolinAudioObj.GetComponent<ViolinAudio>();
		ViolinAudio.channel = channel;
		ViolinAudio.violin = this;
		ViolinAudios.Add(ViolinAudio);
		ViolinAudio.Play((pitch - 43) * 5, Mathf.Min(playTime, 4));
		if (playTime > 4)
		{
			plays = (pitch, playTime - 4, channel);
			pTime.Add(channel, 4);
		}
		else pTime.Remove(channel);
	}

	public void Stop(int channel)
	{
		if (gameManager.GameMode == GameMode.ViolinGame) return;
		Debug.Log(channel);
		if (!ViolinAudios.Any(v => v.channel == channel)) return;
		var ViolinAudio = ViolinAudios.FirstOrDefault(v => v.channel == channel);
		ViolinAudio.Stop();
		pTime.Remove(channel);
		ViolinAudios.Remove(ViolinAudio);
	}
	public void Pause() => ViolinAudios.ForEach(v => v.Pause());
	public void UnPause() => ViolinAudios.ForEach(v => v.UnPause());
	private void Update()
	{
		var pts = new List<int>(pTime.Keys);
		foreach (var i in pts)
		{
			if (pTime[i] > 0)
			{
				pTime[i] -= Time.deltaTime;
			}
			else
			{
				Debug.Log("pppp");
				Play(plays.pitch, plays.playTime, i);
			}
		}
	}
}