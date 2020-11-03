using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{
    public int Count { get; private set; }
    Text CounterText;
    protected GameManager4 GameManager;
    protected MusicScore MusicData;
    protected virtual void Start()
    {
        Count = 0;
        CounterText = gameObject.GetComponent<Text>();
        CounterText.text = "0";
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager4>();
        MusicData = GameManager.Result.MusicData;
    }
    protected virtual void Set(int count)
    {
        Count = count;
        CounterText.text = $"{Count}";
    }
    protected void Add(int count) => Set(Count + count);
}
