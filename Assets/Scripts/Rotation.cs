using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour {

    private Rigidbody ballrigidbody;
    //球移動速度
    public float speed = 1.0F;
    //偵測球是否落在地上時的bool
    //宣告static(靜態)讓其他cs可以呼叫此變數
    public static bool ballcontrolstart = false;
    public static bool ballgoal = false;
    public static bool balljump = false;
    public static bool ball_not_jump_in_time = false;
    //球滾動方向用矩陣
    public Vector3 dir = Vector3.zero;
    //碰撞時音效
    public AudioSource contactaudio;

    private Vector3[] jump;
    private Vector3[] jump_path;
    int jump_nowpoint;

    private void Awake()
    {
        jump = new Vector3[3];
        jump_path = new Vector3[11];
    }

    // Use this for initialization
    void Start () {
        ballrigidbody = gameObject.GetComponent<Rigidbody>();

        contactaudio = gameObject.GetComponent<AudioSource>();

        ballcontrolstart = false;
        ballgoal = false;
        balljump = false;
        ball_not_jump_in_time = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        //當球從一開始的空中落到mainmenu的地上或關卡起點時
        if (collision.gameObject.name == "Plane" || collision.gameObject.name == "Start")
        {
            ballcontrolstart = true;
        }
        //當球碰撞到牆時，發出音效
        if (collision.gameObject.tag == "Wall")
        {
            contactaudio.Play();
        }
        //當球碰到關卡終點時
        if(collision.gameObject.name == "Goal")
        {
            ballcontrolstart = false;
            ballgoal = true;
        }
        //當球碰到跳躍點時
        if ((collision.gameObject.name == "Jump_A" || collision.gameObject.name == "Jump_B") && !balljump && !ball_not_jump_in_time)
        {
            ballcontrolstart = false;
            jump_nowpoint = 0;
            //確定Bezier Approximation的三個控制點
            if (collision.gameObject.name == "Jump_A")
            {
                jump[0] = gameObject.GetComponent<Transform>().position;
                jump[2] = GameObject.Find("Jump_B").GetComponent<Transform>().position;
                jump[1].x = (jump[0].x + jump[2].x) / 2;
                jump[1].y = 30;
                jump[1].z = (jump[0].z + jump[2].z) / 2;
            }
            else
            {
                jump[0] = gameObject.GetComponent<Transform>().position;
                jump[2] = GameObject.Find("Jump_A").GetComponent<Transform>().position;
                jump[1].x = (jump[0].x + jump[2].x) / 2;
                jump[1].y = 17;
                jump[1].z = (jump[0].z + jump[2].z) / 2;
            }
            //參考研究所時的公式
            for(int uInt = 0;uInt <= 100; uInt++)
            {
                float u = uInt / (float)100;
                float x = 0;
                float y = 0;
                float z = 0;
                //因控制點固定3，故i = 3
                for(int bb = 0;bb < 3; bb++)
                {
                    x += BinomialCoefficient(3 - 1, bb) * Mathf.Pow((1 - u), 3 - 1 - bb) * Mathf.Pow(u, bb) * jump[bb].x;
                    y += BinomialCoefficient(3 - 1, bb) * Mathf.Pow((1 - u), 3 - 1 - bb) * Mathf.Pow(u, bb) * jump[bb].y;
                    z += BinomialCoefficient(3 - 1, bb) * Mathf.Pow((1 - u), 3 - 1 - bb) * Mathf.Pow(u, bb) * jump[bb].z;
                }
                //只記101個點中的11個點用作路徑
                if (uInt % 10 == 0)
                {
                    jump_path[uInt / 10].x = x;
                    jump_path[uInt / 10].y = y;
                    jump_path[uInt / 10].z = z;
                    //建立不顯示mesh renderer的隱形Cube當作路徑
                    var pathpoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pathpoint.name = "pathpoint";
                    pathpoint.transform.position = jump_path[uInt / 10];
                    pathpoint.GetComponent<Renderer>().enabled = false;
                }
            }
            balljump = true;
        }
        if (collision.gameObject.name == "pathpoint" && balljump)
        {
            jump_nowpoint++;
            if(jump_nowpoint >= 11)
            {
                balljump = false;
                ballcontrolstart = true;
                ball_not_jump_in_time = true;
            }
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.name == "releaseJump" && ball_not_jump_in_time)
        {
            ball_not_jump_in_time = false;
        }
    }

    int BinomialCoefficient(int n, int m)
    {
        if (m == 0 || n == m)
        {
            return 1;
        }
        else
        {
            return BinomialCoefficient(n - 1, m) + BinomialCoefficient(n - 1, m - 1);
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        //當球落到地上時才可以藉由手機傾斜來移動
        if (ballcontrolstart)
        {
            //球體移動向量          
            dir.z = Input.acceleration.x;
            dir.x = -Input.acceleration.y;          

            dir *= Time.deltaTime;
            //用fixedupdate的rigidbody.MovePosition，會比transform.Translate更好避免持續移動時造成的穿牆狀況
            //在Edit > Project Settings > Time中的Fixed Timestep可設定update的時間
            ballrigidbody.MovePosition(ballrigidbody.position + dir * speed);
        }
        //當球跳躍時
        if (balljump)
        {
            transform.Translate((jump_path[jump_nowpoint] - gameObject.GetComponent<Transform>().position) * Time.deltaTime * speed, Space.World);
        }
    }
}
