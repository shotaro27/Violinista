using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteSpeedController : NoteController
{
	protected override string ToStringFloat(float value) => ((int)value).ToString();
}
