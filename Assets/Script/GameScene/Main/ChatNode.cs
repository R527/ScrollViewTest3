using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// chatNodeにつけるクラス
/// ChatSystemから引き受けたデータをもとにCOなのか、誰がチャット打つのか判断する
/// </summary>
public class ChatNode : MonoBehaviour {

    //class
    public ChatSystem chatSystem;

    //main
    public GameObject COPopup;
    public Text chatText;
    public GameObject statusObj;
    public Text statusText;
    //public Sprite[] iconSprite;//Icon画像配列
    public string playerName;
    [SerializeField] LayoutGroup layoutGroup;
    public  Image chatBoard;
    [SerializeField] Image chatIcon;
    public LayoutElement iconObjLayoutElement;

    public bool chatLive;
    public bool chatWolf;
    public int playerID;

    /// <summary>
    /// ChatSystemからデータを受け取りそれをもとにちゃっとNODEを作る
    /// 名前、CO状況の処理、アイコンやそれに伴うRollName、COスタンプ
    /// </summary>
    /// <param name="chatData"></param>
    public void InitChatNode(ChatData chatData, int iconNo, bool comingOut) {

        //ChatDataはChatSystemのdataを取り入れている
        COPopup = GameObject.FindGameObjectWithTag("COPopup");
        chatSystem = GameObject.FindGameObjectWithTag("ChatSystem").GetComponent<ChatSystem>();
        chatText.text = chatData.inputData;
        chatIcon.sprite = Resources.Load<Sprite>("CoImage/Player" + iconNo);//Spriteの配列ではなくResouces.Loatにして取得

        //チャットにデータを持たせる
        playerID = chatData.playerID;
        chatLive = chatData.chatLive;
        chatWolf = chatData.chatWolf;

        //PlayerがCOしているか否か（COしている場合は名前の横に職業名を記載
        if (chatData.chatType == CHAT_TYPE.GM) {
            statusText.text = "GM";
        } else {
            //stringがnullかから文字化を判定し、その判定をbool型で返す。
            //(chatData.playerID - 1)はGM分をー１にする調整
            if (string.IsNullOrEmpty(chatSystem.comingOutPlayers[(chatData.playerID - 1)])) {
                Debug.Log("ComingOut : 未");
                statusText.text = chatData.playerName;
            } else {
                Debug.Log("Coming:済");
                statusText.text = chatData.playerName + "【" + chatSystem.comingOutPlayers[(chatData.playerID - 1)] +
                    "CO】";
            }
        }
        Debug.Log(chatData.chatType);
        //発言の生成位置の設定　最初だけContentのせいで必ず左寄りに制しえされる問題あり
        if (chatData.chatType == CHAT_TYPE.MINE) {
            Debug.Log("UpperRight");
            layoutGroup.childAlignment = TextAnchor.UpperRight;
        } else if (chatData.chatType == CHAT_TYPE.OTHERS) {
            Debug.Log("UpperLeft");
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
        } else if (chatData.chatType == CHAT_TYPE.GM) {
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
        }


        //COした場合幅等を変更する
        if (comingOut) {
            Debug.Log("ComingOut:" + chatData.playerName);
            chatSystem.chatInputField.text = "";
            //chatBoard.sprite = Resources.Load<Sprite>("CoImage/" + chatData.rollName);
            chatBoard.color = new Color(1f, 1f, 1f, 1f);
            chatBoard.GetComponent<LayoutElement>().preferredWidth = 60;
            chatBoard.GetComponent<LayoutElement>().preferredHeight = 60;
            if(COPopup != null) {
                COPopup.SetActive(false);
            }
            //COすると名前の横にCO状況を表示
            chatSystem.comingOutPlayers[chatData.playerID - 1] = chatData.rollType.ToString();
            statusText.text = chatData.playerName + "【" + chatSystem.comingOutPlayers[chatData.playerID - 1] + "CO】";
        }
        StartCoroutine(CheckTextSize());
    }



    /// <summary>
    /// チャットの長さに応じて折り返すか否かを決める
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckTextSize() {
        //スクリーン上のレンダリングが終わるまで待つ？
        yield return new WaitForEndOfFrame();
        if (chatBoard.rectTransform.sizeDelta.x > this.GetComponent<RectTransform>().sizeDelta.x * 0.64f) {
            //ChatBoardのLayout ElementのpreferredWidthを64％にする
            chatBoard.GetComponent<LayoutElement>().preferredWidth = this.GetComponent<RectTransform>().sizeDelta.x * 0.64f;
        }
    }

}