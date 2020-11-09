using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using DG.Tweening;


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
    public int coTimeLimit;
    public string inputData;
    public List<string>playerNameList = new List<string>();
    public List<Player> playersList = new List<Player>();
    public InputField chatInputField;
    public GameObject chatContent;

    //色変更
    public Color [] color;
    public int boardColor;

    //課金関連
    public int superChatCount;//1Gmae3回まで無料


    private void Start() {
        if(chatInputField != null) {
            chatInputField.onEndEdit.AddListener(delegate { CreateChatNode(false, SPEAKER_TYPE.UNNKOWN); });
        }
    }
    private void Update() {
        if (Input.GetKeyUp(KeyCode.Return)) {
            CreateChatNode(false, SPEAKER_TYPE.UNNKOWN);
        }
    }

    ////////////////////////////
    ///メソッド関連
    ///////////////////////////

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

        //発言者（ETC、GM、Player）の分岐
        //GMの発言
        if (!comingOut && (speaker_Type == SPEAKER_TYPE.GAMEMASTER_OFFLINE || speaker_Type == SPEAKER_TYPE.GAMEMASTER_ONLINE)) {
            //GMは自分の世界のみでChatNodeを生成
            boardColor = 4;
            inputData = gameMasterChatManager.gameMasterChat;

            //データを格納
            ChatData chatData = new ChatData(inputData, 999, boardColor, speaker_Type.ToString(), ROLLTYPE.GM);
            chatData.chatType = CHAT_TYPE.GM;

            //チャットNodeの初期化
            ChatNode chatNode = null;
            //オンラインオフラインで分ける(GMChat
            //Offline
            if (speaker_Type == SPEAKER_TYPE.GAMEMASTER_OFFLINE) {
                chatNode = Instantiate(chatNodePrefab, chatContent.transform, false);
                //チャットデータをもとにちゃっとNodeに情報を持たせる
                chatNode.InitChatNode(chatData, 0, false, true);

                SetChatNode(chatNode, chatData, false);
                //OnLine
            } else if (speaker_Type == SPEAKER_TYPE.GAMEMASTER_ONLINE) {

                photonView.RPC(nameof(CreateGameMasterChatNode), RpcTarget.All, inputData, boardColor, comingOut);
            }

        } else {

            //チャットが空かつ通常チャットは生成しない
            if (!comingOut &&  String.IsNullOrWhiteSpace(chatInputField.text)) {
                return;
            }

            //Playerの発言
            //chatInputField.text == string.Empty 
            //死亡しているプレイヤー
            if (!myPlayer.live) {
                boardColor = 3;
                //狼用の発言
            } else if (inputView.wolfMode) {
                boardColor = 2;
                //青チャット
            } else if (inputView.superChat) {
                //メンバーシップ加入プレイヤーでかつ青チャットを3回打ってないプレイヤーは無料で青チャットを打つことができる
                if(superChatCount >= 3 || PlayerManager.instance.subscribe) {
                    PlayerManager.instance.UseCurrency(10);
                    gameMasterChatManager.gameManager.UpdateCurrencyText();
                } else {
                    superChatCount++;
                }

                CheckSuddenDeath();
                boardColor = 1;
                //通常のチャット
            } else {
                CheckSuddenDeath();
                boardColor = 0;
            }


            inputView.superChatBtnImage.color = inputView.btnColor[0];
            //inputView.superChatButtonText.text = "通常";
            inputView.superChat = false;

            //禁止Wordチェック
            inputData = checkTabooWard.StrMatch(chatInputField.text, taboolist.ngWordList);
            //InputFieldを初期化
            chatInputField.text = "";

            //発言を生成
            myPlayer.CreateNode(id, inputData, boardColor, comingOut,PlayerManager.instance.subscribe);

        }


    }

    /// <summary>
    /// 一度でも話したら突然死回避
    /// </summary>
    private void CheckSuddenDeath() {
        //一言でも発言したら参加しているものとして扱う
        if (!gameMasterChatManager.gameManager.timeController.isSpeaking) {
            Debug.Log("isSpeaking");
            gameMasterChatManager.gameManager.timeController.isSpeaking = true;
            gameMasterChatManager.gameManager.timeController.setSuddenDeath = true;
            gameMasterChatManager.gameManager.timeController.SetSuddenDeath();
        }

        //各プレイヤーが一度でも発言したらLocalPlayerにチェックを入れる
        //各世界でフラグのなく死亡していないプレイヤーを突然死扱いにする
        //PhotonPlayerと一致するPlayerButtonのLieveをfalseにする
        //発言チェックをリセットする
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


        ChatData chatData = new ChatData(inputData, 999, boardColor, SPEAKER_TYPE.GAMEMASTER_ONLINE.ToString(), ROLLTYPE.GM);
        chatData.chatType = CHAT_TYPE.GM;
        chatNode.InitChatNode(chatData, 0, comingOut, true);

        SetChatNode(chatNode, chatData, comingOut);
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
        //カミングアウト時は適応されない
        if (lastChatNode != null)
        {
            if (lastChatNode.playerID == chatData.playerID && !comingOut)
            {
                chatNode.iconObjLayoutElement.minHeight = 0f;
                chatNode.iconObjLayoutElement.preferredHeight = 0f;
                chatNode.iconObj.SetActive(false);
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
            if (coTimeLimit >= 4) {
                //Buttoncomponentにアクセスしないとinteractableが取れない
                //GameObject.FindGameObjectWithTag("COButton").GetComponent<Button>().interactable = false;

                //4回以上Coしたら押せないように制御する
                fillter.comingOutButton.interactable = false;
                DownCOPopUP();
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
        bool isChatSet = false;

        //チャット画面の位置が一番下でないとき全てのチャットをfalseにする
        if (dayOrderButton.isCheckNormalizedPosition) {
            return isChatSet;
        }

        //GameMasterChat
        if (chatNode.playerID == 999){
            isChatSet = true;
            return isChatSet;
        }


        if (!chatNode.chatWolf && chatNode.chatLive) {
            //通常チャットは全員が見れる
            isChatSet = true;
        }else if (!chatNode.chatLive && !myPlayer.live) {
            //死亡チャットは死亡したプレイヤーのみが見れる
            isChatSet = true;

        } else if (chatNode.chatLive && chatNode.chatWolf && myPlayer.wolfChat && myPlayer.live) {
            //狼チャットは狼でいて生きているプレイヤーだけが見れる
            isChatSet = true;

        }


        return isChatSet;
    }
    /// <summary>
    /// 強制的にCOPopUPを下におろす
    /// </summary>
    public void DownCOPopUP() {
        inputView.inputRectTransform.DOLocalMoveY(-67, 0.5f);
        inputView.viewport.DOSizeDelta(new Vector2(202f, 342f), 0.5f);
        StartCoroutine(gameMasterChatManager.gameManager.inputView.PopUpFalse());
    }


}

