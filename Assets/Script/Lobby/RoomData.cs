using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 部屋情報をGameシーンへ送信
/// </summary>
public class RoomData : MonoBehaviour
{
    public static RoomData instance;

    public RoomInfo roomInfo;
    public List<int> rollList = new List<int>();
    public int numLimit;
   

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

}
