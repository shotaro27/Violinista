using UnityEngine;

public class ComboCounter : Counter
{
	public void AddCombo()
	{
		Add(1);
		GetComponent<Animator>().SetTrigger("Count");
	}
    public void ResetCombo() => Set(0);
	protected override void Start()
	{
		base.Start();
	}
}
