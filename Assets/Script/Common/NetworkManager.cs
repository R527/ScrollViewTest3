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


        //プレイヤー作成
        
        //StartCoroutine(FirstCreatePlayerObj());
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

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {
        Debug.Log("OnPlayerEnteredRoom");
        base.OnPlayerEnteredRoom(newPlayer);

        GameObject playerObj = PhotonNetwork.Instantiate("Prefab/Game/Player", gameManager.menbarContent.position, gameManager.menbarContent.rotation);
        Player players = playerObj.GetComponent<Player>();
        players.playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        //menbarContentに出ないので改めて置きなおす
        players.transform.SetParent(gameManager.menbarContent);

        players.FirstSetUp(gameManager);

        //自分の処理（ほかのプレイヤーが入室したときに自分を複数生成してしまうのを抑制する
        if (gameManager.chatSystem.playersList.Count <= 0) {
            gameManager.chatSystem.playersList.Add(players);
            gameManager.chatSystem.myPlayer = players;
            Debug.Log("一人目");
        } else {
            foreach (Player player in gameManager.chatSystem.playersList) {
                if (players.playerID != player.playerID) {
                    gameManager.chatSystem.playersList.Add(players);
                    gameManager.chatSystem.myPlayer = players;
                    Debug.Log("二人目生成");
                } else {
                    Destroy(players);
                    Debug.Log("二人目削除");
                }
            }
        }

        StartCoroutine(SetPlayerData());
    }

    private IEnumerator SetPlayerData() {
        //Debug.Log("SetPlayerData:Start");
        GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");

        //whileの条件式を満たしている間は、繰り返す。
        //参加している人数と探してきた人数が一致しているかどうか
        //条件式をクリアするまで繰り返され、抜けない
        while (playerObjs.Length != PhotonNetwork.PlayerList.Length) {
            playerObjs = GameObject.FindGameObjectsWithTag("Player");
            //yield return null;は1フレーム待つ
            yield return null;
        }
        Debug.Log("参加プレイヤー人数：" + playerObjs.Length);

        //全キャラのPlayerSetUpを実行
        foreach (GameObject playerObj in playerObjs) {
            Player player = playerObj.GetComponent<Player>();
            //plyaerIDが見つかったらcontinue
            if (gameManager.chatSystem.playersList.Find(x => x.playerID == player.playerID)) {
                continue;
            }
            
            //player.PlayerSetUp(this, voteCount, timeController);
            //各リストに登録
            gameManager.chatSystem.playersList.Add(player);
            gameManager.chatSystem.playerNameList.Add(player.playerName);
            playerObj.transform.SetParent(gameManager.menbarContent);
            yield return null;
        }

        //PlayerListを照準に並び替える（chatSystem.playersListの各要素をaに入れる foreachのようなもの
        //＝＞　a.playerIDでは何を基準に並び替えているのかを決めている
        gameManager.chatSystem.playersList.OrderByDescending(player => player.playerID);

        //人数分のカミングアウトを用意(要素数を要しただけで初期化はしてないため、中身は””ではなくnull
        //chatSystem.comingOutPlayers = new string[playerObjs.Length];

        //自分のステートを準備完了に変更しカスタムプロパティを更新(個人のフラグではなく、ネットワークで管理できるフラグにする
        //var properties = new ExitGames.Client.Photon.Hashtable();
        //properties.Add("player-state", "ready");
        //PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        //Debug.Log("SetPlayerData:Complete");
    }


    /// <summary>
    /// ほかのプレイヤーが退出し時に呼び出される
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    //public  void CloseConnection(Photon.Realtime.Player kickPlayer) {

    //}

    /// <summary>
    /// マスターだけが扱える
    /// 他のプレイヤーをキックする
    /// 呼び出した後はOnPlayerLeftRoomが呼ばれる(多分
    /// </summary>
    /// <param name="player"></param>
    public void KickOutPlayer(Photon.Realtime.Player player) {
        PhotonNetwork.CloseConnection(player);
    }

    ///// <summary>
    ///// ゲーム開始前にプレイヤーを作成する
    ///// ロールなど詳細な情報は後程追加する
    ///// </summary>
    //public IEnumerator FirstCreatePlayerObj() {
    //    yield return new WaitForSeconds(1.0f);

    //    gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

    //    GameObject playerObj = PhotonNetwork.Instantiate("Prefab/Game/Player", gameManager.menbarContent.position, gameManager.menbarContent.rotation);
    //    Player player = playerObj.GetComponent<Player>();
    //    player.playerID = PhotonNetwork.LocalPlayer.ActorNumber;

    //    gameManager.chatSystem.myPlayer = player;
    //    Debug.Log("Player" + gameManager.chatSystem.myPlayer);
    //    player.PlayerSetUp(gameManager, gameManager.voteCount, gameManager.timeController);
    //}

}