using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Netconnection : MonoBehaviour {

    //隨著螢幕解析度自動調整GUI大小
    //參考網站 https://godstamps.blogspot.com/2014/05/unityViewAutoScale.html
    public float baseWidth = 477;
    public float baseHeight = 848;
    private float m00;
    private float m11;

    //隨著螢幕解析度自動調整GUI大小
    private void Awake()
    {
        m00 = (float)Screen.width / 477;
        m11 = (float)Screen.height / 848;
    }

    // Use this for initialization
    void Start () {
		
	}

    private void OnGUI()
    {
        //隨著螢幕解析度自動調整GUI大小
        Matrix4x4 _matrix = GUI.matrix;
        _matrix.m00 = m00;
        _matrix.m11 = m11;
        GUI.matrix = _matrix;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.normal.textColor = Color.white;

        //返回按鈕
        GUILayout.BeginArea(new Rect(407, 0, 70, 40));
        if (GUILayout.Button("Back", buttonStyle))
        {
            UnityEngine.Networking.NetworkManagerHUDSRCFontSize.showGUI = false;
            SceneManager.LoadScene("MutipleStagePrepare");
        }
        GUILayout.EndArea();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
