﻿using System.Collections;
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
    public int timeSavingNum;
    public Text timeSavingButtonText;//時短or退出ボタン
    public GameObject LeavePopUp;//ゲーム終了後の退出用PopUP
    public Button exitButton;
    //早朝用
    public int biteID;//噛んだプレイヤーID
    public int protectID;//守ったプレイヤーID
    public Player bitePlayer;


    // Start is called before the first frame update
    void Start()
    {
        timeSavingButton.onClick.AddListener(() => TimeSavingChat());
        exitButton.onClick.AddListener(() => StartCoroutine(ExitButton()));

        //カスタムプロパティ
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"timeSavingNum",timeSavingNum }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    }


    /// <summary>
    /// タイムコントローラのシーンが変更するたびに発言するチャット
    /// </summary>
    public void　TimeManagementChat()
    {
        string gmNode = string.Empty;
        switch (timeController.timeType)
        {
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
    /// 時短希望人数をチェックする
    /// </summary>
    private void CheckTimeSavingNum() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("timeSavingNum", out object timeSavingNumObj)) {
            timeSavingNum = (int)timeSavingNumObj;
        }
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
            }else if (timeSavingButtonText.text == "退出") {
                
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
    /// 処刑プレイヤーをGMチャットに表示する
    /// </summary>
    public void ExecutionChat() {
        gameManager.chatSystem.gameMasterChat = voteCount.executionPlayer.playerName + "が処刑されました。";
        gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_OFFLINE);
    }


    /// <summary>
    /// 騎士が守ったか否か
    /// </summary>
    public void MorningResults() {
        if (biteID == protectID) {
            gameManager.chatSystem.gameMasterChat = "本日の犠牲者はいません。";
            return;
        } else {
            gameManager.chatSystem.gameMasterChat = bitePlayer.playerName + "が襲撃されました。";
        }
        gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_OFFLINE);
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
    /// 霊能者の行動を制御(役職増えると、ここに別の処理を加える
    /// </summary>
    public void PsychicAction() {
        if (gameManager.chatSystem.myPlayer.rollType == ROLLTYPE.霊能者　&& gameManager.chatSystem.myPlayer.live) {
            if (voteCount.executionPlayer == null) {
                return;
            }
            if (voteCount.executionPlayer.fortune == true) {
                gameManager.chatSystem.gameMasterChat = "【霊能結果】\r\n"　+ voteCount.mostVotePlayer.playerName + "は人狼です。";
                Debug.Log(voteCount.mostVotePlayer + "人狼");
            } else {
                gameManager.chatSystem.gameMasterChat = "【霊能結果】\r\n" + voteCount.mostVotePlayer.playerName + "は人狼ではない。";
                Debug.Log(voteCount.mostVotePlayer + "人狼ではない");
            }
            gameManager.chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        }
    }
}
