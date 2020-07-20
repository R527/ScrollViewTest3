using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// チャットログ復元を管理する
/// </summary>
public class ChatLog : MonoBehaviour
{

    public GameManager gameManager;
    PlayerButton playerButton;
    public PlayerButton playerButtonPrefab;
    public Transform buttonTran;
    List<string> saveChatLogList = new List<string>();
    List<string> buttonInfoList = new List<string>();
    int myID;
    string playerName;
    string inputData;
    int boardColor;
    int playerID;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            Debug.Log("保存");
            SetGameChatLog();
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            Debug.Log("復元");
            GetGameChatLog();
        }
    }

    //一つのstringを復元する
    //復元内容、人数とプレイヤー名とチャットログ

    //復元方法、人数とプレイヤー名からボタンを作成して、チャットログと紐づける
    //どのプレイヤーが自分かを紐づける
    //チャットログも複製
    //ボタンなどの処理を追加
    /// <summary>
    /// ログ保存したものを復元するよう
    /// </summary>
    public void CreateLogChat(SPEAKER_TYPE speaker_Type) {


        ChatData chatData = new ChatData(inputData, playerID, boardColor, playerName, ROLLTYPE.ETC);
        if (speaker_Type == SPEAKER_TYPE.GAMEMASTER_OFFLINE) {
            chatData.chatType = CHAT_TYPE.GM;
        }
        ChatNode chatNode = Instantiate(gameManager.chatSystem.myPlayer.chatNodePrefab, gameManager.chatSystem.myPlayer.chatTran, false);
        chatNode.InitChatNode(chatData, 0, false);
        chatNode.chatBoard.color = gameManager.chatSystem.color[chatData.boardColor];
        //gameManager.chatSystem.SetChatNode(chatNode, chatData, false);
        Debug.Log("復元完了");
    }

    /// <summary>
    /// チャットログ復元用
    /// </summary>
    public void GetGameChatLog() {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        PlayerManager.instance.saveChatLog = PlayerPrefs.GetString(PlayerManager.instance.roomName, "");
        PlayerManager.instance.saveChatLog.Substring(0, PlayerManager.instance.saveChatLog.Length - 1);
        Debug.Log(PlayerManager.instance.saveChatLog);

        //復元処理
        saveChatLogList = PlayerManager.instance.saveChatLog.Substring(0, PlayerManager.instance.saveChatLog.Length - 1).Split('%').ToList<string>();
        foreach (string str in saveChatLogList) {
            
            //ボタンの復元
            
            if(buttonInfoList.Count < 2) {
                buttonInfoList.Add(str);
                continue;
            }
            Debug.Log(str);
            //チャット内容、色、発言者に分けてそれぞれ配列に入れる

            string[] getChatLogList = str.Split(',').ToArray<string>();
            inputData = getChatLogList[0];
            Debug.Log(getChatLogList[1]);
            boardColor = int.Parse(getChatLogList[1]);
            playerName = getChatLogList[2];
            playerID = int.Parse(getChatLogList[3]);

            //SPEAKER_TYPEがON OFFどちらでもOFFLINE処理をする
            SPEAKER_TYPE speaker_Type = SPEAKER_TYPE.NULL;
            if (getChatLogList[2] == "GAMEMASTER_OFFLINE" || getChatLogList[2] == "GAMEMASTER_ONLINE") {
                speaker_Type = SPEAKER_TYPE.GAMEMASTER_OFFLINE;
            }
            //チャット生成
            CreateLogChat(speaker_Type);
            Debug.Log("発言内容" + getChatLogList[0]);
            //Debug.Log("色"+color);
            Debug.Log("発言者" + getChatLogList[2]);
        }

        //自分のPlayerIDを登録する
        myID = int.Parse(buttonInfoList[0]);

        //ボタンを生成する
        string[] getButtonList = buttonInfoList[1].Split('&').ToArray<string>();
        foreach(string str in getButtonList) {
            string[] buttonData = null;
            buttonData = str.Split(',').ToArray<string>();
            playerID = int.Parse(buttonData[0]);
            playerName = buttonData[1];
            CreatePlayerButton(playerName, playerID);
        }
        
    }

    /// <summary>
    /// チャットログ保存用
    /// </summary>
    public void SetGameChatLog() {
        //PlayerPrefs.SetInt(roomName, saveChatCount);
        //PlayerPrefs.SetInt("gameLogCount", gameLogCount);
        PlayerManager.instance.SetStringForPlayerPrefs(SaveGameData(), PlayerManager.ID_TYPE.saveChatLog);
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
        str = gameManager.chatSystem.myID + "%" + nameList + PlayerManager.instance.saveChatLog;
        return str;
    }

    private void CreatePlayerButton(string playerName, int playerID) {
        Debug.Log("CreatePlayerButton");

        playerButton = Instantiate(playerButtonPrefab, buttonTran, false);
        playerButton.transform.SetParent(buttonTran);
        playerButton.playerText.text = playerName;
        playerButton.playerID = playerID;

    }

}
