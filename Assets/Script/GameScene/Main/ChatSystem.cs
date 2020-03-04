﻿using Photon.Pun;
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
    public Callout callout;
    public ChatNode lastChatNode;
    public Player myPlayer;
    public CheckTabooWard checkTabooWard;
    public TimeController timeController;
    public RollExplanation rollExplanation;
    public GameManager gameManager;
    public TIME timeType;
    //共通項目
    public int id = 0;
    [SerializeField] public InputField chatInputField;
    [SerializeField] ChatNode chatNodePrefab;
    [SerializeField] GameObject content;
    public string comingOutImage;
    public int coTimeLimit;
    public int calloutTimeLimit;
    public string inputData;
    public NGList taboolist;
    public string[] comingOutPlayers;//CO状況を保存
    public List<string>playerNameList = new List<string>();
    public List<Player> playersList = new List<Player>();
    public int myID;
    public bool citizen;//市民か否か
    public ScrollRect scrollRect;
    public Button wolfButton;
    public Text MenbarViewText;

    //GameMaster関連
    public string gameMasterChat;

    //色変更
    public Color [] color;
    public int boardColor;
    public Wolfmode wolfmode;

    //LIst管理
    public ChatListManager chatListManager;



    //public void ChatSystemStartUp() {
    //    //参加Playerを追加する
    //    for(int i = 0; i < gameManager.numLimit; i++) {
    //        playerNameList.Add("Player" + (i + 1));
    //    }
    //}
    //MineかOthersなのかをボタンで振り分ける
    public void OnClickMineButton() {
        if (gameManager.numLimit > gameManager.num) {
            CreateChatNode( false, "unknown");
        } else {
            CreateChatNode(false);
        }
        
    }

    public void OnClickOtherButton() {
        //CreateChatNode(ChatRoll.OTHERS, 2, ROLLTYPE.狂人, false, playerNameList[2]);
    }


    public void OnClickMineCO(string rollName) {
        //CreateChatNode(ChatRoll.MINE, 1, ROLLTYPE.人狼, true, rollName);
    }

    public void OnClickOtherCO(string rollName) {
        //CreateChatNode(ChatRoll.OTHERS, 2, ROLLTYPE.占い師, true, rollName);
    }

    public void GameMasterChatNode() {
        CreateChatNode(false,"GameMaster");
    }
    private void Update() {
        if (Input.GetKeyUp(KeyCode.Return)) {
            OnClickMineButton();
        } else if (Input.GetKeyUp(KeyCode.I)) {
            OnClickOtherButton();
        }

        //if (scrollRect.verticalNormalizedPosition >= 0.0f) {
        //    scrollRect.StopMovement();
        //    scrollRect.enabled = false;
        //}

        //Debug.Log(scrollRect.verticalNormalizedPosition);
    }

    //第2引数がない場合は自動でNullを入れます。
    //指定がある場合はNullの代わりに別の引数が入る
    private void CreateChatNode(bool comingOut, string rollName = "null") {
        if (chatInputField.text == "" && rollName == "null") {
            return;
        }
        //チャットを管理するためのID
        id++;


        //発言者（ETC、GM、Player）の分岐
        //GMの発言
        if (rollName == "GameMaster") {
            //GMは自分の世界のみでChatNodeを生成
            boardColor = 4;
            inputData = GameMasterChat();
            Debug.Log(inputData);

            ChatData chatData = new ChatData(id, inputData, 999, boardColor, rollName, ROLLTYPE.GM);
            chatData.chatType = CHAT_TYPE.GM;
            ChatNode chatNode = Instantiate(chatNodePrefab, content.transform, false);
            chatNode.InitChatNode(chatData, 0, comingOut);

            Debug.Log("CreateNode: GM");

            SetChatNode(chatNode, chatData, comingOut);

            //Playerの発言
        } else {
            //色変更
            //ETC
            if (rollName == "unknown") {
                boardColor = 0;
            } else if (myPlayer != null) {
                //死亡しているプレイヤー
                if (myPlayer.live == false) {
                    boardColor = 3;
                    //狼用の発言
                } else if (wolfmode.wolf == true) {
                    boardColor = 2;
                    //青チャット
                } else if (callout.callOut == true) {
                    boardColor = 1;
                    //通常のチャット
                } else {
                    boardColor = 0;
                }
            }

            inputData = checkTabooWard.StrMatch(chatInputField.text, taboolist.ngWordList);
            //InputFieldを初期化
            chatInputField.text = "";

            //発言を生成
            if (myPlayer != null) {
                //PlayerはRPCを利用してすべての世界でChatNodeを生成
                myPlayer.CreateNode(id, inputData, boardColor, comingOut);
            } else {
                //ETCはネットワーク・インスタンスを利用してすべての世界でChatNodeを生成
                ChatData chatData = new ChatData(id, inputData, PhotonNetwork.LocalPlayer.ActorNumber, boardColor, PhotonNetwork.LocalPlayer.NickName, ROLLTYPE.ETC);
                if (photonView.IsMine) {
                    chatData.chatType = CHAT_TYPE.MINE;
                } else {
                    chatData.chatType = CHAT_TYPE.OTHERS;
                }

                GameObject chatObj = PhotonNetwork.Instantiate("Prefab/Game/ChatNode", content.transform.position, content.transform.rotation);
                chatObj.transform.SetParent(content.transform);

                ChatNode chatNode = chatObj.GetComponent<ChatNode>();
                chatNode.InitChatNode(chatData, PhotonNetwork.LocalPlayer.ActorNumber, comingOut);

                Debug.Log("CreateNode : ETC");

                SetChatNode(chatNode, chatData, comingOut);

            }
        }
    }

    /// <summary>
    /// 生成されたChatNodeの設定？？
    /// 一旦放置（192～195）がわからない
    /// </summary>
    /// <param name="chatNode"></param>
    /// <param name="chatData"></param>
    /// <param name="comingOut"></param>
    public void SetChatNode(ChatNode chatNode, ChatData chatData, bool comingOut) {
    //    Debug.Log("SetChatNode:問題あり");

    //    float tempPosition = content.transform.localPosition.y;
    //    if (tempPosition <= -280) {
    //        Vector2 returnPos = new Vector2(content.transform.localPosition.x, tempPosition + chatNode.gameObject.transform.localPosition.y);
    //        content.transform.localPosition = returnPos;
    //    }

        //ボードの色を変える
        chatNode.chatBoard.color = color[chatData.boardColor];

        //Chatの種類ごとにList管理
        if (chatData.rollType == ROLLTYPE.ETC) {

            //GM
        } else if (chatData.rollType == ROLLTYPE.GM) {
            chatListManager.gameMasterList.Add(chatNode);

            //Player
        } else {
            if (myPlayer.live == false) {
                chatListManager.deathList.Add(chatNode);
            } else if (wolfmode.wolf == true) {
                chatListManager.wlofList.Add(chatNode);
            } else if (callout.callOut == true) {
                chatListManager.callOutList.Add(chatNode);
            } else {
                chatListManager.normalList.Add(chatNode);
            }
        }

        //PlayerごとにList管理
        if (chatData.rollType != ROLLTYPE.ETC && chatData.rollType != ROLLTYPE.GM) {
            chatListManager.allPlayerList[chatData.playerID].Add(chatNode);
        }

        //SetActiveを制御する
        chatNode.gameObject.SetActive(SetActiveChatObj());

        ////市民の場合
        //if (myPlayer != null) {
        //    if (myPlayer.rollType != ROLLTYPE.人狼) {
        //        if (myPlayer.live == true) {
        //            if (boardColor == color[3] || boardColor == color[2]) {
        //                chat.gameObject.SetActive(false);
        //            }
        //        }
        //        if (myPlayer.live == false) {
        //            if (boardColor == color[2]) {
        //                chat.gameObject.SetActive(false);
        //            }
        //        }
        //    }

        //    //人狼の場合
        //    if (myPlayer.rollType == ROLLTYPE.人狼) {
        //        if (myPlayer.live == true) {
        //            if (boardColor == color[3]) {
        //                chat.gameObject.SetActive(false);
        //            }
        //        }
        //    }
        //}

        //if(chatListManager.isfilter == true) {
        //    chat.gameObject.SetActive(false);
        //}

        //playerが連続でチャットを投稿した場合、アイコンObj等を削除する
        if (lastChatNode != null) {
            if (lastChatNode.playerID == chatData.playerID) {
                chatNode.iconObjLayoutElement.minHeight = 0f;
                chatNode.iconObjLayoutElement.preferredHeight = 0f;
                chatNode.statusObj.SetActive(false);
            } else {
                chatNode.iconObjLayoutElement.preferredHeight = 20f;
                chatNode.iconObjLayoutElement.minHeight = 20f;
                chatNode.statusObj.SetActive(true);
            }
        }
        lastChatNode = chatNode;
        

        //ComingOutと吹き出しの制御
            if (comingOut == true) {
                coTimeLimit++;
                if (coTimeLimit == 3) {
                    //Buttoncomponentにアクセスしないとinteractableが取れない
                    GameObject.FindGameObjectWithTag("COButton").GetComponent<Button>().interactable = false;
                }
            }
            if(chatData.boardColor == 1) {
                calloutTimeLimit++;
                if(calloutTimeLimit == 2) {
                    GameObject.FindGameObjectWithTag("CallOutButton").GetComponent<Button>().interactable = false;
                    callout.CallOutText.text = "通常";
                    //CallOutクラスのcallOutboolを変更
                    callout.callOut = false;
                }
            }
    }

    /// <summary>
    /// ChatObjectのActive状態を制御
    /// </summary>
    /// <returns></returns>
    public bool SetActiveChatObj() {
        bool isChatSet = true;
        
        if (myPlayer != null) {
            //市民の場合
            if (myPlayer.rollType != ROLLTYPE.人狼) {
                if ((myPlayer.live == true && (boardColor == 3 || boardColor == 2)) || (!myPlayer.live && boardColor == 2)) {
                    isChatSet = false;
                }
            }

            //人狼の場合
            if (myPlayer.rollType == ROLLTYPE.人狼) {
                if (myPlayer.live && boardColor == 3) {
                    isChatSet = false;
                    }
            }
        }
        return isChatSet;
    }

    /// <summary>
    /// Debug用
    /// </summary>
    /// <param name="id"></param>
    public void OnClickPlayerID(int id = 0) {
        myID = id;
        foreach(Player player in playersList) {
            if(player.playerID == myID) {
                myPlayer = player;
                MenbarViewText.text = myPlayer.rollType.ToString();
                rollExplanation.rollExplanationButton.interactable = true;
            }
        }
    }

    /// <summary>
    /// GameMasterのチャット管理
    /// </summary>
    public string GameMasterChat() {
        string gmNode = "";
        switch (timeController.timeType) {
            case TIME.昼:
                gmNode = "おはようございます。" + "昨夜は○○が●されました。";
                break;
            case TIME.投票時間:
                gmNode = "投票の時間です";
                break;
            case TIME.夜の行動:
                gmNode = "占え";
                break;
        }
        return gmNode;
    }

}

////この下が分からない。
////enumは定数をまとめることができる
////状態を登録する
////列挙型
////ChatRollが登録された状態を管理する
//public enum ChatRoll {
//    EXT,
//    MINE,
//    OTHERS,
//    GM,
//}

////Classはこのクラスが呼び出されると初めに実行される？
////クラス名とメソッド名を同じにすることをコンストラクト
////クラスが初期化されると自動的に実行される
//[System.Serializable]
//public class ChatData {
//    public int id;
//    public ChatRoll roll = ChatRoll.EXT;
//    public string body;
//    public string rollName;
//    public int playerNum;
//    public Color boardColor;
//    public string playerName;
//    public ROLLTYPE rollType;

//    public ChatData(int id, ChatRoll roll, string body, string rollName, int playerNum, Color boardColor, string playerName, ROLLTYPE rollType) {
//        this.id = id;
//        this.roll = roll;
//        this.body = body;
//        this.rollName = rollName;
//        this.playerNum = playerNum;
//        this.boardColor = boardColor;
//        this.playerName = playerName;
//        this.rollType = rollType;
//    }
//}