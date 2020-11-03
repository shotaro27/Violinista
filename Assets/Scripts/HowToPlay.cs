using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HowToPlay : MonoBehaviour
{
    [SerializeField]
    List<Sprite> HowToPlayPagesRhythm;
    [SerializeField]
    List<Sprite> HowToPlayPagesViolin;
    [SerializeField]
    Sprite SelectSprite;
    [SerializeField]
    Image Page;
    int NowPage;
    bool Select;
    internal int Mode;
    void Start()
    {
        NowPage = 0;
        Select = false;
        Page.sprite = Mode == 0 ? HowToPlayPagesRhythm[0] : HowToPlayPagesViolin[0];
        FinishPage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SwitchPage()
	{
		if (Select)
		{
            SceneManager.UnloadSceneAsync("HowToPlay");
        }
        else
        {
            NowPage++;
            Page.sprite = Mode == 0 ? HowToPlayPagesRhythm[NowPage] : HowToPlayPagesViolin[NowPage];
            FinishPage();
        }
	}
    void FinishPage()
	{
        if (NowPage == (Mode == 0 ? HowToPlayPagesRhythm.Count : HowToPlayPagesViolin.Count) - 1)
        {
            Select = true;
            GetComponent<Image>().sprite = SelectSprite;
        }
    }
}
