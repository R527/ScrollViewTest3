using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Debug関連をまとめます。
/// </summary>
public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;


    
    public bool isDebug;//Debugのboolを管理する

    [Header("TimeControllerの時間管理用")]
    //public bool isTimeController;//TimeControllerの時間管理用のDebug
    public bool isTimeController;
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

    [Header("trueならGameOver走らない")]
    public bool isGameOver;

    [Header("trueなら突然死しない")]
    public bool isCheckSuddenDeath;

    [Header("trueなら処刑しない")]
    public bool isVoteCount;

    [Header("役職管理用")]
    public bool isTestPlay;//プレイヤーの役職を任意に決める
    public List<ROLLTYPE> testRollTypeList = new List<ROLLTYPE>();

    [Header("課金管理　trueなら内部通貨初期化")]
    public bool isCurrency;

    public Text debugText;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void DisplayDebugLog<T>() {

    }

}
