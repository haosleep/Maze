using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.Networking;

public class NetStage : NetworkBehaviour
{
    //隨著螢幕解析度自動調整GUI大小
    //參考網站 https://godstamps.blogspot.com/2014/05/unityViewAutoScale.html
    public float baseWidth = 477;
    public float baseHeight = 848;
    private float m00;
    private float m11;

    //儲存給server端用的迷宮起點／終點／牆壁
    //int[] server_wall = new int[20];
    private SyncListInt server_wall = new SyncListInt();
    [SyncVar]
    private int server_start = 0;
    [SyncVar]
    private int server_goal = 0;
    [SyncVar]
    private bool server_getready = false;

    //儲存給client端用的迷宮起點／終點／牆壁
    //SyncVar不能同步陣列
    //[SyncVar]
    //int[] client_wall = new int[20];
    private SyncListInt client_wall = new SyncListInt();
    [SyncVar]
    private int client_start = 0;
    [SyncVar]
    private int client_goal = 0;
    [SyncVar]
    private bool client_getready = false;

    public bool game_start = false;

    //地圖上的起點
    GameObject StartPoint_OnMap;
    //地圖上的終點
    GameObject GoalPoint_OnMap;

    GameObject[] choosewall_setwall = new GameObject[60];

    //GameObject Ball_OnMap;

    float timer_f;
    int timer_i;
    int starttime;

    int sec;
    int min;

    //server端已到終點
    [SyncVar]
    private bool server_clear = false;
    [SyncVar]
    private int server_time = 0;
    //client端已到終點
    [SyncVar]
    private bool client_clear = false;
    [SyncVar]
    private int client_time = 0;

    private int windowState;
    public const int NOT_CLEAR = 0;
    public const int YOU_CLEAR = 1;
    public const int ALL_CLEAR = 2;

    private void Awake()
    {
        m00 = (float)Screen.width / 477;
        m11 = (float)Screen.height / 848;

        StartPoint_OnMap = GameObject.Find("Start");
        GoalPoint_OnMap = GameObject.Find("Goal");
        //Ball_OnMap = GameObject.Find("Ball");

        for (int i = 0; i < 60; i++)
        {
            //找出i對應的牆壁數字
            int wall_number = (i / 30 + 1) * 100 + (i % 30 / 6 + 1) * 10 + (i % 30 % 6 + 1);
            choosewall_setwall[i] = GameObject.Find(wall_number + "SetWall");
        }
    }

    // Use this for initialization
    void Start () {
        transform.position = new Vector3(-6f, 1.12f, 0.42f);
        server_getready = false;
        client_getready = false;
        game_start = false;
        server_clear = false;
        client_clear = false;

        windowState = NOT_CLEAR;

        //server端將設置的maze傳給client端
        if (isServer)
        {
            CmdGetClientDate();
        }
        //client端將設置的maze傳給server端
        else
        {
            server_start = PlayerPrefs.GetInt("SetStartPoint", 0);
            Debug.Log(server_start);
            server_goal = PlayerPrefs.GetInt("SetGoalPoint", 0);
            Debug.Log(server_goal);
            for (int i = 0; i < 60; i++)
            {
                if (PlayerPrefs.GetInt("SetWall[" + i + "]", 0) == 1)
                {
                    //server_wall[server_i] = i;
                    //server_i++;
                    server_wall.Add(i);
                }
            }
            CmdSetMaze(server_start, server_goal);
            for(int i = 0; i < server_wall.Count; i++)
            {
                CmdSetWall(server_wall[i]);
            }

            //確認迷宮資訊已經傳送完了才開始下一步動作
            CmdReady();
            Debug.Log("client start over");
        }
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
        if (NetRotation.ballgoal)
        {
            GUILayout.Window(0, new Rect(70, 317, 345, 232), windowEvent, "Finish！", windowStyle);
        }
    }

    void windowEvent(int windowID)
    {
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 22;
        labelStyle.normal.textColor = Color.white;
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.normal.textColor = Color.white;

        GUILayout.BeginVertical("box");
        switch (windowState)
        {
            case NOT_CLEAR:
                break;
            case YOU_CLEAR:
                GUILayout.Label("Clear Time ： " + min + " ： " + sec.ToString().PadLeft(2, '0'), labelStyle);
                GUILayout.Label("wait for opponent");
                if (GUILayout.Button("Exit", buttonStyle))
                {
                    SceneManager.LoadScene("MutipleConnectionPrepare");
                }
                break;
            case ALL_CLEAR:
                if (isServer)
                {
                    GUILayout.Label(" Clear  Time ： " + min + " ： " + sec.ToString().PadLeft(2, '0'), labelStyle);
                    GUILayout.Label("Opponent Timr： " + (client_time/60) + " ： " + (client_time%60).ToString().PadLeft(2, '0'), labelStyle);
                    if(server_time < client_time)
                    {
                        GUILayout.Label("You Win！！");
                    }else if (server_time > client_time)
                    {
                        GUILayout.Label("You Lose！！");
                    }
                    else
                    {
                        GUILayout.Label("Flat！！");
                    }
                    if (GUILayout.Button("Exit", buttonStyle))
                    {
                        SceneManager.LoadScene("MutipleConnectionPrepare");
                    }
                }
                else
                {
                    GUILayout.Label(" Clear  Time ： " + min + " ： " + sec.ToString().PadLeft(2, '0'), labelStyle);
                    GUILayout.Label("Opponent Timr： " + (server_time / 60) + " ： " + (server_time % 60).ToString().PadLeft(2, '0'), labelStyle);
                    if (client_time < server_time)
                    {
                        GUILayout.Label("You Win！！");
                    }
                    else if (client_time > server_time)
                    {
                        GUILayout.Label("You Lose！！");
                    }
                    else
                    {
                        GUILayout.Label("Flat！！");
                    }
                    if (GUILayout.Button("Exit", buttonStyle))
                    {
                        SceneManager.LoadScene("MutipleConnectionPrepare");
                    }
                }
                break;
        }
        GUILayout.EndVertical();
    }

    // Update is called once per frame
    void Update () {
        if (!game_start)
        {
            if (isServer)
            {
                Debug.Log("server set ready");
                if (server_getready)
                {
                    //設置起點和球
                    float s_x = -3.6f + (server_start - 10) / 10 * 1.44f;
                    float s_z = -3.6f + (server_start - 1) % 10 * 1.44f;
                    StartPoint_OnMap.transform.position = new Vector3(s_x, 0.005f, s_z);
                    //Ball_OnMap.transform.position = new Vector3(s_x, 17f, s_z);
                    transform.position = new Vector3(s_x, 17f, s_z);
                    //設置終點
                    float g_x = -3.6f + (server_goal - 10) / 10 * 1.44f;
                    float g_z = -3.6f + (server_goal - 1) % 10 * 1.44f;
                    GoalPoint_OnMap.transform.position = new Vector3(g_x, 0.005f, g_z);
                    //設置牆
                    for(int i = 0; i < server_wall.Count; i++)
                    {
                        float x = choosewall_setwall[server_wall[i]].transform.position.x;
                        float z = choosewall_setwall[server_wall[i]].transform.position.z;
                        choosewall_setwall[server_wall[i]].transform.position = new Vector3(x, 0.93f, z);
                    }
                    game_start = true;
                }
                Debug.Log("server set over");
            }
            else
            {
                Debug.Log("client set ready");
                if (client_getready)
                {
                    //設置起點和球
                    float s_x = -3.6f + (client_start - 10) / 10 * 1.44f;
                    float s_z = -3.6f + (client_start - 1) % 10 * 1.44f;
                    StartPoint_OnMap.transform.position = new Vector3(s_x, 0.005f, s_z);
                    //Ball_OnMap.transform.position = new Vector3(s_x, 17f, s_z);
                    transform.position = new Vector3(s_x, 17f, s_z);
                    //設置終點
                    float g_x = -3.6f + (client_goal - 10) / 10 * 1.44f;
                    float g_z = -3.6f + (client_goal - 1) % 10 * 1.44f;
                    GoalPoint_OnMap.transform.position = new Vector3(g_x, 0.005f, g_z);
                    //設置牆
                    for (int i = 0; i < client_wall.Count; i++)
                    {
                        float x = choosewall_setwall[client_wall[i]].transform.position.x;
                        float z = choosewall_setwall[client_wall[i]].transform.position.z;
                        choosewall_setwall[client_wall[i]].transform.position = new Vector3(x, 0.93f, z);
                    }
                    game_start = true;
                }
                //Client連線後沒有取得Server的資料的情況下，讓Server端再重新執行一次資料設置
                else
                {
                    CmdGetClientDate();
                }
                Debug.Log("client set over");
            }
        }
        //總遊戲時間
        timer_f = Time.time;
        //關卡開始(球可以移動)時停止繼續變動starttime，(timer_i - starttime)即可得此關進行的時間
        //而在球到終點(球再度不能移動時)，starttime也同樣維持不動
        //--->唯有ballcontrolstart、ballgoal皆false才進入這if
        if (!NetRotation.ballcontrolstart && !NetRotation.ballgoal)
        {
            starttime = Mathf.FloorToInt(timer_f);
        }
        //球到終點時停止繼續變動timer_i
        //--->ballgoal是false進入這if
        if (!NetRotation.ballgoal)
        {
            timer_i = Mathf.FloorToInt(timer_f);
        }
        //到達終點
        if (NetRotation.ballgoal && windowState != ALL_CLEAR)
        {
            if (isServer)
            {
                server_time = (timer_i - starttime);
                windowState = YOU_CLEAR;
                server_clear = true;
                if (client_clear)
                {
                    windowState = ALL_CLEAR;
                }
            }
            else
            {
                client_time = (timer_i - starttime);
                CmdTime(client_time);
                windowState = YOU_CLEAR;
                client_clear = true;
                CmdClear();
                if (server_clear)
                {
                    windowState = ALL_CLEAR;
                }
            }
        }
    }

    [Command]
    void CmdGetClientDate()
    {
        client_start = PlayerPrefs.GetInt("SetStartPoint", 0);
        Debug.Log(client_start);
        client_goal = PlayerPrefs.GetInt("SetGoalPoint", 0);
        Debug.Log(client_goal);
        for (int i = 0; i < 60; i++)
        {
            if (PlayerPrefs.GetInt("SetWall[" + i + "]", 0) == 1)
            {
                //client_wall[client_i] = i;
                //client_i++;
                client_wall.Add(i);
            }
        }

        //確認迷宮資訊已經傳送完了才開始下一步動作
        client_getready = true;
        Debug.Log("server start over");
    }

    [Command]
    void CmdSetMaze(int s, int g)
    {
        RpcSetMaze(s, g);
    }
    [ClientRpc]
    void RpcSetMaze(int s, int g)
    {
        server_start = s;
        server_goal = g;
        Debug.Log("CmdSetMaze" + s + "  " + g);
    }

    [Command]
    void CmdSetWall(int w)
    {
        RpcSetWall(w);
    }
    [ClientRpc]
    void RpcSetWall(int w)
    {
        server_wall.Add(w);
        Debug.Log("CmdSetWall"+ server_wall.Count + " " + w);
    }

    [Command]
    void CmdReady()
    {
        RpcReady();
    }
    [ClientRpc]
    void RpcReady()
    {
        server_getready = true;
        Debug.Log("CmdReady");
    }

    [Command]
    void CmdTime(int t)
    {
        RpcTime(t);
    }
    [ClientRpc]
    void RpcTime(int t)
    {
        client_time = t;
    }

    [Command]
    void CmdClear()
    {
        client_clear = true;
        RpcClear();
    }
    [ClientRpc]
    void RpcClear()
    {
        client_clear = true;
    }
}
