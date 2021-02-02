using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomNodeList : MonoBehaviour
{

    public static RoomNodeList instance;
    public List<GameObject> roomNodeObjList = new List<GameObject>();//Gameシーンから退出時RoomNodeObjが破壊されるのを防ぐため
    

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log("roomNodeObjListCount" + roomNodeObjList.Count);//Countは1を取り続ける
        //Debug.Log("roomNodeID" + roomNodeObjList[0].GetComponent<RoomNode>().roomId);//シーンを切り替えるとＮｕｌｌになる

    }
}
