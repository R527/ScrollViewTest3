using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    private Dictionary<string, RoomNode> activeEntries = new Dictionary<string, RoomNode>();
    private Stack<RoomNode> inactiveEntries = new Stack<RoomNode>();
    public bool isBanCheck;
    public IEnumerator checkBanListCoroutine = null;
    public IEnumerator banPlayerKickOutOREnteredRoomCoroutine = null;
    public IEnumerator checkFullRoomCoroutine = null;



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
            //MaxPlayers = 3,
            //プライベートにするかしないか
            IsVisible = true,
            //部屋が開いている状態にする
            IsOpen = true
        };
        //BanListの登録用→後程解凍する
        string banListStr = room.GetStringBanList();
        Debug.Log(banListStr);
        //部屋の各役職の人数を一度一つのストリングにまとめたもの→後程解凍
        string numListStr = room.GetStringFromIntArray(room.rollNumList.ToArray());
        //部屋IDを決定する　名前とリアルタイムで
        string roomId = PlayerManager.instance.playerName + DateTime.Now.ToString("yyyyMMddHHmmss");
        //ルームオプションにカスタムプロパティを設定
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            //ルームネームの情報を部屋に与える
            { "roomId", roomId},
            {"roomName",  room.title},
            {"mainTime", room.mainTime },
            { "nightTime", room.nightTime },
            {"fortuneType", room.fortuneType },
            {"openVoting", room.openVoting },
            {"numListStr", numListStr},
            {"banListStr", banListStr }

        };
        //カスタムプロパティで設定したキーをロービーで参照できるようにする
        roomOptions.CustomRoomPropertiesForLobby = new string[] {
            "roomName",
            "mainTime",
            "nightTime",
            "fortuneType",
            "openVoting",
            "roomId",
            "numListStr",
            "banListStr"
        };
        roomOptions.CustomRoomProperties = customRoomProperties;
        //部屋のIdを取得
        //room.SetRoomId(roomId);

        PhotonNetwork.CreateRoom(roomId, roomOptions, TypedLobby.Default);


    }

    /// <summary>
    /// ただのメソッド　部屋に入室するときに使う
    /// </summary>
    /// <param name="roomName"></param>
    public void JoinRoom(string roomName) {
        Debug.Log("joinRoom");
        PhotonNetwork.JoinRoom(roomName);
    }

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
        roomSetting.roomSelectCanvas.SetActive(true);
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

        //bool isBanCheck = false;
        ////自分BANIDとすでに入室しているmyUniqueIDを比べて一致したら退出する
        //Debug.Log(PhotonNetwork.PlayerList.Length);
        //foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerListOthers) {
        //    foreach (string banUniqueID in PlayerManager.instance.banUniqueIDList) {
        //        if ((string)player.CustomProperties["myUniqueID"] == banUniqueID) {
        //            Debug.Log("banPlayerがいます。");

        //            //退出説明のPopUp表示してから退出処理をする
        //            isBanCheck = true;
        //            //Instantiate();

        //            PhotonNetwork.LeaveRoom();
        //            break;
        //        }
        //    }
        //}

        //if (isBanCheck) {
        //    return;
        //}

        Debug.Log("OnJoinedRoom");
        PhotonNetwork.LocalPlayer.NickName = PlayerManager.instance.playerName;
        Debug.Log("NickName;" + PhotonNetwork.LocalPlayer.NickName);
        Debug.Log("RoomName:" + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("HostName:" + PhotonNetwork.MasterClient.NickName);
        Debug.Log("Slots:" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);


        //シーン遷移
        PhotonNetwork.IsMessageQueueRunning = false;
        SceneStateManager.instance.NextScene(SCENE_TYPE.GAME);


        //プレイヤー作成
        StartCoroutine(FirstCreatePlayerObj());

    }

    /// <summary>
    /// ルームを作っていない人が利用する　部屋情報がアップデートで監視
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList) {
        base.OnRoomListUpdate(roomList);

        foreach (Photon.Realtime.RoomInfo info in roomList) {
            RoomNode roomNode;
            //アクティブの部屋がありますか
            if (activeEntries.TryGetValue(info.Name, out roomNode)) {
                //IsOpenがtureの場合表示する
                //最後のプレイヤーがRoomに入った時にfalseにする
                if (!info.RemovedFromList && info.IsOpen) {
                    //部屋情報を読み取ってアクティブ化する
                    Debug.Log("Activate1");
                    roomNode.Activate(info);

                } else if (!info.RemovedFromList && !info.IsOpen) {
                    //部屋をfalse
                    Debug.Log("Deactive" + "オブジェクトを隠す");
                    roomNode.Deactivate();
                } else {
                    Debug.Log("Deactive" + "オブジェクトを消す");

                    //部屋がなくなった場合
                    activeEntries.Remove(info.Name);

                    inactiveEntries.Push(roomNode);
                }
            
                //部屋が一つも作られていないとき
            } else if (!info.RemovedFromList) {

                roomNode = (inactiveEntries.Count > 0) ? inactiveEntries.Pop().SetAsLastSibling() : Instantiate(roomNodePrefab, roomContent.transform, false);
                Debug.Log("Activate2");
                roomNode.Activate(info);

                activeEntries.Add(info.Name, roomNode);
            }
        }
    }


    //部屋入室後処理まとめ
    /// <summary>
    /// 部屋から退出させる。
    /// </summary>
    public void LeaveRoom() {
        gameManager.timeController.isGameOver = false;
        if (PhotonNetwork.InRoom) {
            gameManager.gameMasterChatManager.LeaveRoomChat();
            PhotonNetwork.LeaveRoom();
            Debug.Log("退出完了");
        }
    }

    /// <summary>
    /// 強制退出させます。
    /// </summary>
    public void ForcedExitRoom() {
        gameManager.timeController.isGameOver = false;
        if (PhotonNetwork.InRoom) {
            PhotonNetwork.LeaveRoom();
            Debug.Log("強制退出完了");
        }
    }

    /// <summary>
    /// 部屋から退出したときに自動で呼ばれる
    /// </summary>
    public override void OnLeftRoom() {
        base.OnLeftRoom();

        SceneStateManager.instance.NextScene(SCENE_TYPE.LOBBY);

    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        Debug.Log("OnPlayerEnteredRoom");
        checkFullRoomCoroutine = SetCoroutine(newPlayer);
        banPlayerKickOutOREnteredRoomCoroutine = BanPlayerKickOutOREnteredRoom(newPlayer);
        StartCoroutine(checkFullRoomCoroutine);
        StartCoroutine(banPlayerKickOutOREnteredRoomCoroutine);
    }

    /// <summary>
    /// ほかのプレイヤーが退出し時に呼び出される
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        base.OnPlayerLeftRoom(otherPlayer);

        //ゲーム開始前のみPlayerButtonを削除する
        if (!gameManager.gameStart) {
            gameManager.DestroyPlayerButton(otherPlayer);
        }
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


    //////////////////////////////
    ///メソッド
    //////////////////////////////

    /// <summary>
    /// ゲーム開始前にプレイヤーを作成する
    /// ロールなど詳細な情報は後程追加する
    /// </summary>
    public IEnumerator FirstCreatePlayerObj() {
        yield return new WaitForSeconds(1.0f);
        PhotonNetwork.IsMessageQueueRunning = true;
        Debug.Log("FirstCreatePlayerObj");

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

        Debug.Log(newPlayer.CustomProperties["myUniqueID"]);
        while (newPlayer.CustomProperties["myUniqueID"] == null) {
            Debug.Log(newPlayer.CustomProperties["myUniqueID"]);
            yield return null;
        }
        Debug.Log("BanPlayerKickOutOREnteredRoom");

        var propertiers = new ExitGames.Client.Photon.Hashtable();

        //すでに入室しているBanListと新しく入ってきたPlayerのIDを比べて一致したら退出させる
        //bool isBanCheck = false;
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {

            for (int i = 0; i < 3; i++) {

                if ((string)player.CustomProperties["banUniqueID" + i.ToString()] == (string)newPlayer.CustomProperties["myUniqueID"]) {

                    Debug.Log("banPlayerがいます。");

                    //BanPlayerにbool型を送信します。

                    propertiers.Add("isBanPlayer", true);
                    newPlayer.SetCustomProperties(propertiers);

                    StopCoroutine(checkBanListCoroutine);
                    isBanCheck = true;
                    break;
                }
            }
        }



        //BanPlayerがいる場合ここで処理を停止する
        //満室の場合もここで処理を止める
        if (isBanCheck) {
            Debug.Log("BanPlayerがいます");
            yield break;
        }

        Debug.Log("通過");
        propertiers.Add("isBanPlayer", false);
        newPlayer.SetCustomProperties(propertiers);

        Debug.Log((bool)newPlayer.CustomProperties["isCheckEmptyRoom"]);
        if (!(bool)newPlayer.CustomProperties["isCheckEmptyRoom"]) {
            Debug.Log("満室停止");
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
                isCheck = (bool)isCheckEmptyRoomObj;
                StopCoroutine(checkFullRoomCoroutine);
                yield return null;
            } else {
                yield return null;
            }
        }
    }

    public IEnumerator SetCoroutine(Photon.Realtime.Player newPlayer) {
        yield return StartCoroutine(CheckEmptyRoom(newPlayer));
        Debug.Log("チェック終了");
    }



}