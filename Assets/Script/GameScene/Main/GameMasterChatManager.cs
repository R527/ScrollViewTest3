using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;


/// <summary>
/// GMチャットをまとめています。
/// </summary>
public class GameMasterChatManager : MonoBehaviourPunCallbacks {

    //class
    public TimeController timeController;
    public GameManager gameManager;
    public VoteCount voteCount;


    //main
    public Button timeSavingButton;
    public bool timeSaving;//時短用　希望の場合true
    public int timeSavingNum;//時短の人数確認
    public Text timeSavingButtonText;//時短or退出ボタン
    public GameObject LeavePopUp;//ゲーム終了後の退出用PopUP
    //public Button exitButton;
    public string gameMasterChat;
    public bool isTimeSaving;//時短用のbool
    public ActionPopUp actionPopUpPrefab;
    public Transform gameCancasTran;

    //早朝用
    public int bitedID;//噛んだプレイヤーID
    public int protectedID;//守ったプレイヤーID
    public Player bitedPlayer;
    public Player thePlayer;

    //広告用
    string VIDEO_PLACEMENT_ID = "video";

    void Start() {

        gameCancasTran = GameObject.FindGameObjectWithTag("GameCanvas").transform;

        timeSavingButton.onClick.AddListener(() => TimeSavingOrExitButton());
        //exitButton.onClick.AddListener(ExitButton);
        //カスタムプロパティ
            var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"timeSavingNum",timeSavingNum },//時短の人数確認
            {"bitedID", bitedID },//噛んだプレイヤー
            {"protectedID", protectedID }//守ったプレイヤー
        };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);

    }


    ///////////////////////
    ///メソッド関連
    ///////////////////////


    /// <summary>
    /// 本当の姿を表示する
    /// </summary>
    public void TrueCharacter() {
        gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんの本当の姿は" + gameManager.chatSystem.myPlayer.rollType.ToString() + "です！";
        gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        gameMasterChat = string.Empty;
    }

    /// <summary>
    /// ランダム白の時の設定それ以外の設定は別に行う
    /// </summary>
    public IEnumerator OpeningDayFortune() {
        yield return new WaitForSeconds(2.4f);
        //Debug.Log(gameManager.chatSystem.myPlayer.rollType);
        //Debug.Log(RoomData.instance.roomInfo.fortuneType == FORTUNETYPE.ランダム白);

        //人狼ではないプレイヤーをランダムに選択
        if (RoomData.instance.roomInfo.fortuneType == FORTUNETYPE.ランダム白 && gameManager.chatSystem.myPlayer.rollType == ROLLTYPE.占い師) {
            List<Player> playerObjList = new List<Player>();
            int romdomValue = 0;
            foreach (Player player in gameManager.chatSystem.playersList) {
                if (!player.fortune && player != gameManager.chatSystem.myPlayer) {
                    playerObjList.Add(player);
                }
            }
            romdomValue = Random.Range(0, playerObjList.Count);
            gameMasterChat = "【占い結果】\r\n" + playerObjList[romdomValue].playerName + "は人狼ではない（白）です。";
            gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
            gameMasterChat = string.Empty;
        }
    }

    /// <summary>
    /// タイムコントローラのシーンが変更するたびに発言するチャット
    /// </summary>
    public void TimeManagementChat() {
        switch (timeController.timeType) {

            case TIME.昼:
                if (gameManager.chatSystem.myPlayer.live) {
                    gameMasterChat = "話し合う時間です。\r\n\r\n市民陣営は嘘をついている狼を探しましょう。\r\n\r\n人狼陣営は市民にうまく紛れて市民を騙しましょう！";
                } else {
                    gameMasterChat = "話し合う時間です。\r\n\r\n観戦を楽しめ";
                }
                break;

            case TIME.投票時間:
                if (gameManager.chatSystem.myPlayer.live) {
                    gameMasterChat = "投票の時間です。\r\n\r\n人狼と思われるプレイヤーに投票しましょう。";
                } else {
                    gameMasterChat = "投票の時間です。\r\n\r\n投票時間が終わるまで待ってください。";
                }
                break;

            case TIME.夜の行動:
                if (gameManager.chatSystem.myPlayer.live) {
                    gameMasterChat = "各役職の能力を使い陣営を勝利へと導きましょう。";
                } else {
                    gameMasterChat = "夜の行動時間です。待ってくれ。";
                }
                break;
        }
        gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        gameMasterChat = string.Empty;
    }


    /// <summary>
    /// 時短と退出処理のGMチャットを制御
    /// </summary>
    /// <returns></returns>
    public void TimeSavingOrExitButton() {
        Debug.Log("TimeSavingChat");
        //時短処理
        if (timeSavingButtonText.text == "時短") {
            Debug.Log("時短処理");
            timeSavingNum = GetimeSavingNum();

            //キャンセルorまだ希望していない状態なら
            //時短ボタンの色を変更する処理を後程追加したい
            if (!timeSaving) {
                timeSavingNum++;
                timeSaving = true;
                gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんが時短を希望しました。(" + timeSavingNum + "/" + gameManager.liveNum + ")※過半数を超えると時短されます。";
            } else {
                timeSavingNum--;
                timeSaving = false;
                gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんが時短をキャンセルしました。(" + timeSavingNum + "/" + gameManager.liveNum + ")※過半数を超えると時短されます。";
            }
            gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_ONLINE);

            //timeSavingNum更新
            var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                {"timeSavingNum",timeSavingNum }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
            Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["timeSavingNum"]);

            //もう一度チェック
            timeSavingNum = GetimeSavingNum();

            //時短判定(過半数以上なら時短成立
            //Mathf.CeilToIntは切り上げ
            float value = (float)gameManager.liveNum;
            if (Mathf.CeilToInt(value / 2) <= timeSavingNum) {
                isTimeSaving = true;
                SetIsTimeSaving();
                Debug.Log("時短成立");
            } else {
                Debug.Log("時短不成立");
            }


            //退出処理
        } else if (timeSavingButtonText.text == "退出" ) {
            //&& photonView.IsMine
            Debug.Log("退出");

            //game中なら正常終了しているかの確認を取る

            //課金額が指定した料金に達している場合
            //TODO 初めての退出と初めての青チャットをする際に課金が必要であることをPoPUp等で知らせる処理を追加する
            int exitCurrenecy = 120;
            int currency = PlayerPrefs.GetInt(PlayerManager.ID_TYPE.currency.ToString(), 0);

            if (timeController.isPlay && PlayerManager.instance.currency >= exitCurrenecy) {
                timeController.gameOver.CheckEndGame();
                CurrencyManager.instance.UseCurrency(exitCurrenecy);
            } else if(timeController.isPlay && PlayerManager.instance.currency < exitCurrenecy) {
                //利用額とゲーム内通貨の残高を比較して購入できないなら別のPopUpを呼び出す
                if (exitCurrenecy > currency) {
                    gameManager.inputView.moneyImage.SetActive(true);
                    return;
                }
            }



            //game終了後に退出する場合広告を表示する
            if (timeController.gameOver.isGameOver) {
                Advertisement.Show(VIDEO_PLACEMENT_ID);
            }

            gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんが退出しました。";
            gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_ONLINE);
            gameMasterChat = string.Empty;
            //LeaveRoom();処理をするときにTimeControllerのエラーが出るので消去する
            NetworkManager.instance.LeaveRoom();
        }


    }



    ///// <summary>
    ///// ゲーム終了後の退出ボタンを押したときに出るPoPUP内にある退出ボタン
    ///// ネットワークのチェック完了
    ///// </summary>
    //private void ExitButton() {
    //    NetworkManager.instance.LeaveRoom();
    //}

    /// <summary>
    /// 退出処理全般
    /// </summary>
    /// <returns></returns>
    public void LeaveRoomChat() {
        gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんが退出しました。";
        if (photonView.IsMine) {
            gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_ONLINE);
        }
        
        gameMasterChat = string.Empty;
    }




    /// <summary>
    /// 強制退出 ゲーム開始前にマスタークライアントのみが参加プレイヤーを退出させることができる
    /// </summary>
    public void ForcedEvictionRoom(Photon.Realtime.Player player) {
        gameMasterChat = player.NickName + "さんを強制退出させました。";
        if (photonView.IsMine) {
            gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_ONLINE);
        }
        NetworkManager.instance.KickOutPlayer(player);
        gameMasterChat = string.Empty;
    }

    /// <summary>
    /// 入室時のチャット
    /// </summary>
    public IEnumerator EnteredRoom(Photon.Realtime.Player player) {
        yield return new WaitForSeconds(2.0f);
        gameMasterChat = player.NickName + "さんが参加しました。";
        if (photonView.IsMine) {
            gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_ONLINE);
        }
        Debug.Log("EnteredRoom");
        gameMasterChat = string.Empty;
    }




    /// <summary>
    /// 投票を完了させる
    /// </summary>
    public void Voted(Photon.Realtime.Player player) {
        //if(!live) {
        //    Debug.Log("押せません");
        //    return;
        //}
        gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんは" + player.NickName + "に投票しました。";
        gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        gameMasterChat = string.Empty;
    }

    /// <summary>
    /// 処刑プレイヤーをGMチャットに表示する
    /// </summary>
    public void ExecutionChat() {
        if (PhotonNetwork.IsMasterClient) {

            string strList = string.Empty;
            string str = string.Empty;
            string yajirusi = string.Empty;
            string testNameList = string.Empty;
            string mostVotingNameList = string.Empty;
            string votingNameList = string.Empty;//投票数を表示するリスト
            string votedNameList = string.Empty;//投票された名前を表示するリスト

            //処刑されたプレイヤーの表示
            //Debug.Log("voteCount.executionID" + voteCount.executionID);
            //Debug.Log("処刑されたプレイヤー" + (string)PhotonNetwork.CurrentRoom.CustomProperties["executionPlayerName"]);

            gameMasterChat = (string)PhotonNetwork.CurrentRoom.CustomProperties["executionPlayerName"] + "さんが処刑されました。";
            if (photonView.IsMine) {
                gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_ONLINE);
            }
            //投票数の表示
            foreach (Player playerObj in gameManager.chatSystem.playersList) {
                //処刑されるプレイヤーに投票したプレイヤー名の表示
                foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                    //PhotonのActorNumberとIDを合致させる
                    if (RoomData.instance.roomInfo.openVoting == VOTING.開示する && voteCount.mostVotePlayer.ActorNumber == player.ActorNumber) {
                        mostVotingNameList = (string)player.CustomProperties["voteName"];
                        Debug.Log(mostVotingNameList);
                    }
                }
                if (!playerObj.live) {
                    continue;
                }
                //処刑されなかったプレイヤーの表示
                foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {

                    
                    if (player.ActorNumber == playerObj.playerID && (int)player.CustomProperties["voteNum"] != 0) {

                        //Debug.Log("投票時のPlayerList" + PhotonNetwork.PlayerList);
                        //Debug.Log("投票時のPlayerList" + player.NickName);
                        //Debug.Log("投票数" + player.CustomProperties["voteNum"]);

                        //投票開示する場合
                        if (RoomData.instance.roomInfo.openVoting == VOTING.開示する) {
                            yajirusi = "←";
                            votedNameList = (string)player.CustomProperties["voteName"];
                        }

                        strList = playerObj.playerName + ": " + player.CustomProperties["voteNum"] + "票" + yajirusi + votedNameList.TrimEnd(',') + "\r\n";
                        str = "\r\n\r\n";
                        votingNameList += strList;
                    }
                }
            }

            //GMチャットの表示

            //Debug.Log("処刑されたプレイヤーに投票したプレイヤー" + voteCount.voteNameTable[voteCount.GetExecutionPlayerID()]);

            gameMasterChat = "【投票結果】\r\n" + (string)PhotonNetwork.CurrentRoom.CustomProperties["executionPlayerName"] + ": " + PhotonNetwork.CurrentRoom.CustomProperties["mostVotes"] + "票" + yajirusi + mostVotingNameList.TrimEnd(',') + str + votingNameList.TrimEnd();
            if (photonView.IsMine) {
                gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_ONLINE);
            }
        }
        gameMasterChat = string.Empty;
    }

    /// <summary>
    /// 突然死用のチャット
    /// </summary>
    public void SuddenDeath(Photon.Realtime.Player player) {
        if(PhotonNetwork.IsMasterClient) {
            gameMasterChat = player.NickName + "さんが突然死しました。";
            gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_ONLINE);
            gameMasterChat = string.Empty;
        }
    }

    /// <summary>
    /// 霊能者の行動を制御(役職増えると、ここに別の処理を加える
    /// </summary>
    public void PsychicAction() {
        //霊能者かつ自分が生存中なら
        if (gameManager.chatSystem.myPlayer.rollType == ROLLTYPE.霊能者 && gameManager.chatSystem.myPlayer.live) {
            //処刑プレイヤーがいないならリターン
            if (voteCount.mostVotePlayer == null) {
                return;
            }

            //処刑したプレイヤーを取得
            Player playerObj = null;
            foreach(Player player in gameManager.chatSystem.playersList) {
                if(voteCount.mostVotePlayer.ActorNumber == player.playerID) {
                    playerObj = player;
                }
            }
            //処刑プレイヤーが人狼なら
            if (playerObj.wolf) {
                gameMasterChat = "【霊能結果】\r\n" + playerObj.playerName + "は人狼（黒）です。";
                Debug.Log(voteCount.mostVotePlayer + "人狼");
            } else {
                //処刑プレイヤーが市民なら
                gameMasterChat = "【霊能結果】\r\n" + playerObj.playerName + "は人狼ではない（白）です。";
                Debug.Log(voteCount.mostVotePlayer + "人狼ではない");
            }
            gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        }

        gameMasterChat = string.Empty;
    }


    /// <summary>
    /// 夜の行動を制御する
    /// </summary>
    /// <param name="rollType"></param>
    /// <param name="playerID"></param>
    public void RollAction(int playerID, bool fortune, bool wolf) {
        
        //死亡時もしくは自分のボタンは機能しない
        if (gameManager.chatSystem.myPlayer.playerID == playerID || !gameManager.chatSystem.myPlayer.live) {
            Debug.Log("押せません。");
            return;
        }

        //buttonを押したプレイヤーを指定
        foreach(Player playerObj in gameManager.chatSystem.playersList) {
            if(playerObj.playerID == playerID) {
                thePlayer = playerObj;
            }
        }
        //自分の役職が～～なら
        switch (gameManager.chatSystem.myPlayer.rollType) {

            case ROLLTYPE.人狼:

                //相方のため押せません。
                if (wolf) {
                    Debug.Log("相方です。");
                    return;
                } else {
                    //噛んだプレイヤーを記録
                    //biteID = playerID;

                    ActionPopUp biteObj = Instantiate(actionPopUpPrefab, gameCancasTran, false);
                    biteObj.actionText.text = thePlayer.playerName + "さんを襲撃しますか？";
                    biteObj.buttonText.text = "襲撃";
                    biteObj.gameManager = this.gameManager;
                    biteObj.playerName = thePlayer.playerName;
                    biteObj.playerID = thePlayer.playerID;
                    biteObj.action_Type = ActionPopUp.Action_Type.襲撃;
                }
                break;

            case ROLLTYPE.占い師:
                //初日ではなく もしくは初日占いなし
                if (!timeController.firstDay || RoomData.instance.roomInfo.fortuneType == FORTUNETYPE.あり) {
                    ActionPopUp fortuneObj = Instantiate(actionPopUpPrefab, gameCancasTran, false);
                    fortuneObj.actionText.text = thePlayer.playerName + "さんを占いますか？";
                    fortuneObj.buttonText.text = "占う";
                    fortuneObj.gameManager = this.gameManager;
                    fortuneObj.playerName = thePlayer.playerName;
                    fortuneObj.fortune = fortune;
                    fortuneObj.action_Type = ActionPopUp.Action_Type.占い;
                }
                break;
            case ROLLTYPE.騎士:

                ActionPopUp kightObj = Instantiate(actionPopUpPrefab, gameCancasTran, false);
                kightObj.actionText.text = thePlayer.playerName + "さんを護衛しますか？";
                kightObj.buttonText.text = "護衛";
                kightObj.gameManager = this.gameManager;
                kightObj.playerName = thePlayer.playerName;
                kightObj.action_Type = ActionPopUp.Action_Type.護衛;


                break;
            default:
                Debug.Log("押せません。");
                return;
        }


    }

    /// <summary>
    /// 朝の結果発表
    /// </summary>
    public void MorningResults() {
        //Debug.Log("朝の結果発表");
        protectedID = GetProtectedPlayerID();
        bitedID = GetBitedPlayerID();
        Debug.Log("bitedID" + bitedID);
        if (PhotonNetwork.IsMasterClient) {



            //結果を実行する
            if (bitedID == protectedID) {
                gameMasterChat = "【朝の結果発表】\r\n\r\n本日の犠牲者はいません。";
                //Debug.Log("犠牲者なし");

            } else {

                //狼が噛んだプレイヤーを取得,
                //襲撃決行する

                foreach (Player playerObj in gameManager.chatSystem.playersList) {
                    if (playerObj.playerID == bitedID) {
                        bitedPlayer = playerObj;
                        gameMasterChat = "【朝の結果発表】\r\n\r\n" + bitedPlayer.playerName + "さんが襲撃されました。";
                        break;
                    }
                }



            }

            if (photonView.IsMine) {
                gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_ONLINE);
            }


        }

        //PlayerListの死亡処理
        foreach (Player playerObj in gameManager.chatSystem.playersList) {
            if (playerObj.playerID == bitedID) {
                Debug.Log("かみ殺したID"　+ playerObj.playerID);
                playerObj.live = false;
                break;
            }
        }

        //PlayerButtonの死亡処理
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PlayerButton");
        foreach (GameObject player in objs) {
            PlayerButton playerObj = player.GetComponent<PlayerButton>();
            if (bitedID == playerObj.playerID) {
                playerObj.live = false;
                playerObj.playerText.text += timeController.day + "日目襲撃";
            }
        }

        gameMasterChat = string.Empty;

  
    }

    /// <summary>
    /// 強制退出残り時間わずかの時に発言する
    /// </summary>
    public void EndRoomChat() {
        gameMasterChat = "部屋閉じるまで残り2分です。";
        gameManager.chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        gameMasterChat = string.Empty;
    }


    /////////////////////////////
    ///カスタムプロパティ関連
    /////////////////////////////


    /// <summary>
    /// 時短希望人数をチェックする
    /// </summary>
    private int GetimeSavingNum() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("timeSavingNum", out object timeSavingNumObj)) {
            timeSavingNum = (int)timeSavingNumObj;
        }
        return timeSavingNum;
    }
    /// <summary>
    /// 狩人が守った情報をセットします。
    /// </summary>
    public void SetProtectedPlayerID() {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"protectedID", protectedID }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        //Debug.Log("SetProtected" + (int)PhotonNetwork.CurrentRoom.CustomProperties["protectedID"]);
    }
    /// <summary>
    /// 狼が噛んだプレイヤーをセットします。
    /// </summary>
    public  void SetBitedPlayerID() {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"bitedID", bitedID }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        //Debug.Log("SetBited" + (int)PhotonNetwork.CurrentRoom.CustomProperties["bitedID"]);
    }
    /// <summary>
    /// 噛んだプレイヤーを受け取ります。
    /// </summary>
    /// <returns></returns>
    private int GetBitedPlayerID() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("bitedID", out object bitedIDObj)) {
            bitedID = (int)bitedIDObj;
        }
        return bitedID;
    }
    /// <summary>
    /// 狩人が守ったプレイヤーを受け取ります。
    /// </summary>
    /// <returns></returns>
    private int GetProtectedPlayerID() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("protectedID", out object protectedIDObj)) {
            protectedID = (int)protectedIDObj;
        }
        return protectedID;
    }

    /// <summary>
    /// 時短成立用のboolをセットする
    /// </summary>
    /// <returns></returns>
    public void SetIsTimeSaving() {
        var propertis = new ExitGames.Client.Photon.Hashtable {
            {"isTimeSaving",isTimeSaving }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(propertis);
    }

    public bool GetIsTimeSaving() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("isTimeSaving", out object isTimeSavingObj)) {
            isTimeSaving = (bool)isTimeSavingObj;
        }
        return isTimeSaving;
    }
}
