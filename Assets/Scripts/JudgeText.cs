using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeText : MonoBehaviour
{
	public float DestroyTime { get; set; }
	void Start()
    {
        DestroyTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        DestroyTime += Time.deltaTime;
		if (DestroyTime > 0.8f) Destroy(gameObject);
    }
}
