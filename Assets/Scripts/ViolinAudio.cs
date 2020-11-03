using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViolinAudio : MonoBehaviour, IPausable
{
    public float PlayTime { get; set; }
    float BeatTime;
    AudioSource audioSource;
    bool Playing = false;
	public Violin violin { get; set; }
	public int channel { get; set; }

    // Update is called once per frame
    void Update()
    {
        if (Playing) BeatTime += Time.deltaTime;
        else audioSource.volume -= Time.deltaTime * 2;
		if (audioSource.volume <= 0)
            Destroy(gameObject);
        if (BeatTime >= PlayTime) Stop();
    }

    public void Play(int pitch, float playtime)
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.time = pitch < audioSource.clip.length ? pitch : 0;
        BeatTime = 0;
        PlayTime = playtime;
        Playing = true;
	}
    public void Stop()
    {
		if (Playing)
		{
            Debug.Log("Stop");
            violin.ViolinAudios.Remove(this);
            Playing = false;
        }
    }

    public void Pause()
	{
        if (Playing)
        {
            audioSource.Pause();
        }
    }

    public void UnPause()
	{
        audioSource.UnPause();
    }
}
