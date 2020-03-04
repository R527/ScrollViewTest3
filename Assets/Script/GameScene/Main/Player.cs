using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

/// <summary>
/// MenbarViewにあるPlayer情報などをまとめてる
/// </summary>
public class Player : MonoBehaviourPunCallbacks {


    //class
    public ROLLTYPE rollType = ROLLTYPE.ETC;
    public CHAT_TYPE chatType = CHAT_TYPE.MINE;
    public GameManager gameManager;
    public ChatSystem chatSystem;

    //main
    public int playerID;
    public Text playerText;
    public string playerName;
    public Button playerButton;
    public bool live;//生死
    public bool fortune;//占い結果
    public bool spiritual;//霊能結果
    public bool wolf;//狼か否か
    public bool wolfCamp;//狼陣営か否か
    public Button wolfButton;
    public Text MenbarViewText;

    public ChatNode chatNodePrefab;//チャットノード用のプレふぁぶ
    public int iconNo;//アイコンの絵用
    private  Transform tran;

    //仮
    public bool def;


    /// <summary>
    /// MenbarViewにあるPlayerButtonの設定と役職ごとの判定を追加
    /// </summary>
    public void PlayerSetUp() {
        Debug.Log("Setup");
        live = true;


        chatSystem = GameObject.FindGameObjectWithTag("ChatSystem").GetComponent<ChatSystem>();
        tran = GameObject.FindGameObjectWithTag("ChatContent").transform;


        //自分と他人を分ける分岐
        if (photonView.IsMine) {
            chatSystem.myPlayer = this;
            playerText.text = rollType.ToString();
            playerName = PhotonNetwork.LocalPlayer.NickName;
            //Networkの自分の持っている番号を追加
            iconNo = PhotonNetwork.LocalPlayer.ActorNumber;

            //ラムダ式で引数を充てる。
            playerButton.onClick.AddListener(() => gameManager.PlayerButton(rollType, playerID, live, fortune, def));

            //このクラスは参加人数が9人の場合81個ある状態になる。
            //上の9人分を除いた、72個分をこちらで処理する
        } else {
            //自分の世界に作られたほかのPlayerの設定
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                //photonView.OwnerActorNrは自分の通し番号
                //player.ActorNumberもネットワーク上の自分の番号
                //playerで回すから各プレイヤーの番号を検索できる
                if (player.ActorNumber == photonView.OwnerActorNr) {
                    playerID = player.ActorNumber;
                    rollType = (ROLLTYPE)player.CustomProperties["roll"];
                    playerText.text = rollType.ToString();
                    playerName = player.NickName;
                    iconNo = player.ActorNumber;
                }
            }
        }

        //役職ごとの判定を追加
        if (rollType == ROLLTYPE.人狼) {
            fortune = true;
            spiritual = true;
            wolf = true;
            wolfCamp = true;
        } else if(rollType == ROLLTYPE.狂人) {
            wolfCamp = true;
        }
    }


    /// <summary>
    /// ChatNodeの生成準備
    /// 発言はPlayerクラスで行われる
    /// </summary>
    /// <param name="id"></param>
    /// <param name="inputData"></param>
    /// <param name="boardColor"></param>
    /// <param name="comingOut"></param>
    public void CreateNode(int id, string inputData, int boardColor, bool comingOut) {
        Debug.Log("CreateNode: Player");
        photonView.RPC(nameof(CreateChatNodeFromPlayer), RpcTarget.All, id, inputData, boardColor, comingOut);
    }


    [PunRPC]
    public void CreateChatNodeFromPlayer(int id, string inputData, int boardColor, bool comingOut) {
        Debug.Log("RPC START");

        ChatData chatData = new ChatData(id, inputData, playerID, boardColor, playerName, rollType);
        Debug.Log(chatData.inputData);

        //発言者の分岐
        if (photonView.IsMine) {
            chatData.chatType = CHAT_TYPE.MINE;
        } else {
            chatData.chatType = CHAT_TYPE.OTHERS;
        }

        ChatNode chatNode = Instantiate(chatNodePrefab, tran, false);
        chatNode.InitChatNode(chatData, iconNo, comingOut);
        Debug.Log(comingOut);
        chatSystem.SetChatNode(chatNode, chatData, comingOut);
        Debug.Log("Player RPC END");
    }
}

