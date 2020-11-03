using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class TitleTap : MonoBehaviour
{
	[SerializeField]
	AudioClip TapPiano;
	[SerializeField]
	AudioSource Song;
	bool SceneLoad = false;
	float t = 0;
	string SceneName;
	[SerializeField]
	Image BackBlack;
	[SerializeField]
	float SceneTime;
	[SerializeField]
	bool IsAnimation;
	public void LoadScene(string SceneName)
	{
		GetComponent<AudioSource>().PlayOneShot(TapPiano);
		this.SceneName = SceneName;
		SceneLoad = true;
		if (Song.isPlaying) Song.Stop();
		if (IsAnimation) BackBlack.gameObject.transform.parent.GetComponent<Animator>().SetTrigger("End");
		if (!BackBlack.enabled) BackBlack.enabled = true;
	}
	public void Update()
	{
		if (SceneLoad)
		{
			t += Time.deltaTime;
			if (!IsAnimation) BackBlack.color = new Color(0, 0, 0, Mathf.Min(1, t));
		}
		if (t >= SceneTime)
			SceneManager.LoadScene(SceneName);
	}
}
