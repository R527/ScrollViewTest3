using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


/// <summary>
/// チャットの内容や設定を変更するだけで発言はPlayerクラスで行われる
/// </summary>
public class ChatSystem : MonoBehaviourPunCallbacks {


    //他クラス
    public ChatNode lastChatNode;
    public Player myPlayer;
    public CheckTabooWard checkTabooWard;
    public GameMasterChatManager gameMasterChatManager;
    public Fillter fillter;
    public ChatListManager chatListManager;
    public NGList taboolist;
    public ChatNode chatNodePrefab;
    public InputView inputView;
    public DayOrderButton dayOrderButton;
    //共通項目
    public int id = 0;
    public int myID;//??
    public int coTimeLimit;
    public int calloutTimeLimit;
    public string inputData;
    public List<string>playerNameList = new List<string>();
    public List<Player> playersList = new List<Player>();
    public InputField chatInputField;
    public GameObject chatContent;

    //色変更
    public Color [] color;
    public int boardColor;

    //test
    public string testName;


    private void Update() {
        if (Input.GetKeyUp(KeyCode.Return)) {
            OnClickMineButton();
        }
    }
    
    ////////////////////////////
    ///メソッド関連
    ///////////////////////////

    /// <summary>
    /// 自分のチャットを生成するときに呼ばれる
    /// </summary>
    public void OnClickMineButton() {
        Debug.Log("OnClickMineButton");
         CreateChatNode(false,SPEAKER_TYPE.UNNKOWN);
    }

    //第2引数がない場合は自動でNullを入れます。
    //指定がある場合はNullの代わりに別の引数が入る
    //SPEAKER_TYPE speaker_Type = SPEAKER_TYPE.NULL
    /// <summary>
    /// チャットの生成をまとめている。
    /// GMチャットとPlayerのチャットを分けたり色分けしたりDataを入力するなど
    /// </summary>
    /// <param name="comingOut"></param>
    /// <param name="speaker_Type"></param>
    public void CreateChatNode(bool comingOut,SPEAKER_TYPE speaker_Type) {
        //Debug.Log("CreateChatNode");

        //発言者（ETC、GM、Player）の分岐
        //GMの発言
        if (!comingOut && (speaker_Type == SPEAKER_TYPE.GAMEMASTER_OFFLINE || speaker_Type == SPEAKER_TYPE.GAMEMASTER_ONLINE)) {
            //GMは自分の世界のみでChatNodeを生成
            boardColor = 4;
            inputData = gameMasterChatManager.gameMasterChat;
            //Debug.Log(inputData);

            //データを格納
            ChatData chatData = new ChatData(inputData, 999, boardColor, speaker_Type.ToString(), ROLLTYPE.GM);
            chatData.chatType = CHAT_TYPE.GM;

            //チャットNodeの初期化
            ChatNode chatNode = null;
            //オンラインオフラインで分ける(GMChat
            //Offline
            //Debug.Log("speaker_Type" + speaker_Type);
            if (speaker_Type == SPEAKER_TYPE.GAMEMASTER_OFFLINE) {
                chatNode = Instantiate(chatNodePrefab, chatContent.transform, false);
                //チャットデータをもとにちゃっとNodeに情報を持たせる
                chatNode.InitChatNode(chatData, 0, false);

                SetChatNode(chatNode, chatData, false);
                //OnLine
            } else if(speaker_Type == SPEAKER_TYPE.GAMEMASTER_ONLINE ) {
                
                photonView.RPC(nameof(CreateGameMasterChatNode), RpcTarget.All, inputData, boardColor, comingOut);
            }
            
        } else {

            //ログ作成部分
            //if (logTest) {
            //    CreateLogChat();
            //    return;
            //}
            //Playerの発言
            Debug.Log(chatInputField.text);
            //チャットが空かつ通常チャットは生成しない
            if (!comingOut && chatInputField.text == string.Empty) {
                return;
            }

            //死亡しているプレイヤー
            if (!myPlayer.live) {
                Debug.Log("死亡");
                boardColor = 3;
                //狼用の発言
            } else if (inputView.wolfMode) {
                Debug.Log("赤");
                boardColor = 2;
                //青チャット
            } else if (inputView.superChat) {
                Debug.Log("青");
                boardColor = 1;
                //通常のチャット
            } else {
                Debug.Log("通常");
                boardColor = 0;
            }
            Debug.Log("色変更通過");
            Debug.Log("boardcolor" + boardColor);

            inputView.superChatButtonText.text = "通常";
            inputView.superChat = false;

            //禁止Wordチェック
            inputData = checkTabooWard.StrMatch(chatInputField.text, taboolist.ngWordList);
            //InputFieldを初期化
            chatInputField.text = "";

            //発言を生成
            myPlayer.CreateNode(id, inputData, boardColor, comingOut);


            //一言でも発言したら参加しているものとして扱う
            if (!gameMasterChatManager.gameManager.timeController.isSpeaking) {
                gameMasterChatManager.gameManager.timeController.isSpeaking = true;
            }
        }
    }

    /// <summary>
    /// ログ保存したものを復元するよう
    /// </summary>
    public void CreateLogChat() {
        ChatData chatData = new ChatData(inputData, 999, boardColor, testName, ROLLTYPE.ETC);
        ChatNode chatNode = Instantiate(myPlayer.chatNodePrefab, myPlayer.chatTran, false);

        chatNode.InitChatNode(chatData, 0, false);
        SetChatNode(chatNode, chatData, false);
        Debug.Log("復元完了");
    }
    /// <summary>
    /// GMチャットを生成する
    /// </summary>
    /// <param name="id"></param>
    /// <param name="inputData"></param>
    /// <param name="boardColor"></param>
    /// <param name="comingOut"></param>
    [PunRPC]
    private void CreateGameMasterChatNode(string inputData, int boardColor, bool comingOut) {
        ChatNode chatNode = Instantiate(chatNodePrefab, chatContent.transform, false);
        chatNode.transform.SetParent(chatContent.transform);

        //Debug.Log("CreatNode:GM_ONLINE");

        ChatData chatData = new ChatData(inputData, 999, boardColor, SPEAKER_TYPE.GAMEMASTER_ONLINE.ToString(), ROLLTYPE.GM);
        chatData.chatType = CHAT_TYPE.GM;
        chatNode.InitChatNode(chatData, 0, comingOut);

        SetChatNode(chatNode, chatData, comingOut);
        Debug.Log("CreateGameMasterChatNode");
    }

    /// <summary>
    /// 生成されたChatNodeの設定
    /// 色の設定、Listの追加、連続投稿の制御
    /// </summary>
    /// <param name="chatNode"></param>
    /// <param name="chatData"></param>
    /// <param name="comingOut"></param>
    public void SetChatNode(ChatNode chatNode, ChatData chatData, bool comingOut) {

        //ゲーム中に発言された内容を保存する
        PlayerManager.instance.saveChatLog += PlayerManager.instance.ConvertStringToChatData(chatData) + "%";
        //Debug.Log(PlayerManager.instance.saveChatLog);


        //ボードの色を変える
        chatNode.chatBoard.color = color[chatData.boardColor];

        //Chatの種類ごとにList管理
        if (chatData.rollType == ROLLTYPE.ETC) {

            //GM
        } else if (chatData.rollType == ROLLTYPE.GM) {
            chatListManager.gameMasterList.Add(chatNode);

            //Player
        } else {
            if (!chatNode.chatLive) {
                chatListManager.deathList.Add(chatNode);
            } else if (chatNode.chatWolf) {
                chatListManager.wlofList.Add(chatNode);
            } else {
                chatListManager.normalList.Add(chatNode);
            }
        }

        //PlayerごとにList管理
        if (chatData.rollType != ROLLTYPE.ETC && chatData.rollType != ROLLTYPE.GM) {
            if (!chatNode.chatLive) {
                chatListManager.alldeathList[chatData.playerID - 1].Add(chatNode);
            } else if (chatNode.chatWolf) {
                chatListManager.allwolfList[chatData.playerID - 1].Add(chatNode);
            } else {
                chatListManager.allnormalList[chatData.playerID - 1].Add(chatNode);
            }
        }




        //playerが連続でチャットを投稿した場合、アイコンObj等を削除する
        if (lastChatNode != null)
        {
            if (lastChatNode.playerID == chatData.playerID)
            {
                chatNode.iconObjLayoutElement.minHeight = 0f;
                chatNode.iconObjLayoutElement.preferredHeight = 0f;
                chatNode.statusObj.SetActive(false);
            }
            else
            {
                chatNode.iconObjLayoutElement.preferredHeight = 20f;
                chatNode.iconObjLayoutElement.minHeight = 20f;
                chatNode.statusObj.SetActive(true);
            }
        }
        lastChatNode = chatNode;
        

        //ComingOutと吹き出しの制御
        if (comingOut) {
            coTimeLimit++;
            if (coTimeLimit == 4) {
                //Buttoncomponentにアクセスしないとinteractableが取れない
                //GameObject.FindGameObjectWithTag("COButton").GetComponent<Button>().interactable = false;
                fillter.comingOutButton.interactable = false;
            }
        }

        //SetActiveを制御する
        chatNode.gameObject.SetActive(SetActiveChatObj(chatNode));
    }

    /// <summary>
    /// ChatObjectのActive状態を制御
    /// </summary>
    /// <returns></returns>
    public bool SetActiveChatObj(ChatNode chatNode) {
        Debug.Log("SetActiveChatObj");
        bool isChatSet = false;

        //チャット画面の位置が一番下でないとき全てのチャットをfalseにする
        if (dayOrderButton.isCheckNormalizedPosition) {
            Debug.Log("isCheckNormalizedPosition");
            return isChatSet;
        }
        Debug.Log("isCheckNormalizedPosition2");

        //GameMasterChat
        if (chatNode.playerID == 999){
            isChatSet = true;
            Debug.Log("GMChat");
            return isChatSet;
        }
        if (!chatNode.chatWolf && chatNode.chatLive) {
            //通常チャットは全員が見れる
            isChatSet = true;
            Debug.Log("通常チャット");
        }else if (!chatNode.chatLive && !myPlayer.live) {
            //死亡チャットは死亡したプレイヤーのみが見れる
            isChatSet = true;
            Debug.Log("死亡チャット");

        } else if (chatNode.chatLive && chatNode.chatWolf && myPlayer.wolfChat && myPlayer.live) {
            //狼チャットは狼でいて生きているプレイヤーだけが見れる
            isChatSet = true;
            Debug.Log("狼チャット");

        }
        Debug.Log(isChatSet);
        return isChatSet;
    }
}

