using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NotesType;

public class MicPitch : MonoBehaviour
{
    // Start is called before the first frame update
    AudioSource aud;
    internal List<int> freq = new List<int>();
    internal List<int> plus;
    internal List<int> minus;
    internal GameManager4 gameManager;
    void Start()
    {
        aud = GetComponent<AudioSource>();
        Debug.Log(Microphone.devices[0]);
		aud.clip = null;
		aud.clip = Microphone.Start(Microphone.devices[0], true, (int)Mathf.Ceil(gameManager.Result.MusicData.MusicSource.length /
			gameManager.Settings.SongSpeed) - 1, 44100);
		aud.volume = 1;
        aud.Play();
    }

    // Update is called once per frame
    void Update()
    {
		//if (!Microphone.IsRecording(Microphone.devices[0]))
		//	aud.clip = Microphone.Start(Microphone.devices[0], false, (int)Mathf.Ceil(gameManager.Result.MusicData.MusicSource.length), 44100);
		//else
		//	Debug.Log("recording");
		if (GameManager4.HoldLine.Any())
		{
			Debug.Log("hold");
			return;
		}
		float[] spectrum = new float[4096];
		aud.GetSpectrumData(spectrum, 0, FFTWindow.Triangle);
		Dictionary<int, float> maxIndex = new Dictionary<int, float>();
		for (int i = 1; i < spectrum.Length - 1; i++)
		{
			var val = spectrum[i];
			if (val > spectrum[i - 1] && val > spectrum[i + 1] && val > 0.001f)
			{
				maxIndex.Add(i, val);
			}
		}
		var freq = maxIndex.OrderByDescending(m => m.Value).Take(15)
			.Select(m => (int)(m.Key * AudioSettings.outputSampleRate / 2f / spectrum.Length)).ToList();
		plus = freq.Except(this.freq).ToList();
		minus = this.freq.Except(freq).ToList();
		this.freq = freq;
		Debug.Log(string.Join(", ", plus));
		Notes plusn = plus.Aggregate(GetComponent<Notes>(), (n, p) => n == null ? gameManager.AllJudge(note => note.InRange(p) &&
			 (note.Score.NoteType == Tap || note.Score.NoteType == LongBegin), 0.2f, p) : n);
		if(plusn != null && plusn.Score.NoteType == LongBegin)
			GameManager4.HoldLine.Add(plusn.TapID, plusn.Line);
		//  LongScript Long = null;
		//  var end = minus.Select(m => gameManager.AllJudge(n => n.TapID == m && n.Score.NoteType == LongEnd
		//   && !n.Line.ParentNoteScript.enabled, 0.2f, m))
		//      .Where(n => GameManager4.HoldLine.TryGetValue(n.TapID, out Long))
		//      .DefaultIfEmpty().ToList();
		//if (end[0] != null)
		//          gameManager.JudgeMiss(Long, end[0].TapID);
	}
}