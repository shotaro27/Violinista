using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteSizeController : NoteController
{
	protected override string ToStringFloat(float value) => $"{ToValueFloat(value) * 100}%";
	protected override float ToValueFloat(float value) => Mathf.Round(value * 100) / 100;
}
