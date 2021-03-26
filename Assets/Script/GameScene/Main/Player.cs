using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
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
    public EMPTYROOM emtyRoom;

    //main
    public int playerID;
    public string playerName;
    public bool isMine;
    public bool live;//生死 trueで生存している
    public bool fortune;//占い結果 true=黒
    public bool spiritual;//霊能結果　true = 黒
    public bool wolf;//狼か否か
    public bool wolfChat;//狼チャットに参加できるかどうか
    public bool wolfCamp;//狼陣営か否か
    public ChatNode chatNodePrefab;//チャットノード用のプレふぁぶ
    public int iconNo;//アイコンの絵用
    public PlayerButton playerButton;
    public Transform chatTran;
    private IEnumerator checkEmptyRoomCoroutine = null;

    //投票関連
    public bool isVoteFlag; //投票を下か否か　falseなら非投票
    public bool isRollAction;//夜の行動をとったか否か

    //PlayerButton
    public PlayerButton playerButtonPrefab;
    private Transform buttontran;
    public List<int> playerImageNumList;

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
    private IEnumerator Start() {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        chatSystem = GameObject.FindGameObjectWithTag("ChatSystem").GetComponent<ChatSystem>();
        buttontran = GameObject.FindGameObjectWithTag("MenbarContent").transform;
        chatTran = GameObject.FindGameObjectWithTag("ChatContent").transform;

        //BanListをカスタムプロパティにセットする
        var propertiers = new ExitGames.Client.Photon.Hashtable();
        for (int i = 0; i < PlayerManager.instance.banUniqueIDList.Count; i++) {
            propertiers.Add("banUniqueID" + i.ToString(), PlayerManager.instance.banUniqueIDList[i]);
        }
        propertiers.Add("myUniqueID", PlayerManager.instance.myUniqueId);
        PhotonNetwork.LocalPlayer.SetCustomProperties(propertiers);

        //生存者にする
        live = true;

        //Player.cs自身をPlayerListの子供にする
        transform.SetParent(gameManager.playerListContent);

        //自分の世界の外枠を青色にするためにboolを用意する
        if (photonView.IsMine) {
            isMine = true;
        }
        //自分の世界に生成されたPlayerのオブジェクトなら→Aさんの世界のPlayerAが行う処理
        if (photonView.IsMine) {
            chatSystem.myPlayer = this;
            playerName = PlayerManager.instance.playerName;
            playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        }

        if (!PhotonNetwork.IsMasterClient) {
            //満室チェック
            checkEmptyRoomCoroutine = CheckEmptyRoom();
            yield return StartCoroutine(checkEmptyRoomCoroutine);

            //CheckBanListの待機中
            NetworkManager.instance.checkBanListCoroutine = CheckBanList();
            yield return StartCoroutine(NetworkManager.instance.checkBanListCoroutine);
        }

        //CheckEnteredRoom入室許可
        if (!PhotonNetwork.IsMasterClient) {
            yield return new WaitUntil(() => WaitOtherCreatePlayerButton() == true);
        }

        //プレイヤーボタン作成
        if (photonView.IsMine) {
            AddPlayerImage();
            photonView.RPC(nameof(CreatePlayerButton), RpcTarget.AllBuffered);
        } else if (!photonView.IsMine) {
            //他人の世界に生成された自分のPlayerオブジェクトなら→Bさんの世界のPlayerAが行う処理
            StartCoroutine(SetOtherPlayer());
        }
    }

    /// <summary>
    /// Player.csとは別にPlayerButtonを作成する
    /// </summary>
    [PunRPC]
    private IEnumerator CreatePlayerButton() {
        yield return null;
        playerButton = Instantiate(playerButtonPrefab, buttontran,false);
        StartCoroutine(playerButton.SetUp(playerName, iconNo, playerID, gameManager,isMine));
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
                {"voteNum", 0 },
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
        }

        //役職ごとの判定を追加
        if (rollType == ROLLTYPE.人狼) {
            fortune = true;
            spiritual = true;
            wolf = true;
            wolfChat = true;
            wolfCamp = true;
        } else if (rollType == ROLLTYPE.狂人) {
            wolfCamp = true;
        }
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
    public void CreateNode(string inputData, int boardColor, bool comingOut, bool subescribe) {
        photonView.RPC(nameof(CreateChatNodeFromPlayer), RpcTarget.All, inputData, boardColor, comingOut, subescribe,iconNo);
    }


    /// <summary>
    /// PunRPCでChatNodeを生成する　プレイヤーがMineかOhtersの設定　ちゃっとDataをの設定
    /// </summary>
    /// <param name="id"></param>
    /// <param name="inputData"></param>
    /// <param name="boardColor"></param>
    /// <param name="comingOut"></param>
    [PunRPC]
    public void CreateChatNodeFromPlayer(string inputData, int boardColor, bool comingOut, bool subescribe, int iconNo) {

        ChatData chatData = new ChatData(inputData, playerID, boardColor, playerName, rollType,playerButton.iconNo);
        //発言者の分岐
        if (photonView.IsMine) {
            chatData.chatType = CHAT_TYPE.MINE;
        } else {
            chatData.chatType = CHAT_TYPE.OTHERS;
        }

        //チャットにデータを持たせる用
        chatData.chatLive = live;
        //狼チャットの場合
        if (boardColor == 2) {
            chatData.chatWolf = wolfChat;
        }
        ChatNode chatNode = Instantiate(chatNodePrefab, chatTran, false);

        //RPC内にあるメソッドもRPCと同じ挙動をする
        //そのメソッドの先で呼ばれるメソッドもRPCと同じ挙動をする
        chatNode.InitChatNode(chatData, iconNo, comingOut, subescribe);
        chatSystem.SetChatNode(chatNode, chatData, comingOut);
    }

    /// <summary>
    /// 自分以外のPlayerの情報をセットする
    /// </summary>
    /// <returns></returns>

    private IEnumerator SetOtherPlayer() {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if (player.ActorNumber == photonView.OwnerActorNr) {

                bool isCheack = true;
                while (isCheack) {
                    if (player.CustomProperties.TryGetValue("playerImageNum", out object playerImageNumObj)) {
                        iconNo = (int)playerImageNumObj;
                        isCheack = false;
                        yield return null;
                    } else {
                        yield return null;
                    }

                }

                playerID = player.ActorNumber;
                playerName = player.NickName;
                //iconNo = NetworkManager.instance.GetCustomPropertesOfPlayer<int>("playerImageNum", player);
                break;
            }
        }
        yield return null;
        StartCoroutine(playerButton.SetUp(playerName, iconNo, playerID, gameManager,isMine));

        //参加人数が揃っていたらtrueにしない
        if (NetworkManager.instance.GetCustomPropertesOfRoom<int>("num") != gameManager.numLimit) {
            gameManager.gameMasterChatManager.timeSavingButton.interactable = true;
        }
        gameManager.exitButton.interactable = true;
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

                //投票してない場合
                if (player.CustomProperties["votingCompleted"] == null) {
                    //NetworkManager.instance.GetCustomPropertesOfPlayer<bool>("votingCompleted", player);
                    //bool votingCompleted = false;
                    //if (player.CustomProperties.TryGetValue("votingCompleted", out object votingCompletedObj)) {
                    //    votingCompleted = (bool)votingCompletedObj;
                    //}
                    NetworkManager.instance.SetCustomPropertesOfPlayer("votingCompleted", false, player);
                    //var propertiers = new ExitGames.Client.Photon.Hashtable {
                    //    {"votingCompleted",votingCompleted }
                    //};
                    //player.SetCustomProperties(propertiers);
                }

                //投票した場合
                if ((bool)player.CustomProperties["votingCompleted"] && (bool)player.CustomProperties["live"]) {
                    checkNum++;
                }
            }
            //投票完了人数が生存員数と一致したら時短する
            if (checkNum == gameManager.liveNum) {
                checkTimer = -2;
                timeController.isVotingCompleted = true;
                NetworkManager.instance.SetCustomPropertesOfRoom("isVotingCompleted", timeController.isVotingCompleted);
                //timeController.SetIsVotingCompleted();
            }
        }
    }

    /// <summary>
    /// BanListチェック中待機する
    /// </summary>
    /// <returns></returns>
    public IEnumerator CheckBanList() {
        bool isBanPlayer = true;
        while (isBanPlayer) {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("isBanPlayer", out object isBanPlayerObj)) {
                isBanPlayer = (bool)isBanPlayerObj;
                yield return null;
            } else {
                yield return null;
            }
        }
        yield break;
    }

    private IEnumerator CheckEmptyRoom() {
        //満室チェック
        bool isCheck = false;
        while (!isCheck) {
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("isCheckEmptyRoom", out object isCheckEmptyRoomObj)) {
                emtyRoom = (EMPTYROOM)Enum.Parse(typeof(EMPTYROOM), isCheckEmptyRoomObj.ToString());
                //満室処理
                if (emtyRoom == EMPTYROOM.満室 || emtyRoom == EMPTYROOM.入室許可) {
                    isCheck = true;
                } 
            }
            yield return null;
        }

        if (emtyRoom == EMPTYROOM.満室) {
            Destroy(this);
        }
    }

    /// <summary>
    /// マスタークライアント以外のプレイヤーボタンの生成を入室許可するまで止める
    /// </summary>
    /// <returns></returns>
    private bool WaitOtherCreatePlayerButton() {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("isCheckEnteredRoom", out object isCheckEnteredRoomObj)) {
            return (bool)isCheckEnteredRoomObj;
        }else {
            return false;
        }
    }

    /// <summary>
    /// //マスターがイラストの番号をすべて保存して既に使われているイラストを除外してランダムにセットする
    /// </summary>
    public void AddPlayerImage() {
        Debug.Log(" ColorManger.instance" +
            ColorManger.instance.iconColorList);
        for (int i = 0; i < ColorManger.instance.iconColorList.Count; i++) {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("playerImageNum" + i, out object playerImageNumObj)) {
                if ((int)playerImageNumObj != i) {
                    playerImageNumList.Add(i);
                } else {
                    continue;
                }
            } else {
                playerImageNumList.Add(i);
            }
        }
        iconNo = playerImageNumList[UnityEngine.Random.Range(0, playerImageNumList.Count)];
        
        playerImageNumList.Remove(iconNo);
        //使われた番号オンラインでもを削除する

        NetworkManager.instance.SetCustomPropertesOfRoom("playerImageNum", iconNo);
        NetworkManager.instance.SetCustomPropertesOfPlayer("playerImageNum", iconNo,PhotonNetwork.LocalPlayer);

    }
}

