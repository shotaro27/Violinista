using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopStart : MonoBehaviour
{
	[SerializeField] Animator FromTopToMode;
	[SerializeField] Animator TapToStart;
    public void OnClick(bool Set)
	{
		if (TapToStart != null) TapToStart.enabled = false;
		FromTopToMode.SetBool("Top", Set);
		GetComponent<AudioSource>().Play();
	}
	public void ModeStart()
	{
		if (TapToStart != null) TapToStart.enabled = false;
		FromTopToMode.enabled = false;
		GameObject.Find("Modes").GetComponent<RectTransform>().anchoredPosition += Vector2.left * (1200 - 350);
		GameObject.Find("Title").GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 0);
		GameObject.Find("TapToStart").GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 0);
		gameObject.SetActive(false);
	}
}
