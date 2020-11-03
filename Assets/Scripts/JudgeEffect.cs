using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeEffect : MonoBehaviour, IJudgable
{
	Animator Anim;
	void Start()
	{
		Anim = GetComponent<Animator>();
	}

	public void Judge(Judgement judgement)
	{
		switch (judgement)
		{
			case Judgement.Just:
				Anim.SetBool("Just", true);
				Destroy(gameObject, 0.5f);
				break;
			case Judgement.Early:
			case Judgement.Late:
				Anim.SetBool("Hit", true);
				Destroy(gameObject, 0.5f);
				break;
			case Judgement.Miss:
				Debug.Log("Miss");
				Destroy(gameObject);
				break;
		}
	}
}
