//內含給電腦測試用的code (已註解)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MutipleStagePrepare : MonoBehaviour {

    //紀錄手指觸碰位置
    Vector2 m_screenPos = new Vector2();

    //隨著螢幕解析度自動調整GUI大小
    //參考網站 https://godstamps.blogspot.com/2014/05/unityViewAutoScale.html
    public float baseWidth = 477;
    public float baseHeight = 848;
    private float m00;
    private float m11;

    //是否正在選擇設置起點/終點
    bool ChooseStartPoint = false;
    bool ChooseGoalPoint = false;

    //起點/終點設置位置(playerprefs)
    int SetStartPoint;
    int SetGoalPoint;
    //地圖上的起點
    GameObject StartPoint_OnMap;
    //地圖上的終點
    GameObject GoalPoint_OnMap;

    GameObject[] choosewall_setwall = new GameObject[60];

    //可設置牆壁數(playerprefs)
    int SetWallTotal;

    //設置牆壁與否(playerprefs)
    //順序：
    //０～２９ 橫牆 左上到右下
    //３０～５９ 直牆 左上到右下
    int[] SetWall = new int[60];

    private int checkState;
    public const int NOT_CHECK = 0;
    public const int LACK_STARTPOINT = 1;
    public const int LACK_GOALPOINT = 2;
    public const int MAZE_INVALID = 3;
    public const int MAZE_OK = 4;

    //隨著螢幕解析度自動調整GUI大小
    private void Awake()
    {
        m00 = (float)Screen.width / 477;
        m11 = (float)Screen.height / 848;

        //--電腦測試用的初始值，正式用時可去除--
        /*PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("SetStartPoint", 33);
        PlayerPrefs.SetInt("SetGoalPoint", 44);
        PlayerPrefs.SetInt("SetWall[8]", 1);
        PlayerPrefs.SetInt("SetWall[9]", 1);
        PlayerPrefs.SetInt("SetWall[10]", 1);
        PlayerPrefs.SetInt("SetWall[38]", 1);
        PlayerPrefs.SetInt("SetWall[56]", 1);
        PlayerPrefs.SetInt("SetWall[14]", 1);
        PlayerPrefs.SetInt("SetWall[15]", 1);
        PlayerPrefs.SetInt("SetWall[45]", 1);
        PlayerPrefs.SetInt("SetWall[57]", 1);
        PlayerPrefs.SetInt("SetWall[21]", 1);
        PlayerPrefs.SetInt("SetWall[22]", 1);
        PlayerPrefs.SetInt("SetWallTotal", 9);*/
        //--------------------------------------

        //這些放在Start會沒有作用，要放在Awake
        //抓取地圖上的起點和終點
        StartPoint_OnMap = GameObject.Find("StartPoint_OnMap");
        GoalPoint_OnMap = GameObject.Find("GoalPoint_OnMap");
        SetStartPoint = PlayerPrefs.GetInt("SetStartPoint", 0);
        SetGoalPoint = PlayerPrefs.GetInt("SetGoalPoint", 0);

        //如果起點位置已有設置
        //讓地圖上的起點和終點移動到相對應的位置表示其設置的位置
        //位置數字十位數表第幾橫列，影響x值 -3.6~3.6 間隔1.44
        //位置數字個位數表第幾直行，影響z值
        if (SetStartPoint != 0)
        {
            float x = -3.6f + (SetStartPoint - 10) / 10 * 1.44f;
            float z = -3.6f + (SetStartPoint - 1) % 10 * 1.44f;
            StartPoint_OnMap.transform.position = new Vector3(x, 0.005f, z);
        }
        if (SetGoalPoint != 0)
        {
            float x = -3.6f + (SetGoalPoint - 10) / 10 * 1.44f;
            float z = -3.6f + (SetGoalPoint - 1) % 10 * 1.44f;
            GoalPoint_OnMap.transform.position = new Vector3(x, 0.005f, z);
        }

        for(int i = 0; i < 60; i++)
        {
            //找出i對應的牆壁數字
            int wall_number = (i / 30 + 1) * 100 + (i % 30 / 6 + 1) * 10 + (i % 30 % 6 + 1);
            choosewall_setwall[i] = GameObject.Find(wall_number + "SetWall");
        }
    }

    // Use this for initialization
    void Start () {
        //不允許多點觸碰
        Input.multiTouchEnabled = false;

        checkState = NOT_CHECK;

        ChooseStartPoint = false;
        ChooseGoalPoint = false;

        SetWallTotal = PlayerPrefs.GetInt("SetWallTotal", 20);

        for (int i = 0; i < 60; i++)
        {
            SetWall[i] = PlayerPrefs.GetInt("SetWall[" + i + "]", 0);
            //如果SetWall[i]是1，表示是之前所設置的牆
            if (SetWall[i] == 1)
            {
                choosewall_setwall[i].GetComponent<Renderer>().enabled = true;
            }
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
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 22;
        buttonStyle.normal.textColor = Color.white;

        GUILayout.BeginArea(new Rect(45, 85, 400, 120));

        GUILayout.Label("Set walls, start point and goal point \nfor your map.", labelStyle);

        GUILayout.Label("The number which you can set walls ：" + SetWallTotal, labelStyle);

        GUILayout.EndArea();


        GUILayout.BeginArea(new Rect(220, 670, 220, 120));
        if (GUILayout.Button("OK", buttonStyle))
        {
            checkState = NOT_CHECK;
            //未設置起點
            if (PlayerPrefs.GetInt("SetStartPoint", 0) == 0)
            {
                checkState = LACK_STARTPOINT;
            }
            else
            {
                //未設置終點
                if (PlayerPrefs.GetInt("SetGoalPoint", 0) == 0)
                {
                    checkState = LACK_GOALPOINT;
                }
                else
                {
                    //檢查迷宮是否成立（起點和終點有連通）
                    int result = MazeCheck();
                    if(result == 1)
                    {
                        checkState = MAZE_OK;
                        SceneManager.LoadScene("MutipleConnectionPrepare");
                    }
                    else
                    {
                        checkState = MAZE_INVALID;
                    }
                }
            }
        }
        switch (checkState)
        {
            case NOT_CHECK:
                GUILayout.Label("", labelStyle);
                break;
            case LACK_STARTPOINT:
                GUILayout.Label("StartPoint isn't setting.", labelStyle);
                break;
            case LACK_GOALPOINT:
                GUILayout.Label("GoalPoint isn't setting.", labelStyle);
                break;
            case MAZE_INVALID:
                GUILayout.Label("Maze is invalid.", labelStyle);
                break;
            //Debug用，實際運作時因會切換畫面故此行結果正常來講是看不到的
            case MAZE_OK:
                GUILayout.Label("OK.", labelStyle);
                break;
        }       
        GUILayout.EndArea();


        //回主畫面用按鈕
        GUILayout.BeginArea(new Rect(407, 0, 70, 40));
        if (GUILayout.Button("Back", buttonStyle))
        {
            SceneManager.LoadScene("SampleScene");
        }
        GUILayout.EndArea();
    }

    int MazeCheck()
    {
        int scan_area = 0;
        //先從橫排開始由左往右檢測，把每一格給予scan_area的數值，檢測到左邊有牆就把scan_area++
        //換下一排時scan_area自動+1
        //ex：
        //∥1∥2｜2∥3｜3｜3∥
        //∥4｜....
        //相同數字就表示這些格子是相連通的

        //橫向檢測完後再檢查直行
        //若直的相鄰兩格中間沒有牆
        //就讓數字變得相同(以上圖為例，就是所有４的格子都變成１)

        //最後再看startpoint和goalpoint在的格子數字是否一樣即可

        //用來儲存每一格的值
        //對應號碼：
        //0～5
        //6～11
        //...
        //30～35
        int[] area = new int[36];

        //橫排檢測
        for(int row = 1; row < 7; row++)
        {
            scan_area++;
            area[(row - 1) * 6] = scan_area;
            for (int column = 2; column < 7; column++)
            {
                //直排牆的編號：
                //30 36 42 48 54
                //31 37 ...
                //...
                //35 41 47 53 59

                //如果有牆即表示兩格被隔開，scan_area++
                int c_num = 30 + (row - 1) + (6 * (column - 2));
                if (PlayerPrefs.GetInt("SetWall[" + c_num + "]", 0) == 1){
                    scan_area++;
                }
                area[(row - 1) * 6 + (column - 1)] = scan_area;
            }
        }

        //直行檢測
        for (int column = 1; column < 7; column++)
        {
            for(int row = 2; row < 7; row++)
            {
                //橫排牆的編號：
                //0 1 2 3 4 5
                //6 7 8 ...
                //...
                //24 25 26 27 28 29

                //先檢查上下兩格數字是否已經一致
                //ex：(上下無牆)
                //∥１｜１｜...
                //∥４｜４｜...
                //此例中，第一直行檢查完後，4都會變成1
                //而在檢查第二直行時，就可以不用再檢測一遍
                if(area[(column - 1) + (6 * (row - 2))] != area[(column - 1) + (6 * (row - 1))])
                {
                    //如果沒有牆就表示兩邊相通，讓下面的數字等於上面的數字(先轉換相同數字的其他格)
                    int c_num = (column - 1) + (6 * (row - 2));
                    if (PlayerPrefs.GetInt("SetWall[" + c_num + "]", 0) == 0)
                    {
                        //(先轉換其他格)
                        for(int a = 0; a < 36; a++)
                        {
                            //此格並不是正在比較中的兩格
                            if(a != (column - 1) + (6 * (row - 2)) && a != (column - 1) + (6 * (row - 1)))
                            {
                                //若此格跟正在比較的『下面』那格數字相同
                                //將此格的數字變成正在比較的『上面』那格
                                if (area[a] == area[(column - 1) + (6 * (row - 1))])
                                {
                                    area[a] = area[(column - 1) + (6 * (row - 2))];
                                }
                            }
                        }
                        //最後再轉換比較中的下面格
                        area[(column - 1) + (6 * (row - 1))] = area[(column - 1) + (6 * (row - 2))];
                    }
                }
            }
        }

        //將儲存的數字格式轉換為檢測用的格式
        int check_startpoint = PlayerPrefs.GetInt("SetStartPoint", 0);
        check_startpoint = (check_startpoint / 10 - 1) * 6 + (check_startpoint % 10 - 1);
        int check_goalpoint = PlayerPrefs.GetInt("SetGoalPoint", 0);
        check_goalpoint = (check_goalpoint / 10 - 1) * 6 + (check_goalpoint % 10 - 1);

        if(area[check_startpoint] == area[check_goalpoint])
        {
            return 1;
        }
        else
        {
            return 0;
        }    
    }

    // Update is called once per frame
    void Update () {
        //手指觸碰螢幕
        if (Input.touches[0].phase == TouchPhase.Began)
        {
            //紀錄觸碰位置
            m_screenPos = Input.touches[0].position;

            //選擇起點的紅框
            var choose_start = GameObject.Find("ChooseStart");
            //選擇終點的紅框
            var choose_goal = GameObject.Find("ChooseGoal");

            //設置射線
            Ray ray = Camera.main.ScreenPointToRay(m_screenPos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //點選起點
                if (hit.collider.name == "Start")
                {
                    //若是還沒選擇的狀態就改為選擇／若是已選擇的狀態則改為取消
                    ChooseStartPoint = !ChooseStartPoint;
                    //選擇紅框顯示狀況即對應起點選擇
                    choose_start.GetComponent<Renderer>().enabled = ChooseStartPoint;
                    //如果終點正在選擇的情況選擇了起點，就把終點的選擇取消掉（避免起點和終點都保持同時選擇的狀態）
                    ChooseGoalPoint = false;
                    choose_goal.GetComponent<Renderer>().enabled = false;
                }
                if (hit.collider.name == "Goal")
                {
                    ChooseGoalPoint = !ChooseGoalPoint;                   
                    choose_goal.GetComponent<Renderer>().enabled = ChooseGoalPoint;

                    ChooseStartPoint = false;
                    choose_start.GetComponent<Renderer>().enabled = false;
                }
                if (hit.collider.tag == "ChooseWall")
                {
                    //抓取點選的位置（檔案命名方式：XXXchoosewall）
                    string choosewall_number = hit.collider.name.Substring(0, 3);
                    //牆編號
                    //首數字1為橫牆、2為直牆
                    //第二數字表第幾橫/直列牆
                    //第三數字為第幾道牆
                    //故編號各減一，第一數*30 + 第二數*6 + 第三數就能變為code
                    //code順序：
                    //０～２９ 橫牆 左上到右下
                    //３０～５９ 直牆 左上到右下
                    int choosewall_code = (int.Parse(choosewall_number.Substring(0, 1)) - 1) * 30 + (int.Parse(choosewall_number.Substring(1, 1)) - 1) * 6 + (int.Parse(choosewall_number.Substring(2, 1)) - 1);
                    //如果此處沒設牆
                    if (SetWall[choosewall_code] == 0)
                    {
                        //如果可設置的牆壁數量還有剩
                        if(SetWallTotal > 0)
                        {
                            //將牆壁顯示表示設置
                            choosewall_setwall[choosewall_code].GetComponent<Renderer>().enabled = true;
                            //可設置牆壁數量減一，並將新的數字存進SetWallTotal
                            SetWallTotal = SetWallTotal - 1;
                            PlayerPrefs.SetInt("SetWallTotal", SetWallTotal);
                            //對應的SetWall改成1表示設牆，並存進SetWall
                            SetWall[choosewall_code] = 1;
                            PlayerPrefs.SetInt("SetWall[" + choosewall_code + "]", SetWall[choosewall_code]);
                        }
                    }
                    //如果此處有設牆
                    else
                    {
                        choosewall_setwall[choosewall_code].GetComponent<Renderer>().enabled = false;
                        SetWallTotal = SetWallTotal + 1;
                        PlayerPrefs.SetInt("SetWallTotal", SetWallTotal);
                        SetWall[choosewall_code] = 0;
                        PlayerPrefs.SetInt("SetWall[" + choosewall_code + "]", SetWall[choosewall_code]);
                    }
                }
                if (hit.collider.tag == "ChoosePlace")
                {
                    if (ChooseStartPoint)
                    {
                        //抓取點選的位置（檔案命名方式：XXchooseplace）
                        string chooseplace_number = hit.collider.name.Substring(0, 2);
                        //把抓取到的位置存進SetStartPoint
                        SetStartPoint = int.Parse(chooseplace_number);
                        PlayerPrefs.SetInt("SetStartPoint", SetStartPoint);
                        //移動地圖上的起點位置來表示已設置在所選之處
                        float x = -3.6f + (SetStartPoint - 10) / 10 * 1.44f;
                        float z = -3.6f + (SetStartPoint - 1) % 10 * 1.44f;
                        StartPoint_OnMap.transform.position = new Vector3(x, 0.005f, z);
                        //如果選擇的地方已經設置了終點
                        if(SetStartPoint == SetGoalPoint)
                        {
                            //取消終點的設置
                            SetGoalPoint = 0;
                            PlayerPrefs.SetInt("SetGoalPoint", SetGoalPoint);
                            //將地圖上的終點移到畫面之外（此座標並沒有特殊意義）
                            GoalPoint_OnMap.transform.position = new Vector3(2.58f, 0.005f, 9.36f);
                        }

                        ChooseStartPoint = false;
                        choose_start.GetComponent<Renderer>().enabled = false;
                    }
                    if (ChooseGoalPoint)
                    {
                        //抓取點選的位置（檔案命名方式：XXchooseplace）
                        string chooseplace_number = hit.collider.name.Substring(0, 2);
                        //把抓取到的位置存進SetGoalPoint
                        SetGoalPoint = int.Parse(chooseplace_number);
                        PlayerPrefs.SetInt("SetGoalPoint", SetGoalPoint);
                        //移動地圖上的終點位置來表示已設置在所選之處
                        float x = -3.6f + (SetGoalPoint - 10) / 10 * 1.44f;
                        float z = -3.6f + (SetGoalPoint - 1) % 10 * 1.44f;
                        GoalPoint_OnMap.transform.position = new Vector3(x, 0.005f, z);
                        //如果選擇的地方已經設置了起點
                        if (SetGoalPoint == SetStartPoint)
                        {
                            //取消起點的設置
                            SetStartPoint = 0;
                            PlayerPrefs.SetInt("SetStartPoint", SetStartPoint);
                            //將地圖上的起點移到畫面之外（此座標並沒有特殊意義）
                            StartPoint_OnMap.transform.position = new Vector3(2.58f, 0.005f, 9.36f);
                        }

                        ChooseGoalPoint = false;
                        choose_goal.GetComponent<Renderer>().enabled = false;
                    }
                }
            }
        }
    }
}
