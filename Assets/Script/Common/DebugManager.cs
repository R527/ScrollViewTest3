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
    public int testVotingTime;
    public int testExecutionTime;
    public int testCheckGameOverTime;
    public int testResultTime;
    public int num;//GameManagerの参加人数設定
    public int enterNum;//GameManagerの参加希望人数設定
    public int numLimit;
    public float setEnterNumTime;
    public VOTING openVoting;
    public FORTUNETYPE fortuneType;
    public TIME timeType;

    [Header("PlayerPrefs削除 trueなら削除")]
    public bool isPlayerPrefsDeleteAll;

    [Header("isGameOver　trueならGameOver走らない")]
    public bool isGameOver;

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
