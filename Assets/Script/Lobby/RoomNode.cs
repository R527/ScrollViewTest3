using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;


/// <summary>
/// ルームノードの設定
/// </summary>
public class RoomNode : MonoBehaviourPunCallbacks {

    //calss
    public Search search;
    public FORTUNETYPE fortuneType;
    public VOTING openVoting;
    public ROOMSELECTION roomSelection;
    public SUDDENDEATH_TYPE suddenDeath_Type;

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
    public int settingNum;
    public string roomId;
    public List<int> rollNumList = new List<int>();
    private List<string> banList = new List<string>();
    public bool isCheckBanList;//banListのチェックtrueならBanListに登録されている


    /// <summary>
    /// 部屋を作った人（マスターだけが利用する
    /// </summary>
    /// <param name="roomInfo"></param>
    /// <param name="numList"></param>
    /// <param name="rollSumNum"></param>
    public void InitRoomNode(room_information roomInfo, List<int> numList,int rollSumNum) {
        //タイトル設定
        titleText.text = roomInfo.title;
        
        //ルール設定
        //TODO テキストの最後に突然死関連を入れると部屋が表示されない
        ruleText.text = "時間:" + roomInfo.mainTime + "/" + roomInfo.nightTime + "\r\n占い:" + roomInfo.fortuneType + "\r\n投票:" + roomInfo.openVoting + "\r\n凸数：" + suddenDeath_Type.ToString().Trim('_');
        //+ "\r\n凸数:" + roomInfo.suddenDeath_Type
        DisplayRollList(numList);

        //最大人数をButtonTextへ
        enterButtonText.text = 1 + "/" + rollSumNum + "入室";

        //Room情報保存
        mainTime = roomInfo.mainTime;
        nightTime = roomInfo.nightTime;
        fortuneType = roomInfo.fortuneType;
        openVoting = roomInfo.openVoting;
        settingNum = rollSumNum;
        roomSelection = roomInfo.roomSelection;
        title = roomInfo.title;
        suddenDeath_Type = roomInfo.suddenDeath_Type;
        rollNumList = numList;
    }



    /// <summary>
    /// ルームを作成されたときに相手側の画面で情報を取得する
    /// </summary>
    /// <param name="roomInfo"></param>
    public void Activate(Photon.Realtime.RoomInfo roomInfo) {
        //入室処理
        enterButton.onClick.RemoveAllListeners();
        enterButton.onClick.AddListener(OnClickJoinRoom);
        //部屋の設定を表示する
        title = (string)roomInfo.CustomProperties["roomName"];
        settingNum = (int)roomInfo.MaxPlayers;
        titleText.text = title;
        enterButtonText.text = roomInfo.PlayerCount + "/" + settingNum + "入室";
        //roomInfo.CustomProperties["playerCount"] = roomInfo.PlayerCount;
        //banListStrを解凍する
        string banListStr = (string)roomInfo.CustomProperties["banListStr"];
        banList = banListStr.Split(',').ToList<string>();

        //ルール設定を表示する
        mainTime = (int)roomInfo.CustomProperties["mainTime"];
        nightTime = (int)roomInfo.CustomProperties["nightTime"];
        fortuneType = (FORTUNETYPE)roomInfo.CustomProperties["fortuneType"];
        openVoting = (VOTING)roomInfo.CustomProperties["openVoting"];
        roomSelection = (ROOMSELECTION)roomInfo.CustomProperties["roomSelect"];
        suddenDeath_Type = (SUDDENDEATH_TYPE)roomInfo.CustomProperties["suddenDeath_Type"];
        ruleText.text = "時間:" + mainTime + "/" + nightTime + "\r\n占い:" + fortuneType + "\r\n投票:" + openVoting + "\r\n凸数:" + suddenDeath_Type.ToString().Trim('_');

        //GameObjectがNullでなければ、
        //かつ自分がBanListに登録されていなければtrueにする

        gameObject.SetActive(false);

        if (gameObject != null && !CheckBanListToRoomOwner()) {

            if(search == null) {
                search = GameObject.FindGameObjectWithTag("Search").GetComponent<Search>();
            }

            //未設定を処理する
            FORTUNETYPE lastSearchFortuneType = search.searchFortuneType;
            VOTING lastSearchOpenVoting = search.searchOpenVoting;
            int lastSearchJoinNum = search.searchJoinNum;
            if (search.searchFortuneType == FORTUNETYPE.未設定) {
                search.searchFortuneType = fortuneType;
            }
            if (search.searchOpenVoting == VOTING.未設定) {
                search.searchOpenVoting = openVoting;
            }

            //フィルターにかける
            //人数設定が未設定
            if (search.isNumLimit == true) {
                if (fortuneType == search.searchFortuneType && openVoting == search.searchOpenVoting && search.searchRoomSelection == roomSelection && CheckSuddenDeath()) {
                    gameObject.SetActive(true);
                }
                //人数設定が設定されている場合
            } else {
                if (fortuneType == search.searchFortuneType && openVoting == search.searchOpenVoting && settingNum == search.searchJoinNum && search.searchRoomSelection == roomSelection && CheckSuddenDeath()) {
                    gameObject.SetActive(true);
                }
            }

            //未設定処理初期化
            search.searchFortuneType = lastSearchFortuneType;
            search.searchOpenVoting = lastSearchOpenVoting;

        }

        //人数が満員だったら押せない
        enterButton.interactable = (roomInfo.PlayerCount < roomInfo.MaxPlayers);

        //ルームID取得
        roomId = (string)roomInfo.CustomProperties["roomId"];

        //役職情報取得
        string roll = (string)roomInfo.CustomProperties["numListStr"];
        //RollListを解凍する
        int[] intArray = roll.Split(',').Select(int.Parse).ToArray();
        rollNumList = intArray.ToList();

        //役職テキスト表示
        DisplayRollList(rollNumList);

        //新規のRoomNodeを一番下に置く
        SetAsLastSibling();
    }


    /// <summary>
    /// 突然死数を見て部屋の表示を決定する
    /// </summary>
    public bool CheckSuddenDeath() {
        bool isCheck = false;

        //凸1回以下
        if (PlayerManager.instance.totalNumberOfSuddenDeath == 1 && (suddenDeath_Type == SUDDENDEATH_TYPE._1回以下 || suddenDeath_Type == SUDDENDEATH_TYPE._制限なし) && PlayerManager.instance.totalNumberOfMatches >= 25) {
            isCheck = true;
        
        //凸2回以上
        } else if (PlayerManager.instance.totalNumberOfSuddenDeath > 1 && suddenDeath_Type == SUDDENDEATH_TYPE._2回以上) {
            isCheck = true;

        //凸0回
        } else if (PlayerManager.instance.totalNumberOfSuddenDeath == 0 && PlayerManager.instance.totalNumberOfMatches >= 25) {
            isCheck = true;

        //制限なし
        }else if(PlayerManager.instance.totalNumberOfSuddenDeath == 0 && suddenDeath_Type == SUDDENDEATH_TYPE._制限なし) {
            isCheck = true;

        }
        return isCheck;
    }
    /// <summary>
    /// 部屋のBanListと自分のIDが一致した場合部屋を非アクティブにする
    /// </summary>
    /// <returns></returns>
    private bool CheckBanListToRoomOwner() {
        isCheckBanList = false;
        foreach (string banUniqueID in banList) {
            if(banUniqueID.Contains(PlayerManager.instance.myUniqueId)) {
                isCheckBanList = true;
                break;
            } 
        }
        return isCheckBanList;
    }

    /// <summary>
    /// 役職のテキスト表示
    /// </summary>
    /// <param name="numList"></param>
    private void DisplayRollList(List<int> numList) {
        rollText.text = string.Empty;
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

    ///部屋が閉じられた時の処理
    public void Deactivate() {
       Destroy(gameObject);
    }

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
    private void OnClickJoinRoom() {

        //Roomを削除する際にエラーが発生するのでそれを回避するためのroomInfoとObjを登録する
        foreach (RoomInfo roomInfo in NetworkManager.instance.roomInfoList) {
            if (roomId == (string)roomInfo.CustomProperties["roomId"]) {
                NetworkManager.instance.joinedRoom = roomInfo;
                Debug.Log("NetworkManager.instance.joinedRoom" + NetworkManager.instance.joinedRoom.CustomProperties["roomId"]);
                //Debug.Log("roomCount" + (int)NetworkManager.instance.joinedRoom.CustomProperties["playerCount"]);
            }
        }
        foreach (RoomNode roomObj in NetworkManager.instance.roomNodeObjList) {
            if (roomId == roomObj.roomId) {
                NetworkManager.instance.joinedRoomObj = roomObj.gameObject;
            }
        }

        //banListのチェックを入れる、はじく場合はPopUpを出して
        NetworkManager.instance.JoinRoom(roomId);
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

    /// <summary>
    /// マスターが部屋を作った時に実行される
    /// 自分のBanListを一つの文字列にする
    /// </summary>
    /// <returns></returns>
    public string GetStringBanList() {
        string banListStr = "";
        foreach(string str in PlayerManager.instance.banUniqueIDList) {
            banListStr += str + ",";
        }
        if (banListStr != "") {
            //最後のカンマを取り除く
            return banListStr.Substring(0, banListStr.Length - 1);
        } else {
            return "";
        }
    }
}
