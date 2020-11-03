using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static NotesType;

public enum NotesType
{
	Rest,
	Tap,
	Flick,
	LongBegin,
	LongEnd,
}
public class Notes : MonoBehaviour
{
	internal protected RectTransform RTransform;
	public float Speed { get; set; }
	public float Size { get; set; }
	public Vector2 TargetPosition { get; set; }
	public Vector2 StartPosition { get; set; }
	public float Timing { get; private set; }
	public float NoteTime;
	public Sprite EndNoteSprite;
	public MusicNote Score { get; set; }
	public float ScoreTime { get; set; }
	public GameObject TargetString { get; set; }
	public UIPointLine NotesLine { get; set; }
	public LongScript Line { get; set; }
	public JudgeEffect Effect { get; set; }
	int _TapID;
	public Action<Judgement, Notes, int> Judge { get; set; }
	public int TapID
	{
		get { return _TapID; }
		set
		{
			_TapID = value;
			if (Score.NoteType == LongBegin)
			{
				Line.TapID = value;
			}
		}
	}
	bool playingJudge; 
	public bool PlayingAuto { get; set; }
	bool PlayingAuto2;
	Vector2 start, target;
	Vector2[] points;
	public GameObject Violinist { get; set; }
	RectTransform ViolinPosition;
	public static int Curve = 200;
	float T01;
	public float MissPoint { get; set; }
	public (float min, float max) Minmax { get; set; }
	protected virtual void Start()
	{
		PlayingAuto2 = PlayingAuto;
		RTransform = gameObject.GetComponent<RectTransform>();
		RTransform.localScale = Vector2.zero;
		NoteTime = Timing = 1.5f / Speed;
		ViolinPosition = (RectTransform)RTransform.parent.parent;
		RTransform.anchoredPosition = StartPosition + ViolinPosition.anchoredPosition * new Vector2(-1, 1);
		gameObject.GetComponent<Image>().color = TargetString.GetComponent<Image>().color;
		//if (Score.NoteType == NotesType.LongEnd) Size *= 0.5f;
		playingJudge = false;
		target = TargetPosition;
		if (Score.NoteType == LongBegin || Score.NoteType == LongEnd)
		{
			Line.StartPosition = StartPosition;
			Line.ViolinPosition = ViolinPosition;
			NotesLine.color = TargetString.GetComponent<Image>().color * new Color(1, 1, 1, 0.4f);
			NotesLine.points[2] = target;
		}
		if (Score.NoteType == LongEnd)
		{
			TapID = Line.TapID;
			transform.GetChild(0).GetComponent<Image>().sprite = EndNoteSprite;
		}
	}
	protected virtual void Update()
	{
		start = StartPosition - ViolinPosition.anchoredPosition + Vector2.right * GameObject.Find("GameArea").GetComponent<RectTransform>().rect.width / 2;
		Timing -= Time.deltaTime;
		T01 = Math.Min(1 - Timing / NoteTime, 1);
		points = new Vector2[] { start, GetMiddlePoint(start, target), target };
		RTransform.anchoredPosition = UIPointLine.GetPoint(points, T01);
		if (Score.NoteType == LongBegin) NotesLine.posTEnd = T01;
		if (Score.NoteType == LongEnd) NotesLine.posTStart = T01;
		if (Score.NoteType == LongBegin || Score.NoteType == LongEnd)
		{
			NotesLine.points = points;
			NotesLine.SetVerticesDirty();
		}
		RTransform.localScale = Vector2.one * Size * T01;
		if (Timing < 0 && PlayingAuto)
		{
			PlayingAuto = false;
			Judge(Judgement.Just, this, 0);
		}
		if (Timing < 0 && Score.NoteType == LongEnd && !Line.ParentNote.GetComponent<Notes>().enabled && !playingJudge && !PlayingAuto2)
		{
			playingJudge = true;
			Judge(Judgement.Just, this, TapID);
		}
		if (Timing < MissPoint)
		{
			Judge(Judgement.Miss, this, 0);
		}
	}
	internal bool InRange(int frequency)
	{
		double min, max;
		min = 442 * Math.Pow(2.0, (Score.GetPitch() - Minmax.min) / 12.0);
		max = 442 * Math.Pow(2.0, (Score.GetPitch() - Minmax.max) / 12.0);
		return min < frequency && frequency < max;
	}
	public static Vector2 GetMiddlePoint(Vector2 start, Vector2 target)
		=> new Vector2((start.x + target.x) / 2, start.y + Curve);
}
