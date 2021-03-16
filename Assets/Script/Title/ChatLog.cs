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
    public DayOrderButton dayOrderButton;

    ChatLogPlayerButton playerButton;
    public ChatLogPlayerButton playerButtonPrefab;
    public Transform buttonTran;
    public ChatNode chatNodePrefab;
    public NextDay nextDayPrefab;
    public int day;
    public Transform chatTran;
    
    List<ChatNode> chatNodeList = new List<ChatNode>();
    public ChatNode lastChatNode;

    public GameObject titleCanvas;
    public GameObject underCanvas;


    //折畳ボタン
    public Button foldingButton;
    public bool isFolding;
    public RectTransform mainRectTransform;
    public Transform inputRectTransform;
    public Sprite upBtnSprite;
    public Sprite downBtnSprite;
    public Image foldingImage;

    public Button exitButtn;


    private void Start() {
        foldingButton.onClick.AddListener(FoldingPosition);
        exitButtn.onClick.AddListener(CloseChatLog);
    }
     
    /// <summary>
    /// ログ保存したものを復元する
    /// </summary>
    public void CreateLogChat(SPEAKER_TYPE speaker_Type, string inputData, int playerID, int boardColor,string playerName, int iconNo) {

        ChatData chatData = new ChatData(inputData, playerID, boardColor, playerName, ROLLTYPE.ETC, iconNo);
        if (speaker_Type == SPEAKER_TYPE.GAMEMASTER_OFFLINE) {
            playerName = "GM";
            chatData.chatType = CHAT_TYPE.GM;
        }
        ChatNode chatNode = Instantiate(chatNodePrefab, chatTran, false);
        chatNode.InitChatNodeLog(chatData);
        SetChatNode(chatNode,chatData);
        chatNode.chatBoard.color = chatSystem.color[chatData.boardColor];
        chatNode.statusText.text = playerName;

        //GMだけ例外処理する
        if(iconNo != 9999) {
            chatNode.iconImage.color = ColorManger.instance.iconColorList[iconNo];
        }

        //ComingOutなら横幅を調節する
        if (inputData == "") {
            chatNode.chatObjLayoutElement.preferredWidth = 60;
            chatNode.chatObjLayoutElement.preferredHeight = 60;
        }

        chatNodeList.Add(chatNode);
    }

    /// <summary>
    /// 日付を作成します
    /// </summary>
    public void CreateNextDay() {
        day++;
        NextDay nextDayObj = Instantiate(nextDayPrefab, chatTran, false);
        nextDayObj.nextDayText.text = day + "日目";
        dayOrderButton.nextDaysList.Add(nextDayObj.gameObject);

    }

    /// <summary>
    /// Playerボタンを作成します
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="playerID"></param>
    /// <param name="gameManager"></param>
    public void CreatePlayerButton(string playerName, int playerID, string roll, int iconNo) {

        playerButton = Instantiate(playerButtonPrefab, buttonTran, false);
        playerButton.transform.SetParent(buttonTran);
        playerButton.playerNameText.text = playerName;
        playerButton.playerID = playerID;
        playerButton.rollText.text = roll;
        playerButton.iconNo = iconNo;

        if (iconNo != 9999) {
            playerButton.playerBtnImage.color = ColorManger.instance.iconColorList[iconNo];
        }
        if (PlayerManager.instance.myID == playerID) {
            playerButton.playerBtnImage.GetComponent<Outline>().enabled = true;
        }
        //PlayerButtonにフィルタ機能を追加
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

    void ResetRillter() {
        foreach (ChatNode chatObj in chatNodeList) {
           chatObj.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 折畳ボタンの制御
    /// </summary>
    public void FoldingPosition() {
        if (!isFolding) {
            //下へ
            isFolding = true;
            inputRectTransform.DOLocalMoveY(-72, 0.5f);
            mainRectTransform.DOSizeDelta(new Vector2(202f, 359.8427f), 0.5f);
            foldingImage.sprite = upBtnSprite;
            ResetRillter();
        } else {
            //上へ
            isFolding = false;
            inputRectTransform.DOLocalMoveY(0, 0.5f); 
            mainRectTransform.DOSizeDelta(new Vector2(202f, 287.3251f), 0.5f);
            foldingImage.sprite = downBtnSprite;
        }
    }


    /// <summary>
    /// チャットログを閉じるときに使われる
    /// </summary>
    public void CloseChatLog() {
        Destroy(gameObject);
        titleCanvas.SetActive(true);
        underCanvas.SetActive(true);
    }


    /// <summary>
    /// チャットノードをセットするときに配置などを変更する
    /// 
    /// </summary>
    public void SetChatNode(ChatNode chatNode, ChatData chatData) {
        //連続で同じプレイヤーがチャットした場合アイコンなどを消去する
        if (lastChatNode != null) {
            if (lastChatNode.playerID == chatData.playerID) {
                chatNode.iconObjLayoutElement.minHeight = 0f;
                chatNode.iconObjLayoutElement.preferredHeight = 0f;
                chatNode.iconObj.SetActive(false);
                chatNode.statusObj.SetActive(false);
            } else {
                chatNode.iconObjLayoutElement.preferredHeight = 20f;
                chatNode.iconObjLayoutElement.minHeight = 20f;
                chatNode.statusObj.SetActive(true);
            }
        }
        lastChatNode = chatNode;
    }

}
