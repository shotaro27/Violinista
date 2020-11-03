using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteController : MonoBehaviour
{
	internal float Value { get; set; }
	public void SetSlide()
	{
		GetComponent<Slider>().value = Value;
	}
	public void OnValueChanged(float value)
	{
		Value = ToValueFloat(value);
		transform.GetChild(0).GetComponent<Text>().text = ToStringFloat(value);
	}
	protected virtual float ToValueFloat(float value) => value;
	protected virtual string ToStringFloat(float value) => value.ToString();
}
