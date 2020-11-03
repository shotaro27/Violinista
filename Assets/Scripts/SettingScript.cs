using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Judgement;
using static NotesType;
using UnityEngine.SceneManagement;

public class SettingScript : MonoBehaviour
{
	internal GameSettings Settings = new GameSettings();
	internal GameResult Result = new GameResult();
	[SerializeField]
	GameObject Astring;
	[SerializeField]
	GameObject NoteObj;
	[SerializeField]
	GameObject JudgeEffect;
	[SerializeField]
	GameObject NoteArea;
	[SerializeField]
	GameObject EffectArea;
	[SerializeField]
	NoteSizeController SizeController;
	[SerializeField]
	NoteSizeController OrchestraController;
	[SerializeField]
	NoteSizeController ViolinController;
	[SerializeField]
	NoteSpeedController SpeedController;
	[SerializeField]
	Toggle LockToggle;
	[SerializeField]
	List<Toggle> Types;
	[SerializeField]
	List<Sprite> Violins;
	[SerializeField]
	Image S1;
	[SerializeField]
	AudioClip Do;
	[SerializeField]
	AudioSource Orchestra;
	[SerializeField]
	Image LockorScale;
	[SerializeField]
	List<Sprite> LockandScale;
	[SerializeField]
	GameObject Violin;
	void Start()
    {
		if (Settings.Mode == GameMode.ViolinGame)
		{
			Settings.Violin = 0;
			Violin.SetActive(false);
		}
		SpeedController.OnValueChanged(Settings.NotesSpeed);
		SpeedController.SetSlide();
		SizeController.OnValueChanged(Settings.Size);
		SizeController.SetSlide();
		OrchestraController.OnValueChanged(Settings.Orchestra);
		OrchestraController.SetSlide();
		ViolinController.OnValueChanged(Settings.Violin);
		ViolinController.SetSlide();
		GetComponent<Metronome>().SceneLoaded += SelectSceneLoaded;
		LockToggle.isOn = Settings.Mode == GameMode.RhythmGame ? Settings.SlideLock : Settings.ShowScale;
		Types[Settings.Typeindex].isOn = true;
		S1.sprite = Settings.Type;
		LockorScale.sprite = LockandScale[(int)Settings.Mode];
	}
	public void ToggleGameOption(bool state)
	{
		if (Settings.Mode == GameMode.RhythmGame) Settings.SlideLock = state;
		else Settings.ShowScale = state;
	}

	public void ResetValue()
	{
		SpeedController.OnValueChanged(5);
		SpeedController.SetSlide();
		SizeController.OnValueChanged(1);
		SizeController.SetSlide();
		OrchestraController.OnValueChanged(1);
		OrchestraController.SetSlide();
		ViolinController.OnValueChanged(1);
		ViolinController.SetSlide();
		Types[0].isOn = true;
		Type1(true);
		S1.sprite = Settings.Type;
		Settings.SlideLock = false;
		LockToggle.isOn = false;
	}

	// Update is called once per frame
	float T = 0;
	private void SelectSceneLoaded(Scene next, LoadSceneMode mode)
	{
		var MusicScroller = GameObject.Find("Scroll View").GetComponent<MusicScroller>();
		MusicScroller.Settings = Settings;
		MusicScroller.Result = Result;
		SceneManager.sceneLoaded -= SelectSceneLoaded;
	}
	void Update()
	{
		Orchestra.volume = OrchestraController.Value;
		Settings.NotesSpeed = SpeedController.Value;
		Settings.Size = SizeController.Value;
		Settings.Orchestra = OrchestraController.Value;
		Settings.Violin = ViolinController.Value;
		T += Time.deltaTime;
		if (T >= 1f && !GetComponent<Metronome>().s)
		{
			T = 0;
			InitNotes(new MusicNote() { NoteType = Tap });
		}
    }
	Notes InitNotes(MusicNote musicNote) //ノーツの初期化
	{
		var Note = Instantiate(NoteObj, NoteArea.transform);
		var NoteScript = Note.GetComponent<Notes>(); //ノーツのスクリプトを読み込む
		NoteScript.Score = new MusicNote() { NoteType = Rest };
		NoteScript.TapID = -1;
		NoteScript.Speed = GameManager4.SetSpeed(Settings.NotesSpeed); //ノーツのスピード
		NoteScript.TargetString = Astring; //ノーツの降る弦を取得する
		NoteScript.TargetPosition = new Vector2(-3200 + 1600f / 12 * 7, 70 + 75); //ノーツのターゲット位置を取得する
		NoteScript.StartPosition = new Vector2(700, 250 + 75);
		Note.GetComponent<RectTransform>().anchoredPosition = NoteScript.StartPosition;
		//ノーツのスタートする位置を設定する
		NoteScript.Score = musicNote; //ノーツの種類
		NoteScript.Size = Settings.Size;
		var effect = Instantiate(JudgeEffect, EffectArea.transform);
		NoteScript.Effect = effect.GetComponent<JudgeEffect>();
		if (Settings.ShowScale)
			effect.GetComponentInChildren<Text>().text = GameManager4.Pitches[(int)musicNote.PitchStep][musicNote.Alter + 1];
		else
			effect.GetComponentInChildren<Text>().enabled = false;
		NoteScript.PlayingAuto = true;
		NoteScript.Judge = Judge;
		effect.GetComponent<RectTransform>().anchoredPosition = NoteScript.TargetPosition + Vector2.up * 50;
		return NoteScript;
	}

	void Judge(Judgement judgement, Notes Note, int tapID)
	{
		GetComponent<AudioSource>().volume = Settings.Violin / 2;
		GetComponent<AudioSource>().PlayOneShot(Do);
		Note.Effect.Judge(judgement);
		Note.enabled = false;
		Destroy(Note.gameObject);
	}

	void SetViolinType(bool state, int type)
	{
		if (!state) return;
		Settings.Type = S1.sprite = Violins[Settings.Typeindex = type];
	}

	public void Type1(bool state) => SetViolinType(state, 0);
	public void Type2(bool state) => SetViolinType(state, 1);
	public void Type3(bool state) => SetViolinType(state, 2);
}

struct GameSettings
{
	public bool SlideLock { get; set; }
	public float Orchestra { get; set; }
	public float Violin { get; set; }
	public float NotesSpeed { get; set; }
	public float SongSpeed { get; set; }
	public float Size { get; set; }
	public int Typeindex { get; set; }
	public Sprite Type { get; set; }
	public int Division { get; set; }
	public bool AutoPlay { get; set; }
	public GameMode Mode { get; set; }
	public bool ShowScale { get; set; }
}