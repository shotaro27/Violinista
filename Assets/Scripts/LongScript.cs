using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongScript : MonoBehaviour
{
	public GameObject ParentNote { get; set; }
	public Notes ParentNoteScript { get; set; }
	public GameObject EndNote { get; set; }
	public Vector2 StartPosition { get; set; }
	Vector2 start, target;
	UIPointLine UIPointLine;
	public RectTransform ViolinPosition { get; set; }

	int _TapID;
	public int TapID
	{
		get { return _TapID; }
		set
		{
			_TapID = value;
			if (EndNote != null)
			{
				EndNote.GetComponent<Notes>().TapID = value;
			}
		}
	}
	private void Start()
	{
		UIPointLine = gameObject.GetComponent<UIPointLine>();
	}
	private void Update()
	{
		if (!ParentNoteScript.enabled && EndNote == null)
		{
			start = StartPosition - ViolinPosition.anchoredPosition + Vector2.right * GameObject.Find("GameArea").GetComponent<RectTransform>().rect.width / 2;
			target = UIPointLine.points[2];
			UIPointLine.posTEnd = 1;
			UIPointLine.points[0] = start;
			UIPointLine.points[1] = Notes.GetMiddlePoint(start, target);
		}
	}
}
