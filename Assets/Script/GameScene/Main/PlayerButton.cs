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
    public Text playerInfoText;
    public Text playerNameText;
    public Image iconImage;
    public RectTransform tran;

    public int playerID;
    public string myUniqueId;
    public string playerName;
    public int iconNo;//アイコンの絵用
    public ROLLTYPE rollType = ROLLTYPE.ETC;
    public bool live;//生死 trueで生存している
    public Text rollText;
    public ActionPopUp actionPopUpPrefab;
    public Transform gameCancasTran;
    
    //後からもらう
    public bool fortune;//占い結果 true=黒
    public bool spiritual;//霊能結果　true = 黒
    public bool wolf;//狼か否か
    public bool wolfChat;//狼チャットに参加できるかどうか
    public bool wolfCamp;//狼陣営か否か

    private Transform menbartran;


    private void Start() {
        gameCancasTran = GameObject.FindGameObjectWithTag("GameCanvas").transform;
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
        playerNameText.text = playerName;

        this.iconNo = iconNo;
        this.playerID = playerID;
        //ボタンにゆにーくIDを登録する
        //自分のボタンではない場合
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if (player.ActorNumber == playerID) {
                myUniqueId = (string)player.CustomProperties["myUniqueID"];
                //Debug.Log("myUniqueId" + myUniqueId);
            }
        }

        live = true;

        //自分の世界の自分のボタンだけ外枠を青くする、ロールテキストを有効にする
        if (isMine) {
            playerButton.GetComponent<Outline>().enabled = true;
            rollText.enabled = true;
        }

        menbartran = GameObject.FindGameObjectWithTag("MenbarContent").transform;
        transform.SetParent(menbartran);

        if (PhotonNetwork.IsMasterClient && gameManager.GetNum() != gameManager.numLimit) {
            gameManager.gameMasterChatManager.timeSavingButton.interactable = true;
        }
    }

    
    /// <summary>
    /// 役職ごとのセッティングをする
    /// </summary>
    /// <param name="player"></param>
    public void SetRollSetting(Player player) {

        rollType = player.rollType;
        rollText.text = rollType.ToString();
        //役職ごとに判定を設ける
        if (rollType == ROLLTYPE.人狼) {
            fortune = true;
            spiritual = true;
            wolf = true;
            wolfChat = true;
            wolfCamp = true;
        } else if (rollType == ROLLTYPE.狂人) {
            wolfCamp = true;
        }

        //自分の役職が他プレイヤーを開示する場合
        OpenRoll();
    }

    /// <summary>
    /// ゲーム開始時に役職を開示するべきプレイヤーがいる場合開示する
    /// </summary>
    private void OpenRoll() {
        if (gameManager.chatSystem.myPlayer.wolf && wolf) {
            rollText.enabled = true;
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

            //TODO 必要？
            //BanListの追加
            if (!live) {
                AddBanPlayer();
            }

            switch (gameManager.timeController.timeType) {

                //投票する処理
                case TIME.投票時間:
                    //ここでは投票をするだけで他プレイヤーとの比較判定はしない
                    //比較はVoteCount.csで行われる

                    ActionPopUp voteObj = Instantiate(actionPopUpPrefab, gameCancasTran, false);
                    voteObj.actionText.text = playerName + "さんに投票しますか？";
                    voteObj.buttonText.text = "投票する";
                    voteObj.gameManager = this.gameManager;
                    voteObj.playerID = this.playerID;
                    voteObj.action_Type = ActionPopUp.Action_Type.投票;

                    break;

                //夜の行動をとる処理
                case TIME.夜の行動:
                    Debug.Log("夜の行動");
                    if (!gameManager.chatSystem.myPlayer.isRollAction) {
                        gameManager.gameMasterChatManager.RollAction(playerID, fortune, wolf);
                    }
                    break;

                //マスターのみ他プレイヤーを退出できる
                //強制退出させたプレイヤーはBanListに追加される
                case TIME.開始前:
                    Debug.Log("強制退出");
                    ActionPopUp obj = Instantiate(actionPopUpPrefab, gameCancasTran, false);
                    obj.actionText.text = playerName + "さんを強制退出させますか？";
                    obj.buttonText.text = "強制退場";
                    obj.gameManager = this.gameManager;
                    obj.playerID = this.playerID;
                    obj.action_Type = ActionPopUp.Action_Type.強制退場;

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
        //枠は1つまで
        if (PlayerManager.instance.banListIndex == PlayerManager.instance.banListMaxIndex) {
            GameObject list = GameObject.FindGameObjectWithTag("BanPlayerList");
            list.GetComponent<CanvasGroup>().alpha = 1;
            list.GetComponent<CanvasGroup>().blocksRaycasts = true;
        } else {
            //枠が空いている場合
            Debug.Log("追加されました");

            ActionPopUp obj = Instantiate(actionPopUpPrefab, gameCancasTran, false);
            obj.actionText.text = playerName + "さんを回避しますか？";
            obj.buttonText.text = "回避する";
            obj.gameManager = this.gameManager;
            obj.playerID = this.playerID;
            obj.playerName = playerName;
            obj.myUniqueId = this.myUniqueId;
            obj.action_Type = ActionPopUp.Action_Type.Ban;

            
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
