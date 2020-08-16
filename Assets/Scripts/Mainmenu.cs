//內含給電腦測試用的code (已註解)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour {

    //紀錄手指觸碰位置
    Vector2 m_screenPos = new Vector2();

    //遊戲介面狀態

    //主畫面
    public const int STATE_MAINMENU = 0;
    //單人遊戲介面
    public const int STATE_SINGLEGAME = 1;
    //連線遊戲介面
    public const int STATE_NETGAME = 2;

    //當前遊戲狀態
    private int gameState;

    //按鍵音效
    public AudioSource clickaudio;

    // Use this for initialization
    void Start () {
        //初始化遊戲狀態為主畫面
        gameState = STATE_MAINMENU;

        //不允許多點觸碰
        Input.multiTouchEnabled = false;

        clickaudio = gameObject.GetComponent<AudioSource>();
    }

    private void OnGUI()
    {
        switch (gameState)
        {
            case STATE_SINGLEGAME:
                SceneManager.LoadScene("SingleStageChoose");
                break;
            case STATE_NETGAME:
                SceneManager.LoadScene("MutipleStagePrepare");
                break;
        }
        //-----電腦測試用-----
        /*GUILayout.BeginArea(new Rect(0, 0, 200, 200));
        if (GUILayout.Button("Test"))
        {
            SceneManager.LoadScene("MutipleStagePrepare");
        }
        GUILayout.EndArea();*/
        //-----電腦測試用-----
    }

    // Update is called once per frame
    void Update () {
        //手指觸碰螢幕
        if (Input.touches[0].phase == TouchPhase.Began)
        {
            //紀錄觸碰位置
            m_screenPos = Input.touches[0].position;

            //設置射線
            Ray ray = Camera.main.ScreenPointToRay(m_screenPos);
            RaycastHit hit;
            if(Physics.Raycast(ray,out hit))
            {
                //射線碰觸到了按鈕（玩家點擊了按鈕）
                if(hit.collider.name == "SingleButton")
                {
                    clickaudio.Play();
                    gameState = STATE_SINGLEGAME;
                }
                if (hit.collider.name == "NetButton")
                {
                    clickaudio.Play();
                    gameState = STATE_NETGAME;
                }
            }
        }
    }
}
