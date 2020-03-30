using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public Button exitButton;
    //早朝用
    public int bitedID;//噛んだプレイヤーID
    public int protectedID;//守ったプレイヤーID
    public Player bitedPlayer;
    public Player thePlayer;


    // Start is called before the first frame update
    void Start() {
        timeSavingButton.onClick.AddListener(() => TimeSavingChat());
        exitButton.onClick.AddListener(() => StartCoroutine(ExitButton()));
        //カスタムプロパティ
        if (!gameManager.isOffline) {
            var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"timeSavingNum",timeSavingNum },//時短の人数確認
            {"bitedID", bitedID },//噛んだプレイヤー
            {"protectedID", protectedID }//守ったプレイヤー
        };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);

        }
    }

    /// <summary>
    /// 時短希望人数をチェックする
    /// </summary>
    private void CheckTimeSavingNum() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("timeSavingNum", out object timeSavingNumObj)) {
            timeSavingNum = (int)timeSavingNumObj;
        }
    }
    /// <summary>
    /// 狩人が守った情報をセットします。
    /// </summary>
    private void SetProtectedPlayerID() {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                            {"protectedID", protectedID }
                        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log("SetProtected" + (int)PhotonNetwork.CurrentRoom.CustomProperties["protectedID"]);
    }
    /// <summary>
    /// 狼が噛んだプレイヤーをセットします。
    /// </summary>
    private void SetBitedPlayerID() {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                            {"bitedID", bitedID }
                        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log("SetBited"+(int)PhotonNetwork.CurrentRoom.CustomProperties["bitedID"]);
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
    /// 本当の姿を表示する
    /// </summary>
    public void TrueCharacter() {
        //本当の姿
        gameManager.chatSystem.gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんの本当の姿は" + gameManager.chatSystem.myPlayer.rollType.ToString() + "です！";
        gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_OFFLINE);
    }

    /// <summary>
    /// タイムコントローラのシーンが変更するたびに発言するチャット
    /// </summary>
    public void TimeManagementChat() {
        //string gmNode = string.Empty;
        switch (timeController.timeType) {
            case TIME.昼:
                gameManager.chatSystem.gameMasterChat = "話し合う時間です。\r\n\r\n市民陣営は嘘をついている狼を探しましょう。\r\n\r\n人狼陣営は市民にうまく紛れて市民を騙しましょう！";
                break;
            case TIME.投票時間:
                gameManager.chatSystem.gameMasterChat = "投票の時間です。\r\n\r\n人狼と思われるプレイヤーに投票しましょう。";
                break;
            case TIME.夜の行動:
                gameManager.chatSystem.gameMasterChat = "各役職の能力を使い陣営を勝利へと導きましょう。";
                break;
        }
        gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_OFFLINE);
    }


    /// <summary>
    /// 時短と退出処理のGMチャットを制御
    /// </summary>
    /// <returns></returns>
    public void TimeSavingChat() {

        if (!gameManager.isOffline) {
            //時短処理
            if (timeSavingButtonText.text == "時短") {
                CheckTimeSavingNum();

                //キャンセルorまだ希望していない状態なら
                if (!timeSaving) {
                    timeSavingNum++;
                    timeSaving = true;
                    gameManager.chatSystem.gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんが時短を希望しました。" + "(" + timeSavingNum + "/" + gameManager.liveNum + ")※過半数を超えると時短されます。";
                } else {
                    timeSavingNum--;
                    timeSaving = false;
                    gameManager.chatSystem.gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんが時短をキャンセルしました。" + timeSavingNum + "/" + gameManager.liveNum + "※過半数を超えると時短されます。";
                }
                gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_ONLINE);
                //timeSavingNum更新
                var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                    {"timeSavingNum",timeSavingNum }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
                Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["timeSavingNum"]);

                //もう一度チェック
                CheckTimeSavingNum();

                //時短判定(過半数以上なら時短成立
                //Mathf.CeilToIntは切り上げ
                Debug.Log(Mathf.CeilToInt(gameManager.liveNum / 2));
                Debug.Log(timeSavingNum);

                if (Mathf.CeilToInt(gameManager.liveNum / 2) <= timeSavingNum) {
                    timeController.totalTime = 0;
                    Debug.Log("時短成立");
                } else {
                    Debug.Log("時短不成立");
                }

                //退出処理
            } else if (timeSavingButtonText.text == "退出") {

                //ゲーム開始前
                if (!gameManager.gameStart) {
                    gameManager.chatSystem.gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんが退出しました。";
                    gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_ONLINE);
                    //LeaveRoom();処理をするときにTimeControllerのエラーが出るので消去する
                    Destroy(timeController.gameObject);
                    NetworkManager.instance.LeaveRoom();

                    //ゲーム終了後or死亡後
                } else {
                    //PoPUpを出したい
                    LeavePopUp.SetActive(true);
                }
            }
        }
    }



    /// <summary>
    /// ゲーム終了後の退出ボタンを押したときに出るPoPUP内にある退出ボタン
    /// ネットワークのチェック完了
    /// </summary>
    private IEnumerator ExitButton() {
        gameManager.chatSystem.gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんが退出しました。";
        gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_ONLINE);
        yield return new WaitForSeconds(3.0f);

        NetworkManager.instance.LeaveRoom();
    }


    /// <summary>
    /// 投票を完了させる
    /// </summary>
    public void Voted(Photon.Realtime.Player player, bool live, bool wolf) {
        if(!live && wolf) {
            Debug.Log("押せません");
            return;
        }

        gameManager.chatSystem.gameMasterChat = PhotonNetwork.LocalPlayer.NickName + "さんは" + player.NickName + "に投票しました。";

        //設定で投票を開示するか否か
        if (RoomData.instance.roomInfo.openVoting == VOTING.開示しない) {
            //開示しない場合
            gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        } else {
            //開示する場合
            gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_ONLINE);
        }
        Debug.Log("Voted");
    }

    /// <summary>
    /// 処刑プレイヤーをGMチャットに表示する
    /// </summary>
    public void ExecutionChat() {
        Debug.Log("voteCount.executionID"+voteCount.executionID);
        Debug.Log(voteCount.executionPlayer.playerName);
        gameManager.chatSystem.gameMasterChat = voteCount.executionPlayer.playerName[voteCount.executionID] + "さんが処刑されました。";
        Debug.Log(voteCount.executionPlayer.playerName);
        gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_ONLINE);
        //gameManager.chatSystem.gameMasterChat = "【投票結果】\r\n\r\n" + voteCount.executionPlayer.playerName + ": " + voteCount.executionPlayer.voteCount + "票";
        //gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_ONLINE);
    }


    /// <summary>
    /// 霊能者の行動を制御(役職増えると、ここに別の処理を加える
    /// </summary>
    public void PsychicAction() {
        if (gameManager.chatSystem.myPlayer.rollType == ROLLTYPE.霊能者 && gameManager.chatSystem.myPlayer.live) {
            if (voteCount.executionPlayer == null) {
                return;
            }
            if (voteCount.executionPlayer.fortune == true) {
                gameManager.chatSystem.gameMasterChat = "【霊能結果】\r\n" + voteCount.mostVotePlayer.playerName + "は人狼（黒）です。";
                Debug.Log(voteCount.mostVotePlayer + "人狼");
            } else {
                gameManager.chatSystem.gameMasterChat = "【霊能結果】\r\n" + voteCount.mostVotePlayer.playerName + "は人狼ではない（白）です。";
                Debug.Log(voteCount.mostVotePlayer + "人狼ではない");
            }
            gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        }
    }


    /// <summary>
    /// 夜の行動を制御する
    /// </summary>
    /// <param name="rollType"></param>
    /// <param name="playerID"></param>
    public void RollAction(int playerID, bool live, bool fortune, bool wolf) {
        
        //死亡時もしくは自分のボタンは機能しない
        if (gameManager.chatSystem.myID == playerID || !live) {
            Debug.Log("押せません。");
            return;
        }

        //ボタンを押した対象のプレイヤーを代入
        //Debug.Log(playerID - 1);
        //Debug.Log(gameManager.chatSystem.playersList[playerID - 1]);
        //Player thePlayer = gameManager.chatSystem.playersList[playerID - 1];
        //Debug.Log(thePlayer.playerName);

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
                    bitedID = thePlayer.playerID;
                    Debug.Log("噛んだID" + thePlayer.playerID);
                    SetBitedPlayerID();
                    Debug.Log("噛んだID" + bitedID);
                    gameManager.chatSystem.gameMasterChat = thePlayer.playerName + "さんを襲撃します。";
                    Debug.Log(thePlayer.playerName + "襲撃します。");
                }
                break;

            case ROLLTYPE.占い師:

                if (!fortune) {
                    gameManager.chatSystem.gameMasterChat = "【占い結果】\r\n" + thePlayer.playerName + "は人狼ではない（白）です。";
                    Debug.Log("白");
                } else {
                    gameManager.chatSystem.gameMasterChat = "【占い結果】\r\n" + thePlayer.playerName + "は人狼（黒）です。";
                    Debug.Log("黒");
                }
                break;
            case ROLLTYPE.騎士:
                gameManager.chatSystem.gameMasterChat = thePlayer.playerName + "さんを護衛します。";
                Debug.Log("守ります");
                //守ったプレイヤーを記録
                protectedID = thePlayer.playerID;
                SetProtectedPlayerID();
                Debug.Log("守ったID" + protectedID);
                break;
            default:
                Debug.Log("押せません。");
                return;
        }
        gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        Debug.Log("RollAction");
    }

    /// <summary>
    /// 朝の結果発表
    /// </summary>
    public void MorningResults() {
        Debug.Log("朝の結果発表");

        ////オンラインプレイヤーから必要な情報を共有する
        //switch (gameManager.chatSystem.myPlayer.rollType) {
        //    case ROLLTYPE.人狼:
        //        protectedID = GetProtectedPlayerID();
        //        Debug.Log("守ったID" + protectedID);
        //        Debug.Log("噛んだID" + bitedID);
        //        break;
        //    case ROLLTYPE.騎士:
        //        bitedID = GetBitedPlayerID();
        //        Debug.Log("守ったID" + protectedID);
        //        Debug.Log("噛んだID" + bitedID);
        //        break;
        //    default:
        //        protectedID = GetProtectedPlayerID();
        //        bitedID = GetBitedPlayerID();
        //        Debug.Log("守ったID" + protectedID);
        //        Debug.Log("噛んだID" + bitedID);
        //        break;
        //}
        //protectedID = GetProtectedPlayerID();
        //bitedID = GetBitedPlayerID();
        //Debug.Log("守ったID" + protectedID);
        //Debug.Log("噛んだID" + bitedID);
        if (PhotonNetwork.IsMasterClient) {
            protectedID = GetProtectedPlayerID();
            bitedID = GetBitedPlayerID();

            foreach(Player playerObj in gameManager.chatSystem.playersList) {
                if(playerObj.playerID == bitedID) {
                    bitedPlayer = playerObj;
                }
            }

            //結果を実行する
            if (bitedID == protectedID) {
            gameManager.chatSystem.gameMasterChat = "【朝の結果発表】\r\n\r\n本日の犠牲者はいません。";
            Debug.Log("犠牲者なし");

            } else {
                //bitedPlayer = 
                gameManager.chatSystem.gameMasterChat = "【朝の結果発表】\r\n\r\n" + bitedPlayer.playerName + "さんが襲撃されました。";
                Debug.Log("襲撃成功");
            }
            gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_ONLINE);

        
            //初期化
            bitedID = 99;
            protectedID = 99;
            SetProtectedPlayerID();
            SetBitedPlayerID();
        }

    }
}
