using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


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

    public Text debugText;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定した Player 1人のカスタムプロパティの表示
    /// </summary>
    /// <typeparam name="T">キャストして使いたい型の指定</typeparam>
    /// <param name="key">取得したいカスタムプロパティの key</param>
    /// <param name="player">表示する Player の指定</param>
    public void DisplayCustomPropertyOfPlayer<T>(string key, Photon.Realtime.Player player) {
        if (player.CustomProperties.TryGetValue(key, out object obj)) {
            Debug.Log(player.NickName + ":" + key + ":" + (T)obj);
        } else {
            Debug.Log("null");
        }
    }

    /// <summary>
    /// 指定した Player 全員のカスタムプロパティの表示
    /// </summary>
    /// <typeparam name="T">キャストして使いたい型の指定</typeparam>
    /// <param name="key">取得したいカスタムプロパティの key</param>
    /// <param name="players">表示する Player 群の指定を配列で行う　PhotonNetwork.PlayerList に合わせてある</param>
    public void DisplayCustomPropertyOfPlayerList<T>(string key, Photon.Realtime.Player[] players) {
        foreach(Photon.Realtime.Player player in players) {
            if (player.CustomProperties.TryGetValue(key, out object obj)) {
                Debug.Log(player.NickName + ":" + key + ":" + (T)obj);
            } else {
                Debug.Log("null");
            }
        }
    }

    /// <summary>
    /// ルームのカスタムプロパティの表示
    /// </summary>
    /// <typeparam name="T">キャストして使いたい型の指定</typeparam>
    /// <param name="key">取得したいカスタムプロパティの key</param>

    public void DisplayCustomPropertyOfRoom<T>(string key) {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object obj)) {
            Debug.Log(PhotonNetwork.CurrentRoom.Name + ":" + key + ":" + (T)obj);
        } else {
            Debug.Log("null");
        }
    }
}
