using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// chatNodeにつけるクラス
/// ChatSystemから引き受けたデータをもとにCOなのか、誰がチャット打つのか判断する
/// </summary>
public class ChatNode : MonoBehaviourPunCallbacks {

    //class
    public ChatSystem chatSystem;
    public ComingOut comingOutClass;

    //main
    public Text chatText;
    public GameObject statusObj;
    public GameObject iconObj;
    public Text statusText;
    [SerializeField] LayoutGroup layoutGroup;
    public  Image chatBoard;
    public LayoutElement iconObjLayoutElement;
    public LayoutElement chatObjLayoutElement;
    public VerticalLayoutGroup chatVerticalLayoutGroup;
    public Transform chatTran;//チャットとIconを入れ替えるよう

    public bool chatLive;
    public bool chatWolf;
    public int playerID;

    /// <summary>
    /// ChatSystemからデータを受け取りそれをもとにちゃっとNODEを作る
    /// 名前、CO状況の処理、アイコンやそれに伴うRollName、COスタンプ
    /// </summary>
    /// <param name="chatData"></param>
    public void InitChatNode(ChatData chatData, int iconNo, bool comingOut) {
        Debug.Log("InitChatNode");
        chatSystem = GameObject.FindGameObjectWithTag("ChatSystem").GetComponent<ChatSystem>();
        comingOutClass = GameObject.FindGameObjectWithTag("ComingOut").GetComponent<ComingOut>();
        

        //ChatDataはChatSystemのdataを取り入れている
        chatText.text = chatData.inputData;
        //chatIcon.sprite = Resources.Load<Sprite>("CoImage/Player" + iconNo);//Spriteの配列ではなくResouces.Loatにして取得

        //チャットにデータを持たせる
        playerID = chatData.playerID;
        chatLive = chatData.chatLive;
        chatWolf = chatData.chatWolf;


        //GMか否か
        if (chatData.chatType == CHAT_TYPE.GM) {
            statusText.text = "GM";
        } else {

            //PlayerがCOしているか否か（COしている場合は名前の横に職業名を記載
            //stringがnullかから文字かを判定し、その判定をbool型で返す。
            //(chatData.playerID - 1)はGM分をー１にする調整
            if (comingOutClass.GetComingOutText(playerID) == "") {
                Debug.Log("ComingOut : 未");
                statusText.text = chatData.playerName;
            } else {
                Debug.Log("Coming:済");
                statusText.text = chatData.playerName + "【" + comingOutClass.GetComingOutText(playerID) +
                    "CO】";
                //チャットデータ。PlayerNameにCo状況も載せてチャットログを複製するときに使用する
                chatData.comingOutText = "【" + comingOutClass.GetComingOutText(playerID) +
                    "CO】";
            }
        }

        //発言の生成位置の設定　最初だけContentのせいで必ず左寄りに制しえされる問題あり
        if (chatData.chatType == CHAT_TYPE.MINE) {
            Debug.Log("UpperRight");
            layoutGroup.childAlignment = TextAnchor.UpperRight;
            chatVerticalLayoutGroup.childAlignment = TextAnchor.LowerRight;
            statusText.alignment = TextAnchor.MiddleRight;
            chatTran.SetSiblingIndex(0);
            chatText.alignment = TextAnchor.MiddleLeft;
        } else {
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
        }
        //Debug.Log("CO" + comingOut);
        //COした場合幅等を変更する
        if (comingOut) {
            Debug.Log("ComingOut:" + chatData.playerName);
            chatSystem.chatInputField.text = "";

            //COすると名前の横にCO状況を表示
            if (comingOutClass.GetComingOutText(playerID) != string.Empty) {
               
                chatObjLayoutElement.preferredWidth = 60;
                chatObjLayoutElement.preferredHeight = 60;
                statusText.text = chatData.playerName + "【" + comingOutClass.GetComingOutText(playerID) + "CO】";
            } else {
                chatText.text = "カミングアウトを取り消します。";
            }
            
        }
        //yield return StartCoroutine(CheckTextSize());
    }

    /// <summary>
    /// 復元したチャットを初期化する
    /// </summary>
    /// <param name="chatData"></param>
    /// <param name="iconNo"></param>
    /// <param name="comingOut"></param>
    public void InitChatNodeLog(ChatData chatData, int iconNo, bool comingOut) {
        chatSystem = GameObject.FindGameObjectWithTag("ChatSystem").GetComponent<ChatSystem>();

        //ChatDataはChatSystemのdataを取り入れている
        chatText.text = chatData.inputData;
        //chatIcon.sprite = Resources.Load<Sprite>("CoImage/Player" + iconNo);//Spriteの配列ではなくResouces.Loatにして取得

        //チャットにデータを持たせる
        playerID = chatData.playerID;

        //GMか否か
        if (chatData.chatType == CHAT_TYPE.GM) {
            statusText.text = "GM";
        }
        //} else {

        //    //PlayerがCOしているか否か（COしている場合は名前の横に職業名を記載
        //    //stringがnullかから文字かを判定し、その判定をbool型で返す。
        //    //(chatData.playerID - 1)はGM分をー１にする調整
        //    if (comingOutClass.GetComingOutText(playerID) == "") {
        //        Debug.Log("ComingOut : 未");
        //        statusText.text = chatData.playerName;
        //    } else {
        //        Debug.Log("Coming:済");
        //        statusText.text = chatData.playerName + "【" + comingOutClass.GetComingOutText(playerID) +
        //            "CO】";
        //    }
        //}

        //発言の生成位置の設定　最初だけContentのせいで必ず左寄りに制しえされる問題あり
        if (chatData.chatType == CHAT_TYPE.MINE) {
            Debug.Log("UpperRight");
            layoutGroup.childAlignment = TextAnchor.UpperRight;
            chatVerticalLayoutGroup.childAlignment = TextAnchor.LowerRight;
            statusText.alignment = TextAnchor.MiddleRight;
            chatTran.SetSiblingIndex(0);
            chatText.alignment = TextAnchor.MiddleLeft;
        } else {
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
        }



        //Debug.Log("CO" + comingOut);
        ////COした場合幅等を変更する
        //if (comingOut) {
        //    Debug.Log("ComingOut:" + chatData.playerName);
        //    chatSystem.chatInputField.text = "";

        //    //COすると名前の横にCO状況を表示
        //    if (comingOutClass.GetComingOutText(playerID) != string.Empty) {

        //        chatObjLayoutElement.preferredWidth = 60;
        //        chatObjLayoutElement.preferredHeight = 60;
        //        statusText.text = chatData.playerName + "【" + comingOutClass.GetComingOutText(playerID) + "CO】";
        //    } else {
        //        chatText.text = "カミングアウトを取り消します。";
        //    }

        //}
        //StartCoroutine(CheckTextSize());
    }



    ///// <summary>
    ///// チャットの長さに応じて折り返すか否かを決める
    ///// </summary>
    ///// <returns></returns>
    //public IEnumerator CheckTextSize() {
    //    //スクリーン上のレンダリングが終わるまで待つ
    //    //yield return new WaitForEndOfFrame();
    //    Debug.Log("CheckTextSize2");
    //    yield return null;
    //    Debug.Log("CheckTextSize");//OFFFilter時に取得できない　OFFFilter側に問題あり？

    //    if (chatBoard.rectTransform.sizeDelta.x > this.GetComponent<RectTransform>().sizeDelta.x * 0.64f) {
    //        //ChatBoardのLayout ElementのpreferredWidthを64％にする
    //        chatBoard.GetComponent<LayoutElement>().preferredWidth = this.GetComponent<RectTransform>().sizeDelta.x * 0.64f;
    //    }
    //    Debug.Log("CheckTextSize3");
    //}

}