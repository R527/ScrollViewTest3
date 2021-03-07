﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using System.Linq;



public class PlayerManager : MonoBehaviour {

    public static PlayerManager instance;

    //main
    public string playerName;
    /// <summary>
    /// Prefsで保存
    /// </summary>
    //=================================
    //Ban関連
    //=================================
    [Header("Ban関連")]
    public List<string> banUniqueIDList = new List<string>();
    public List<string> banUserNickNameList = new List<string>();
    public List<string> roomBanUniqueIdList = new List<string>();
    public string roomBanUniqueIdStr;
    public int banIndex;//ban番号の通し番号
    public string myUniqueId;//自分の端末番号
    public int banListIndex;//BanListの今の登録数
    [Header("BanListの最大の登録数")]
    public int banListMaxIndex;//BanListの最大の登録数

    //============================
    //戦績関連
    //============================
    [Header("戦績関連")]
    //総合
    public float totalNumberOfMatches;//総対戦回数
    public float totalNumberOfWins;//総勝利数
    public float totalNumberOfLoses;//総敗北数
    public float totalNumberOfSuddenDeath;//突然死数
    //初心者
    public float beginnerTotalNumberOfMatches;
    public float beginnerTotalNumberOfWins;
    public float beginnerTotalNumberOfLoses;
    //一般
    public float generalTotalNumberOfMatches;
    public float generalTotalNumberOfWins;
    public float generalTotalNumberOfLoses;

    public int checkTotalNumberOfMatches;//25線ごとにチェックして突然死数を減らす

    [Header("対戦log")]
    
    public string roomName;
    public string saveChatLog;
    public List<string> getChatLogList = new List<string>();
    public int saveRoomCount;
    public int myID;

    //==========================
    //課金関連
    //==========================
    public int currency;
    public bool subscribe;//falseならサブすく中
    public string productId;//test_jinrou_subscribe
    private Result subscResult;

    /// <summary>
    /// Prefs保存用
    /// </summary>
    public enum ID_TYPE {
        //BanList
        myUniqueId,
        banUniqueID,
        banUserNickName,
        banIndex,
        banListMaxIndex,

        //main
        playerName,
        friendId,

        //チャットログ
        saveChatLog,
        roomName,
        saveRoomCount,

        //音量
        bgmVolume,
        seVolume,

        //課金用
        currency,//ゲーム内通貨
        superChat,
        exit
    }

    /// <summary>
    /// 戦績関連
    /// </summary>
    public enum BATTLE_RECORD_TYPE {
        //総合
        総対戦回数,
        総勝利回数,
        総敗北回数,

        //初心者
        初心者対戦回数,
        初心者勝利回数,
        初心者敗北回数,

        //一般
        一般対戦回数,
        一般勝利回数,
        一般敗北回数,

        突然死数,
        突然死減少チェック,
    }

    /// <summary>
    /// 突然死用
    /// </summary>
    public enum SuddenDeath_TYPE {
        ゲーム正常終了,
        ゲーム開始,
        不参加,
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
                saveRoomCount = PlayerPrefs.GetInt(ID_TYPE.saveRoomCount.ToString(), 0);
                PlayerPrefs.SetString("roomNum" + saveRoomCount, setString);
                saveRoomCount++;
                break;
            //課金用のPopUpの表示非表示を決める
            case ID_TYPE.superChat:
                PlayerPrefs.SetString(ID_TYPE.superChat.ToString(), setString);
                break;
            case ID_TYPE.exit:
                PlayerPrefs.SetString(ID_TYPE.exit.ToString(), setString);
                break;
        }

        //端末の中に保存する
        PlayerPrefs.Save();
    }


    /// <summary>
    /// PlayerPrefsに数字をセットする
    /// </summary>
    /// <param name="setInt"></param>
    /// <param name="idType"></param>
    public void SetIntForPlayerPrefs(int setInt, ID_TYPE idType) {
        switch (idType) {

            //BanList関連
            case ID_TYPE.banIndex:
                PlayerPrefs.SetInt(ID_TYPE.banIndex.ToString(), setInt);
                break;
            case ID_TYPE.banListMaxIndex:
                PlayerPrefs.SetInt(ID_TYPE.banListMaxIndex.ToString(), setInt);
                break;
            case ID_TYPE.saveRoomCount:
                PlayerPrefs.SetInt("saveRoomCount", setInt);
                break;

            //課金関連
            case ID_TYPE.currency:
                PlayerPrefs.SetInt(ID_TYPE.currency.ToString(), setInt);
                break;
        }
        PlayerPrefs.Save();
    }

    public void SetFloatForPlayerPrefs(float setFloat, ID_TYPE idType) {
        switch (idType) {
            case ID_TYPE.bgmVolume:
                PlayerPrefs.SetFloat(ID_TYPE.bgmVolume.ToString(), setFloat);
                break;
            case ID_TYPE.seVolume:
                PlayerPrefs.SetFloat(ID_TYPE.seVolume.ToString(), setFloat);
                break;
        }
        PlayerPrefs.Save();

    }


    public void SetBattleRecordForPlayerPrefs(float setfloat, BATTLE_RECORD_TYPE type) {
        //戦績関連
        switch (type) {

            //総合
            case BATTLE_RECORD_TYPE.総対戦回数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.総対戦回数.ToString(), setfloat);
                break;
            case BATTLE_RECORD_TYPE.総勝利回数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.総勝利回数.ToString(), setfloat);
                break;
            case BATTLE_RECORD_TYPE.総敗北回数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.総敗北回数.ToString(), setfloat);
                break;

            //初心者
            case BATTLE_RECORD_TYPE.初心者対戦回数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.初心者対戦回数.ToString(), setfloat);
                break;
            case BATTLE_RECORD_TYPE.初心者勝利回数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.初心者勝利回数.ToString(), setfloat);
                break;
            case BATTLE_RECORD_TYPE.初心者敗北回数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.初心者敗北回数.ToString(), setfloat);
                break;

            //一般
            case BATTLE_RECORD_TYPE.一般対戦回数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.一般対戦回数.ToString(), setfloat);
                break;
            case BATTLE_RECORD_TYPE.一般勝利回数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.一般勝利回数.ToString(), setfloat);
                break;
            case BATTLE_RECORD_TYPE.一般敗北回数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.一般敗北回数.ToString(), setfloat);
                break;

            case BATTLE_RECORD_TYPE.突然死数:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.突然死数.ToString(), setfloat);
                break;
            case BATTLE_RECORD_TYPE.突然死減少チェック:
                PlayerPrefs.SetFloat(BATTLE_RECORD_TYPE.突然死減少チェック.ToString(), setfloat);
                break;

        }
        PlayerPrefs.Save();
    }

    
    /////////////
    ///突然死管理
    /////////////

    /// <summary>
    /// 突然死用のStringをセットする
    /// </summary>
    /// <param name="setString"></param>
    /// <param name="type"></param>
    public void SetStringSuddenDeathTypeForPlayerPrefs(SuddenDeath_TYPE type) {
        PlayerPrefs.SetString("突然死用のフラグ", type.ToString());
        PlayerPrefs.Save();
    }


    //////////////////
    ///チャットログ復元
    //////////////////

    /// <summary>
    /// チャットデータを一つの文字列に変換する
    /// ゲーム終了後にゲーム内容を復元するのに使う
    /// </summary>
    /// <param name="chatData"></param>
    /// <returns></returns>
    public string ConvertStringToChatData(ChatData chatData) {
        string str = "";
        string comingOut = "";

        if(chatData.comingOutText != "") {
            comingOut = "," + chatData.comingOutText;
        }

        str = chatData.inputData + "," + chatData.boardColor + "," + chatData.playerName + "," + chatData.playerID + "," + chatData.iconNo + "," + comingOut;
        return str;
    }

    /// <summary>
    /// チャットログ保存用
    /// </summary>
    public void SetGameChatLog(bool isWin) {
        SetStringForPlayerPrefs(SaveGameData(isWin), ID_TYPE.saveChatLog);
        SetIntForPlayerPrefs(saveRoomCount, ID_TYPE.saveRoomCount);
    }


    /// <summary>
    /// ゲーム情報を復元用に保存します
    /// </summary>
    /// <returns></returns>
    public string SaveGameData(bool isWin) {
        GameManager gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        string str = "";
        //自分がどのプレイヤーがにあたるかの情報
        //プレイヤーID順にボタン用の名前を登録する
        string nameList = "";
        foreach (Player player in gameManager.chatSystem.playersList) {
            nameList += player.playerID + "," + player.playerName + "," +  player.rollType + "," + player.playerButton.iconNo + "&";
        }
        nameList = nameList.Substring(0, nameList.Length - 1);

        //役職一覧作成
        string rollList = "";
        foreach(int rollNum in RoomData.instance.rollList) {
            rollList += rollNum + ",";
        }
        rollList = rollList.Substring(0, rollList.Length - 1);
        //役職一覧を取得する
        //勝敗　役職一覧 playerNum nameList　　チャットログ
        str = isWin + "%" + rollList +　"%" +  gameManager.chatSystem.myPlayer.playerID + "%" + nameList + "%" + saveChatLog;
        return str;
    }

    /// <summary>
    ///タイトル画面でRoomCount保存したログを配列に保存する
    /// </summary>
    public void GetSaveRoomData() {
        getChatLogList.Clear();
        saveRoomCount = PlayerPrefs.GetInt("saveRoomCount", 0);
        if (saveRoomCount < 10) {
            for (int i = saveRoomCount - 1; 0 <= i; i--) {
                getChatLogList.Add(PlayerPrefs.GetString("roomNum" + i, ""));
            }
        } else {
            for (int i = saveRoomCount - 1; saveRoomCount - 10 < i; i--) {
                getChatLogList.Add(PlayerPrefs.GetString("roomNum" + i, ""));
            }
        }
    }

    /// <summary>
    /// チャットログ復元用
    /// </summary>
    public void GetGameChatLog(int roomNum) {
        ChatLog chatLog = GameObject.FindGameObjectWithTag("ChatLog").GetComponent<ChatLog>();
        //復元処理
        string[] saveChatLogList = getChatLogList[roomNum].Substring(0, getChatLogList[roomNum].Length - 1).Split('%').ToArray<string>();

        List<string> buttonInfoList = new List<string>();

        foreach (string str in saveChatLogList) {
            //ボタンの復元
            if (buttonInfoList.Count < 4) {
                buttonInfoList.Add(str);
                continue;
            }
            //チャット内容、色、発言者に分けてそれぞれ配列に入れる
            string[] getChatLogList = str.Split(',').ToArray<string>();
            string inputData = getChatLogList[0];
            int boardColor = int.Parse(getChatLogList[1]);
            playerName = getChatLogList[2] + getChatLogList[5];
            int playerID = int.Parse(getChatLogList[3]);
            int iconNo = int.Parse(getChatLogList[4]);

            //SPEAKER_TYPEがON OFFどちらでもOFFLINE処理をする
            SPEAKER_TYPE speaker_Type = SPEAKER_TYPE.NULL;
            if (getChatLogList[2] == "GAMEMASTER_OFFLINE" || getChatLogList[2] == "GAMEMASTER_ONLINE") {
                speaker_Type = SPEAKER_TYPE.GAMEMASTER_OFFLINE;
            }

            if(boardColor == 7777) {
                //NextDay作成
                chatLog.CreateNextDay();
            } else {
                //チャット生成
                chatLog.CreateLogChat(speaker_Type, inputData, playerID, boardColor, playerName, iconNo);
            }
        }

        //自分のPlayerIDを登録する
        myID = int.Parse(buttonInfoList[2]);

        //ボタンを生成する
        string[] getButtonList = buttonInfoList[3].Split('&').ToArray<string>();
        foreach (string str in getButtonList) {
            string[] buttonData = null;
            buttonData = str.Split(',').ToArray<string>();
            int playerID = int.Parse(buttonData[0]);
            playerName = buttonData[1];
            string roll = buttonData[2];
            int iconNo = int.Parse(buttonData[3]);
            chatLog.CreatePlayerButton(playerName, playerID, roll,iconNo);
        }
    }


    //=====================
    //課金関連
    //======================

    /// <summary>
    /// ゲーム内通貨を利用する
    /// </summary>
    public void UseCurrency(int useCurrency) {
        currency -= useCurrency;
        SetIntForPlayerPrefs(currency, ID_TYPE.currency);
    }


    /// <summary>
    /// サブスクライブの設定をする
    /// </summary>
    public bool SetSubscribe() {

        SubscriptionInfo subscInfo = new SubscriptionInfo(productId);
        if (subscInfo != null) {
            subscResult = subscInfo.isSubscribed();

            //TODO 課金システムの確認　常にTrueが返ってくる
            if (subscResult == Result.False) {
                Debug.Log("課金中");
                //return false;
            } else {
                Debug.Log("未課金");
                //return true;
            }
        }
        return false;
    }
}
