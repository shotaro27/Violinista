using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System.Linq;
using System;

public class ScoreXML : MonoBehaviour
{
	public TextAsset ScoreData = null;
	public MusicScore Score = new MusicScore();
	void Awake() => InitScore();

	public void InitScore()
	{
		float bpm = 120;
		try
		{
			if (ScoreData == null) return;
			XDocument xml = XDocument.Parse(ScoreData.text);
			XElement root = xml.Root;
			Score.Title = root.Descendants("work-title").FirstOrDefault().Value;
			Score.Composer = root.Descendants("creator").FirstOrDefault().Value;
			Score.Difficulty = root.Descendants("credit-words").FirstOrDefault().Value.Split('|').Select(s => int.Parse(s)).ToList();
			Score.MusicSource = Resources.Load(Score.Title, typeof(AudioClip)) as AudioClip;
			var measure = 2 / float.Parse(root.Descendants("divisions").FirstOrDefault().Value);
			Score.Measure = int.Parse(root.Descendants("beats").FirstOrDefault().Value);
			var part = root.Element("part");
			bpm = float.Parse((from el in root.Descendants("per-minute") select el).First().Value);
			var notes = part.Descendants("note");
			Score.Notes.Add(new MusicNote()
			{
				NoteType = NotesType.Rest,
				Duration = 0
			});
			MusicNote NewNote = new MusicNote();
			foreach (var note in notes)
			{
				XElement notePitch = note.Element("pitch");
				if (notePitch == null)
				{
					Score.Notes.Add(new MusicNote()
					{
						NoteType = NotesType.Rest,
						Duration = float.Parse(note.Element("duration").Value) * measure
					});
				}
				else
				{
					if (note.Element("tie") != null)
					{
						var tied = note.Element("tie").Attribute("type");
						if (tied.Value == "stop")
						{
							NewNote.Duration += float.Parse(note.Element("duration").Value) * measure;
							continue;
						}
					}
					Score.NotesCount++;
					NewNote = new MusicNote()
					{
						NoteType = NotesType.Tap,
						PitchStep = (Pitch)System.Enum.Parse(typeof(Pitch), notePitch.Element("step").Value),
						Octave = int.Parse(notePitch.Element("octave").Value),
						Duration = float.Parse(note.Element("duration").Value) * measure,
					};
					if (NewNote.Duration >= 2)
					{
						NewNote.NoteType = NotesType.LongBegin;
						Score.NotesCount++;
					}
					if (notePitch.Element("alter") != null)
						NewNote.Alter = int.Parse(notePitch.Element("alter").Value);
					if (note.Descendants("tenuto").Any())
					{
						NewNote.String = Pitch.G;
					}
					NewNote.BPM = bpm;
					Score.Notes.Add(NewNote);
				}
			}
			Debug.Log($"BPM: {bpm}");
		}
		catch (System.Exception i_exception)
		{
			Debug.LogErrorFormat("はにゃ{0}", i_exception);
		}
		Score.BPM = bpm;
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	
}

public enum Pitch
{
	C = 0,
	D = 2,
	E = 4,
	F = 5,
	G = 7,
	A = 9,
	B = 11
}
public class MusicNote
{
	public Pitch PitchStep { get; set; } = Pitch.C;
	public Pitch String { get; set; } = Pitch.C;
	public int Octave { get; set; } = 0;
	public int Alter { get; set; } = 0;
	public NotesType NoteType { get; set; }
	public int GetPitch() => (int)PitchStep + Octave * 12 + Alter;
	public int GetFrequency() => (int)(442 * Math.Pow(2.0, (GetPitch() - 57) / 12.0));
	public float Duration { get; set; } = 0;
	public float BPM { get; set; } = 60;
	public float GetTime() => 60 / BPM * Duration;
	public override string ToString()
	{
		if (NoteType == NotesType.Rest)
			return $"Rest Duration: {Duration}";
		else
			return $"Pitch: {GetPitch()} Duration: {Duration} Type: {NoteType}";
	}
	public MusicNote ToType(NotesType Type)
	{
		var typenote = (MusicNote)MemberwiseClone();
		typenote.NoteType = Type;
		return typenote;
	}
}

public class MusicScore
{
	public string Title { get; set; }
	public string Composer { get; set; }
	public List<MusicNote> Notes { get; set; }
	public List<int> Difficulty { get; set; }
	public int Measure { get; set; }
	public int NotesCount { get; set; }
	public MusicScore()
	{
		Notes = new List<MusicNote>();
		Title = "";
		Composer = "";
		NotesCount = 0;
	}
	public float BPM { get; set; }
	public AudioClip MusicSource { get; set; }
	public List<MusicNote> InRestNotes(float durationR)
	{
		var InRestScore = new List<MusicNote>();
		foreach (var note in Notes)
		{
			var nnote = note.ToType(note.NoteType);
			nnote.Duration = note.Duration * durationR;
			var nrest = note.ToType(NotesType.Rest);
			nrest.Duration = note.Duration * (1 - durationR);
			InRestScore.Add(nnote);
			InRestScore.Add(nrest);
		}
		foreach (var note in InRestScore) Debug.Log(note);
		return InRestScore;
	}
}