using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameOverを見る　GameOver後の処理
/// </summary>
public class GameOver : MonoBehaviour
{
    //class
    public ChatSystem chatSystem;
    public GameManager gameManager;
    public TimeController timeController;

    //main
    public int wolfCount;
    public int liverCount;
    public GameObject timeSavingButton;

    /// <summary>
    /// 人狼の人数を把握して
    /// </summary>
    public void CheckGameOver() {
        foreach(Player Obj in chatSystem.playersList) {
            if(Obj.wolf == true && Obj.live == true) {
                wolfCount++;
            }
            if(Obj.live == true && Obj.wolf == false) {
                liverCount++;
            }

        }

        //役職ごとの勝利判定を追加

        if (wolfCount == 0) {
            if (chatSystem.myPlayer.wolfCamp == true) {
                chatSystem.gameMasterChat = "人狼の敗北";
                Debug.Log("人狼の敗北");
            } else {
                chatSystem.gameMasterChat = "人狼の勝利";
                Debug.Log("人狼の勝利");
            }
            chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER);
            timeController.isGameOver = false;
            return;
        } else if (liverCount <= wolfCount) {
            Debug.Log(gameManager.liveNum);
            Debug.Log(wolfCount);
            timeController.timeType = TIME.終了;
            if (chatSystem.myPlayer.wolfCamp == true) {
                chatSystem.gameMasterChat = "市民の勝利";
                Debug.Log("市民の勝利");
            } else {
                chatSystem.gameMasterChat = "市民の敗北";
                Debug.Log("市民の敗北");
            }
            chatSystem.CreateChatNode(false, ChatSystem.SPEAKER_TYPE.GAMEMASTER);
            timeController.isGameOver = false;
            return;
        } else if (liverCount > wolfCount) {
            liverCount = 0;
            wolfCount = 0;
        }
    }


}
