using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Metronome : MonoBehaviour
{
    float time;
    internal bool s;
    string scene;
    internal UnityEngine.Events.UnityAction<Scene, LoadSceneMode> SceneLoaded;
    public void Settings(string scene)
    {
        Destroy(GameObject.Find("Selects"));
        GetComponent<Animator>().SetTrigger("Settings");
        s = true;
        this.scene = scene;
    }
    void Start()
    {
        s = false;
        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
		if (s)
		{
            time += Time.deltaTime;
		}
		if (time >= 0.5f)
		{
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.LoadScene(scene);
		}
    }
}
