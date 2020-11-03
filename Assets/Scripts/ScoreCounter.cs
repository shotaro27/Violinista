using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ScoreCounter : Counter, IJudgable
{
    [SerializeField]
    Image Bar;
    [SerializeField]
    ComboCounter Combo;
	[SerializeField]
	StarCounter StarCounter;
	float ScoreLevel;
	public void Judge(Judgement judge) => Add(CalScoreDelta(Combo.Count) * (4 - (int)judge));
    int CalScoreDelta(int count) => count * 10 + 1000;
	protected override void Set(int count)
	{
		base.Set(count);
        Bar.fillAmount = ScoreLevel = Mathf.Min(1,
			(float)Count / (MusicData.NotesCount * 4000));
		StarCounter.Set(ScoreLevel);
	}
}
