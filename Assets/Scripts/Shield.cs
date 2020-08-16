using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour {

    string buttoncode;
    bool button = true;
    public AudioSource buttonaudio;

    // Use this for initialization
    void Start () {
        //擷取按鈕代號
        buttoncode = gameObject.name.Substring(13, 1);
        buttonaudio = gameObject.GetComponent<AudioSource>();
        button = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        //跟球碰觸且按鈕打開
        if (collision.gameObject.name == "Ball" && button)
        {
            //抓取對應的遮蔽物
            var shield_correspond = GameObject.Find("Shield_" + buttoncode);
            //將遮蔽物消去
            shield_correspond.GetComponent<Renderer>().enabled = false;
            //縮小按鈕的scale，看上去像是被按下的感覺
            gameObject.transform.localScale = new Vector3(1f, 0.1f, 1f);
            //播放按按鈕的音效
            buttonaudio.Play();
            //將按鈕關閉
            button = false;
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
