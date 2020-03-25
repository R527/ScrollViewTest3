using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Debug関連をまとめます。
/// </summary>
public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;

    [Header("TimeControllerの時間管理用")]
    public bool isDebug;//TimeControllerの時間管理用のDebug
    public int testMainTime;
    public int testNightTime;

    [Header("役職管理用")]
    public bool isTestPlay;//プレイヤーの役職を任意に決める
    public List<ROLLTYPE> testRollTypeList = new List<ROLLTYPE>();

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}
