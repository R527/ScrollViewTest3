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
    public TimeController timeController;
    public RollExplanation rollExplanation;
    public GameManager gameManager;
    public GameMasterChatManager gameMasterChatManager;
    public TIME timeType;
    public Fillter fillter;
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
    //public string[] comingOutPlayers;//CO状況を保存
    public List<string>playerNameList = new List<string>();
    public List<Player> playersList = new List<Player>();
    public int myID;
    public bool citizen;//市民か否か
    public ScrollRect scrollRect;
    public Button wolfButton;
    public Text MenbarViewText;


    //色変更
    public Color [] color;
    public int boardColor;

    //LIst管理
    public ChatListManager chatListManager;


    private void Update() {
        if (Input.GetKeyUp(KeyCode.Return)) {
            OnClickMineButton();
        }
    }
    
    public void OnClickMineButton() {
        Debug.Log("OnClickMineButton");
         CreateChatNode(false,SPEAKER_TYPE.UNNKOWN);
    }



    [PunRPC]
    private void CreateGameMasterChatNode(int id, string inputData, int boardColor, bool comingOut) {
        ChatNode chatNode = Instantiate(chatNodePrefab, content.transform, false);
        chatNode.transform.SetParent(content.transform);

        //Debug.Log("CreatNode:GM_ONLINE");

        ChatData chatData = new ChatData(id, inputData, 999, boardColor, SPEAKER_TYPE.GAMEMASTER_ONLINE.ToString(), ROLLTYPE.GM);
        chatData.chatType = CHAT_TYPE.GM;
        chatNode.InitChatNode(chatData, 0, comingOut);
        SetChatNode(chatNode, chatData, comingOut);
    }

    //第2引数がない場合は自動でNullを入れます。
    //指定がある場合はNullの代わりに別の引数が入る
    public  void CreateChatNode(bool comingOut, SPEAKER_TYPE speaker_Type = SPEAKER_TYPE.NULL) {
        Debug.Log("CreateChatNode");
        //チャットを無効化する
        if (chatInputField.text == "" && speaker_Type == SPEAKER_TYPE.NULL) {
            return;
        }
        //チャットを管理するためのID
        id++;


        //発言者（ETC、GM、Player）の分岐
        //GMの発言
        if (speaker_Type == SPEAKER_TYPE.GAMEMASTER_OFFLINE || speaker_Type == SPEAKER_TYPE.GAMEMASTER_ONLINE) {
            //GMは自分の世界のみでChatNodeを生成
            boardColor = 4;
            inputData = gameMasterChatManager.gameMasterChat;
            Debug.Log(inputData);

            //データを格納
            ChatData chatData = new ChatData(id, inputData, 999, boardColor, speaker_Type.ToString(), ROLLTYPE.GM);
            chatData.chatType = CHAT_TYPE.GM;

            //オンラインオフラインで分ける(GMChat
            ChatNode chatNode = null;
            //Offline
            if (speaker_Type == SPEAKER_TYPE.GAMEMASTER_OFFLINE) {
                chatNode = Instantiate(chatNodePrefab, content.transform, false);
                //チャットデータをもとにちゃっとNodeに情報を持たせる
                chatNode.InitChatNode(chatData, 0, false);
                SetChatNode(chatNode, chatData, false);
                //OnLine
            } else if(speaker_Type == SPEAKER_TYPE.GAMEMASTER_ONLINE) {
                photonView.RPC(nameof(CreateGameMasterChatNode), RpcTarget.All, id, inputData, boardColor, comingOut);
            }
            


            //Playerの発言
        } else {
            //色変更
            //ETC

            Debug.Log("playerChat");
            if (myPlayer != null) {
                Debug.Log("notNull");
                //死亡しているプレイヤー
                if (!myPlayer.live) {
                    Debug.Log("死亡");
                    boardColor = 3;
                    //狼用の発言
                } else if (fillter.wolfMode) {
                    Debug.Log("赤");
                    boardColor = 2;
                    //青チャット
                } else if (fillter.superChat) {
                    Debug.Log("青");
                    boardColor = 1;
                    //通常のチャット
                } else {
                    Debug.Log("通常");
                    boardColor = 0;
                }
                Debug.Log("色変更通過");
            }

            //禁止Wordチェック
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
    /// 生成されたChatNodeの設定
    /// 色の設定、Listの追加、連続投稿の制御
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
            } else if (fillter.wolfMode) {
                chatListManager.wlofList.Add(chatNode);
            } else if (fillter.superChat) {
                chatListManager.callOutList.Add(chatNode);
            } else {
                chatListManager.normalList.Add(chatNode);
            }
        }

        //PlayerごとにList管理
        if (chatData.rollType != ROLLTYPE.ETC && chatData.rollType != ROLLTYPE.GM) {
            if (myPlayer.live == false) {
                chatListManager.alldeathList[chatData.playerID - 1].Add(chatNode);
            } else if (fillter.wolfMode == true) {
                chatListManager.allwolfList[chatData.playerID - 1].Add(chatNode);
            } else {
                chatListManager.allnormalList[chatData.playerID - 1].Add(chatNode);
            }
            
        }

        //SetActiveを制御する
        chatNode.gameObject.SetActive(SetActiveChatObj());



        //if(chatListManager.isfilter == true) {
        //    chat.gameObject.SetActive(false);
        //}

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
            if (comingOut == true) {
                coTimeLimit++;
                if (coTimeLimit == 3) {
                    //Buttoncomponentにアクセスしないとinteractableが取れない
                    GameObject.FindGameObjectWithTag("COButton").GetComponent<Button>().interactable = false;
                }
            }
    }

    /// <summary>
    /// ChatObjectのActive状態を制御
    /// </summary>
    /// <returns></returns>
    public bool SetActiveChatObj() {
        bool isChatSet = true;

        //フィルター中でないなら狼チャットに参加できるか否かを判別する
        if (!chatListManager.isfilter) {
            if(gameManager.timeController.timeType == TIME.開始前) {
                isChatSet = true;
                return isChatSet;
            }
            if (myPlayer != null) {
                //市民の場合
                if (!myPlayer.wolfChat) {
                    //生存していてかつ狼or死亡チャット　もしくは自分が死んでいてかつ狼の発言の場合false
                    if ((myPlayer.live && (boardColor == 3 || boardColor == 2)) || (!myPlayer.live && boardColor == 2)) {
                        isChatSet = false;
                    }
                }

                //人狼の場合
                if (myPlayer.wolfChat) {
                    //自分が生きていている場合は死亡チャットをfalse　
                    if (myPlayer.live && boardColor == 3) {
                        isChatSet = false;
                    }
                }
            }
        } else {
            isChatSet = false;
        }
        
        return isChatSet;
    }


   }

