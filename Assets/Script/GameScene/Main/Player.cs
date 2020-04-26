using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;


/// <summary>
/// MenbarViewにあるPlayer情報のまとめ
/// MenbarViewにあるボタンの制御
/// </summary>
public class Player : MonoBehaviourPunCallbacks {


    //class
    public ROLLTYPE rollType = ROLLTYPE.ETC;
    public GameManager gameManager;
    public ChatSystem chatSystem;
    public VoteCount voteCount;
    public TimeController timeController;

    //main
    public int playerID;
    public Text playerText;
    public string playerName;
    public Button playerButton;
    public bool live;//生死 trueで生存している
    public bool fortune;//占い結果 true=黒
    public bool spiritual;//霊能結果　true = 黒
    public bool wolf;//狼か否か
    public bool wolfChat;//狼チャットに参加できるかどうか
    public bool wolfCamp;//狼陣営か否か
    public ChatNode chatNodePrefab;//チャットノード用のプレふぁぶ
    public int iconNo;//アイコンの絵用
    private Transform tran;

    //投票関連
    public bool isVoteFlag; //投票を下か否か　falseなら非投票
    public int voteNum;//そのPlayerの投票数
    public string voteName;//投票したプレイヤーの名前を記載
    public bool votingCompleted;//投票完了
    //夜の行動
    public bool isRollAction;//夜の行動をとったか否か


    //masterのみCheckOnLine用
    private float checkTimer;
    private int checkNum;


    /////////////////////////
    ///SetUp関連
    /////////////////////////

    /// <summary>
    /// Playerが生成されたらそれぞれの設定を追加する
    /// 役職などの詳細は後程設定する
    /// </summary>

    private void Start() {
        Debug.Log("FirstSetUp");
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        chatSystem = GameObject.FindGameObjectWithTag("ChatSystem").GetComponent<ChatSystem>();
        tran = GameObject.FindGameObjectWithTag("ChatContent").transform;
        transform.SetParent(gameManager.menbarContent);
        playerButton.onClick.AddListener(() => OnClickPlayerButton());

        //生存者にする
        live = true;

        transform.SetParent(gameManager.menbarContent);
        //自分の世界に生成されたPlayerのオブジェクトなら→Aさんの世界のPlayerAが行う処理
        if (photonView.IsMine) {
            Debug.Log("IsMine");
            chatSystem.myPlayer = this;
            playerName = PhotonNetwork.LocalPlayer.NickName;
            iconNo = PhotonNetwork.LocalPlayer.ActorNumber;
            playerID = PhotonNetwork.LocalPlayer.ActorNumber;
            //SetPlayerName();
            playerText.text =  playerName;

        } else {
            //他人の世界に生成された自分のPlayerオブジェクトなら→Bさんの世界のPlayerAが行う処理
            StartCoroutine(SetOtherPlayer());
        }
    }


    /// <summary>
    /// MenbarViewにあるPlayerButtonの設定と役職ごとの判定を追加
    /// </summary>
    public void PlayerSetUp(GameManager gameManager,VoteCount voteCount,TimeController timeController) {
        this.voteCount = voteCount;
        this.timeController = timeController;

        //Aさんの世界のAさんの処理
        if (photonView.IsMine) {
            //voteCountをプロパティーにセット
            var properties = new ExitGames.Client.Photon.Hashtable {
                {"voteNum", voteNum },
                {"live", live }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

            //このクラスは参加人数が9人の場合81個ある状態になる。
            //上の9人分を除いた、72個分をこちらで処理する
        } else {
            //他の世界にいるAさんの処理
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                //photonView.OwnerActorNrは自分の通し番号
                //player.ActorNumberもネットワーク上の自分の番号
                //playerで回すから各プレイヤーの番号を検索できる
                if (player.ActorNumber == photonView.OwnerActorNr) {
                    rollType = (ROLLTYPE)player.CustomProperties["roll"];
                }
            }
            //自分以外のプレイヤーは青色のラインがない
            //gameObject.GetComponent<Outline>().enabled = false;
        }

        //役職ごとの判定を追加
        if (rollType == ROLLTYPE.人狼) {
            fortune = true;
            spiritual = true;
            wolf = true;
            wolfChat = true;
            wolfCamp = true;
        } else if(rollType == ROLLTYPE.狂人) {
            wolfCamp = true;
        }

        playerText.text = rollType.ToString() + playerName;
    }


    ////////////////
    ///監視関連
    ////////////////

    /// <summary>
    /// オンラインチェック用
    /// </summary>
    private void Update() {

        //masterのみ
        if (PhotonNetwork.IsMasterClient) {
            if (timeController != null && timeController.timeType == TIME.投票時間) {
                CheckVoted();
            }
        }
        

        //全員が投票完了したら時短成立


    }

    ///////////////////////
    ///メソッド関連
    ////////////////////////


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


    /// <summary>
    /// PunRPCでChatNodeを生成する　プレイヤーがMineかOhtersの設定　ちゃっとDataをの設定
    /// </summary>
    /// <param name="id"></param>
    /// <param name="inputData"></param>
    /// <param name="boardColor"></param>
    /// <param name="comingOut"></param>
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

        //チャットにデータを持たせる用
        chatData.chatLive = live;
        chatData.chatWolf = wolfChat;

        ChatNode chatNode = Instantiate(chatNodePrefab, tran, false);

        //RPC内にあるメソッドもRPCと同じ挙動をする
        //そのメソッドの先で呼ばれるメソッドもRPCと同じ挙動をする
        chatNode.InitChatNode(chatData, iconNo, comingOut);
        Debug.Log(comingOut);
        chatSystem.SetChatNode(chatNode, chatData, comingOut);
        Debug.Log("Player RPC END");
    }



    /// <summary>
    ///投票、フィルター、夜の行動を制御 
    /// </summary>
    public void OnClickPlayerButton()
    {
        Debug.Log(".ActorNumber" + PhotonNetwork.LocalPlayer.ActorNumber);
        Debug.Log("playerID" + playerID);

        //フィルター機能ON
        if (gameManager.chatListManager.isfilter) { 
            gameManager.chatListManager.OnFilter(playerID);

        //フィルター機能Off時
        //生存していて、自分以外のプレイヤーを指定
        } else if(live && !gameManager.chatListManager.isfilter && PhotonNetwork.LocalPlayer.ActorNumber != playerID) {
            //フィルター機能がOFFの時は各時間ごとの機能をする
            Debug.Log(gameManager.timeController.timeType);

            switch (gameManager.timeController.timeType)
            {

                //投票する処理
                case TIME.投票時間:
                    //ここでは投票をするだけで他プレイヤーとの比較判定はしない
                    //比較はVoteCount.csで行われる
                    if (!chatSystem.myPlayer.isVoteFlag && live) {
                        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                            //Photonが用意しているActorNumberという数字とPLayerクラスが持っているPlayerIDが合致したら
                            //playerIDはActorNumberからもらっているから合致したら同じ番号を持っているクラスだといえる
                            if (player.ActorNumber == playerID) {
                            
                                //最新の投票数を取得する

                                //指定されたplayerのキーに登録する
                                if (player.CustomProperties.TryGetValue("voteNum", out object voteCountObj)) {
                                    voteNum = (int)voteCountObj;
                                }
                                voteNum++;

                                //投票したプレイヤーの名前を登録します
                                if (player.CustomProperties.TryGetValue("voteName", out object voteNameObj)) {
                                    voteName = (string)voteNameObj;
                                }
                                voteName += chatSystem.myPlayer.playerName + ",";

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
                        chatSystem.myPlayer.isVoteFlag = true;
                    }
                    break;

                //夜の行動をとる処理
                case TIME.夜の行動:
                    Debug.Log("夜の行動");
                    if (!chatSystem.myPlayer.isRollAction) {
                        
                        gameManager.gameMasterChatManager.RollAction(playerID, live, fortune, wolf);
                        chatSystem.myPlayer.isRollAction = true;
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


    /// <summary>
    /// 自分以外のPlayerの情報をセットする
    /// </summary>
    /// <returns></returns>

    private IEnumerator SetOtherPlayer() {
        yield return new WaitForSeconds(3.0f);

        Debug.Log("othetrs");
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if (player.ActorNumber == photonView.OwnerActorNr) {
                playerID = player.ActorNumber;
                playerName = player.NickName;
                Debug.Log("palyerName" + playerName);
                iconNo = player.ActorNumber;
            }
        }
        playerText.text = rollType.ToString() + playerName;
        gameObject.GetComponent<Outline>().enabled = false;
    }

    /// <summary>
    /// プレイヤーが投票したかをチェックする
    /// </summary>
    private void CheckVoted() {
        checkTimer += Time.deltaTime;
        if (checkTimer >= 1) {
            checkTimer = 0;
            checkNum = 0;

            //投票完了しているかをチェックする
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {

                if (player.CustomProperties["votingCompleted"] == null) {
                    if (player.CustomProperties.TryGetValue("votingCompleted", out object votingCompletedObj)) {
                        votingCompleted = (bool)votingCompletedObj;
                    }
                    var propertiers = new ExitGames.Client.Photon.Hashtable {
                                    {"votingCompleted",votingCompleted }
                                };
                    player.SetCustomProperties(propertiers);
                }

                if ((bool)player.CustomProperties["votingCompleted"] && (bool)player.CustomProperties["live"]) {
                    checkNum++;
                    Debug.Log("投票完了したプレイヤー" + player.NickName);
                }
            }
            //投票完了人数が生存員数と一致したら時短する
            if (checkNum == gameManager.liveNum) {
                checkTimer = -2;
                timeController.isVotingCompleted = true;
                timeController.SetIsVotingCompleted();
                Debug.Log("全員投票完了");
            }
        }
    }


    /////////////////////
    ///カスタムプロパティ関連
    /////////////////////


    //private void SetPlayerName() {
    //    Debug.Log(playerName);
    //    var propertis = new ExitGames.Client.Photon.Hashtable {
    //            {"playerName",playerName }
    //        };
    //    Debug.Log(PhotonNetwork.LocalPlayer.NickName);
    //    PhotonNetwork.LocalPlayer.SetCustomProperties(propertis);
    //    Debug.Log("playerName" + (string)PhotonNetwork.LocalPlayer.CustomProperties["playerName"]);
    //}

    //private string GetPlayerName(Photon.Realtime.Player player) {
    //    if (player.CustomProperties.TryGetValue("playerName", out object playerNameObj)) {
    //        playerName = (string)playerNameObj;
    //    }
    //    Debug.Log("playerName" + playerName);
    //    return playerName;
    //}
}

