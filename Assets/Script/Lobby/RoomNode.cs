using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;


/// <summary>
/// ルームノードの設定
/// </summary>
public class RoomNode : MonoBehaviour
{


    //main
    public Text titleText;
    public Text rollText;
    public Text ruleText;
    public Text enterButtonText;
    public Button enterButton;//入室ボタン
    public List<ROLLTYPE> rollList = new List<ROLLTYPE>();
    public RectTransform rectTransform;
 
    //Room情報保存
    public string title;
    public int mainTime;
    public int nightTime;
    public FORTUNETYPE fortuneType;
    public VOTING openVoting;
    public int settingNum;
    public ROOMSELECTION roomSelection;
    public string roomId;
    public List<int> rollNumList = new List<int>();
    public Photon.Realtime.RoomInfo roomInfo;
    
    /// <summary>
    /// 部屋を作った人（マスターだけが利用する
    /// </summary>
    /// <param name="roomData"></param>
    /// <param name="numList"></param>
    /// <param name="rollSumNum"></param>
    public void InitRoomNode(RoomInfo roomData, List<int> numList,int rollSumNum) {

        //タイトル設定
        titleText.text = roomData.title;
        //ルール設定
        ruleText.text = "時間:" + roomData.mainTime + "/" + roomData.nightTime + "\r\n占い:" + roomData.fortuneType + "\r\n投票:" + roomData.openVoting;

        DisplayRollList(numList);

        //最大人数をButtonTextへ
        enterButtonText.text = 1 + "/" + rollSumNum + "入室";

        //Room情報保存
        mainTime = roomData.mainTime;
        nightTime = roomData.nightTime;
        fortuneType = roomData.fortuneType;
        openVoting = roomData.openVoting;
        settingNum = rollSumNum;
        roomSelection = roomData.roomSelection;
        title = roomData.title;

        rollNumList = numList;
    }



    /// <summary>
    /// ルームを作成されたときに相手側の画面で情報を取得する
    /// </summary>
    /// <param name="info"></param>
    public void Activate(Photon.Realtime.RoomInfo info) {
        Debug.Log("Activate通過");
        roomInfo = info;
        //入室処理
        enterButton.onClick.AddListener(OnClickJoinRoom);
        //部屋の設定を表示する
        title = (string)info.CustomProperties["roomName"];
        settingNum = (int)info.MaxPlayers;
        titleText.text = title;
        enterButtonText.text = info.PlayerCount + "/" + settingNum + "入室";
        enterButton.interactable = (info.PlayerCount < info.MaxPlayers);
        if(gameObject != null) {
            gameObject.SetActive(true);
        }
        

        //ルール設定を表示する
        mainTime = (int)info.CustomProperties["mainTime"];
        nightTime = (int)info.CustomProperties["nightTime"];
        fortuneType = (FORTUNETYPE)info.CustomProperties["fortuneType"];
        openVoting = (VOTING)info.CustomProperties["openVoting"];
        ruleText.text = "時間:" + mainTime + "/" + nightTime + "\r\n占い:" + fortuneType + "\r\n投票:" + openVoting;

        //ルームID取得
        Debug.Log((string)info.CustomProperties["roomId"]);
        roomId = (string)info.CustomProperties["roomId"];

        //役職情報取得
        string roll = (string)info.CustomProperties["numListStr"];
        Debug.Log(roll);
        //NumListを解凍する
        int[] intArray = roll.Split(',').Select(int.Parse).ToArray();
        rollNumList = intArray.ToList();

        //役職テキスト表示
        DisplayRollList(rollNumList);
    }

    /// <summary>
    /// 役職のテキスト表示
    /// </summary>
    /// <param name="numList"></param>
    private void DisplayRollList(List<int> numList) {
        //役職テキスト
        int num = 0;
        for (int i = 0; i < rollList.Count; i++) {
            if (numList[i] != 0) {
                string emptyStr = "";
                num++;
                if (num != 0 && num % 3 == 1) {
                    emptyStr = "\r\n";
                }
                string str = rollList[i] + ": " + numList[i];
                rollText.text += emptyStr + str;
            }
        }
    }

    //部屋が閉じられた時の処理
    //public void Deactivate() {
    //    gameObject.SetActive(false);
    //}


    /// <summary>
    ///部屋のListのTransformを一番下へ
    /// </summary>
    /// <returns></returns>
    public RoomNode SetAsLastSibling() {
        rectTransform.SetAsLastSibling();
        return this;
    }

    /// <summary>
    /// 部屋のIDを取得する
    /// </summary>
    /// <param name="roomId"></param>
    public void SetRoomId(string roomId) {
        this.roomId = roomId;
    }

    /// <summary>
    /// roomIDをもとに部屋に参加する
    /// </summary>
    public void OnClickJoinRoom() {
            NetworkManager.instance.JoinRoom(roomId);
            Debug.Log(roomId);
    }

    /// <summary>
    /// 役職それぞれの数をstringにして戻す
    /// </summary>
    /// <param name="intAry"></param>
    /// <returns></returns>
    public string GetStringFromIntArray(int[] intAry) {
        string retStr = "";
        for(int i = 0; i < intAry.Length; i++) {
            //配列の値を一つづつ取り出して、それを文字列として繋げていく
            //つなげた文字の最後にはカンマを入れておく
            retStr += intAry[i] + ",";
        }
        if(retStr != "") {
            //最後のカンマを取り除く
            return retStr.Substring(0, retStr.Length - 1);
        } else {
            return "";
        }
    }
}
