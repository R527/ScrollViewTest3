using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    /// <summary>
    /// 人狼の人数を把握してゲーム終了かどうかをマスターだけが確認する
    /// </summary>
    public void CheckGameOver() {
        foreach (Player Obj in chatSystem.playersList) {
            //生存者を格納し、狼の人数と生存者数を確認する
            if (Obj.wolf == true && Obj.live == true) {
                wolfCount++;
            }
            if (Obj.live == true && Obj.wolf == false) {
                liverCount++;
            }
        }
        Debug.Log("生存者数：" + liverCount);
        Debug.Log("狼の人数:" + wolfCount);



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

        }else if (liverCount > wolfCount) {
            //勝利条件を満たしていない場合（ゲーム続行
            liverCount = 0;
            wolfCount = 0;
            return;
        }

        //ゲーム終了
        timeController.timeType = TIME.終了;
        chatSystem.CreateChatNode(false, SPEAKER_TYPE.GAMEMASTER_OFFLINE);
        timeController.isGameOver = false;
        gameManager.gameMasterChatManager.timeSavingButtonText.text = "退出";

    }

}
