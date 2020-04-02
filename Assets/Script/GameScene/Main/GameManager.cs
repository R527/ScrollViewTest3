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
    public RollAction rollAction;
    public TimeController timeController;
    public ChatListManager chatListManager;
    public ChatSystem chatSystem;
    public Player playerPrefab;
    public RollExplanation rollExplanation;
    public VoteCount voteCount;
    public TIME timeType;
    public GameMasterChatManager gameMasterChatManager;
    public Fillter fillter;

    //main
    public Text NumText;//入室してる人数
    public int num;//入場している人数
    public int enterNum;//参加希望人数
    public int numLimit;//人数制限
    public int liveNum; //生存人数
    public float enterNumTime;//30秒タイマーの残り時間（ゲーム参加再確認の時間
    public int second;//陰とがたに変更
    public GameObject confirmationImage;//確認Image
    public Text confirmationTimeText;//30秒タイマー
    public Text confirmationNumText;//参加人数
    public Text confirmationEnterButtonText;//参加ボタン
    public Text ruleConfiramationButtonText;//ルール確認ボタン
    public GameObject ruleConfiramationObj;//ルール確認Obj
    public GameObject maskObj;//ルール確認用Objのマスク
    public bool gameStart;
    public Button friendButton;
    public GameObject damyObj;
    private bool isNumComplete;//ルームの規定人数が揃ったら
    public float setEnterNumTime = 25.0f;//規定人数が揃ったら使われる
    private float checkEnterTimer;//人数チェック用タイム
    private float checkTimer;//人数チェック用タイム
    private bool isJoined = false;//参加不参加の確認用
    public Transform menbarContent;

    //ボタン
    public Button timeControllButton;//時短or退出ボタン

    [Header("役職リスト")]
    public List<ROLLTYPE> rollTypeList = new List<ROLLTYPE>();//設定されている役職を追加
    //


    [Header("オフライン用(trueならOff)")]
    public bool isOffline;
    public List<ROLLTYPE> testRollTypeList = new List<ROLLTYPE>();






    void Start() {

        if (DebugManager.instance.isDebug) {
            num = DebugManager.instance.num;
            enterNum = DebugManager.instance.enterNum;
        }
        //offlineなら以下の処理をする
        if (!isOffline) {
            //部屋を作った人は初めての人なのでこの処理はない
            //二人目以降の人が値を取得する
            CheckNum();
            //参加人数一人追加
            num++;

            //トータルの参加人数を更新して、カスタムプロパティに保存する
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"num", num },
            {"enterNum", enterNum },
            {"enterNumTime", enterNumTime },
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

            if (PhotonNetwork.IsMasterClient) {
                var customProperties = new ExitGames.Client.Photon.Hashtable {
                { "testMainTime", DebugManager.instance.testMainTime },
                { "testNightTime", DebugManager.instance.testNightTime }
            };
                PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
            }

        }

    }

    /// <summary>
    /// 入場者数をチェックする
    /// </summary>
    private void CheckNum() {
        //ルームに保存されているnumという情報があったら、それをキャストして変数に入れる
        //numというKeyがセットされていて、numがあった場合
        //outとしてobject型のnumObjが作られる。
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("num", out object numObj)) {
            //numObjがobject型なのでintにキャスト
            num = (int)numObj;
        }
    }

    /// <summary>
    /// ゲーム参加確認画面の人数チェック
    /// </summary>
    private void CheckEnterNum() {

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("enterNum", out object enterNumObj)) {
            enterNum = (int)enterNumObj;
        }
    }

    /// <summary>
    /// ゲーム参加確認画面の人数チェック中の時間管理
    /// </summary>
    private float CheckEnterNumTime() {
        float value = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("enterNumTime", out object enterNumTimeObj)) {
            value = (float)enterNumTimeObj;
        }
        return value;
    }

    private void SetEnterNumTime() {
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                            {"enterNumTime", enterNumTime }
                        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    }

    public void SetLiveNum() {
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                            {"liveNum", liveNum}
                        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["liveNum"]);
    }

    public int GetLiveNum() {
        int value = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("liveNum", out object liveNumObj)) {
            value = (int)liveNumObj;
        }
        return value;
    }
    /// <summary>
    /// 入室と退室を管理
    /// 人数設定と入室者が同じ数になるとゲーム参加再確認を行う
    /// </summary>
    void Update() {

        //ルームにいる場合のみ
        if (PhotonNetwork.InRoom || isOffline) {
            if (!gameStart) {

                //テスト用役職を決定してゲーム開始
                if (enterNum == numLimit && DebugManager.instance.isTestPlay) {
                    confirmationImage.SetActive(false);
                    gameStart = true;
                    StartCoroutine(SetTestRoll());
                }

                //正規のデータを利用してゲーム開始
                if (enterNum == numLimit && !DebugManager.instance.isTestPlay) {
                    confirmationImage.SetActive(false);
                    gameStart = true;
                    StartCoroutine(SetRandomRoll());
                }

                //1秒ごとに部屋参加人数を確認する
                if (num != numLimit) {
                    checkTimer += Time.deltaTime;
                    if (checkTimer >= 1) {
                        checkTimer = 0;
                        //オフライン用
                        if (!isOffline) {
                        CheckNum();
                        }
                        NumText.text = num + "/" + numLimit;
                    }
                }

                //部屋参加人数がそろったら1秒ごとにenterNumを確認
                if (num == numLimit) {
                    if (!isNumComplete) {
                        isNumComplete = true;
                    }
                    checkTimer += Time.deltaTime;
                    if (checkTimer >= 1) {
                        checkTimer = 0;
                        //オフライン用
                        if (!isOffline) {
                        CheckEnterNum();
                        }
                        confirmationNumText.text = enterNum + "/" + numLimit;
                    }
                }

                //ネットワーク処理が必要
                //入場人数と制限人数が一致してかつ、参加希望人数と制限人数が一致していない場合
                if (num == numLimit && enterNum != numLimit) {
                    timeController.savingText.text = "時短";
                    //フレンド招待ボタン
                    friendButton.gameObject.SetActive(false);
                    damyObj.SetActive(true);
                    confirmationImage.SetActive(true);

                    //時間を書き込む
                    if (PhotonNetwork.IsMasterClient) {

                        if (!isNumComplete) {
                            enterNumTime = CheckEnterNumTime();
                            //人数が揃ったらEnterNumTimeを初期値に
                            enterNumTime = setEnterNumTime;
                            SetEnterNumTime();
                        }
                        checkEnterTimer += Time.deltaTime;
                        if (checkEnterTimer >= 1) {
                            checkEnterTimer = 0;
                            enterNumTime--;
                            SetEnterNumTime();
                        }
                    } else {
                        enterNumTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["enterNumTime"];
                    }
                    confirmationTimeText.text = (int)enterNumTime + "秒";
                    //参加確認の時間が切れたら
                    if (enterNumTime <= 0) {
                        confirmationImage.SetActive(false);
                        //ゲームマスターのみ
                        if (PhotonNetwork.IsMasterClient) {
                            JoinReset();
                            Debug.Log("master");
                        }
                        //参加意思表示ないプレイヤーのみ強制退出
                        if (!isJoined) {
                            NetworkManager.instance.LeaveRoom();
                            Debug.Log("その他プレイヤー");
                        } else {
                            //意思表示があるプレイヤーは一度isJoinedをリセット
                            isJoined = false;
                            ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
                            {"isJoined", isJoined }
                        };
                            PhotonNetwork.LocalPlayer.SetCustomProperties(customPlayerProperties);
                        }
                        //オンラインにしてマスターに管理(リセット
                        if (PhotonNetwork.IsMasterClient) {
                            enterNumTime = setEnterNumTime;
                            ExitGames.Client.Photon.Hashtable customRoomMasterProperties = new ExitGames.Client.Photon.Hashtable {
                            {"enterNumTime", enterNumTime }
                        };
                            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomMasterProperties);
                            Debug.Log((float)PhotonNetwork.CurrentRoom.CustomProperties["enterNumTime"]);
                        }
                        friendButton.gameObject.SetActive(true);
                        damyObj.SetActive(false);
                    }
                    //左上の参加人数記載
                    NumText.text = num + "/" + numLimit;
                    confirmationNumText.text = enterNum + "/" + numLimit;
                }
            }
        }
    }


    /// <summary>
    /// 参加人数確認
    /// </summary>
    private void JoinReset() {
        //プレイヤーを一人ずつ確認して、参加意思表示ないプレイヤーをチェックする
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            bool isCheckJoin = (bool)player.CustomProperties["isJoined"];
            //参加意思がないプレイヤーぶんだけnumをマイナスにする
            if (!isCheckJoin) {
                CheckNum();
                num--;
                ExitGames.Client.Photon.Hashtable SetCustomProperties = new ExitGames.Client.Photon.Hashtable {
                    {"num", num }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(SetCustomProperties);
            }
        }
        //enterNumもりセット
        //Timeもりセット
        enterNum = 0;
        //トータルの参加人数を更新して、カスタムプロパティに保存する
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"enterNum", enterNum },
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    }




    /// <summary>
    /// 参加確認Imageにて参加orキャンセルする
    /// </summary>
    public void EnterButton() {
        //時間が0になったらボタンの無効化
        if (enterNumTime <= 0) {
            return;
        }
        //ネットワーク上に保存されているキーがあるかを確認
        //保存されていたらEnterNumの値が書き換わる
        if (!isOffline) {
        CheckEnterNum();
        }
        switch (confirmationEnterButtonText.text) {
            case "参加":
                //参加するなら
                isJoined = true;
                enterNum++;
                confirmationEnterButtonText.text = "キャンセル";
                break;
            case "キャンセル":
                //不参加なら
                isJoined = false;
                enterNum--;
                confirmationEnterButtonText.text = "参加";
                break;
        }
        //isJoinedを各プレイヤーごとに更新（LocalPlayer
        ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
            {"isJoined", isJoined }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customPlayerProperties);
        //Debug.Log((bool)PhotonNetwork.LocalPlayer.CustomProperties["isJoined"]);

        //enterNumを更新
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"enterNum", enterNum }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["enterNum"]);

        confirmationNumText.text = enterNum + "/" + numLimit;

    }

    /// <summary>
    /// 参加確認Imageにて退出する
    /// </summary>
    public void ExitButton() {
        //時間が0になったら時間の無効化
        if (enterNumTime <= 0) {
            return;
        }
        num--;
        enterNum = 0;
        confirmationImage.SetActive(false);
        friendButton.gameObject.SetActive(true);
        damyObj.SetActive(false);
        maskObj.SetActive(true);
    }



　   /// <summary>
    /// 人数が規定人数揃った後の確認UIにあるルール表示ボタン
    /// </summary>
    public void RuleConfirmation() {
        if (ruleConfiramationButtonText.text == "↓") {
            ruleConfiramationObj.SetActive(true);
            ruleConfiramationButtonText.text = "↑";
        } else {
            ruleConfiramationObj.SetActive(false);
            ruleConfiramationButtonText.text = "↓";
        }
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
        SetRoomData();

        //Playerの生成
        StartCoroutine(CreatePlayers());

    }

    /// <summary>
    /// ルームデータで決められた役職をセットし、それに合わせてボタンのセットとランダムに配布します。
    /// </summary>
    /// <returns></returns>
    private IEnumerator SettingRondomRollType() {


        List<ROLLTYPE> randomRollTypeList = new List<ROLLTYPE>();

        for (int x = 0; x < RoomData.instance.numList.Count; x++) {
            if (RoomData.instance.numList[x] > 0) {
                for (int i = 0; i < RoomData.instance.numList[x]; i++) {
                    randomRollTypeList.Add((ROLLTYPE)x);
                    //Debug.Log((ROLLTYPE)x);
                }
                //役職説明のボタンを追加している
                rollTypeList.Add((ROLLTYPE)x);
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
        SetRoomData();

        //Playerの生成
        StartCoroutine(CreatePlayers());

    }

    /// <summary>
    /// Player作成　ゲームスタート
    /// </summary>
    /// <returns></returns>
    private IEnumerator CreatePlayers() {
        //Debug.Log("CreatePlayers:通過");
        //各自が自分の分のプレイヤーを作る
        CreatePlayerObj();

        //自分が参加者全員のプレイヤーをもらってリストにする
        yield return StartCoroutine(SetPlayerData());
        //Debug.Log("Playerリスト　作成完了");

        //参加者全員がPlayerのリストを作りおわるまで（上の処理が終わるまで）待機する
        //WatiUntilは条件を満たすまで待機（Trueになるまで）
        //CheckPlayerInGame()で取得できるReadyのフラグはネットワークで共有化されている情報
        //よって参加者全員からリストを作り終わるまで次の処理に行かない

        //PhotonNetwork.PlayerListは配列だからLengthで対応する
        yield return new WaitUntil(() => PhotonNetwork.PlayerList.Length == CheckPlayerInGame());



        //Startで反応しない場合は処理中に書くとよい

        rollExplanation.RollExplanationSetUp(rollTypeList);
        chatSystem.OnClickPlayerID();
        timeController.Init(isOffline);
        chatListManager.PlayerListSetUp(numLimit, chatSystem.myPlayer.wolfChat, chatSystem.myPlayer.live);
        voteCount.VoteCountListSetUp(numLimit);
        liveNum = PhotonNetwork.PlayerList.Length;
        if (PhotonNetwork.IsMasterClient) {
            SetLiveNum();
        }

        //自分が狼チャットが使えるなら
        if (chatSystem.myPlayer.wolfChat) {
            fillter.wolfModeButtonText.text = "狼";
            fillter.wolfModeButton.interactable = false;
            fillter.wolfMode = true;
        }
        //Debug.Log("参加者全員がPlayerList　準備OK");
    }

    /// <summary>
    /// PlayerObjの作成
    /// </summary>
    private void CreatePlayerObj() {

        //ネットワークオブジェクトとして生成（相手の世界にも自分のプレイヤーが作られる）
        GameObject playerObj = PhotonNetwork.Instantiate("Prefab/Game/Player", menbarContent.position, menbarContent.rotation);
        Player player = playerObj.GetComponent<Player>();
        player.playerID = PhotonNetwork.LocalPlayer.ActorNumber;

        //取得した自分の番号と同じ番号を探して役職を設定する
        foreach (Photon.Realtime.Player playerData in PhotonNetwork.PlayerList) {

            if (player.playerID == playerData.ActorNumber) {
                player.rollType = (ROLLTYPE)playerData.CustomProperties["roll"];
                break;
            }



            //自分だけにGameManager.csを入れる。
            //自分のプレイヤークラスを使う時用。
            chatSystem.myPlayer = player;
            Debug.Log("Player" + chatSystem.myPlayer);
            player.PlayerSetUp(this,voteCount);
        }
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
            player.PlayerSetUp(this,voteCount);
            //各リストに登録
            chatSystem.playersList.Add(player);
            chatSystem.playerNameList.Add(player.playerName);
            playerObj.transform.SetParent(menbarContent);
            yield return null;
        }

        //PlayerListを照準に並び替える（chatSystem.playersListの各要素をaに入れる foreachのようなもの
        //＝＞　a.playerIDでは何を基準に並び替えているのかを決めている
        chatSystem.playersList.OrderByDescending(player => player.playerID);

        //人数分のカミングアウトを用意(要素数を要しただけで初期化はしてないため、中身は””ではなくnull
        chatSystem.comingOutPlayers = new string[playerObjs.Length];

        //自分のステートを準備完了に変更しカスタムプロパティを更新(個人のフラグではなく、ネットワークで管理できるフラグにする
        var properties = new ExitGames.Client.Photon.Hashtable();
        properties.Add("player-state", "ready");
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        //Debug.Log("SetPlayerData:Complete");
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
        //Debug.Log(retReadyPlayerCount);
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
            //Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayers"]);
            RoomData.instance.settingNum = (int)PhotonNetwork.CurrentRoom.MaxPlayers;
            string roll = (string)PhotonNetwork.CurrentRoom.CustomProperties["numListStr"];
            int[] intArray = roll.Split(',').Select(int.Parse).ToArray();
            RoomData.instance.numList = intArray.ToList();

        }

        if (DebugManager.instance.isDebug) {
            //Debug.Log((int)PhotonNetwork.CurrentRoom.CustomProperties["testMainTime"]);
            RoomData.instance.roomInfo.mainTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["testMainTime"];
            RoomData.instance.roomInfo.nightTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["testNightTime"];
            //Debug.Log("isDebugOn");
        }
    }
}

