using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingleStage : MonoBehaviour {

    //總關卡數
    private int finalstage = 3;

    //隨著螢幕解析度自動調整GUI大小
    //參考網站 https://godstamps.blogspot.com/2014/05/unityViewAutoScale.html
    //不過用此方法的話，GUI設定的位置盡量別用Screen.width等按螢幕大小調整的相對位置(因為GUI.matrix已調整一次了)
    public float baseWidth = 477;
    public float baseHeight = 848;
    private float m00;
    private float m11;

    //抓取當前關卡數
    Scene nowscene;
    string scenename;
    int nowstage;

    float timer_f;
    int timer_i;
    int starttime;

    int sec;
    int min;

    //隨著螢幕解析度自動調整GUI大小
    private void Awake()
    {
        m00 = (float)Screen.width / 477;
        m11 = (float)Screen.height / 848;
    }

    // Use this for initialization
    void Start () {
        //取得當前的scene
        nowscene = SceneManager.GetActiveScene();
        //抓取當前scene的名稱
        scenename = nowscene.name;
        //scene名稱格式固定為("StageXX")，用substring取第5個字元開始總長度scenename.Length - 5的子字串(可剛好取到關卡數字)，再轉換為int
        nowstage = int.Parse(scenename.Substring(5, scenename.Length - 5));
        //Debug.Log("Stage"+nowstage);
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

        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.fontSize = 22;
        windowStyle.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(200, 0, 85, 85));

        //計時器
        sec = (timer_i - starttime) % 60;
        min = (timer_i - starttime) / 60;
        GUILayout.Label(min.ToString().PadLeft(2, '0') + " ： " + sec.ToString().PadLeft(2, '0'), labelStyle);

        GUILayout.EndArea();

        //當球到達終點時
        if (Rotation.ballgoal)
        {
            GUILayout.Window(0, new Rect(70, 317, 345, 232), windowEvent, "Congratulations！", windowStyle);
        }
    }

    void windowEvent(int windowID)
    {

        //更新最新一關的關卡數
        if(nowstage >= PlayerPrefs.GetInt("laststage"))
        {
            PlayerPrefs.SetInt("laststage", nowstage + 1);
        }

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 22;
        labelStyle.normal.textColor = Color.white;
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.normal.textColor = Color.white;

        GUILayout.BeginVertical("box");

        //過關時間
        GUILayout.Label("Clear Time ： " + PlayerPrefs.GetString("username", "No Name") + "　　" + min + " ： " + sec.ToString().PadLeft(2, '0'), labelStyle);

        //抓取最佳紀錄
        string key_score = "stage_bestrecord_score[" + (nowstage - 1) + "]";
        string key_name = "stage_bestrecord_name[" + (nowstage - 1) + "]";
        //若存在最佳紀錄
        if (PlayerPrefs.HasKey(key_score))
        {
            //比較秒數
            if((timer_i - starttime) < PlayerPrefs.GetInt(key_score))
            {
                PlayerPrefs.SetInt(key_score, (timer_i - starttime));
                PlayerPrefs.SetString(key_name, PlayerPrefs.GetString("username", "No Name"));
            }
        }
        //不存在最佳紀錄（第一次破關）
        else
        {
            PlayerPrefs.SetInt(key_score, (timer_i - starttime));
            PlayerPrefs.SetString(key_name, PlayerPrefs.GetString("username", "No Name"));
        }

        //最佳時間
        GUILayout.Label("Best Time  ： " + PlayerPrefs.GetString(key_name, "No Name") + "　　" + (PlayerPrefs.GetInt(key_score) / 60) + " ： " + (PlayerPrefs.GetInt(key_score) % 60).ToString().PadLeft(2, '0'), labelStyle);

        //檢查是否存在下一關
        if ((nowstage + 1) <= finalstage)
        {
            //前往下一關
            if (GUILayout.Button("Next Stage", buttonStyle))
            {
                SceneManager.LoadScene("Stage" + (nowstage + 1));
            }
        }

        //回到關卡選擇
        if (GUILayout.Button("Stage Choose", buttonStyle))
        {
            SceneManager.LoadScene("SingleStageChoose");
        }

        GUILayout.EndVertical();
    }

    // Update is called once per frame
    void Update () {
        //總遊戲時間
        timer_f = Time.time;
        //關卡開始(球可以移動)時停止繼續變動starttime，(timer_i - starttime)即可得此關進行的時間
        //而在球到終點(球再度不能移動時)，starttime也同樣維持不動
        //球跳躍時因為會把ballcontrolstart變回false，故balljump也需作為判斷依據
        //--->唯有ballcontrolstart、ballgoal和balljump皆false才進入這if
        if (!Rotation.ballcontrolstart && !Rotation.ballgoal && !Rotation.balljump)
        {
            starttime = Mathf.FloorToInt(timer_f);
        }
        //球到終點時停止繼續變動timer_i
        //--->ballgoal是false進入這if
        if (!Rotation.ballgoal)
        {
            timer_i = Mathf.FloorToInt(timer_f);
        }     
        //Debug.Log(timer_i);
	}
}
