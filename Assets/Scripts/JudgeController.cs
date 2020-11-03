using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JudgeController : MonoBehaviour, IJudgable
{
    [SerializeField]
    GameObject JudgeText;
    [SerializeField]
    List<Sprite> JudgeSprites;
    GameObject JudgeObj;
    void Start()
    {
        
    }
    void Update()
    {
        
    }

    public void Judge(Judgement judgement)
	{
		if (JudgeObj == null)
        {
            JudgeObj = Instantiate(JudgeText, transform);
        }
		else
		{
            JudgeObj.GetComponent<Animator>().SetTrigger("Judge");
            JudgeObj.GetComponent<JudgeText>().DestroyTime = 0;
        }
        JudgeObj.GetComponent<Image>().sprite = JudgeSprites[(int)judgement];
    }
}
