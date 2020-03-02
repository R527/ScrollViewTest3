using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatSystem : MonoBehaviourPunCallbacks {

    //他クラス
    public Callout callout;
    public ChatNode lastChatNode;
    public Player myPlayer;
    public CheckTabooWard checkTabooWard;
    public TimeController timeController;
    public RollExplanation rollExplanation;
    public GameManager gameManager;
    public TIME timeType;
    //共通項目
    public int id = 0;
    [SerializeField] public InputField chatInputField;
    [SerializeField] ChatNode chatNodePrefab;
    [SerializeField] GameObject content;
    public string comingOutImage;
    public int coTimeLimit;
    public int calloutTimeLimit;
    public string inputData;
    public NGList taboolist;
    public string[] comingOut;//CO状況を保存
    public List<string>playerNameList = new List<string>();
    public List<Player> playersList = new List<Player>();
    public int myID;
    public bool citizen;//市民か否か
    public ScrollRect scrollRect;
    public Button wolfButton;
    public Text MenbarViewText;

    //GameMaster関連
    public string gameMasterChat;

    //色変更
    public Color [] color;
    public Color boardColor;
    public Wolfmode wolfmode;

    //LIst管理
    public ChatListManager chatListManager;



    public void ChatSystemStartUp() {
        //参加Playerを追加する
        for(int i = 0; i < gameManager.numLimit; i++) {
            playerNameList.Add("Player" + (i + 1));
        }
    }
    //MineかOthersなのかをボタンで振り分ける
    public void OnClickMineButton() {
        if (gameManager.numLimit > gameManager.num) {
            CreateChatNode(ChatRoll.MINE, 1, ROLLTYPE.ETC, false, "unknown");
        } else {
            CreateChatNode(ChatRoll.MINE, 1, myPlayer.rollType, false, playerNameList[1]);
        }
        
    }

    public void OnClickOtherButton() {
        CreateChatNode(ChatRoll.OTHERS, 2,ROLLTYPE.狂人,false, playerNameList[2]);
    }


    public void OnClickMineCO(string rollName) {
        CreateChatNode(ChatRoll.MINE, 1, ROLLTYPE.人狼,true, rollName);
    }

    public void OnClickOtherCO(string rollName) {
        CreateChatNode(ChatRoll.OTHERS,2, ROLLTYPE.占い師 , true , rollName);
    }

    public void GameMasterChatNode() {
        CreateChatNode(ChatRoll.GM,0,ROLLTYPE.GM,false,playerNameList[0],"GameMaster");
    }
    private void Update() {
        if (Input.GetKeyUp(KeyCode.Return)) {
            OnClickMineButton();
        } else if (Input.GetKeyUp(KeyCode.I)) {
            OnClickOtherButton();
        }

        //if (scrollRect.verticalNormalizedPosition >= 0.0f) {
        //    scrollRect.StopMovement();
        //    scrollRect.enabled = false;
        //}

        //Debug.Log(scrollRect.verticalNormalizedPosition);
    }

    //第2引数がない場合は自動でNullを入れます。
    //指定がある場合はNullの代わりに別の引数が入る
    private void CreateChatNode(ChatRoll roll, int playerNum,ROLLTYPE rollType,bool comingOut,string playerName,string rollName = "null") {
        if (chatInputField.text == "" && rollName == "null") {
            return;
        }
        //チャットを管理するためのID
        id++;

        //GMかPlayerかの分岐
        if (playerNum != 0) {
            inputData = checkTabooWard.StrMatch(chatInputField.text, taboolist.ngWordList);
            //inputData = chatInputField.text;
            //InputFieldを初期化

            chatInputField.text = "";
        } else {
            GameMasterChat();
            inputData = gameMasterChat;
        }

        //色変更
        if (rollType == ROLLTYPE.ETC) {
            boardColor = color[0];
        } else if (rollName == "GameMaster") {
            boardColor = color[4];
        } else if (myPlayer.live == false) {
            boardColor = color[3];
        } else if (wolfmode.wolf == true) {
            boardColor = color[2];
        } else if (callout.callOut == true) {
            boardColor = color[1];
        } else {
            boardColor = color[0];
        }



        //Dataをそれぞれ代入しInstatiate＆ChatNodeへ処理を移す
        ChatData data = new ChatData(id, roll, inputData, rollName, playerNum, boardColor, playerName,rollType);

        float tempPosition = content.transform.localPosition.y;
        ChatNode chat = Instantiate(chatNodePrefab, content.transform, false);
        chat.InitChatNode(data);
        if(tempPosition <= -280) {
            Vector2 returnPos = new Vector2(content.transform.localPosition.x, tempPosition + chat.gameObject.transform.localPosition.y);
            content.transform.localPosition = returnPos;
        }

        //Chatの種類ごとにList管理
        if (rollType == ROLLTYPE.ETC) {

        } else
        if (rollName == "GameMaster") {
            chatListManager.gameMasterList.Add(chat);
        } else if (myPlayer.live == false) {
            chatListManager.deathList.Add(chat);
        } else if (wolfmode.wolf == true) {
            chatListManager.wlofList.Add(chat);
        } else if (callout.callOut == true) {
            chatListManager.callOutList.Add(chat);
        } else {
            chatListManager.normalList.Add(chat);
        }

        //PlayerごとにList管理
        if(rollType != ROLLTYPE.ETC) {
            chatListManager.allPlayerList[playerNum].Add(chat);
        }

        //SetActiveを制御する
        //市民の場合
        if (myPlayer != null) {
            if (myPlayer.rollType != ROLLTYPE.人狼) {
                if (myPlayer.live == true) {
                    if (boardColor == color[3] || boardColor == color[2]) {
                        chat.gameObject.SetActive(false);
                    }
                }
                if (myPlayer.live == false) {
                    if (boardColor == color[2]) {
                        chat.gameObject.SetActive(false);
                    }
                }
            }

            //人狼の場合
            if (myPlayer.rollType == ROLLTYPE.人狼) {
                if (myPlayer.live == true) {
                    if (boardColor == color[3]) {
                        chat.gameObject.SetActive(false);
                    }
                }
            }
        }

        //if(chatListManager.isfilter == true) {
        //    chat.gameObject.SetActive(false);
        //}

        //playerが連続でチャットを投稿した場合、アイコンObj等を削除する
        if (lastChatNode != null) {
            if (lastChatNode.chatRoll == chat.chatData.roll) {
                chat.iconObj.GetComponent<LayoutElement>().minHeight = 0f;
                chat.iconObj.GetComponent<LayoutElement>().preferredHeight = 0f;
                chat.statusObj.SetActive(false);
            } else {
                chat.iconObj.GetComponent<LayoutElement>().preferredHeight = 20f;
                chat.iconObj.GetComponent<LayoutElement>().minHeight = 20f;
                chat.statusObj.SetActive(true);
            }
        }
        lastChatNode = chat;
        lastChatNode.chatRoll = roll;

        //ComingOutと吹き出しの制御
            if (comingOut == true) {
                coTimeLimit++;
                if (coTimeLimit == 3) {
                    //Buttoncomponentにアクセスしないとinteractableが取れない
                    GameObject.FindGameObjectWithTag("COButton").GetComponent<Button>().interactable = false;
                }
            }
            if(boardColor == color[1]) {
                calloutTimeLimit++;
                if(calloutTimeLimit == 2) {
                    GameObject.FindGameObjectWithTag("CallOutButton").GetComponent<Button>().interactable = false;
                    callout.CallOutText.text = "通常";
                    //CallOutクラスのcallOutboolを変更
                    callout.callOut = false;
                }
            }

    }

    /// <summary>
    /// Debug用
    /// </summary>
    /// <param name="id"></param>
    public void OnClickPlayerID(int id = 0) {
        myID = id;
        foreach(Player player in playersList) {
            if(player.playerID == myID) {
                myPlayer = player;
                MenbarViewText.text = myPlayer.rollType.ToString();
                rollExplanation.rollExplanationButton.interactable = true;
            }
        }
    }

    /// <summary>
    /// GameMasterのチャット管理
    /// </summary>
    public void GameMasterChat() {
        switch (timeController.timeType) {
            case TIME.昼:
                gameMasterChat = "おはようございます。" + "昨夜は○○が●されました。";
                break;
            case TIME.投票時間:
                gameMasterChat = "投票の時間です";
                break;
            case TIME.夜の行動:
                gameMasterChat = "占え";
                break;
        }
    }

}

//この下が分からない。
//enumは定数をまとめることができる
//状態を登録する
//列挙型
//ChatRollが登録された状態を管理する
public enum ChatRoll {
    EXT,
    MINE,
    OTHERS,
    GM,
}

//Classはこのクラスが呼び出されると初めに実行される？
//クラス名とメソッド名を同じにすることをコンストラクト
//クラスが初期化されると自動的に実行される
[System.Serializable]
public class ChatData {
    public int id;
    public ChatRoll roll = ChatRoll.EXT;
    public string body;
    public string rollName;
    public int playerNum;
    public Color boardColor;
    public string playerName;
    public ROLLTYPE rollType;

    public ChatData(int id, ChatRoll roll, string body, string rollName, int playerNum, Color boardColor, string playerName, ROLLTYPE rollType) {
        this.id = id;
        this.roll = roll;
        this.body = body;
        this.rollName = rollName;
        this.playerNum = playerNum;
        this.boardColor = boardColor;
        this.playerName = playerName;
        this.rollType = rollType;
    }
}