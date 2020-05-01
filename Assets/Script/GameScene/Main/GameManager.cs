using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks {
    //class
    public TimeController timeController;
    public ChatListManager chatListManager;
    public ChatSystem chatSystem;
    public RollExplanation rollExplanation;
    public VoteCount voteCount;
    public GameMasterChatManager gameMasterChatManager;
    public Fillter fillter;
    public ComingOut comingOut;
    public InputView inputView;

    //入室関連
    public Text NumText;//入室してる人数
    public int num;//入場している人数
    public int numLimit;//人数制限
    public int liveNum; //生存人数
    public bool gameStart;
    private float checkTimer;//人数チェック用タイム

    //入室確認
    public int enterNum;//参加希望人数
    public float enterNumTime;//30秒タイマーの残り時間（ゲーム参加再確認の時間
    public GameObject confirmationImage;//確認Image
    public Text confirmationTimeText;//30秒タイマー
    public Text confirmationNumText;//参加人数
    public Text confirmationEnterButtonText;//参加ボタン

    public GameObject damyObj;
    private bool isNumComplete;//ルームの規定人数が揃ったら
    public float setEnterNumTime;//規定人数が揃ったら使われる
    private float checkEnterTimer;//人数チェック用タイム
    private bool isJoined = false;//参加不参加の確認用
    public Transform menbarContent;
    public Transform playerListContent;
    public bool isTimeUp;
    public bool isExit;//確認画面で退出時に使われる
    public List<PlayerButton> playerButtonList = new List<PlayerButton>();


    //ボタン
    public Button exitButton;
    public Button enterButton;

    //その他
    

    [Header("役職リスト")]
    public List<ROLLTYPE> rollTypeList = new List<ROLLTYPE>();//設定されている役職を追加
    public List<ROLLTYPE> ComingOutButtonList = new List<ROLLTYPE>();


    [Header("オフライン用(trueならOff)")]
    public bool isOffline;
    public List<ROLLTYPE> testRollTypeList = new List<ROLLTYPE>();






    void Start() {


        //Onlineなら以下の処理をする
        if (!isOffline) {

            //人数制限をセットする
            if (DebugManager.instance.isDebug) {
                numLimit = DebugManager.instance.numLimit;
            } else {
                numLimit = RoomData.instance.numLimit;

            }
            //部屋を作った人は初めての人なのでこの処理はない
            //二人目以降の人が値を取得する

            num = GetNum();
            enterNum = GetEnterNum();

            if (DebugManager.instance.isDebug && PhotonNetwork.IsMasterClient) {
                num = DebugManager.instance.num;
                enterNum = DebugManager.instance.enterNum;
                var customProperties = new ExitGames.Client.Photon.Hashtable {
                { "testMainTime", DebugManager.instance.testMainTime },
                { "testNightTime", DebugManager.instance.testNightTime },
                {"enterNumTime", DebugManager.instance.setEnterNumTime },

            };
                PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            }
            //参加人数一人追加
            num++;

            //トータルの参加人数を更新して、カスタムプロパティに保存する
            var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                {"num", num },
                {"enterNum", enterNum },
                //{"numLimit",numLimit }
        };

            if (PhotonNetwork.CurrentRoom.IsOpen && PhotonNetwork.CurrentRoom.PlayerCount >= PhotonNetwork.CurrentRoom.MaxPlayers) {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                Debug.Log("IsOpne" + PhotonNetwork.CurrentRoom.IsOpen);
            }
            //カスタムプロパティに更新した変数をセットする
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);

            //PhotonのPlayerクラスに新しい情報を追加
            PhotonNetwork.LocalPlayer.NickName = PlayerManager.instance.name;
            ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
                { "isJoined", isJoined }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(customPlayerProperties);


            SetRoomData();
        }


        //ボタンの追加
        exitButton.onClick.AddListener(ExitButton);
        enterButton.onClick.AddListener(EnterButton);
    }

    
    /// <summary>
    /// 入室と退室を管理
    /// 人数設定と入室者が同じ数になるとゲーム参加再確認を行う
    /// </summary>
    void Update() {
        //ルームにいる場合のみ
        if (PhotonNetwork.InRoom || isOffline) {

            //監視
            if (PhotonNetwork.IsMasterClient && timeController.timeType == TIME.開始前 && numLimit == GetNum()) {
                //参加意思表示確認画面の監視
                CheckEnterNum();
            }

            //ゲーム開始前
            if (!gameStart) {

                //一人以上のプレイヤーが退出した場合isJoinedisExitの値をリセットして確認PopUPを削除する
                if (GetIsExit()) {
                    confirmationImage.SetActive(false);
                    isJoined = false;
                    isExit = false;
                    enterNumTime = 25.0f;
                    SetEnterNumTime();
                    SetIsJoin();
                    SetIsExit();
                }

                //人数が揃ったら役職セット
                SetRoll();
                //人数を確認する
                CountNum();

                //Debug.Log("GetEntrerNUm" + GetEnterNum());
                //Debug.Log("numLimit" + numLimit);
                //Debug.Log("GetNum" + GetNum());

                //確認PopUP表示して参加確認を行う
                if (GetNum() == numLimit && GetEnterNum() != numLimit) {
                    Debug.Log(gameStart);
                    //参加意思表示の人数確認
                    CountDownEnterNumTime();
                    //参加意思表示の確認の時間切れ
                    TimeUpEnterNumTime();
                    //左上の参加人数記載
                    NumText.text = GetNum() + "/" + numLimit;
                    Debug.Log("GetEnterNumUpdate"+GetEnterNum());
                    confirmationNumText.text = enterNum + "/" + numLimit;
                    confirmationTimeText.text = (int)enterNumTime + "秒";
                }
            }
        }
    }




    /////////////////////////////////
    ///メソッド関連
    /////////////////////////////////



    /// <summary>
    /// マスターだけが参加意思表示あるプレイヤーを監視します。
    /// 参加意思があるならenterNumを加算します。
    /// </summary>
    public void CheckEnterNum() {
        checkTimer += Time.deltaTime;
        if (checkTimer >= 1) {
            checkTimer = 0;
            int num = 0;
            enterNum = 0;
            foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                if ((bool)player.CustomProperties["isJoined"]) {
                    num++;
                }
            }
            enterNum = num;
            SetEnterNum();
        }
    }
    /// <summary>
    /// テスト、通常時両方を含めたメソッド
    /// 役職をセットします。
    /// </summary>
    public void SetRoll() {
        Debug.Log("SetRoll");
        //テスト用役職を決定してゲーム開始
        if (GetEnterNum() == numLimit && DebugManager.instance.isTestPlay) {
            confirmationImage.SetActive(false);
            gameStart = true;
            StartCoroutine(SetTestRoll());
        }

        //正規のデータを利用してゲーム開始
        if (GetEnterNum() == numLimit && !DebugManager.instance.isTestPlay) {
            confirmationImage.SetActive(false);
            gameStart = true;
            StartCoroutine(SetRandomRoll());
        }
    }

    /// <summary>
    /// 部屋の参加人数を確認します。
    /// </summary>
    public void CountNum() {
        //1秒ごとに部屋参加人数を確認する
        if (GetNum() != numLimit) {
            checkTimer += Time.deltaTime;
            if (checkTimer >= 1) {
                checkTimer = 0;
                //オフライン用
                if (!isOffline) {
                    num = GetNum();
                }
                NumText.text = num + "/" + numLimit;
            }
        }
    }

    /// <summary>
    /// 参加意思表示を確認する時間をカウントダウンします。
    /// </summary>
    public void CountDownEnterNumTime() {
        Debug.Log("CountDown");
        timeController.savingText.text = "時短";
        //フレンド招待ボタン
        //friendButton.gameObject.SetActive(false);
        damyObj.SetActive(true);
        confirmationImage.SetActive(true);

        //マスターだけ時間を書き込む
        if (PhotonNetwork.IsMasterClient) {

            //人数が揃ったらenterNumTimeをセットする
            if (!isNumComplete) {
                isNumComplete = true;
                enterNumTime = 25.0f;
                SetEnterNumTime();
                Debug.Log("enterNumTime" + enterNumTime);
            }

            //カウントダウン
            checkEnterTimer += Time.deltaTime;
            if (checkEnterTimer >= 1) {
                checkEnterTimer = 0;
                enterNumTime--;
                SetEnterNumTime();
            }

            //マスター以外の処理
        } else {
            enterNumTime = GetEnterNumTime();
        }
    }

    /// <summary>
    /// 参加意思表示の確認時間が切れたら行われる処理
    /// </summary>
    public void TimeUpEnterNumTime() {
        Debug.Log("TimeUpEnterNumTime");
        //参加確認の時間が切れたら ゲームマスターのみ
        if (GetEnterNumTime() < 1 && PhotonNetwork.IsMasterClient && !GetIsTimeUp()) {
            isTimeUp = true;
            SetIsTimeUp();
        }

        //時間切れになったらマスターから時間をもらう
        if (GetIsTimeUp()) {
            Debug.Log((bool)PhotonNetwork.LocalPlayer.CustomProperties["isJoined"]);
            //参加意思表示ないプレイヤーのみ強制退出
            if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["isJoined"]) {
                NetworkManager.instance.LeaveRoom();
                Debug.Log("その他プレイヤー");
            } else {
                //意思表示があるプレイヤーは一度isJoinedをリセット
                isJoined = false;
                ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
                    {"isJoined", isJoined }
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(customPlayerProperties);
                Debug.Log((bool)PhotonNetwork.LocalPlayer.CustomProperties["isJoined"]);
            }
            
            //friendButton.gameObject.SetActive(true);
            damyObj.SetActive(false);
            confirmationImage.SetActive(false);

            //マスターに管理(リセット
            if (PhotonNetwork.IsMasterClient) {
                enterNumTime = setEnterNumTime;
                isTimeUp = false;
                SetIsTimeUp();
                SetEnterNumTime();
                JoinReset();
            }
        }
        
    }

    /// <summary>
    /// 参加意思表示のないプレイヤー分numをマイナスする
    /// </summary>
    private void JoinReset() {
        //プレイヤーを一人ずつ確認して、参加意思表示ないプレイヤーをチェックする
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            //参加意思がないプレイヤーぶんだけnumをマイナスにする
            if (!(bool)player.CustomProperties["isJoined"]) {
                num = GetNum();
                num--;
                SetNum();
            }
        }


        //enterNumもりセット
        enterNum = 0;
        //トータルの参加人数を更新して、カスタムプロパティに保存する
        SetEnterNum();
    }

    /// <summary>
    /// 任意に選択した役職をセッティングする
    /// </summary>
    /// <returns></returns>
    private IEnumerator SettingTestRollType() {
        int i = 0;
        //参加人数分だけランダムに
        //PhotonNetwork.PlayerList　=　今部屋に参加しているプレイヤーのリスト9人ぶん
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
                    {"roll", testRollTypeList[i] }
                };
            player.SetCustomProperties(customPlayerProperties);
            i++;
        }
        var properties = new ExitGames.Client.Photon.Hashtable() {
            {"isSetup", false },
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        yield return new WaitForSeconds(0.5f);
    }


    /// <summary>
    /// 任意に選択した役職をセットする
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetTestRoll() {
        bool isSetup = true;

        //マスターはゲームに使用する役職を用意する
        if (PhotonNetwork.IsMasterClient) {
            foreach (ROLLTYPE rollObj in DebugManager.instance.testRollTypeList) {
                testRollTypeList.Add(rollObj);
            }

            yield return StartCoroutine(SettingTestRollType());
        }
        //上記の役職をランダムに選択している間は待つ
        while (isSetup) {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("isSetup", out object setupObj)) {
                isSetup = (bool)setupObj;
                //Debug.Log(isSetup);
            }
            yield return null;
        }

        //時間などのRooｍDataにある情報を追加する
        //SetRoomData();
        //Playerの生成
        StartCoroutine(StartGame());

    }

    /// <summary>
    /// ルームデータで決められた役職をセットし、それに合わせてボタンのセットとランダムに配布します。
    /// </summary>
    /// <returns></returns>
    private IEnumerator SettingRondomRollType() {


        List<ROLLTYPE> randomRollTypeList = new List<ROLLTYPE>();

        //ルームデータから取得する
        for (int x = 0; x < RoomData.instance.numList.Count; x++) {
            if (RoomData.instance.numList[x] > 0) {
                for (int i = 0; i < RoomData.instance.numList[x]; i++) {
                    randomRollTypeList.Add((ROLLTYPE)x);
                    //Debug.Log((ROLLTYPE)x);
                }
            　   
                
            }
        }


        //参加人数分だけランダムに
        //PhotonNetwork.PlayerList　=　今部屋に参加しているプレイヤーのリスト9人ぶん
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            //プレイヤーのカスタムプロパティに役職を登録しておく
            int randomValue = Random.Range(0, randomRollTypeList.Count);

            ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
                    {"roll", randomRollTypeList[randomValue] }
                };
            player.SetCustomProperties(customPlayerProperties);
            //Debug.Log("ランダム役職：" + player.CustomProperties["roll"]);
            randomRollTypeList.Remove(randomRollTypeList[randomValue]);
        }
        var properties = new ExitGames.Client.Photon.Hashtable() {
            {"isSetup", false },
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        yield return new WaitForSeconds(0.5f);

    }



    /// <summary>
    /// 役職をランダムに配布
    /// </summary>
    private IEnumerator SetRandomRoll() {
        bool isSetup = true;

        //マスターはゲームに使用する役職を用意する
        //yield return StartCoroutineはそのメソッドの処理が終わるまで次の処理へと進まない
        if (PhotonNetwork.IsMasterClient) {
            yield return StartCoroutine(SettingRondomRollType());
        }

        //上記の役職をランダムに選択している間は待つ
        while (isSetup) {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("isSetup", out object setupObj)) {
                isSetup = (bool)setupObj;
                //Debug.Log(isSetup);
            }
            yield return null;
        }

        //時間などのRooｍDataにある情報を追加する
        //SetRoomData();

        //Playerの生成
        StartCoroutine(StartGame());

    }

    /// <summary>
    /// Player作成　ゲームスタート
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartGame() {
        //Debug.Log("CreatePlayers:通過");
        //各自が自分の分のプレイヤーを作る
        SetUpMyRollType();

        //自分が参加者全員のプレイヤーをもらってリストにする
        yield return StartCoroutine(SetPlayerData());
        //Debug.Log("Playerリスト　作成完了");
        Debug.Log("SetPlayerData:end");
        //参加者全員がPlayerのリストを作りおわるまで（上の処理が終わるまで）待機する
        //WatiUntilは条件を満たすまで待機（Trueになるまで）
        //CheckPlayerInGame()で取得できるReadyのフラグはネットワークで共有化されている情報
        //よって参加者全員からリストを作り終わるまで次の処理に行かない

        //PhotonNetwork.PlayerListは配列だからLengthで対応する
        yield return new WaitUntil(() => PhotonNetwork.PlayerList.Length == CheckPlayerInGame());
        Debug.Log("CheckPlayerInGame:end");

        //取得したPlayerButtonに役職をセットしてListに追加
        yield return StartCoroutine(SetPlayerButtonList());

        //Startで反応しない場合は処理中に書くとよい
        rollExplanation.RollExplanationSetUp(rollTypeList);
        comingOut.ComingOutSetUp(ComingOutButtonList);
        timeController.Init(isOffline);
        chatListManager.PlayerListSetUp(chatSystem.myPlayer.wolfChat, chatSystem.myPlayer.live);
        //voteCount.VoteCountListSetUp(numLimit);
        liveNum = PhotonNetwork.PlayerList.Length;
        if (PhotonNetwork.IsMasterClient) {
            SetLiveNum();
        }

        //自分が狼チャットが使えるなら
        if (chatSystem.myPlayer.wolfChat) {
            inputView.wolfModeButtonText.text = "狼";
            inputView.wolfModeButton.interactable = false;
            inputView.wolfMode = true;
        }
        //Debug.Log("参加者全員がPlayerList　準備OK");
    }

    /// <summary>
    /// PlayerObjの作成
    /// </summary>
    private void SetUpMyRollType() {
        Debug.Log("SetUpMyRollType");
        //取得した自分の番号と同じ番号を探して役職を設定する
        foreach (Photon.Realtime.Player playerData in PhotonNetwork.PlayerList) {

            if (chatSystem.myPlayer.playerID == playerData.ActorNumber) {
                chatSystem.myPlayer.rollType = (ROLLTYPE)playerData.CustomProperties["roll"];
                break;
            }
        }
            //自分だけにGameManager.csを入れる。
            //自分のプレイヤークラスを使う時用。
            Debug.Log("Player" + chatSystem.myPlayer);
            chatSystem.myPlayer.PlayerSetUp(this,voteCount,timeController);
    }

    /// <summary>
    /// 各プレイヤーのプレイヤークラスをもらってリストにする
    /// </summary>
    /// <returns></returns>
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
            player.PlayerSetUp(this,voteCount,timeController);
            //各リストに登録
            chatSystem.playersList.Add(player);
            chatSystem.playerNameList.Add(player.playerName);
            playerObj.transform.SetParent(playerListContent);
            yield return null;
        }

        //PlayerListを照準に並び替える（chatSystem.playersListの各要素をaに入れる foreachのようなもの
        //＝＞　a.playerIDでは何を基準に並び替えているのかを決めている
        chatSystem.playersList.OrderByDescending(player => player.playerID);

        //人数分のカミングアウトを用意(要素数を要しただけで初期化はしてないため、中身は””ではなくnull
        //chatSystem.comingOutPlayers = new string[playerObjs.Length];

        //自分のステートを準備完了に変更しカスタムプロパティを更新(個人のフラグではなく、ネットワークで管理できるフラグにする
        var properties = new ExitGames.Client.Photon.Hashtable();
        properties.Add("player-state", "ready");
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }


    /// <summary>
    /// 各PlayerのPhotonデータからStateがReadyの人数を合計して戻す
    /// </summary>
    /// <returns></returns>
    private int CheckPlayerInGame() {
        int retReadyPlayerCount = 0;
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            //各プレイヤーのStateがReadyかどうかを確認し、カウントする
            //player.CustomProperties["player-state"]がnullではないかつreadyが入っている場合カウントを進める
            //player.CustomProperties["player-state"]がnullは確認のために入れている。
            if (player.CustomProperties["player-state"] != null && player.CustomProperties["player-state"].ToString() == "ready") {
                retReadyPlayerCount++;
            }
        }
        return retReadyPlayerCount;
    }


    /// <summary>
    ///役職を設定するときと一緒に追加されるRoomData
    /// </summary>
    private void SetRoomData() {
        //マスター以外RoomDataをもらう
        if (!PhotonNetwork.IsMasterClient) {

            RoomData.instance.roomInfo.mainTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["mainTime"];
            RoomData.instance.roomInfo.nightTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["nightTime"];
            RoomData.instance.roomInfo.fortuneType = (FORTUNETYPE)PhotonNetwork.CurrentRoom.CustomProperties["fortuneType"];
            RoomData.instance.roomInfo.openVoting = (VOTING)PhotonNetwork.CurrentRoom.CustomProperties["openVoting"];
            RoomData.instance.roomInfo.title = (string)PhotonNetwork.CurrentRoom.CustomProperties["roomName"];
            RoomData.instance.numLimit = PhotonNetwork.CurrentRoom.MaxPlayers;
            Debug.Log(RoomData.instance.numLimit);
            string roll = (string)PhotonNetwork.CurrentRoom.CustomProperties["numListStr"];
            int[] intArray = roll.Split(',').Select(int.Parse).ToArray();
            RoomData.instance.numList = intArray.ToList();

        }
        //全員の世界にComingOuottonと役職説明用のボタンを追加する
        for(int i = 0; i < RoomData.instance.numList.Count; i++) {
            ComingOutButtonList.Add((ROLLTYPE)i);
            //役職説明のボタンを追加している
            rollTypeList.Add((ROLLTYPE)i);
        }

        if (DebugManager.instance.isDebug) {
            RoomData.instance.roomInfo.mainTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["testMainTime"];
            RoomData.instance.roomInfo.nightTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["testNightTime"];

        }
    }

    /// <summary>
    /// PlayerButtonのListに追加する
    /// PlayerButtonに情報を追加する
    /// </summary>
    public IEnumerator SetPlayerButtonList() {
        yield return null;
        //ゲーム名にあるPlyaerButtonをすべて取得
        //プレイヤーが入室したらPlayerButtonを取得
        GameObject[] buttonObjs = GameObject.FindGameObjectsWithTag("PlayerButton");

        //取得したPlayerButtonに役職をセットしてListに追加
        //すでにListに追加されているButtonを除外して新たに追加されたButtonだけを追加する
        foreach (GameObject obj in buttonObjs) {
            PlayerButton buttonObj = obj.GetComponent<PlayerButton>();
            foreach (Player player in chatSystem.playersList) {
                if (buttonObj.playerID == player.playerID) {
                    buttonObj.SetRollSetting(player);

                    playerButtonList.Add(buttonObj);
                    //break;
                }
            }

        }

    }

    /// <summary>
    /// Playerが退出したときにButtonを削除する処理
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="otherPlayer"></param>
    public void DestroyPlayerButton(Photon.Realtime.Player otherPlayer) {
        Debug.Log("Destory");
        //Listから削除する
        GameObject[] buttonObjs = GameObject.FindGameObjectsWithTag("PlayerButton");
        foreach (GameObject obj in buttonObjs) {
            PlayerButton buttonObj = obj.GetComponent<PlayerButton>();
            if (buttonObj.playerID == otherPlayer.ActorNumber) {
                Destroy(obj.gameObject);
                break;
            }
        }

    }




    /////////////////////////////////////
    ///ボタン関連
    /////////////////////////////////////

    /// <summary>
    /// 参加確認Imageにて参加orキャンセルする
    /// </summary>
    public void EnterButton() {
        //時間が0になったらボタンの無効化
        if (GetEnterNumTime() <= 0) {
            return;
        }
        //ネットワーク上に保存されているキーがあるかを確認

        switch (confirmationEnterButtonText.text) {
            case "参加":
                //参加するなら
                isJoined = true;
                confirmationEnterButtonText.text = "キャンセル";
                break;
            case "キャンセル":
                //不参加なら
                isJoined = false;
                confirmationEnterButtonText.text = "参加";
                break;
        }
        //isJoinedを各プレイヤーごとに更新（LocalPlayer
        SetIsJoin();
    }

    /// <summary>
    /// 参加確認Imageにて退出する
    /// </summary>
    public void ExitButton() {
        //時間が0になったら時間の無効化
        if (GetEnterNumTime() <= 0) {
            return;
        }
        num--;
        enterNum = 0;

        SetNum();
        SetEnterNum();

        //誰かが退出したことを知らせるためのbool型
        isExit = true;
        SetIsExit();

        NetworkManager.instance.LeaveRoom();

    }







    /////////////////////////////
    ///カスタムプロパティ関連
    /////////////////////////////


    /// <summary>
    /// 入場者数をチェックする
    /// </summary>
    public int GetNum() {
        //ルームに保存されているnumという情報があったら、それをキャストして変数に入れる
        //numというKeyがセットされていて、numがあった場合
        //outとしてobject型のnumObjが作られる。
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("num", out object numObj)) {
            //numObjがobject型なのでintにキャスト
            num = (int)numObj;
        }
        return num;
    }

    public void SetNum() {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"num", num }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    }

    /// <summary>
    /// ゲーム参加確認画面の人数をもらう
    /// </summary>
    public int GetEnterNum() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("enterNum", out object enterNumObj)) {
            enterNum = (int)enterNumObj;
        }
        return enterNum;
    }

    /// <summary>
    /// ゲーム参加確認画面の人数をセットする
    /// </summary>
    public  void SetEnterNum() {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"enterNum", enterNum }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log("setEnterNUm" + (int)PhotonNetwork.CurrentRoom.CustomProperties["enterNum"]);
    }

    /// <summary>
    /// ゲーム参加確認画面の人数チェック中の時間管理
    /// </summary>
    private float GetEnterNumTime() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("enterNumTime", out object enterNumTimeObj)) {
            enterNumTime = (float)enterNumTimeObj;
        }
        Debug.Log(enterNumTime);
        return enterNumTime;
    }

    /// <summary>
    /// EnterNumTimeをセットします。
    /// </summary>
    private void SetEnterNumTime() {
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"enterNumTime", enterNumTime }
          };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log(PhotonNetwork.CurrentRoom.CustomProperties["enterNumTime"]);
    }

    /// <summary>
    /// 生存人数をセットします
    /// </summary>
    public void SetLiveNum() {
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                            {"liveNum", liveNum}
                        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["liveNum"]);
    }

    /// <summary>
    /// 生存人数をもらいます
    /// </summary>
    /// <returns></returns>
    public int GetLiveNum() {
        int value = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("liveNum", out object liveNumObj)) {
            value = (int)liveNumObj;
        }
        return value;
    }

    /// <summary>
    /// タイムアップのboolをセットします
    /// </summary>
    private void SetIsTimeUp() {
        var propertis = new ExitGames.Client.Photon.Hashtable {
                                    {"isTimeUp",isTimeUp }
                                };
        PhotonNetwork.CurrentRoom.SetCustomProperties(propertis);
        Debug.Log("IsTimeUp" + (bool)PhotonNetwork.CurrentRoom.CustomProperties["isTimeUp"]);
    }
    /// <summary>
    /// タイムアップのbool型をもらいます
    /// </summary>
    /// <returns></returns>
    private bool GetIsTimeUp() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("isTimeUp", out object isTimeUpObj)) {
            isTimeUp = (bool)isTimeUpObj;
        }
        Debug.Log("IsTimeUp" + isTimeUp);
        return isTimeUp;
    }
    /// <summary>
    /// 確認画面にて誰が退出した場合に使われる
    /// </summary>
    private bool GetIsExit() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("isExit", out object isExitObj)) {
            isExit = (bool)isExitObj;
        }
        return isExit;
    }
    /// <summary>
    /// 確認画面にて誰が退出した場合に使われる
    /// </summary>
    private void SetIsExit() {
        ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
                    {"isExit", isExit }
                };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customPlayerProperties);
        Debug.Log((bool)PhotonNetwork.CurrentRoom.CustomProperties["isExit"]);
    }
    //public int GetNumLimit() {
    //    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("numLimit", out object numLimitObj)) {
    //        numLimit = (int)numLimitObj;
    //    }
    //    return numLimit;
    //}


    /// <summary>
    /// 参加者の意思表示のbool型をセットする
    /// </summary>
    private void SetIsJoin() {
        ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
                    {"isJoined", isJoined }
                };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customPlayerProperties);
        Debug.Log((bool)PhotonNetwork.LocalPlayer.CustomProperties["isJoined"]);
    }

    
}

