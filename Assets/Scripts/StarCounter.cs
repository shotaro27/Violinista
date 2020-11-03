using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class StarCounter : MonoBehaviour
{
    List<Animator> Star = new List<Animator>();
	internal int Count { get; private set; }
	void Start()
    {
        foreach (RectTransform star in transform) Star.Add(star.gameObject.GetComponent<Animator>());
        Count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Count = Star.Where(s => !s.GetCurrentAnimatorStateInfo(0).IsName("White")).Count();
    }
    public void Set(float ScoreLevel)
	{
        for (int i = 0; i < Star.Count; i++)
        {
			if (ScoreLevel >= (i + 1) / 3f && Star[i].GetCurrentAnimatorStateInfo(0).IsName("White"))
            {
                Star[i].SetBool("star", true);
            }
        }
    }
}
