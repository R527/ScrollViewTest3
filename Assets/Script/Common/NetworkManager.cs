﻿using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;


/// <summary>
/// Photonを管理するクラス
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks {

    //class
    public RoomSetting roomSetting;
    public static NetworkManager instance;
    public GameManager gameManager;

    //部屋入室処理関連
    public RoomNode roomNodePrefab;
    public GameObject roomContent;
    public Dictionary<string, RoomNode> activeEntries = new Dictionary<string, RoomNode>();
    public Stack<RoomNode> inactiveEntries = new Stack<RoomNode>();
    public List<RoomNode> roomNodeObjList = new List<RoomNode>();
    public bool isBanCheck;
    public EMPTYROOM emtyRoom;
    public IEnumerator checkBanListCoroutine = null;
    public IEnumerator banPlayerKickOutOREnteredRoomCoroutine = null;
    public IEnumerator checkEmptyRoomCoroutine = null;
    public string banListStr;
    public string roomName;
    public Photon.Realtime.RoomInfo joinedRoom;
    public GameObject joinedRoomObj;
    public List<Photon.Realtime.RoomInfo> roomInfoList;
    public bool isRoomMaster;//trueなら部屋を作成した人

    //Lobby関連
    public string sceneType;

    //Lobby入室完了確認
    public bool isCheckJoinLobby;//Lobbyに入室しているかどうかの確認 falseなら入室していない

    /////////////////////
    /// SetUp
    /////////////////////

    private void Awake() {
        if (instance == null) {
            instance = this;
            PhotonNetwork.AutomaticallySyncScene = false;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }


    public void SetUp() {
        PhotonNetwork.ConnectUsingSettings();
        roomSetting = GameObject.FindGameObjectWithTag("roomSetting").GetComponent<RoomSetting>();
        roomContent = GameObject.FindGameObjectWithTag("content");
    }

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
        Debug.Log("OnConnectedToMaster");
        isCheckJoinLobby = true;
        roomSetting.GetComponent<RoomSetting>().createRoomButton.interactable = true;
    }

    //部屋作成関連まとめ
    /// <summary>
    /// サーバーに部屋情報を渡す　
    /// </summary>
    /// <param name="maxPlayer"></param>
    /// <param name="room"></param>
    public void PreparateCreateRoom(int maxPlayer, RoomNode room) {
        //部屋情報を確定する
        RoomOptions roomOptions = new RoomOptions {
            //プロパティを設定している
            MaxPlayers = (byte)maxPlayer,
            //プライベートにするかしないか
            IsVisible = true,
            //部屋が開いている状態にする
            IsOpen = true
        };
        //BanListの登録用→後程解凍する
        string banListStr = room.GetStringBanList();
        //部屋の各役職の人数を一度一つのストリングにまとめたもの→後程解凍
        string numListStr = room.GetStringFromIntArray(room.rollNumList.ToArray());
        //部屋IDを決定する　名前とリアルタイムで
        string roomId = PlayerManager.instance.playerName + DateTime.Now.ToString("yyyyMMddHHmmss");
        //ルームオプションにカスタムプロパティを設定
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            //ルームネームの情報を部屋に与える
            {"roomId", roomId},
            {"roomName",  room.title},
            {"mainTime", room.mainTime },
            { "nightTime", room.nightTime },
            {"fortuneType", room.fortuneType },
            {"openVoting", room.openVoting },
            {"numListStr", numListStr},
            {"banListStr", banListStr},
            {"roomSelect", room.roomSelection },
            {"numLimit", maxPlayer},
            {"playerCount", 1},
            {"suddenDeath_Type", room.suddenDeath_Type}
        };
        //カスタムプロパティで設定したキーをロービーで参照できるようにする
        roomOptions.CustomRoomPropertiesForLobby = new string[] {
            "roomId",
            "roomName",
            "mainTime",
            "nightTime",
            "fortuneType",
            "openVoting",
            "numListStr",
            "banListStr",
            "roomSelect",
            "numLimit",
            "suddenDeath_Type"
        };
        roomOptions.CustomRoomProperties = customRoomProperties;
        //部屋のIdを取得

        PhotonNetwork.CreateRoom(roomId, roomOptions, TypedLobby.Default);
        isRoomMaster = true;
        Destroy(room.gameObject);
    }


    //////////////////
    /// PhotonNetwork関連
    //////////////////

    //条件を満たすと自動で呼び出す
    //ロビーを離れたとき
    public override void OnLeftLobby() {
        Debug.Log("OnLeftLobby");
    }
    //部屋ができたとき
    public override void OnCreatedRoom() {
        Debug.Log("OnCreatedRoom");
    }
    //部屋を作るのに失敗
    public override void OnCreateRoomFailed(short returnCode, string message) {
        Debug.Log("OnCreateRoomFailed");
        roomSetting.rollSettingCanvas.SetActive(true);
        roomSetting.roomSettingCanvas.SetActive(false);
    }
    /// <summary>
    /// 入室時に使われる
    /// </summary>
    public override void OnJoinedRoom() {

        //InRoom＝そのプレイヤーが部屋にいるかどうか～tureなら
        if (!PhotonNetwork.InRoom) {
            return;
        }

        Debug.Log("OnJoinedRoom");
        PhotonNetwork.LocalPlayer.NickName = PlayerManager.instance.playerName;

        //シーン遷移
        PhotonNetwork.IsMessageQueueRunning = false;
        SceneStateManager.instance.NextScene(SCENE_TYPE.GAME);

        //プレイヤー作成
        StartCoroutine(FirstCreatePlayerObj());
    }

    /// <summary>
    /// 入室に失敗しました
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnJoinRoomFailed(short returnCode, string message) {
        Debug.Log("OnJoinRoomFailed");
        OnRoomListUpdate(roomInfoList);
        roomSetting.rollSetting.wrongPopUpObj.SetActive(true);
        roomSetting.rollSetting.wrongPopUp.wrongText.text = "入室に失敗しました。";
    }

    /// <summary>
    /// ルームを作っていない人が利用する　
    /// 自分がLobbyに入った時とRoom情報が変更されたときに動作する
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList) {

        Debug.Log("OnRoomListUpdate");
        //Debug.Log("roomListCount" + roomList.Count);
        roomInfoList = roomList;

        foreach (Photon.Realtime.RoomInfo info in roomList) {
            //Debug.Log("info.RemovedFromList" + info.RemovedFromList);

            RoomNode roomNode = new RoomNode();

            //アクティブの部屋な部屋がある場合その部屋の更新を行う
            if (activeEntries.TryGetValue(info.Name, out roomNode)) {
                //info.CustomProperties["roomId"];
                //IsOpenがtureの場合表示する
                //最後のプレイヤーがRoomに入った時にfalseにする
                if (!info.RemovedFromList && info.IsOpen) {
                    roomNode.Activate(info);
                } else {
                    //部屋がなくなった場合
                    activeEntries.Remove(info.Name);
                    roomNode.Deactivate();
                    inactiveEntries.Push(roomNode);
                }

                //まだアクティブ化されていないRoomがある場合こちらで生成する
            }else if (!info.RemovedFromList || sceneType == "GAME") {

                roomNode = (inactiveEntries.Count > 0) ? inactiveEntries.Pop().SetAsLastSibling() : Instantiate(roomNodePrefab, roomContent.transform, false);
                roomNode.Activate(info);
                roomNodeObjList.Add(roomNode);
                activeEntries.Add(info.Name, roomNode);

                OnRoomListUpdate(roomList);
            }
        }
        sceneType = "LOBBY";
    }


    
    /// <summary>
    /// 部屋に新しいプレイヤーが入室した際に呼ばれる
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        Debug.Log("OnPlayerEnteredRoom");
        checkEmptyRoomCoroutine = SetCoroutine(newPlayer);
        banPlayerKickOutOREnteredRoomCoroutine = BanPlayerKickOutOREnteredRoom(newPlayer);
        StartCoroutine(checkEmptyRoomCoroutine);
        StartCoroutine(banPlayerKickOutOREnteredRoomCoroutine);
    }

    /// <summary>
    /// ほかのプレイヤーが退出し時に呼び出される
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        base.OnPlayerLeftRoom(otherPlayer);

        //ゲーム開始前Playerを削除する
        if (!gameManager.gameStart && !gameManager.isConfirmation) {
            DeleateOtherPlayer(otherPlayer);

            //人数を減らす
            gameManager.num--;
            var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                {"num", gameManager.num },
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);

            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }


    //////////////////////////////
    ///メソッド
    //////////////////////////////

    //部屋入室後処理まとめ
    /// <summary>
    /// 部屋から退出させる。
    /// </summary>
    public void LeaveRoom() {
        gameManager.timeController.isPlay = false;
        sceneType = "GAME";


        //部屋を作成した人ではないが、最後の一人になった時に部屋を閉じた場合　自分の部屋を削除する
        if (!isRoomMaster) {
            if (joinedRoom.CustomProperties.TryGetValue("playerCount", out object joinedRoomPlayerCountObj)) {
                if ((int)joinedRoomPlayerCountObj == 1) {
                    Destroy(joinedRoomObj);
                    joinedRoom = null;
                }
            }
        }

        if (PhotonNetwork.InRoom) {
            PhotonNetwork.LeaveRoom();
        }

        isRoomMaster = false;
    }

    /// <summary>
    /// 強制退出させます。
    /// </summary>
    public void ForcedExitRoom() {
        gameManager.timeController.isPlay = false;
        if (PhotonNetwork.InRoom) {
            PhotonNetwork.LeaveRoom();
        }
    }

    /// <summary>
    /// 部屋から退出したときに自動で呼ばれる
    /// </summary>
    public override void OnLeftRoom() {
        base.OnLeftRoom();
        Destroy(gameManager.timeController);
        Debug.Log("OnLeftRoom");
        PhotonNetwork.Disconnect();

        SceneStateManager.instance.NextScene(SCENE_TYPE.LOBBY);
        StartCoroutine(ReSetSetUp());
    }


    /// <summary>
    /// 退出時に一度ネットワークを閉じてからつなぐ処理をする
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReSetSetUp() {
        yield return new WaitUntil(() => PhotonNetwork.NetworkingClient.DisconnectedCause == DisconnectCause.DisconnectByClientLogic);
        SetUp();
    }

    /// <summary>
    /// 部屋に入室するときに使う
    /// </summary>
    /// <param name="roomName"></param>
    public void JoinRoom(string roomName) {
        this.roomName = roomName;
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// マスターだけが扱える
    /// 他のプレイヤーをキックする
    /// 呼び出した後はOnPlayerLeftRoomが呼ばれる(多分
    /// </summary>
    /// <param name="player"></param>
    public void KickOutPlayer(Photon.Realtime.Player player) {
        PhotonNetwork.CloseConnection(player);
    }

    /// <summary>
    /// ゲーム開始前にプレイヤーを作成する
    /// ロールなど詳細な情報は後程追加する
    /// </summary>
    public IEnumerator FirstCreatePlayerObj() {

        yield return null;
        PhotonNetwork.IsMessageQueueRunning = true;
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Game");

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        GameObject playerObj = PhotonNetwork.Instantiate("Prefab/Game/Player", gameManager.menbarContent.position, gameManager.menbarContent.rotation);
        Player player = playerObj.GetComponent<Player>();
        gameManager.chatSystem.myPlayer = player;

    }


    /// <summary>
    /// 他プレイヤーが入室したときにBanListをみてプレイヤーをキックするか否かを決める
    /// </summary>
    /// <param name="newPlayer"></param>
    /// <returns></returns>
    private IEnumerator BanPlayerKickOutOREnteredRoom(Photon.Realtime.Player newPlayer) {

        while (newPlayer.CustomProperties["myUniqueID"] == null) {
            yield return null;
        }

        var propertiers = new ExitGames.Client.Photon.Hashtable();

        //すでに入室しているBanListと新しく入ってきたPlayerのIDを比べて一致したら退出させる
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            for (int i = 0; i < 3; i++) {
                if ((string)player.CustomProperties["banUniqueID" + i.ToString()] == (string)newPlayer.CustomProperties["myUniqueID"]) {
                    KickBanPlayer(propertiers,newPlayer);
                    break;
                }
            }
        }

        foreach(string banID in PlayerManager.instance.roomBanUniqueIdList) {
            if(banID == (string)newPlayer.CustomProperties["myUniqueID"]) {
                KickBanPlayer(propertiers, newPlayer);
                break;
            }
        }

        //BanPlayerがいる場合ここで処理を停止する
        //満室の場合もここで処理を止める
        if (isBanCheck) {
            yield break;
        }

        propertiers.Add("isBanPlayer", false);
        newPlayer.SetCustomProperties(propertiers);

        if (emtyRoom == EMPTYROOM.満室) {
            yield break;
        }

        StartCoroutine(gameManager.gameMasterChatManager.EnteredRoom(newPlayer));
    }

    /// <summary>
    /// 満室チェック
    /// </summary>
    /// <param name="newPlayer"></param>
    /// <returns></returns>
    public IEnumerator CheckEmptyRoom(Photon.Realtime.Player newPlayer) {
        bool isCheck = false;
        while (!isCheck) {
            if (newPlayer.CustomProperties.TryGetValue("isCheckEmptyRoom", out object isCheckEmptyRoomObj)) {
                emtyRoom = (EMPTYROOM)Enum.Parse(typeof(EMPTYROOM), isCheckEmptyRoomObj.ToString());
                //満室処理
                if (emtyRoom == EMPTYROOM.満室 || emtyRoom == EMPTYROOM.入室許可) {
                    isCheck = true;
                }
            }
            yield return null;
        }

        if (emtyRoom == EMPTYROOM.満室) {
            yield break;
        }
    }

    public IEnumerator SetCoroutine(Photon.Realtime.Player newPlayer) {
        yield return StartCoroutine(CheckEmptyRoom(newPlayer));
    }

    public Dictionary<string,RoomNode> GetActiveEntries() {
        return activeEntries;
    }

    /// <summary>
    /// Playerが抜けたときの処理
    /// BanListの更新とPlayerButtonの削除
    /// </summary>
    public void DeleateOtherPlayer(Photon.Realtime.Player otherPlayer) {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        //Playerが抜けたときにBanListの更新をする
        string banListStr = "";
        // 抜けたプレイヤーのBanListを無視して更新する
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if (player == otherPlayer) {
                continue;
            }
            banListStr += (string)player.CustomProperties["myBanListStr"];
        }
        //RoomBanListも追加する
        banListStr += PlayerManager.instance.roomBanUniqueIdStr;
        if (banListStr != "") {
            banListStr.Substring(0, banListStr.Length - 1);
        }
        var customRoomBanListProperties = new ExitGames.Client.Photon.Hashtable {
            {"banListStr",banListStr }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomBanListProperties);

        //PlayerButton削除
        gameManager.DestroyPlayerButton(otherPlayer);
    }

    private void KickBanPlayer(ExitGames.Client.Photon.Hashtable propertiers,Photon.Realtime.Player newPlayer) {
        //BanPlayerにbool型を送信します。
        propertiers.Add("isBanPlayer", true);
        newPlayer.SetCustomProperties(propertiers);
        if (checkBanListCoroutine != null) {
            StopCoroutine(checkBanListCoroutine);
        }
        isBanCheck = true;
    }
}