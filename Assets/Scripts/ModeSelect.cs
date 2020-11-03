using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelect : MonoBehaviour
{
	[SerializeField] GameMode Mode;
	public void OnClick() => SceneManager.sceneLoaded += ResultSceneLoadedOnPlay;
	private void ResultSceneLoadedOnPlay(Scene next, LoadSceneMode mode)
	{
		var scroller = GameObject.Find("Scroll View").GetComponent<MusicScroller>();
		scroller.Settings.Mode = Mode;
		SceneManager.sceneLoaded -= ResultSceneLoadedOnPlay;
	}
}
