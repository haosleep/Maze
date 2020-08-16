using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testposition : MonoBehaviour {

    private GameObject ball;

	// Use this for initialization
	void Start () {
        ball = GameObject.Find("Ball");
	}

    private void OnGUI()
    {
        GUILayout.Label("球的位置：" + ball.transform.position);
        GUILayout.Label("螢幕傾度：" + Input.acceleration);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
