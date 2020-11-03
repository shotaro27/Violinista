using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDif : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValueChanged(bool state)
	{
        GetComponent<Image>().color = state ? new Color(0.8f, 0.5f, 0) : new Color(0.5f, 0.3f, 0);
	}
}
