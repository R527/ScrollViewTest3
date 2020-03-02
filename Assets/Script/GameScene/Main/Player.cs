using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MenbarViewにあるPlayer情報などをまとめてる
/// </summary>
public class Player : MonoBehaviour {


    //class
    public ROLLTYPE rollType;
    public GameManager gameManager;
    public ChatSystem chatSystem;

    //main
    public int playerID;
    public Text plyaerText;
    public string playerName;
    public Button playerButton;
    public bool live;//生死
    public bool fortune;//占い結果
    public bool spiritual;//霊能結果
    public bool wolf;//狼か否か
    public bool wolfCamp;//狼陣営か否か
    public Button wolfButton;
    public Text MenbarViewText;

    //仮
    public bool def;


    /// <summary>
    /// MenbarViewにあるPlayerButtonの設定と役職ごとの判定を追加
    /// </summary>
    public void PlayerSetUp() {
        //ラムダ式で引数をあてる
        playerButton.onClick.AddListener(() => gameManager.PlayerButton(rollType, playerID,live,fortune,def));
        live = true;
        chatSystem = GameObject.Find("GameCanvas/ChatSystem").GetComponent<ChatSystem>();
        plyaerText.text = rollType.ToString();
        //ニックネームに変更する予定
        playerName = chatSystem.playerNameList[playerID];

        //役職ごとの判定を追加
        if (rollType == ROLLTYPE.人狼) {
            fortune = true;
            spiritual = true;
            wolf = true;
            wolfCamp = true;
        } else if(rollType == ROLLTYPE.狂人) {
            wolfCamp = true;
        }
    }
}

