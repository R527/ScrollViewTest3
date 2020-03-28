using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;


/// <summary>
/// Photonを管理するクラス
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks {

    //class
    public RoomSetting roomSetting;
    public static NetworkManager instance;

    //部屋入室処理関連
    public RoomNode roomNodePrefab;
    public GameObject content;
    private Dictionary<string, RoomNode> activeEntries = new Dictionary<string, RoomNode>();
    private Stack<RoomNode> inactiveEntries = new Stack<RoomNode>();


    
    private void Awake() {
        if (instance == null) {
            instance = this;
            PhotonNetwork.AutomaticallySyncScene = false;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public  void SetUp() {
        PhotonNetwork.ConnectUsingSettings();
        roomSetting = GameObject.FindGameObjectWithTag("roomSetting").GetComponent<RoomSetting>();
        content = GameObject.FindGameObjectWithTag("content");
    }

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
    }

    //部屋作成関連まとめ
    /// <summary>
    /// サーバーに部屋情報を渡す？？　
    /// </summary>
    /// <param name="maxPlayer"></param>
    /// <param name="room"></param>
    public void PreparateCreateRoom(int maxPlayer, RoomNode room) {
        //部屋情報を確定する
        RoomOptions roomOptions = new RoomOptions {
            //プロパティを設定している
            //MaxPlayers = (byte)maxPlayer,
            MaxPlayers = 3,
            //プライベートにするかしないか
            IsVisible = true,
            //部屋が開いている状態にする
            IsOpen = true
        };

        //部屋の各役職の人数を一度一つのストリングにまとめたもの→後程解凍
        string numListStr = room.GetStringFromIntArray(room.rollNumList.ToArray());
        //部屋IDを決定する　名前とリアルタイムで
        string roomId = PlayerManager.instance.name + DateTime.Now.ToString("yyyyMMddHHmmss");
        //ルームオプションにカスタムプロパティを設定
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            //ルームネームの情報を部屋に与える
            { "roomId", roomId},
            {"roomName",  room.title},
            {"mainTime", room.mainTime },
            { "nightTime", room.nightTime },
            {"fortuneType", room.fortuneType },
            {"openVoting", room.openVoting },
            {"numListStr", numListStr}
        };
        //カスタムプロパティで設定したキーをロービーで参照できるようにする
        roomOptions.CustomRoomPropertiesForLobby = new string[] {
            "roomName",
            "mainTime",
            "nightTime",
            "fortuneType",
            "openVoting",
            "roomId",
            "numListStr"
        };
        roomOptions.CustomRoomProperties = customRoomProperties;
        //部屋のIdを取得
        room.SetRoomId(roomId);

        PhotonNetwork.CreateRoom(roomId, roomOptions, TypedLobby.Default);


    }

    //？？
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
        Debug.Log("OnJoinedRoom");
        //InRoom＝そのプレイヤーが部屋にいるかどうか～tureなら
        if (PhotonNetwork.InRoom) {
            PhotonNetwork.LocalPlayer.NickName = PlayerManager.instance.name;
            Debug.Log("NickName;" + PhotonNetwork.LocalPlayer.NickName);
            Debug.Log("RoomName:" + PhotonNetwork.CurrentRoom.Name);
            Debug.Log("HostName:" + PhotonNetwork.MasterClient.NickName);
            Debug.Log("Slots:" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);
        }
        //シーン遷移
        SceneStateManager.instance.NextScene(SCENE_TYPE.GAME);
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
                       roomNode.Activate(info);
                } else {
                    //部屋がなくなった場合
                    activeEntries.Remove(info.Name);
                   //部屋をfalse
                    //roomNode.Deactivate();
                    inactiveEntries.Push(roomNode);
                }
                //部屋が一つも作られていないとき
            } else if (!info.RemovedFromList) {
                roomNode = (inactiveEntries.Count > 0) ? inactiveEntries.Pop().SetAsLastSibling() : Instantiate(roomNodePrefab, content.transform, false);
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
        if (PhotonNetwork.InRoom) {
            PhotonNetwork.LeaveRoom();
            Debug.Log("退出完了");
        }
        
    }

    /// <summary>
    /// 部屋から退出したときに自動で呼ばれる
    /// </summary>
    public override void OnLeftRoom() {
        base.OnLeftRoom();
       // PhotonNetwork.IsMessageQueueRunning = false;

        SceneStateManager.instance.NextScene(SCENE_TYPE.LOBBY);

    }

    //public static bool HasStartTime(this Room room) {
    //    return room.CustomProperties.ContainsKey(KeyStartTime);
    //}

}