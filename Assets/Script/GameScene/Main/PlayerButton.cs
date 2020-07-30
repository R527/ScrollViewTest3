using System.Collections;
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
    public RectTransform tran;

    public int playerID;
    public string myUniqueId;
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


    private void Start() {
        //Debug.Log("PlayerButtonStart");
        playerButton.onClick.AddListener(() => OnClickPlayerButton());
        tran.localScale = new Vector3(1, 1, 1);
        
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="iconNo"></param>
    /// <param name="playerID"></param>
    /// <param name="gameManager"></param>
    /// <returns></returns>
    public IEnumerator SetUp(string playerName,int iconNo, int playerID,GameManager gameManager,bool isMine) {
        yield return null;
        this.gameManager = gameManager;
        this.playerName = playerName;
        this.iconNo = iconNo;
        this.playerID = playerID;
        //ボタンにゆにーくIDを登録する
        //自分のボタンではない場合
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if (player.ActorNumber == playerID) {
                myUniqueId = (string)player.CustomProperties["myUniqueID"];
                Debug.Log("myUniqueId" + myUniqueId);
            }
        }

        live = true;

        //自分の世界のボタンだけ外枠を青くする
        if (isMine) {
            playerButton.GetComponent<Outline>().enabled = true;
        }

        playerText.text = playerName;
        menbartran = GameObject.FindGameObjectWithTag("MenbarContent").transform;
        transform.SetParent(menbartran);

        if (PhotonNetwork.IsMasterClient) {
            gameManager.gameMasterChatManager.timeSavingButton.interactable = true;
        }
    }

    public void SetRollSetting(Player player) {
        rollType = player.rollType;

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
        } else if (!gameManager.chatListManager.isfilter && PhotonNetwork.LocalPlayer.ActorNumber != playerID) {
            //フィルター機能がOFFの時は各時間ごとの機能をする
            Debug.Log(gameManager.timeController.timeType);

            //BanListの追加
            if (!live) {
                AddBanPlayer();
            }

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

                //マスターのみ他プレイヤーを退出できる
                //強制退出させたプレイヤーはBanListに追加される
                case TIME.開始前:
                    Debug.Log("強制退出");
                    if (PhotonNetwork.IsMasterClient) {
                        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                            if (player.ActorNumber == playerID && gameManager.chatSystem.myPlayer.playerID != playerID) {
                                PlayerManager.instance.roomBanUniqueIdList.Add((string)player.CustomProperties["myUniqueID"]);
                                PlayerManager.instance.roomBanUniqueIdStr += (string)player.CustomProperties["myUniqueID"] + ",";
                                gameManager.gameMasterChatManager.ForcedEvictionRoom(player);
                                break;
                            }
                        }
                    }
                    break;
                    //BanPlayerの追加
                case TIME.終了:
                    AddBanPlayer();
                    break;
            }
        }
    }

    private void AddBanPlayer() {
        Debug.Log("BanList追加");
        //枠がいっぱいの場合
        if (PlayerManager.instance.banListMaxIndex == 3) {
            GameObject list = GameObject.FindGameObjectWithTag("BanPlayerList");
            list.GetComponent<CanvasGroup>().alpha = 1;
            list.GetComponent<CanvasGroup>().blocksRaycasts = true;
        } else {
            //枠が空いている場合
            Debug.Log("追加されました");

            PlayerManager.instance.banIndex = PlayerManager.instance.banListMaxIndex;
            Debug.Log(PlayerManager.instance.banIndex);
            Debug.Log(PlayerManager.instance.banListMaxIndex);
            PlayerManager.instance.SetStringForPlayerPrefs(myUniqueId, PlayerManager.ID_TYPE.banUniqueID);
            PlayerManager.instance.SetStringForPlayerPrefs(playerName, PlayerManager.ID_TYPE.banUserNickName);
            PlayerManager.instance.banListMaxIndex++;
            
            PlayerManager.instance.SetIntForPlayerPrefs(PlayerManager.instance.banListMaxIndex, PlayerManager.ID_TYPE.banListMaxIndex);
        }
    }

    private void SetRoomBanPlayerID(Photon.Realtime.Player player) {
        
        var propertis = new ExitGames.Client.Photon.Hashtable {
            {"roomBanPlayerID",(string)player.CustomProperties["myUniqueID"] }
        };
        Debug.Log(PhotonNetwork.LocalPlayer.NickName);
        PhotonNetwork.CurrentRoom.SetCustomProperties(propertis);
        Debug.Log("playerName" + (string)PhotonNetwork.LocalPlayer.CustomProperties["playerName"]);
    }


}
