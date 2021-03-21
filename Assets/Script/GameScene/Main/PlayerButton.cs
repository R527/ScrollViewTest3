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
    public int iconNo;//アイコンの絵用
    public Image iconImage;
    public RectTransform tran;

    public int playerID;
    public string otherUniqueId;
    public string playerName;
    public ROLLTYPE rollType = ROLLTYPE.ETC;
    public bool live;//生死 trueで生存している
    public Text rollText;
    public ActionPopUp actionPopUpPrefab;
    public Transform gameCancasTran;
    public Outline playBtnOutLine;
    
    //後からもらう
    public bool fortune;//占い結果 true=黒
    public bool spiritual;//霊能結果　true = 黒
    public bool wolf;//狼か否か
    public bool wolfChat;//狼チャットに参加できるかどうか
    public bool wolfCamp;//狼陣営か否か

    private void Start() {
        gameCancasTran = GameObject.FindGameObjectWithTag("GameCanvas").transform;
        playerButton.onClick.AddListener(() => OnClickPlayerButton());
        tran.localScale = new Vector3(1, 1, 1);
    }


    /// <summary>
    /// PlayerBtnが生成されたときに使われる
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
        iconImage.color = ColorManger.instance.iconColorList[iconNo];
        this.playerID = playerID;
        //ボタンにゆにーくIDを登録する
        //自分のボタンではない場合
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if (player.ActorNumber == playerID) {
                otherUniqueId = (string)player.CustomProperties["myUniqueID"];
                object playerImageNumObj = null;
                yield return new WaitUntil(() => player.CustomProperties.TryGetValue("playerImageNum", out playerImageNumObj));
                this.iconNo = (int)playerImageNumObj;
            }
        }
        live = true;

        //自分の世界の自分のボタンだけ外枠を青くする、ロールテキストを有効にする
        if (isMine) {
            playBtnOutLine.enabled = true;
            rollText.enabled = true;
        }

        if (PhotonNetwork.IsMasterClient && NetworkManager.instance.GetCustomPropertesOfRoom<int>("num") != gameManager.numLimit) {
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
        //フィルター機能ON
        if (gameManager.chatListManager.isfilter) {
            gameManager.chatListManager.OnFilter(playerID);

            //フィルター機能Off時
            //生存していて、自分以外のプレイヤーを指定
        } else if (!gameManager.chatListManager.isfilter && PhotonNetwork.LocalPlayer.ActorNumber != playerID) {
            //フィルター機能がOFFの時は各時間ごとの機能をする
            if (!gameManager.chatSystem.myPlayer.live) {
                AddBanPlayer();
            }

            switch (gameManager.timeController.timeType) {

                //投票する処理
                case TIME.投票時間:
                    //ここでは投票をするだけで他プレイヤーとの比較判定はしない
                    //比較はVoteCount.csで行われる
                    if (gameManager.chatSystem.myPlayer.live) {
                        ActionPopUp voteObj = Instantiate(actionPopUpPrefab, gameCancasTran, false);
                        gameManager.gameMasterChatManager.destoryedObj = voteObj.gameObject;
                        voteObj.actionText.text = playerName + "さんに投票しますか？";
                        voteObj.buttonText.text = "投票する";
                        voteObj.gameManager = gameManager;
                        voteObj.playerID = playerID;
                        voteObj.action_Type = ActionPopUp.Action_Type.投票;
                    }
                    break;
                //夜の行動をとる処理
                case TIME.夜の行動:
                    if (!gameManager.chatSystem.myPlayer.isRollAction) {
                        gameManager.gameMasterChatManager.RollAction(playerID, fortune, wolf);
                    }
                    break;

                //マスターのみ他プレイヤーを退出できる
                //強制退出させたプレイヤーはBanListに追加される
                case TIME.開始前:
                    if (PhotonNetwork.IsMasterClient) {
                        ActionPopUp obj = Instantiate(actionPopUpPrefab, gameCancasTran, false);
                        obj.actionText.text = playerName + "さんを強制退出させますか？";
                        obj.buttonText.text = "強制退場";
                        obj.gameManager = this.gameManager;
                        obj.playerID = this.playerID;
                        obj.action_Type = ActionPopUp.Action_Type.強制退場;
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
        //枠がいっぱいの場合
        //枠は1つまで
        if (PlayerManager.instance.banListIndex == PlayerManager.instance.banListMaxIndex) {
            GameObject listObj = GameObject.FindGameObjectWithTag("BanPlayerList");

            foreach (Transform n in listObj.transform.Find("Image").transform.Find("List"). gameObject.transform) {
                Destroy(n.gameObject);
            }

            gameManager.CreateBanList();
            listObj.GetComponent<CanvasGroup>().alpha = 1;
            listObj.GetComponent<CanvasGroup>().blocksRaycasts = true;
        } else {
            //枠が空いている場合
            ActionPopUp obj = Instantiate(actionPopUpPrefab, gameCancasTran, false);
            obj.actionText.text = playerName + "さんを回避しますか？";
            obj.buttonText.text = "回避する";
            obj.gameManager = this.gameManager;
            obj.playerID = this.playerID;
            obj.playerName = playerName;
            obj.otherUniqueId = this.otherUniqueId;
            obj.action_Type = ActionPopUp.Action_Type.Ban;
        }
    }
}
