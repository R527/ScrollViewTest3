﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class PlayerButton : MonoBehaviourPunCallbacks {


    //class
    public GameManager gameManager;


    //main
    public Button playerButton;
    public Text playerText;
    public Image iconImage;

    public int playerID;
    public string playerName;
    public int iconNo;//アイコンの絵用
    public ROLLTYPE rollType = ROLLTYPE.ETC;
    public bool live;//生死 trueで生存している
    
    //後からもらう
    public bool fortune;//占い結果 true=黒
    public bool spiritual;//霊能結果　true = 黒
    public bool wolf;//狼か否か
    public bool wolfChat;//狼チャットに参加できるかどうか
    public bool wolfCamp;//狼陣営か否か

    private Transform menbartran;

    public IEnumerator SetUp(string playerName,int iconNo, int playerID,GameManager gameManager) {
        this.gameManager = gameManager;
        this.playerName = playerName;
        this.iconNo = iconNo;
        this.playerID = playerID;
        live = true;
        playerButton.onClick.AddListener(() => OnClickPlayerButton());
        gameObject.GetComponent<Outline>().enabled = false;

        yield return new WaitForSeconds(2.0f);
        menbartran = GameObject.FindGameObjectWithTag("MenbarContent").transform;
        transform.SetParent(menbartran);
    }


    /// <summary>
    ///投票、フィルター、夜の行動を制御 
    /// </summary>
    public void OnClickPlayerButton() {
        Debug.Log(".ActorNumber" + PhotonNetwork.LocalPlayer.ActorNumber);
        Debug.Log("playerID" + playerID);

        //フィルター機能ON
        if (gameManager.chatListManager.isfilter) {
            gameManager.chatListManager.OnFilter(playerID);

            //フィルター機能Off時
            //生存していて、自分以外のプレイヤーを指定
        } else if (live && !gameManager.chatListManager.isfilter && PhotonNetwork.LocalPlayer.ActorNumber != playerID) {
            //フィルター機能がOFFの時は各時間ごとの機能をする
            Debug.Log(gameManager.timeController.timeType);

            switch (gameManager.timeController.timeType) {

                //投票する処理
                case TIME.投票時間:
                    //ここでは投票をするだけで他プレイヤーとの比較判定はしない
                    //比較はVoteCount.csで行われる
                    if (!gameManager.chatSystem.myPlayer.isVoteFlag && live) {
                        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                            //Photonが用意しているActorNumberという数字とPLayerクラスが持っているPlayerIDが合致したら
                            //playerIDはActorNumberからもらっているから合致したら同じ番号を持っているクラスだといえる
                            if (player.ActorNumber == playerID) {

                                //最新の投票数を取得する
                                int voteNum = 0;
                                string voteName = string.Empty;
                                bool votingCompleted = false;
                                //指定されたplayerのキーに登録する
                                if (player.CustomProperties.TryGetValue("voteNum", out object voteCountObj)) {
                                    voteNum = (int)voteCountObj;
                                }
                                voteNum++;

                                //投票したプレイヤーの名前を登録します
                                if (player.CustomProperties.TryGetValue("voteName", out object voteNameObj)) {
                                    voteName = (string)voteNameObj;
                                }
                                voteName += gameManager.chatSystem.myPlayer.playerName + ",";

                                //一人のプレイヤーが投票を完了したらtrue
                                if (player.CustomProperties.TryGetValue("votingCompleted", out object votingCompletedObj)) {
                                    votingCompleted = (bool)votingCompletedObj;
                                }
                                votingCompleted = true;

                                var propertiers = new ExitGames.Client.Photon.Hashtable {
                                    {"voteName", voteName },
                                    {"voteNum", voteNum },
                                    {"votingCompleted",votingCompleted }
                                };

                                player.SetCustomProperties(propertiers);

                                Debug.Log((int)player.CustomProperties["voteNum"]);
                                Debug.Log((string)player.CustomProperties["voteName"]);
                                Debug.Log((bool)player.CustomProperties["votingCompleted"]);

                                //投票数の表示をディクショナリーで管理
                                //voteCount.voteCountTable[playerID] = voteNum;


                                //Debug.Log(voteCount.voteNameTable[playerID]);
                                //投票のチャット表示
                                gameManager.gameMasterChatManager.Voted(player, live);

                                Debug.Log("投票完了");


                            }
                        }
                        gameManager.chatSystem.myPlayer.isVoteFlag = true;
                    }
                    break;

                //夜の行動をとる処理
                case TIME.夜の行動:
                    Debug.Log("夜の行動");
                    if (!gameManager.chatSystem.myPlayer.isRollAction) {

                        gameManager.gameMasterChatManager.RollAction(playerID, live, fortune, wolf);
                        gameManager.chatSystem.myPlayer.isRollAction = true;
                    }
                    break;

                //マスターのみプレイヤーを退出できる
                case TIME.開始前:
                    Debug.Log("強制退出");
                    if (PhotonNetwork.IsMasterClient) {
                        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                            if (player.ActorNumber == playerID) {
                                gameManager.gameMasterChatManager.ForcedEvictionRoom(player);
                                Debug.Log(player.NickName);
                            }
                        }
                    }
                    break;
            }
        }
    }

    //private void Update() {
    //    if(menbartran != null) {
           
    //    }
    //}
}
