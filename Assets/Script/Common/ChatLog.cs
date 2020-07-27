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
    ChatLogPlayerButton playerButton;
    public ChatLogPlayerButton playerButtonPrefab;
    public Transform buttonTran;
    public ChatNode chatNodePrefab;
    public Transform chatTran;
    
    List<ChatNode> chatNodeList = new List<ChatNode>();
    string playerName;


    //折畳ボタン
    public Button foldingButton;
    public Text foldingText;
    public Transform mainRectTransform;
    public Transform inputRectTransform;
    public GameObject mainCanvas;
    public GameObject underBarCanvas;

    public Button exitButtn;


    private void Start() {
        foldingButton.onClick.AddListener(FoldingPosition);
        exitButtn.onClick.AddListener(CloseChatLog);
    }

    /// <summary>
    /// ログ保存したものを復元するよう
    /// </summary>
    public void CreateLogChat(SPEAKER_TYPE speaker_Type, string inputData, int playerID, int boardColor) {


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
    /// Playerボタンを作成します
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="playerID"></param>
    /// <param name="gameManager"></param>
    public void CreatePlayerButton(string playerName, int playerID) {
        Debug.Log("CreatePlayerButton");

        playerButton = Instantiate(playerButtonPrefab, buttonTran, false);
        //playerButton.gameManager = gameManager;
        playerButton.transform.SetParent(buttonTran);
        playerButton.playerNameText.text = playerName;
        playerButton.playerID = playerID;
        Debug.Log(playerID);
        Debug.Log(PlayerManager.instance.myID);
        if (PlayerManager.instance.myID == playerID) {
            
            playerButton.GetComponent<Outline>().enabled = true;
        }
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

    public void CloseChatLog() {
        GameObject obj = GameObject.FindGameObjectWithTag("ChatContent");
        foreach(Transform tran in obj.transform) {
            Destroy(tran.gameObject);
        }
        gameObject.GetComponent<CanvasGroup>().alpha = 0;
        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        mainCanvas.SetActive(true);
        underBarCanvas.SetActive(true);
    }

    //save場所の管理
    //save元よりlog管理画面をインスタンスする

}
