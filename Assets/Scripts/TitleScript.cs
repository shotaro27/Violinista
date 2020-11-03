using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScript : MonoBehaviour
{
    internal GameSettings Settings;
    internal GameResult Result;
    float t = 0;
    void Start()
    {
        GetComponent<Text>().text = Result.MusicData.Title;
        transform.parent.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(64 * Result.MusicData.Title.Length + 100, 3);
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
		if (t > 5)
		{
            SceneManager.sceneLoaded += GameSceneLoadedOnPlay;
            SceneManager.LoadScene("PlayScene");
        }
    }
    private void GameSceneLoadedOnPlay(Scene next, LoadSceneMode mode)
    {
        var gameManager = GameObject.Find("GameManager").GetComponent<GameManager4>();
        gameManager.Settings = Settings;
        gameManager.Result = Result;
        SceneManager.sceneLoaded -= GameSceneLoadedOnPlay;
    }
}
