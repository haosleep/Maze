using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;

public class NetRotation : NetworkBehaviour
{
    private Rigidbody ballrigidbody;
    //球移動速度
    public float speed = 1.0F;
    //偵測球是否落在地上時的bool
    //宣告static(靜態)讓其他cs可以呼叫此變數
    public static bool ballcontrolstart = false;
    public static bool ballgoal = false;
    //球滾動方向用矩陣
    public Vector3 dir = Vector3.zero;
    //碰撞時音效
    public AudioSource contactaudio;

    private void Awake()
    {
    }

    // Use this for initialization
    void Start()
    {
        ballrigidbody = gameObject.GetComponent<Rigidbody>();

        contactaudio = gameObject.GetComponent<AudioSource>();

        ballcontrolstart = false;
        ballgoal = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        //當球從一開始的空中落到關卡起點時
        if (collision.gameObject.name == "Start")
        {
            ballcontrolstart = true;
        }
        //當球碰撞到牆時，發出音效
        if (collision.gameObject.tag == "Wall")
        {
            contactaudio.Play();
        }
        //當球碰到關卡終點時
        if (collision.gameObject.name == "Goal")
        {
            ballcontrolstart = false;
            ballgoal = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
    }
}
