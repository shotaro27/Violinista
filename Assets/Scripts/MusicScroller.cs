using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class MusicScroller : MonoBehaviour
{
    internal GameSettings Settings = new GameSettings()
    {
        Orchestra = 1,
        Violin = 1,
        Size = 1,
        NotesSpeed = 5,
        SongSpeed = 1,
        Typeindex = 0,
        SlideLock = false,
        ShowScale = false
    };
    internal GameResult Result = new GameResult()
    {
        Combo = 0,
        Score = 0,
        Judgements = new Dictionary<Judgement, int>()
		{
            [Judgement.Just] = 0,
            [Judgement.Early] = 0,
            [Judgement.Late] = 0,
            [Judgement.Miss] = 0,
        },
        Star = 0
    };
    [SerializeField]
    List<TextAsset> Datas;
    List<ScoreXML> ScoreData = new List<ScoreXML>();
    [SerializeField]
    GameObject MusicList;
	[SerializeField]
	GameObject SelectMusic;
    [SerializeField]
    List<GameObject> Difficulties;
    RectTransform Contents;
    List<SelectButton> SelectButtons = new List<SelectButton>();
    [SerializeField]
    RectTransform ViolinDiv;
    [SerializeField]
    Sprite TypeDefault;
    [SerializeField]
    AudioSource Music;
    [SerializeField]
    bool ResetScore;
    int[] Divs = new int[3];
    ScoreXML SelectScore;
    [SerializeField]
    GameObject RhythmModeSettings;
    [SerializeField]
    GameObject ViolinModeSettings;

    void Awake()
	{
		foreach (var Data in Datas)
		{
            var score = gameObject.AddComponent<ScoreXML>();
            score.ScoreData = Data;
            ScoreData.Add(score);
            score.InitScore();
        }
	}
	private void GameSceneLoadedOnSettings(Scene next, LoadSceneMode mode)
	{
		var Setting = GameObject.Find("Metronome").GetComponent<SettingScript>();
		Setting.Settings = Settings;
        Setting.Result = Result;
        SceneManager.sceneLoaded -= GameSceneLoadedOnSettings;
	}
	void Start()
    {
        if (ResetScore) PlayerPrefs.DeleteAll();
        Contents = GetComponent<ScrollRect>().content;
        for (int i = 0; i < Mathf.Max(6, ScoreData.Count); i++)
        {
            var x = i % ScoreData.Count;
            var Select = Instantiate(MusicList, Contents);
            Select.GetComponent<RectTransform>().anchoredPosition = Vector2.down * (i * 200 - 100);
            Select.transform.GetChild(0).gameObject.GetComponent<Text>().text = ScoreData[x].Score.Composer;
            Select.transform.GetChild(1).gameObject.GetComponent<Text>().text = ScoreData[x].Score.Title;
            Select.transform.GetChild(1).gameObject.GetComponent<Text>().fontSize = Mathf.Min(70,
                106 - 4 * ScoreData[x].Score.Title.Length);
            var SelectButton = Select.GetComponent<SelectButton>();
            SelectButtons.Add(SelectButton);
            SelectButton.ScoreData = ScoreData[x];
            for (int j = 0; j < 3; j++)
                if (!PlayerPrefs.HasKey(ScoreData[x].Score.Title + j + Settings.Mode))
                {
                    PlayerPrefs.SetInt(ScoreData[x].Score.Title + j + Settings.Mode, 0);
                    PlayerPrefs.Save();
                }
            if (!PlayerPrefs.HasKey(ScoreData[x].Score.Title + "Speed" + Settings.Mode))
            {
                PlayerPrefs.SetFloat(ScoreData[x].Score.Title + "Speed" + Settings.Mode, 1);
                PlayerPrefs.Save();
            }
            SelectButton.Mypos = Select.GetComponent<RectTransform>().anchoredPosition;
        }
        OnScrolled(Vector2.zero);
        SetMusic(true);
        if (Settings.Mode == GameMode.RhythmGame)
        {
            RhythmModeSettings.SetActive(true);
            Difficulties[Result.Difficulty].GetComponent<Toggle>().isOn = true;
        }
        else
		{
            ViolinModeSettings.SetActive(true);
            ViolinModeSettings.GetComponentInChildren<Toggle>().isOn = Settings.ShowScale;
		}
        GameObject.Find("Settings").GetComponent<Metronome>().SceneLoaded += GameSceneLoadedOnSettings;
        Settings.Type = Settings.Type ?? TypeDefault;
        ViolinDiv.GetChild(0).GetChild(4).GetComponent<Image>().sprite = Settings.Type;
        ViolinDiv.GetChild(0).GetChild(5).GetComponent<Image>().sprite = Settings.Type;
        GameObject.Find("NotesSpeed").GetComponent<NoteSpeedController>().OnValueChanged(Settings.NotesSpeed);
        GameObject.Find("NotesSpeed").GetComponent<NoteSpeedController>().SetSlide();
        var scoreSpeedKey = SelectScore.Score.Title + "Speed" + Settings.Mode;
        GameObject.Find("SongSpeed").GetComponent<NoteSizeController>().OnValueChanged(PlayerPrefs.GetFloat(scoreSpeedKey));
        GameObject.Find("SongSpeed").GetComponent<NoteSizeController>().SetSlide();
    }
    public void ShowScale(bool Scale) => Settings.ShowScale = Scale;
    public void SetNotesSpeed(float Speed) => Settings.NotesSpeed = Speed;
    public void SetSongSpeed(float Speed)
    {
        PlayerPrefs.SetFloat(SelectScore.Score.Title + "Speed" + Settings.Mode, Speed);
        PlayerPrefs.Save();
        Settings.SongSpeed = Speed;
        Music.outputAudioMixerGroup.audioMixer.SetFloat("SongSpeed", Settings.SongSpeed);
        Music.outputAudioMixerGroup.audioMixer.SetFloat("SongSpeedPitch", 1 / Settings.SongSpeed);
    }
    public void OnClick(bool Auto) => Settings.AutoPlay = Auto;
    public void GameLoad() => SceneManager.sceneLoaded += GameSceneLoadedOnPlay;
    public void TopLoad() => SceneManager.sceneLoaded += TopLoadedOnPlay;
    private void GameSceneLoadedOnPlay(Scene next, LoadSceneMode mode)
    {
        var gameManager = GameObject.Find("Title").GetComponent<TitleScript>();
        gameManager.Settings = Settings;
        gameManager.Result = Result;
        SceneManager.sceneLoaded -= GameSceneLoadedOnPlay;
    }
    private void TopLoadedOnPlay(Scene next, LoadSceneMode mode)
    {
        var topStart = GameObject.Find("Top").GetComponent<TopStart>();
        topStart.ModeStart();
        SceneManager.sceneLoaded -= TopLoadedOnPlay;
    }

    public void OnScrolled(Vector2 pos) // 無限スクロール
    {
        Contents.anchoredPosition = Vector2.zero;
        if (Mathf.Round(pos.y * 6) / 6 == 1) return;
        SetMusic(false);
        foreach (RectTransform child in Contents)
        {
            child.anchoredPosition = child.GetComponent<SelectButton>().Mypos + Vector2.down * pos.y * 200;
            if (child.anchoredPosition.y <= -200 * SelectButtons.Count / 2)
                child.anchoredPosition += Vector2.up * 200 * SelectButtons.Count;
            if (child.anchoredPosition.y >= 200 * SelectButtons.Count / 2)
                child.anchoredPosition += Vector2.down * 200 * SelectButtons.Count;
        }
    }

    void SetMusic(bool setMusic)
	{
        List<RectTransform> childs = new List<RectTransform>();
        foreach (RectTransform child in Contents) childs.Add(child);
        SelectScore = childs.Where(r => r.anchoredPosition.y <= 200 && r.anchoredPosition.y >= 0)
            .First().gameObject.GetComponent<SelectButton>().ScoreData;
        Result.MusicData = SelectScore.Score;
        var scoreSpeedKey = SelectScore.Score.Title + "Speed" + Settings.Mode;
        GameObject.Find("SongSpeed").GetComponent<NoteSizeController>().OnValueChanged(PlayerPrefs.GetFloat(scoreSpeedKey));
        GameObject.Find("SongSpeed").GetComponent<NoteSizeController>().SetSlide();
        SelectMusic.transform.GetChild(0).gameObject.GetComponent<Text>().text = SelectScore.Score.Composer;
        SelectMusic.transform.GetChild(1).gameObject.GetComponent<Text>().text = SelectScore.Score.Title;
        SelectMusic.transform.GetChild(1).gameObject.GetComponent<Text>().fontSize = Mathf.Min(70,
            106 - 4 * SelectScore.Score.Title.Length);
        for (int i = 0; i < Difficulties.Count; i++)
            Difficulties[i].transform.GetChild(1).GetChild(1).GetComponent<Text>().text
                = (Divs[i] = SelectScore.Score.Difficulty[i]).ToString();
        SetDifficulty(true, Result.Difficulty);
        if (Music.isPlaying) Music.Stop();
		if (setMusic)
        {
            Music.clip = SelectScore.Score.MusicSource;
            Music.time = 3 - 60 / SelectScore.Score.BPM * (4 - SelectScore.Score.Notes.TakeWhile(n => n.NoteType == NotesType.Rest)
                .Sum(n => n.Duration) / SelectScore.Score.Measure * 2);
            Music.Play();
        }
    }

    public void SetScrollPos()
    {
        foreach (RectTransform child in Contents)
            child.GetComponent<SelectButton>().Mypos = child.anchoredPosition + Vector2.up * 200;
    }

    public void SetScrollEnd()
    {
        foreach (RectTransform child in Contents)
            child.anchoredPosition = Vector2.up * (Mathf.Round((child.anchoredPosition.y - 100) / 200) * 200 + 100);
        SetMusic(true);
    }
    void SetDifficulty(bool state, int difficulty)
    {
        if (!state) return;
        Debug.Log(difficulty);
        Result.Difficulty = difficulty;
        Settings.Division = Divs[difficulty];
        ViolinDiv.localScale = new Vector2(1f / Settings.Division * 6, 1);
        ViolinDiv.anchoredPosition = new Vector2(640f / Settings.Division * 6, 1);
    }

	public void Beginner(bool state) => SetDifficulty(state, 0);
    public void Normal(bool state) => SetDifficulty(state, 1);
    public void Pro(bool state) => SetDifficulty(state, 2);
    public void HowToPlay()
    {
        SceneManager.sceneLoaded += HowToPlayLoad;
        SceneManager.LoadScene("HowToPlay", LoadSceneMode.Additive);
    }
    private void HowToPlayLoad(Scene next, LoadSceneMode mode)
    {
        var HowTo = GameObject.Find("Next").GetComponent<HowToPlay>();
        HowTo.Mode = (int)Settings.Mode;
        SceneManager.sceneLoaded -= HowToPlayLoad;
    }
}

struct GameResult
{
    public MusicScore MusicData { get; set; }
    public int HighScore { get; set; }
    public int Score { get; set; }
    public int Star { get; set; }
    public int Combo { get; set; }
    public Dictionary<Judgement, int> Judgements { get; set; }
    public int Difficulty { get; set; }
}