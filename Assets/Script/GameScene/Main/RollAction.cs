using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 夜の行動を制御
/// </summary>
public class RollAction : MonoBehaviour
{

    //class
    public ChatSystem chatSystem;
    public VoteCount voteCount;
    public GameManager gameManager;

    //main
    public Button wolfButton;
    public Text MenbarViewText;
    public string spiritualResult;//霊能結果
    //早朝用
    public int biteID;//噛んだプレイヤーID
    public int protectID;//守ったプレイヤーID
    public Player bitePlayer;



    /// <summary>
    /// 夜の行動を制御する
    /// </summary>
    /// <param name="rollType"></param>
    /// <param name="id"></param>
    public void RollActionButton(ROLLTYPE rollType, int id, bool live, bool fortune,bool def) {
        if (chatSystem.myID == id || live == false) {
            Debug.Log("死んでるので押せません。");
            return;
        }
        //自分の役職と比べる
        switch (chatSystem.myPlayer.rollType) {
            case ROLLTYPE.人狼:

                switch (rollType) {
                    case ROLLTYPE.人狼:
                        Debug.Log("相方です。");
                        break;
                    case ROLLTYPE.市民:
                    case ROLLTYPE.占い師:
                    case ROLLTYPE.狂人:
                    case ROLLTYPE.騎士:
                    case ROLLTYPE.霊能者:
                        if(def == true) {
                            Debug.Log("守られた");
                        } else {
                            Debug.Log("かみ殺す。");
                            //噛んだプレイヤーを記録
                            biteID = id;
                            bitePlayer = chatSystem.playersList[id];
                        }
                        break;
                }
                break;
            case ROLLTYPE.占い師:

                if(fortune == false) {
                    Debug.Log("白");
                } else {
                    Debug.Log("黒");
                }
                break;
            case ROLLTYPE.騎士:
                Debug.Log("守ります");
                //守ったプレイヤーを記録
                protectID = id;
                break;  
            
        }
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
    /// 霊能者の行動を制御(役職増えると、ここに別の処理を加える
    /// </summary>
    public void PsychicAction() {
        if (chatSystem.myPlayer.rollType == ROLLTYPE.霊能者) {
            if(voteCount.executionPlayer == null) {
                return;
            }
            if (voteCount.executionPlayer.fortune == true) {
                Debug.Log(voteCount.mostVotePlayer + "人狼");
            } else {
                Debug.Log(voteCount.mostVotePlayer + "人狼ではない");
            }

        }
    }

}
