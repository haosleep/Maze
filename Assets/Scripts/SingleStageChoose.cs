using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingleStageChoose : MonoBehaviour {

    //總關卡數
    private int finalstage = 3;

    //隨著螢幕解析度自動調整GUI大小
    //參考網站 https://godstamps.blogspot.com/2014/05/unityViewAutoScale.html
    public float baseWidth = 477;
    public float baseHeight = 848;
    private float m00;
    private float m11;

    //使用者名稱(playerprefs)
    private string username;

    //最新一關的關卡數(playerprefs)
    private int laststage;

    //過關數據記錄(playerprefs)
    private string [] stage_bestrecord_name;
    private int[] stage_bestrecord_score;

    Vector2 scrollposition;

    //隨著螢幕解析度自動調整GUI大小
    private void Awake()
    {
        m00 = (float)Screen.width / 477;
        m11 = (float)Screen.height / 848;
    }

    // Use this for initialization
    void Start () {
        laststage = PlayerPrefs.GetInt("laststage", 1);
	}

    private void OnGUI()
    {
        //隨著螢幕解析度自動調整GUI大小
        Matrix4x4 _matrix = GUI.matrix;
        _matrix.m00 = m00;
        _matrix.m11 = m11;
        GUI.matrix = _matrix;

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 22;
        labelStyle.normal.textColor = Color.white;
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.normal.textColor = Color.white;
        GUIStyle textfieldStyle = new GUIStyle(GUI.skin.textField);
        textfieldStyle.fontSize = 22;
        textfieldStyle.normal.textColor = Color.white;


        //初次進入遊戲時的預設名稱
        username = PlayerPrefs.GetString("username", "No Name");

        //牆內範圍
        //寬50----435  長85----790
        GUILayout.BeginArea(new Rect(50, 85, 380, 705));

        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Enter your name：", labelStyle);
        username = GUILayout.TextField(username, 8, textfieldStyle);
        PlayerPrefs.SetString("username", username);
        GUILayout.EndHorizontal();

        scrollposition = GUILayout.BeginScrollView(scrollposition, GUILayout.Width(385), GUILayout.Height(740));
        int i;
        for(i = 1; i < laststage; i++)
        {
            GUILayout.BeginHorizontal("box");
            string key_name = "stage_bestrecord_name[" + (i - 1) + "]";
            string key_score = "stage_bestrecord_score[" + (i - 1) + "]";
            int sec = PlayerPrefs.GetInt(key_score) % 60;
            int min = PlayerPrefs.GetInt(key_score) / 60;
            // Stage 1 ： XXX  00 ： 00
            GUILayout.Label("Stage " + i + " ：　" + PlayerPrefs.GetString(key_name, "No Name") + "　　" + min + " ： " + sec.ToString().PadLeft(2, '0'), labelStyle);
            if (GUILayout.Button("Retry", buttonStyle))
            {
                SceneManager.LoadScene("Stage" + i);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal("box");
        if (i <= finalstage)
        {
            GUILayout.Label("Stage " + i + " ：　", labelStyle);
            if (GUILayout.Button("Challenge", buttonStyle))
            {
                SceneManager.LoadScene("Stage" + i);
            }
        }
        else
        {
            GUILayout.Label("Stage " + i + " ：　Coming soon...", labelStyle);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();

        GUILayout.EndArea();


        //回主畫面用按鈕
        GUILayout.BeginArea(new Rect(407, 0, 70, 40));
        if (GUILayout.Button("Back", buttonStyle))
        {
            SceneManager.LoadScene("SampleScene");
        }
        GUILayout.EndArea();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
