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
    public GameMasterChatManager gameMasterChatManager;

    //main
    public Button wolfButton;
    public Text MenbarViewText;
    public string spiritualResult;//霊能結果




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
                            gameMasterChatManager.biteID = id;
                            gameMasterChatManager.bitePlayer = chatSystem.playersList[id];
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
                gameMasterChatManager.protectID = id;
                break;  
            
        }
    }





}
