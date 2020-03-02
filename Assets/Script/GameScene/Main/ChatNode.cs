using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// chatNodeにつけるクラス
/// ChatSystemから引き受けたデータをもとにCOなのか、誰がチャット打つのか判断する
/// </summary>
public class ChatNode : MonoBehaviour {

    //class
    public ChatData chatData;
    public ChatSystem chatSystem;

    //main
    public GameObject COPopup;
    public GameObject ChatPrefab;
    public Text chatText;
    public GameObject statusObj;
    public Sprite[] iconSprite;//Icon画像配列
    public string playerName;
    public GameObject iconObj;
    public ChatRoll chatRoll = ChatRoll.EXT;
    public Text statusText;
    public int playerNum;
    [SerializeField] LayoutGroup layoutGroup;
    [SerializeField] Image chatBoard;
    [SerializeField] Image chatIcon;
    [SerializeField] Sprite mineSprite;
    [SerializeField] Sprite othersSprite;


    /// <summary>
    /// ChatSystemからデータを受け取りそれをもとにちゃっとNODEを作る
    /// 名前、CO状況の処理、アイコンやそれに伴うRollName、COスタンプ
    /// </summary>
    /// <param name="data"></param>
    public void InitChatNode(ChatData data) {

        //ChatDataはChatSystemのdataを取り入れている
        chatData = data;
        COPopup = GameObject.FindGameObjectWithTag("COPopup");
        chatSystem = GameObject.Find("GameCanvas/ChatSystem").GetComponent<ChatSystem>();
        chatText.text = chatData.body;
        chatIcon.sprite = iconSprite[chatData.playerNum];
        chatBoard.color = chatData.boardColor;
        playerName = chatData.playerName;

        //PlayerがCOしているか否か
        if (chatSystem.comingOut[chatData.playerNum] == "") {
            statusText.text = playerName;
        } else {
            statusText.text = playerName[chatData.playerNum] + "【" + chatSystem.comingOut[chatData.playerNum] + "CO】";
        }
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        //COした場合幅等を変更する
        if (chatData.rollName != "null" && chatData.rollName != "GameMaster") {
            chatSystem.chatInputField.text = "";
            chatBoard.sprite = Resources.Load<Sprite>("CoImage/" + chatData.rollName);
            chatBoard.color = new Color(1f, 1f, 1f, 1f);
            chatBoard.GetComponent<LayoutElement>().preferredWidth = 60;
            chatBoard.GetComponent<LayoutElement>().preferredHeight = 60;
            COPopup.SetActive(false);
            //COすると名前の横にCO状況を表示
            chatSystem.comingOut[chatData.playerNum] = chatData.rollName;
            statusText.text = chatData.roll + "【" + chatSystem.comingOut[chatData.playerNum] + "CO】";

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