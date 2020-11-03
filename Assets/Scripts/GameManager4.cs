using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using static NotesType;
using static Judgement;
using UnityEngine.SceneManagement;

public enum Judgement
{
	Just,
	Early,
	Late,
	Miss
}

public enum GameMode
{
	RhythmGame,
	ViolinGame
}
public class GameManager4 : MonoBehaviour, IJudgable, IPausable
{
	internal static List<Notes> NotesLine = new List<Notes>(); //今画面上に存在しているノーツ
	internal static Dictionary<int, LongScript> HoldLine = new Dictionary<int, LongScript>(); //今ホールドされているロングノーツ
	GameObject NoteArea, LongArea, EffectArea; //ノーツを置く場所
	[SerializeField]
	GameObject NoteObject; //ノーツのオブジェクト
	[SerializeField]
	GameObject LongObject; //ロングノーツのオブジェクト
	[SerializeField]
	GameObject JudgeEffect;
	[SerializeField]
	internal GameSettings Settings;
	[SerializeField]
	float Offset; //ノーツのタイミング調整
	float Beat; //拍
	float BPM; //曲のBPM
	int NoteIndex; //ノーツの通し番号
	LongScript LineScript; //ロングのライン
	UIPointLine LineDrawing; //ロングのライン
	bool SongPlay; //曲が再生中か
	List<MusicNote> NotesScore; //譜面
	AudioSource Music; //曲
	[SerializeField]
	GameObject ViolinObj; //バイオリン
	[SerializeField]
	GameObject StringObj; //弦
	List<GameObject> Strings = new List<GameObject>(); //弦
	RectTransform CanvasRect; //キャンバスの位置
	Dictionary<int, Vector2> tapPositions; //タップされている場所リスト
	[SerializeField]
	Violin ViolinSound; //バイオリンの音
	GameObject Note, LongLine;
	RectTransform ViolinRect, StringsRect;
	internal GameMode GameMode;
	public GameObject Violinists;
	List<GameObject> Violinist = new List<GameObject>();
	[SerializeField]
	GameObject JudgeText;
	[SerializeField]
	ComboCounter ComboCounter;
	[SerializeField]
	ScoreCounter ScoreCounter;
	[SerializeField]
	JudgeController JudgeController;
	[SerializeField]
	StarCounter StarCounter;
	[SerializeField]
	MicPitch Mic;
	Vector2 VPos;
	bool IsPause;
	List<Touch> Touches = new List<Touch>();
	[SerializeField]
	AudioSource Clap;
	[SerializeField]
	Animator Conductor;
	bool Ended;
	internal GameResult Result;
	float Speed;
	float SongSpeed;
	public static readonly string[][] Pitches = new string[][]
	{
		new string[]{"Ces", "C", "Cis"}, null,
		new string[]{"Des", "D", "Dis"}, null,
		new string[]{"Es", "E", "Eis"},
		new string[]{"Fes", "F", "Fis"}, null,
		new string[]{"Ges", "G", "Gis"}, null,
		new string[]{"As", "A", "Ais"}, null,
		new string[]{"B", "H", "His"},
	};
	public static float SetSpeed(float s) => (s - 1) / 6f + 0.5f;
	void Start()
	{
		SongSpeed = Settings.SongSpeed;
		GameMode = Settings.Mode;
		//Offset = GameMode == GameMode.ViolinGame ? 0.3f : 0;
		Offset = 0.2f;
		Speed = SetSpeed(Settings.NotesSpeed); 
		NotesScore = Result.MusicData.InRestNotes(GameMode == GameMode.RhythmGame ? 0.5f : 0.75f); //譜面→ノーツのリスト
		BPM = Result.MusicData.BPM * SongSpeed;
		Debug.Log(BPM);
		Beat = 3f / SongSpeed - 1.5f / Speed + Offset - 60 / BPM * Result.MusicData.Measure; //拍を初期化 1.5/speed秒
		NoteIndex = 0; //ノーツ番号を0に
		Ended = false;
		SongPlay = false; //曲の再生を一旦切る
		Music = gameObject.GetComponent<AudioSource>(); //曲を読み込む
		Music.outputAudioMixerGroup.audioMixer.SetFloat("SongSpeed", SongSpeed);
		Music.outputAudioMixerGroup.audioMixer.SetFloat("SongSpeedPitch", 1 / SongSpeed);
		Music.clip = Result.MusicData.MusicSource;
		Music.PlayScheduled(AudioSettings.dspTime + 4 - 3 / SongSpeed + 120 / BPM); //曲を再生する
		SongPlay = true; //演奏をONにする
		CanvasRect = GameObject.Find("GameArea").GetComponent<RectTransform>(); //キャンバス位置を読み込む
		NoteArea = GameObject.Find("NoteArea"); //ノーツのエリアを読み込む
		LongArea = GameObject.Find("LongArea"); //ロングノーツの線のエリアを読み込む
		EffectArea = GameObject.Find("EffectArea"); //えふぇくとのエリアを読み込む
		tapPositions = new Dictionary<int, Vector2>(); //タップ位置のリストを初期化する
		ViolinRect = ViolinObj.GetComponent<RectTransform>();
		StringsRect = StringObj.GetComponent<RectTransform>();
		StringsRect.GetChild(4).GetComponent<Image>().sprite = Settings.Type;
		StringsRect.GetChild(5).GetComponent<Image>().sprite = Settings.Type;
		foreach (Transform violinist in Violinists.transform) Violinist.Add(violinist.gameObject);
		UIPointLine.weight = Settings.Size * 100;
		StringsRect.localScale = new Vector2(6f / Settings.Division, 1);
		Debug.Log(ViolinRect.sizeDelta.x);
		StringsRect.anchoredPosition = new Vector2(ViolinRect.sizeDelta.x * (6f / Settings.Division - 1) / 2, StringsRect.anchoredPosition.y);
		for (int i = 0; i < 4; i++) Strings.Add(StringObj.transform.GetChild(i).gameObject);
		Touches.AddRange(Enumerable.Range(0, Input.touchCount).Select(i => Input.GetTouch(i)));
		foreach (var touch in Touches) tapPositions.Add(touch.fingerId, ToCanvasPos(touch.position));
		Touches = new List<Touch>();
		Conductor.SetFloat("WaitTime", 0.5f);
		Conductor.SetFloat("BpmSpeed", BPM / 120);
		Destroy(Conductor.gameObject, 4 + 120 / BPM);
		Music.volume = Settings.Orchestra;
		ViolinSound.Volume = Settings.Violin;
		if (GameMode == GameMode.ViolinGame)
			GameObject.Find("Microphone").AddComponent<MicPitch>().gameManager = this;
	}
	Notes InitNotes(GameObject Note, MusicNote musicNote) //ノーツの初期化
	{
		var NoteScript = Note.GetComponent<Notes>(); //ノーツのスクリプトを読み込む
		NoteScript.Score = new MusicNote() { NoteType = Rest };
		NoteScript.TapID = -1;
		NoteScript.ScoreTime = Beat - 0.75f * Speed - 0.75f; //演奏時間上のタイミング
		NoteScript.Speed = Speed; //ノーツのスピード
		NoteScript.TargetString = GetTargetStringFromNotes(musicNote); //ノーツの降る弦を取得する
		NoteScript.TargetPosition = GetTargetPositionFromNotes(musicNote); //ノーツのターゲット位置を取得する
		VPos = NoteScript.StartPosition = musicNote.NoteType == LongEnd
			? VPos : GetStartPositionFromTargetPosition(NoteScript.TargetPosition);
		Note.GetComponent<RectTransform>().anchoredPosition = NoteScript.StartPosition;
		//ノーツのスタートする位置を設定する
		NoteScript.Score = musicNote; //ノーツの種類
		NoteScript.Size = Settings.Size;
		var effect = Instantiate(JudgeEffect, EffectArea.transform);
		NoteScript.Effect = effect.GetComponent<JudgeEffect>();
		if (Settings.ShowScale)
			effect.GetComponentInChildren<Text>().text = Pitches[(int)musicNote.PitchStep][musicNote.Alter + 1];
		else
			effect.GetComponentInChildren<Text>().enabled = false;
		NoteScript.PlayingAuto = Settings.AutoPlay;
		NoteScript.Judge = GameObject.Find("GameManager").GetComponent<GameManager4>().Judge;
		NoteScript.MissPoint = GameMode == GameMode.ViolinGame ? -0.7f : -0.5f;
		NoteScript.Minmax = GameMode == GameMode.ViolinGame ? (57.25f, 56.75f)
			: Settings.Division >= 10 ? (59, 55) : (58, 56);
		effect.GetComponent<RectTransform>().anchoredPosition = NoteScript.TargetPosition + Vector2.up * 50;
		NotesLine.Add(NoteScript); //ノーツラインに追加する
		return NoteScript;
	}
	Vector2 GetStartPositionFromTargetPosition(Vector2 TargetPosition) //ターゲットからスタートを得る
	{
		var TargetAnchored = TargetPosition + ViolinRect.anchoredPosition + Vector2.left * 800;
		var SelectedViolinist = Violinist
			.OrderByDescending(v => Vector2.Distance(v.GetComponent<RectTransform>().anchoredPosition, TargetAnchored))
			.FirstOrDefault();
		if (SelectedViolinist.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("New State"))
			foreach (Transform violinist in Violinists.transform)
			{
				violinist.GetComponent<Animator>().SetTrigger("PlayStart");
			}
		return SelectedViolinist
			.GetComponent<RectTransform>().anchoredPosition;
	}
	float endtime = 0;
	float d = 0;
	float dxf;
	void Update()
	{
		if (SongPlay && Music.time / SongSpeed >= Beat + NotesScore[NoteIndex].Duration * 60f / BPM / 2f) //演奏中であるか
		{
			Beat += NotesScore[NoteIndex].Duration * 60f / BPM / 2f; //拍を取得する
			if (NotesScore[NoteIndex].NoteType == LongBegin && LongLine != null) //ノーツタイプがロング開始であるか
			{
				Note = Instantiate(NoteObject, NoteArea.transform); //ノーツを生成する
				var NoteScript = InitNotes(Note, NotesScore[NoteIndex].ToType(LongEnd)); //ロング終了を設定する
				NoteScript.NotesLine = LineDrawing; //ロング判定ラインを設定する
				LineScript.EndNote = Note;
				NoteScript.Line = LineScript;
			}
			NoteIndex++; //ノーツの通し番号を更新する
			if (NotesScore[NoteIndex].NoteType != Rest) //休符でないなら
			{
				Note = Instantiate(NoteObject, NoteArea.transform); //ノーツを生成する
				var NoteScript = InitNotes(Note, NotesScore[NoteIndex]); //ノーツを初期化する
				if (NoteScript.Score.NoteType == LongBegin) //ノーツタイプがロング開始であるか
				{
					LongLine = Instantiate(LongObject, LongArea.transform); //判定ラインを生成する
					LineDrawing = LongLine.GetComponent<UIPointLine>();
					NoteScript.NotesLine = LineDrawing; //ノートの判定ラインに設定する
					LineScript = LongLine.GetComponent<LongScript>();
					LineScript.ParentNote = Note;
					LineScript.ParentNoteScript = NoteScript;
					NoteScript.Line = LineScript;
				}
			}
			if (NoteIndex >= NotesScore.Count - 1) SongPlay = false; //ノーツが終わったら曲を終了する
		}
		if ((Settings.AutoPlay || GameMode == GameMode.ViolinGame) && NotesLine.Count > 0) //自動スライド
		{
			if (ViolinRect.anchoredPosition.x + NotesLine[0].TargetPosition.x < 0
				|| ViolinRect.anchoredPosition.x + NotesLine[0].TargetPosition.x > 1600 || Mathf.Abs(dxf - d) > 10)
			{
				dxf = NotesLine[0].TargetPosition.x + 1600 - Settings.Division * 400;
				d = Mathf.Lerp(d, dxf, 0.1f);
				//	ViolinRect.anchoredPosition + Vector2.right * CanvasRect.rect.width / 2
				//if (ViolinRect.anchoredPosition.x + NotesLine[0].TargetPosition.x < -800
				//	|| ViolinRect.anchoredPosition.x + NotesLine[0].TargetPosition.x > 800)
				ViolinRect.anchoredPosition = new Vector2(Clamp(ViolinRect.sizeDelta.x * (6f / Settings.Division - 1) / 2 - d,
					4800 - (800f / 3 * Result.MusicData.Difficulty[0]) / Settings.Division * 6,
					3200), ViolinRect.anchoredPosition.y);
			}
		}
		if (!SongPlay && (Music.time > Music.clip.length - 2 || !Music.isPlaying))
		{
			if (endtime == 0)
			{
				Violinist.ForEach(v => {
					var animator = v.GetComponent<Animator>();
					animator.enabled = false;
				});
				Ended = true;
				Clap.Play();
				Microphone.End(Microphone.devices[0]);
			}
			endtime += Time.deltaTime;
			if (Ended && endtime >= 4 && endtime < 5)
			{
				Ended = false;
				GameObject.Find("Back").GetComponent<Animator>().SetTrigger("End");
			}
			if (!Ended && endtime >= 5)
			{
				Ended = true;
				if (Settings.AutoPlay)
				{
					SceneManager.sceneLoaded += SelectSceneLoadedOnPlay;
					SceneManager.LoadScene("SelectScene");
				}
				else
				{
					SceneManager.sceneLoaded += ResultSceneLoadedOnPlay;
					SceneManager.LoadScene("ResultScene");
				}
			}
		}
		if (Settings.AutoPlay) return;
		var touchCount = Input.touchCount;
		Touches.AddRange(Enumerable.Range(0, touchCount).Select(i => Input.GetTouch(i)));
		for (var i = 0; i < Touches.Count; i++) //各タップごとに
		{
			var touch = Touches[i]; //タップ情報を取得する
			var tapPos = ToCanvasPos(touch.position);
			//Debug.Log(touch.fingerId);
			if (!IsPause && GameMode != GameMode.ViolinGame)
			{
				switch (touch.phase) //タッチフェーズ
				{
					case TouchPhase.Canceled:
					case TouchPhase.Ended: //タップ終了
						LongScript Long;
						var End = AllJudge(n => n.TapID == touch.fingerId && n.Score.NoteType == LongEnd
						 && !n.Line.ParentNoteScript.enabled, 0.2f, touch.fingerId);
						if (End == null && HoldLine.TryGetValue(touch.fingerId, out Long))
						{
							ViolinSound.Stop(touch.fingerId);
							JudgeMiss(Long, touch.fingerId); //ホールド中のラインはミスにする
						}
						tapPositions.Remove(touch.fingerId); //位置を消す
						break;
					case TouchPhase.Began: //タップ開始
						Vector2 j = new Vector2();
						if (!tapPositions.TryGetValue(touch.fingerId, out j))
							tapPositions.Add(touch.fingerId, tapPos); //タップリストに登録する
						Debug.Log($"Tap {NotesLine.Any(n => n.InRange(GetFrequencyFromPosition(tapPos)))}");
						if (!ViolinRect.rect.Contains(tapPos - ViolinRect.anchoredPosition + Vector2.right * 800)) break;
						var Note = AllJudge(n => n.InRange(GetFrequencyFromPosition(tapPos)) &&
						(n.Score.NoteType == Tap || n.Score.NoteType == LongBegin), 0.1f, touch.fingerId);
						if (Note != null && Note.Score.NoteType == LongBegin) //ロング開始なら
						{
							HoldLine.Add(touch.fingerId, Note.Line); //判定ラインをリストに追加する
						}
						break;
					case TouchPhase.Moved: //タップ移動
						if (!Settings.SlideLock ||
							!ViolinRect.rect.Contains(tapPos - ViolinRect.anchoredPosition + Vector2.right * 800))
						{
							var dx = tapPos.x - tapPositions[touch.fingerId].x;
							ViolinRect.anchoredPosition = new Vector2(Clamp(ViolinRect.anchoredPosition.x + dx,
								//4800 - 2400f / Settings.Division * 6,
								4800 - (800f / 3 * Result.MusicData.Difficulty[0]) / Settings.Division * 6,
								3200), ViolinRect.anchoredPosition.y);
						}
						tapPositions[touch.fingerId] = tapPos; //タップ更新
						break;
				}
			}
			
		}
		if (!IsPause)
		{
			Touches = new List<Touch>();
		}
	}
	Vector2 ToCanvasPos(Vector2 ScreenPos) => new Vector2
			((ScreenPos.x - Screen.width / 2) / Screen.width * 1600,
			(ScreenPos.y - Screen.height / 2) / Screen.width * 1600);
	public Notes AllJudge(Func<Notes, bool> func, float judgeTiming, int id) //判定
	{
		Predicate<Notes> pred = func.Invoke;
		//Debug.Log(id + "Judge!" + string.Join(", ", NotesLine.Select(n => n.TapID)));
		if (NotesLine.Any(func)) //ノーツ判定にタップ位置が含まれていたら
		{
			Debug.Log("Hit!");
			var Note = NotesLine.Find(pred); //ノーツを取得する
			if (Note.Timing < judgeTiming && Note.Timing > -judgeTiming) Judge(Just, Note, id);
			else if (Note.Timing < judgeTiming + 0.2f && Note.Timing > judgeTiming) Judge(Early, Note, id);
			else if (Note.Timing > -judgeTiming - 0.2f && Note.Timing < -judgeTiming) Judge(Late, Note, id);
			else return null;
			return Note;
		}
		return null;
	}
	public void Pause()
	{
		if (IsPause) UnPause();
		else
		{
			Time.timeScale = 0;
			ViolinSound.Pause();
			Music.Pause();
			IsPause = true;
		}
	}
	private void ResultSceneLoadedOnPlay(Scene next, LoadSceneMode mode)
	{
		var resultManager = GameObject.Find("Results").GetComponent<ResultManager>();
		resultManager.Settings = Settings;
		Result.Score = ScoreCounter.Count;
		Result.Star = StarCounter.Count;
		resultManager.Result = Result;
		SceneManager.sceneLoaded -= ResultSceneLoadedOnPlay;
	}
	private void SelectSceneLoadedOnPlay(Scene next, LoadSceneMode mode)
	{
		var MusicScroller = GameObject.Find("Scroll View").GetComponent<MusicScroller>();
		MusicScroller.Settings = Settings;
		Result.Combo = 0;
		Result.Score = 0;
		Result.Star = 0;
		Result.Judgements = new Dictionary<Judgement, int>()
		{
			[Just] = 0,
			[Early] = 0,
			[Late] = 0,
			[Miss] = 0,
		};
		MusicScroller.Result = Result;
		SceneManager.sceneLoaded -= SelectSceneLoadedOnPlay;
	}
	public void UnPause()
	{
		Time.timeScale = 1;
		ViolinSound.UnPause();
		Music.UnPause();
		IsPause = false;
	}
	public void JudgeMiss(Notes note) => Judge(Miss, note, 0);
	public void Judge(Judgement judgement, Notes Note, int tapID) //判定
	{
		var noteType = Note.Score.NoteType;
		if ((noteType == LongBegin && judgement == Miss) || (noteType == LongEnd && judgement != Miss))
			//ロング開始でミスorロング終了でミス以外
			Judge(judgement, Note.Line, tapID);
		else
			Judge(judgement);
		Note.Effect.Judge(judgement);
		NotesLine.Remove(Note);
		if (judgement != Miss) //ミス以外
		{
			Note.TapID = tapID;
			Debug.Log(Note.Score.GetFrequency());
			if (noteType != LongEnd) ViolinSound.Play(Note.Score.GetPitch(), Note.Score.GetTime() / SongSpeed, tapID);
			if (Note.NotesLine != null) //ラインが存在しているか
				Note.NotesLine.posTEnd = 1;
			Note.gameObject.GetComponent<RectTransform>().localScale = Vector2.one * Note.Size;
			Note.gameObject.GetComponent<RectTransform>().anchoredPosition = Note.TargetPosition;
			Note.enabled = false;
		}
		if (noteType != LongBegin)
		{
			Destroy(Note.gameObject);
		}
	}
	public void Judge(Judgement judgement)
	{
		JudgeController.Judge(judgement);
		if (judgement == Just || judgement == Early || judgement == Late)
		{
			ComboCounter.AddCombo();
			ScoreCounter.Judge(judgement);
		}
		else
			ComboCounter.ResetCombo();
		Result.Judgements[judgement]++;
		Result.Combo = Mathf.Max(Result.Combo, ComboCounter.Count);
	}
	public void JudgeMiss(LongScript Long, int tapID) => Judge(Miss, Long, tapID);
	public void Judge(Judgement judgement, LongScript Line, int tapID)
	{
		Judge(judgement);
		Debug.Log("line");
		HoldLine.Remove(tapID);
		Destroy(Line.ParentNote);
		if (Line.EndNote != null)
		{
			Line.EndNote.GetComponent<Notes>().Effect.Judge(judgement);
			NotesLine.Remove(Line.EndNote.GetComponent<Notes>());
			Destroy(Line.EndNote);
		}
		Destroy(Line.gameObject);
	}

	Vector2 GetTargetPositionFromNotes(MusicNote Note)
	{
		var pitch = Note.GetPitch();
		var Opens = new MusicNote[]{
			new MusicNote() { PitchStep = Pitch.G, Octave = 3 },
			new MusicNote() { PitchStep = Pitch.D, Octave = 4 },
			new MusicNote() { PitchStep = Pitch.A, Octave = 4 },
			new MusicNote() { PitchStep = Pitch.E, Octave = 5 },
		};
		int pitchStr = GetTargetStringIndexFromNotes(Note);
		//int pitchStr = (pitch - 43) / 7;
		int pitchPos = pitch - 43 - pitchStr * 7;
		//if (pitchStr >= 4)
		//{
		//	pitchPos = pitch - 64;
		//	pitchStr = 3;
		//}
		var pitchNote = new MusicNote() { PitchStep = Opens[pitchStr].PitchStep + pitchPos, Octave = Opens[pitchStr].Octave };
		var pitchFrequency = pitchNote.GetFrequency();
		var StringY = Strings[pitchStr].GetComponent<RectTransform>().anchoredPosition.y;
		//return new Vector2(Mathf.Log(Opens[pitchStr].GetFrequency() / (float)pitchFrequency, 2) * -3200 - 3200 + 1600f / 12f, StringY);
		return new Vector2(Mathf.Log(Opens[pitchStr].GetFrequency() / (float)pitchFrequency, 2) * -3200 / Settings.Division * 6f
			- 3200 + 1600f / Settings.Division / 2f, StringY);
	}

	int GetFrequencyFromPosition(Vector2 pos)
	{
		var StPos = Strings.Select((s, index) =>
			((s.GetComponent<RectTransform>().anchoredPosition + ViolinRect.anchoredPosition).y, index))
			.OrderBy(s => Math.Abs(pos.y - s.y)).FirstOrDefault();
		var Opens = new MusicNote[]{
			new MusicNote() { PitchStep = Pitch.G, Octave = 3 },
			new MusicNote() { PitchStep = Pitch.D, Octave = 4 },
			new MusicNote() { PitchStep = Pitch.A, Octave = 4 },
			new MusicNote() { PitchStep = Pitch.E, Octave = 5 },
		};
		pos -= ViolinRect.anchoredPosition;
		//var pit2e = pos.x / CanvasRect.rect.width / 2 + 1d / 4d - 1d / 24d + 1;
		var pit2e = (pos.x / CanvasRect.rect.width / 2 + 1d / 4d + 1) * Settings.Division / 6 - 1d / 24d;
		//	var pit = Opens[StPos.index].GetFrequency() * Math.Pow(2, Math.Round(pit2e * 12) / 12);
		var pit = Opens[StPos.index].GetFrequency() * Math.Pow(2, pit2e);
		return (int)pit;
	}
	int GetTargetStringIndexFromNotes(MusicNote Note)
	{
		if (Note.String != Pitch.C)
			return Note.String == Pitch.G ? 0 : (Note.String == Pitch.D ? 1 : (Note.String == Pitch.A ? 2 : 3));
		int pitch = Note.GetPitch();
		int pitchStr = (pitch - 43) / 7;
		if (pitchStr >= 4) pitchStr = 3;
		return pitchStr;
	}
	GameObject GetTargetStringFromNotes(MusicNote Note) => Strings[GetTargetStringIndexFromNotes(Note)];

	public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
		=> (value.CompareTo(min) < 0) ? min : (value.CompareTo(max) > 0) ? max : value;
}

interface IJudgable
{
	void Judge(Judgement judgement);
}

interface IPausable
{
	void Pause();
	void UnPause();
}