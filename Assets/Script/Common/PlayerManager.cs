using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;



public class PlayerManager : MonoBehaviour
{

    public static PlayerManager instance;
    public GameManager gameManager;

    //main
    public string playerName;
    

    //Ban関連
    [Header("Ban関連")]
    public List<string> banUniqueIDList = new List<string>();
    public List<string> banUserNickNameList = new List<string>();
    public List<string> roomBanUniqueIdList = new List<string>();
    public string roomBanUniqueIdStr;
    public int banIndex;//ban番号の通し番号
    public Dictionary<string, string> banTable = new Dictionary<string, string>();
    public string myUniqueId;//自分の端末番号
    public int banListMaxIndex;

    //戦績関連
    [Header("戦績関連")]
    public int totalNumberOfMatches;//総対戦回数
    public int totalNumberOfWins;//総勝利数
    public int totalNumberOfLoses;//総敗北数
    public int totalNumberOfSuddenDeath;//突然死数

    [Header("対戦log")]
    
    public int gameLogCount;
    public string roomName;
    public string saveChatLog;
    //public List<string> getChatLogList = new List<string>();
    //public int saveChatCount;

    /// <summary>
    /// Ban関連
    /// </summary>
    public enum ID_TYPE {
        myUniqueId,
        banUniqueID,
        banUserNickName,
        banIndex,
        banListMaxIndex,
        playerName,
        friendId,
        saveChatLog,
        roomName,

    }

    /// <summary>
    /// 戦績関連
    /// </summary>
    public enum BATTLE_RECORD_TYPE {
        総対戦回数,
        勝利回数,
        敗北回数,
        突然死数
    }

    /// <summary>
    /// 突然死用
    /// </summary>
    public enum SuddenDeath_TYPE {
        ゲーム開始,
        不参加,
        ゲーム正常終了,
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// PlayerPrefsにstringをセットする
    /// </summary>
    /// <param name="setString"></param>
    /// <param name="idType"></param>
    public void SetStringForPlayerPrefs(string setString,ID_TYPE idType) {
        Debug.Log(idType);
        Debug.Log(setString);

        switch (idType) {
            //端末のIDをセットする
            case ID_TYPE.myUniqueId:
                PlayerPrefs.SetString(ID_TYPE.myUniqueId.ToString(), setString);
                break;

            //banUniqueIDListへUniqueIDを追加する
            case ID_TYPE.banUniqueID:
                PlayerPrefs.SetString(ID_TYPE.banUniqueID.ToString() + banIndex.ToString(), setString);
                break;
            //banUserNickNameListへバンした名前を追加
            case ID_TYPE.banUserNickName:
                PlayerPrefs.SetString(ID_TYPE.banUserNickName.ToString() + banIndex.ToString(), setString);
                break;
            case ID_TYPE.playerName:
                PlayerPrefs.SetString(ID_TYPE.playerName.ToString(), setString);
                break;
            case ID_TYPE.friendId:
                break;
            //チャットログ保存用
            case ID_TYPE.saveChatLog:
                PlayerPrefs.SetString("test", setString);
                //saveChatCount++;
                break;
                //case ID_TYPE.roomName:
                //    PlayerPrefs.SetString("Game" + gameLogCount, roomName);
                //    gameLogCount++;
                //    break;
        }


        //端末の中に保存する
        PlayerPrefs.Save();
    }


    /// <summary>
    /// PlayerPrefsにBanLIstに関する数字をセットする
    /// </summary>
    /// <param name="setInt"></param>
    /// <param name="idType"></param>
    public void SetBanListForPlayerPrefs(int setInt, ID_TYPE idType) {
        switch (idType) {

            //BanList関連
            case ID_TYPE.banIndex:
                PlayerPrefs.SetInt(ID_TYPE.banIndex.ToString(), setInt);
                break;
            case ID_TYPE.banListMaxIndex:
                PlayerPrefs.SetInt(ID_TYPE.banListMaxIndex.ToString(), setInt);
                break;
        }
        PlayerPrefs.Save();
    }


    public void SetBattleRecordForPlayerPrefs(int setInt, BATTLE_RECORD_TYPE type) {
        //戦績関連
        switch (type) {
            case BATTLE_RECORD_TYPE.総対戦回数:
                PlayerPrefs.SetInt(BATTLE_RECORD_TYPE.総対戦回数.ToString(), setInt);
                break;
            case BATTLE_RECORD_TYPE.勝利回数:
                PlayerPrefs.SetInt(BATTLE_RECORD_TYPE.勝利回数.ToString(), setInt);
                break;
            case BATTLE_RECORD_TYPE.敗北回数:
                PlayerPrefs.SetInt(BATTLE_RECORD_TYPE.敗北回数.ToString(), setInt);
                break;
            case BATTLE_RECORD_TYPE.突然死数:
                PlayerPrefs.SetInt(BATTLE_RECORD_TYPE.突然死数.ToString(), setInt);
                break;
            
        }
        PlayerPrefs.Save();
    }

    
    /// <summary>
    /// 突然死用のStringをセットする
    /// </summary>
    /// <param name="setString"></param>
    /// <param name="type"></param>
    public void SetStringSuddenDeathTypeForPlayerPrefs(SuddenDeath_TYPE type) {
        PlayerPrefs.SetString("突然死用のフラグ", type.ToString());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// チャットデータを一つの文字列に変換する
    /// ゲーム終了後にゲーム内容を復元するのに使う
    /// </summary>
    /// <param name="chatData"></param>
    /// <returns></returns>
    public string ConvertStringToChatData(ChatData chatData) {
        string str = "";
        str = chatData.inputData + "," + chatData.boardColor + "," + chatData.playerName + "," + chatData.playerID;
        return str;
    }

    /// <summary>
    /// チャットログ保存用
    /// </summary>
    public void SetGameChatLog() {
        SetStringForPlayerPrefs(SaveGameData(), ID_TYPE.saveChatLog);
        PlayerPrefs.Save();
    }


    /// <summary>
    /// ゲーム情報を復元用に保存します
    /// </summary>
    /// <returns></returns>
    public string SaveGameData() {
        GameManager gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        string str = "";
        //自分がどのプレイヤーがにあたるかの情報
        //プレイヤーID順にボタン用の名前を登録する
        string nameList = "";
        foreach (Player player in gameManager.chatSystem.playersList) {
            nameList += player.playerID + "," + player.playerName + "&";
        }
        nameList = nameList.Substring(0, nameList.Length - 1) + "%";
        str = gameManager.chatSystem.myID + "%" + nameList + instance.saveChatLog;
        return str;
    }
}
