using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameOverを見る　GameOver後の処理
/// </summary>
public class GameOver : MonoBehaviour {
    //class
    public ChatSystem chatSystem;
    public GameManager gameManager;
    public TimeController timeController;

    //main
    public int wolfCount;
    public int liverCount;
    public GameObject timeSavingButton;
    public string winnerList;
    public bool isGameOver;

    //gameOverになると時間を計測する
    private float chekTimer;
    public int time;
    public bool isCheck;
    public bool isCheckGMChat;
    public GameObject endRoomPopUp;


    private void Update() {
        //geme終了後時間を計測する
        if (timeController.timeType == TIME.終了) {
            chekTimer += Time.deltaTime;
            if (chekTimer >= 1) {
                chekTimer = 0;
                time--;
            }
        }

        if(time == 120 && !isCheckGMChat) {
            isCheckGMChat = true;
            gameManager.gameMasterChatManager.EndRoomChat();
        }

        //一定時間を過ぎると強制退出する処理
        if(time <= 0 && !isCheck) {
            isCheck = true;
            GameObject obj = GameObject.FindGameObjectWithTag("GameCanvas");
            Instantiate(endRoomPopUp, obj.transform, false);
        }
    }

    /// <summary>
    /// 人狼の人数を把握してゲーム終了かどうかをマスターだけが確認する
    /// </summary>
    public void CheckGameOver() {
        foreach (Player Obj in chatSystem.playersList) {
            //生存者を格納し、狼の人数と生存者数を確認する
            if (Obj.wolf == true && Obj.live == true) {
                wolfCount++;
            }
            if (Obj.wolf == false && Obj.live == true) {
                liverCount++;
            }
        }
        Debug.Log("生存者数：" + liverCount);
        Debug.Log("狼の人数:" + wolfCount);

        bool isWin = false;

        //狼が0人の場合(市民の勝利
        if (wolfCount == 0) {
            //勝利したプレイヤーNameを取り出す
            foreach (Player Obj in chatSystem.playersList) {
                if (!Obj.wolfCamp) {
                    winnerList += "\r\n" + Obj.playerName;
                }
                Debug.Log(winnerList);
            }
            gameManager.gameMasterChatManager.gameMasterChat = "市民陣営の勝利\r\n" + winnerList;
            Debug.Log("市民陣営の勝利");

            if (gameManager.chatSystem.myPlayer.wolfCamp == false) {
                isWin = true;
            } 

            //狼の人数が生存者と同数かそれ以上の場合(人狼の勝利
        } else if (liverCount <= wolfCount) {
            //勝利したプレイヤーNameを取り出す
            foreach (Player Obj in chatSystem.playersList) {
                if (Obj.wolfCamp) {
                    winnerList += "\r\n" + Obj.playerName;
                }
                Debug.Log(winnerList);
            }
            gameManager.gameMasterChatManager.gameMasterChat = "人狼陣営の勝利\r\n" + winnerList;
            Debug.Log("人狼陣営の勝利");

            if (gameManager.chatSystem.myPlayer.wolfCamp == true) {
                isWin = true;
            }

        } else if (liverCount > wolfCount) {
            //勝利条件を満たしていない場合（ゲーム続行
            liverCount = 0;
            wolfCount = 0;
            return;
        }

        //ゲーム終了
        ResultBattleRecord(isWin);

        //不参加状態でゲームを終了した場合突然死数を増やす
        CheckEndGame();

        //役職公開
        OpenRoll();

        //ゲーム終了後の処理
        timeController.timeType = TIME.終了;
        chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        timeController.isPlay = false;
        isGameOver = true;
        //gameManager.fillter.folding = false;
        gameManager.gameMasterChatManager.timeSavingButtonText.text = "退出";
        gameManager.gameMasterChatManager.timeSavingButton.interactable = true;
        gameManager.inputView.wolfMode = false;
        gameManager.inputView.wolfBtnImage.color = gameManager.inputView.btnColor[0];
        gameManager.inputView.wolfModeButtonText.text = "市民";
        gameManager.inputView.superChatBtnImage.color = gameManager.inputView.btnColor[0];
        gameManager.inputView.superChat = false;
        gameManager.inputView.superChatButton.interactable = true;
        gameManager.inputView.foldingButton.interactable = true;
        gameManager.timeController.inputField.interactable = true;
        gameManager.chatSystem.myPlayer.live = true;

        //チャットログを全表示する
        gameManager.chatListManager.ReleaseChatLog();

        //チャットログを保存する
        PlayerManager.instance.SetGameChatLog(isWin);
        
    }

    /// <summary>
    /// 戦績の反映
    /// </summary>
    /// <param name="isWin">勝利陣営にいた場合true、敗北した場合はfalse</param>
    private void ResultBattleRecord(bool isWin) {
        //戦績反映
        PlayerManager.instance.totalNumberOfMatches++;
        PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfMatches, PlayerManager.BATTLE_RECORD_TYPE.総対戦回数);

        //突然死数減少処理
        //突然死が0なら関係なし　1以上なら突然死チェックの数値を一つ増加
        //突然死チェックの数が25を超えたら凸死数を一つ減らす
        if(PlayerManager.instance.totalNumberOfSuddenDeath > 0) {
            PlayerManager.instance.checkTotalNumberOfMatches++;
            PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.checkTotalNumberOfMatches, PlayerManager.BATTLE_RECORD_TYPE.突然死減少チェック);
            if(PlayerManager.instance.checkTotalNumberOfMatches == 25) {
                PlayerManager.instance.checkTotalNumberOfMatches = 0;
                PlayerManager.instance.totalNumberOfSuddenDeath--;
            }
        }

        //勝敗
        if (isWin) {
            PlayerManager.instance.totalNumberOfWins++;
            PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfWins, PlayerManager.BATTLE_RECORD_TYPE.総勝利回数);
        } else {
            PlayerManager.instance.totalNumberOfLoses++;
            PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfLoses, PlayerManager.BATTLE_RECORD_TYPE.総敗北回数);
        }

        //初心者部屋
        if(RoomData.instance.roomInfo.roomSelection == ROOMSELECTION.初心者) {
            //対戦回数を加算
            PlayerManager.instance.beginnerTotalNumberOfMatches++;
            PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfWins, PlayerManager.BATTLE_RECORD_TYPE.初心者対戦回数);

            //勝敗を加算
            if (isWin) {
                PlayerManager.instance.beginnerTotalNumberOfWins++;
                PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfWins, PlayerManager.BATTLE_RECORD_TYPE.初心者勝利回数);
            } else {
                PlayerManager.instance.beginnerTotalNumberOfLoses++;
                PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfLoses, PlayerManager.BATTLE_RECORD_TYPE.初心者敗北回数);
            }

        //一般部屋用の戦績反映
        } else if(RoomData.instance.roomInfo.roomSelection == ROOMSELECTION.一般) {
            //対戦回数を加算
            PlayerManager.instance.generalTotalNumberOfMatches++;
            PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfWins, PlayerManager.BATTLE_RECORD_TYPE.一般対戦回数);

            //勝敗を加算
            if (isWin) {
                PlayerManager.instance.generalTotalNumberOfWins++;
                PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfWins, PlayerManager.BATTLE_RECORD_TYPE.一般勝利回数);
            } else {
                PlayerManager.instance.generalTotalNumberOfLoses++;
                PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfLoses, PlayerManager.BATTLE_RECORD_TYPE.一般敗北回数);
            }
        }
    }

    /// <summary>
    /// 正常にゲームを終了したかをチェックする
    /// </summary>
    public void CheckEndGame() {
        if (PlayerPrefs.GetString("突然死用のフラグ") == PlayerManager.SuddenDeath_TYPE.不参加.ToString()) {
            Debug.Log("不参加");
            PlayerManager.instance.totalNumberOfSuddenDeath++;
            PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfSuddenDeath, PlayerManager.BATTLE_RECORD_TYPE.突然死数);
        }
        PlayerManager.instance.SetStringSuddenDeathTypeForPlayerPrefs(PlayerManager.SuddenDeath_TYPE.ゲーム正常終了);
    }

    /// <summary>
    /// ゲーム終了後役職を開示
    /// </summary>
    private void OpenRoll() {
        GameObject[] btnObjs = GameObject.FindGameObjectsWithTag("PlayerButton"); 
        foreach(GameObject btnObj in btnObjs) {
            btnObj.GetComponent<PlayerButton>().rollText.enabled = true;
        }
    }
}
