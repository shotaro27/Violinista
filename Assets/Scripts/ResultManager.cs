using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class ResultManager : MonoBehaviour
{
    internal GameResult Result;
    internal GameSettings Settings;
    [SerializeField]
    Text Title;
    [SerializeField]
    Text Difficulty;
    [SerializeField]
    Text Score;
    [SerializeField]
    Text HighScore;
    [SerializeField]
    Text Combo;
    [SerializeField]
    List<Text> JudgeText;
    [SerializeField]
    List<Animator> Stars;
    [SerializeField]
    GameObject FullCombo;
    [SerializeField]
    GameObject Buttons;
    [SerializeField]
    List<AudioClip> StarsSounds;
    Dictionary<int, string> Difficulties = new Dictionary<int, string>
    {
        [0] = "Beginner",
        [1] = "Normal",
        [2] = "Pro",
    };
    void Start()
    {
        Title.text = Result.MusicData.Title;
        Title.fontSize = Mathf.Min(70, 100 - 4 * Title.text.Length);
        Difficulty.text = Settings.Mode == GameMode.ViolinGame ? "Violin" : Difficulties[Result.Difficulty];
        Score.text = Result.Score.ToString();
        Combo.text = Result.Combo.ToString();
		if (PlayerPrefs.GetInt(Result.MusicData.Title + Result.Difficulty + Settings.Mode) < Result.Score)
		{
            Score.color = Color.yellow;
            PlayerPrefs.SetInt(Result.MusicData.Title + Result.Difficulty + Settings.Mode, Result.Score);
            PlayerPrefs.Save();
        }
        HighScore.text = PlayerPrefs.GetInt(Result.MusicData.Title + Result.Difficulty + Settings.Mode).ToString();
        foreach (Judgement j in Enum.GetValues(typeof(Judgement))) JudgeText[(int)j].text = Result.Judgements[j].ToString();
        FullCombo.SetActive(Result.Judgements[Judgement.Miss] == 0);
        Debug.Log(Result.Star);
        if (Result.Star >= 3) Result.Star = 3;
    }
    float t = 0;
    int s = 0;
    void Update()
    {
        if (t <= Result.Star + 1) t += Time.deltaTime;
		if (s < Result.Star && t > s + 2)
		{
            GetComponent<AudioSource>().PlayOneShot(StarsSounds[s]);
            Stars[s].SetTrigger("star");
            s++;
        }
		if (t >= Result.Star + 1) Buttons.SetActive(true);
    }
    public void Retry()
    {
        SceneManager.sceneLoaded += GameSceneLoadedOnPlay;
    }
    public void SelectMusic()
    {
        SceneManager.sceneLoaded += SelectSceneLoadedOnPlay;
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
            [Judgement.Just] = 0,
            [Judgement.Early] = 0,
            [Judgement.Late] = 0,
            [Judgement.Miss] = 0,
        };
        MusicScroller.Result = Result;
        SceneManager.sceneLoaded -= SelectSceneLoadedOnPlay;
    }
    private void GameSceneLoadedOnPlay(Scene next, LoadSceneMode mode)
    {
        var gameManager = GameObject.Find("Title").GetComponent<TitleScript>();
        gameManager.Settings = Settings;
        Result.Combo = 0;
        Result.Score = 0;
        Result.Star = 0;
        Result.Judgements = new Dictionary<Judgement, int>()
        {
            [Judgement.Just] = 0,
            [Judgement.Early] = 0,
            [Judgement.Late] = 0,
            [Judgement.Miss] = 0,
        };
        gameManager.Result = Result;
        SceneManager.sceneLoaded -= GameSceneLoadedOnPlay;
    }
}
