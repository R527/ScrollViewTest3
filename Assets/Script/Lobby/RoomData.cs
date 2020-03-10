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
    public List<int> numList = new List<int>();
    public int settingNum;
    public int testMainTime;//debug用
    public int testNightTime;
    [Header("debug用")]
    public bool isDebugOn;//時間管理のデバッグ

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

}
