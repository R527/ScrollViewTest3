using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

/// <summary>
/// チャットログ復元を管理する
/// </summary>
public class ChatLog : MonoBehaviour
{
    //main
    public ChatSystem chatSystem;
    PlayerButton playerButton;
    public PlayerButton playerButtonPrefab;
    public Transform buttonTran;
    public ChatNode chatNodePrefab;
    public Transform chatTran;
    List<string> saveChatLogList = new List<string>();
    List<string> buttonInfoList = new List<string>();
    List<ChatNode> chatNodeList = new List<ChatNode>();
    int myID;
    string playerName;
    string inputData;
    int boardColor;
    int playerID;

    //折畳ボタン
    public Button foldingButton;
    public Text foldingText;
    public Transform mainRectTransform;
    public Transform inputRectTransform;


    private void Start() {
        foldingButton.onClick.AddListener(FoldingPosition);
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            Debug.Log("保存");
            PlayerManager.instance.SetGameChatLog();
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
        ChatNode chatNode = Instantiate(chatNodePrefab, chatTran, false);
        chatNode.InitChatNodeLog(chatData, 0, false);
        chatNode.chatBoard.color = chatSystem.color[chatData.boardColor];
        chatNodeList.Add(chatNode);
        //gameManager.chatSystem.SetChatNode(chatNode, chatData, false);
        Debug.Log("復元完了");
    }

    /// <summary>
    /// チャットログ復元用
    /// </summary>
    public void GetGameChatLog() {

        PlayerManager.instance.saveChatLog = PlayerPrefs.GetString("test", "");
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
    /// Playerボタンを作成します
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="playerID"></param>
    /// <param name="gameManager"></param>
    private void CreatePlayerButton(string playerName, int playerID) {
        Debug.Log("CreatePlayerButton");
        
        playerButton = Instantiate(playerButtonPrefab, buttonTran, false);
        //playerButton.gameManager = gameManager;
        playerButton.transform.SetParent(buttonTran);
        playerButton.playerText.text = playerName;
        playerButton.playerID = playerID;
        //PlayerButtonにフィルタ機能を追加
        //playerButton.playerButton.onClick.RemoveAllListeners();
        playerButton.playerButton.onClick.AddListener(() => FillterButton(playerID));
    }

    /// <summary>
    /// フィルター制御を追加します。
    /// </summary>
    private void FillterButton(int playerID) {
        foreach(ChatNode chatObj in chatNodeList) {
            chatObj.gameObject.SetActive(false);
        }

        foreach(ChatNode chatObj in chatNodeList) {
            if(chatObj.playerID == playerID) {
                chatObj.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 折畳ボタンの制御
    /// </summary>
    public void FoldingPosition() {
        if (foldingText.text == "↓") {
            inputRectTransform.DOLocalMoveY(-67, 0.5f);
            mainRectTransform.DOLocalMoveY(0, 0.5f);
            foldingText.text = "↑";
        } else {
            inputRectTransform.DOLocalMoveY(0, 0.5f);
            mainRectTransform.DOLocalMoveY(72, 0.5f);
            foldingText.text = "↓";
        }
    }
}
