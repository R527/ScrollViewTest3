using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    public string myBanListStr;//自分のBanList
    public BanPlayer banPlayerPrefab;
    public Transform banListTran;

    private bool isCheckEnteredRoom;//入室チェック
    public Transform menbarContent;
    public Transform playerListContent;
    private bool isSetRoll;
    public List<PlayerButton> playerButtonList = new List<PlayerButton>();

    public Button firstDayButton;
    public Button prevDayButton;
    public Button nextDayButton;
    public Button lastDayButton;

    //タイトル上部にある情報をのテキスト
    public Text titleRollText;
    public Text currencyText;

    //課金用のPopUp
    public CurrencyTextPopUp currencyTextPopUpObj;
    public int superChatCurrency;
    public int extitCurrency;
    public bool showPopUp;

    //ボタン
    public Button exitButton;
    public Button rollBtn;//左上のボタン

    //その他
    [Header("役職リスト")]
    public List<ROLLTYPE> rollTypeList = new List<ROLLTYPE>();//設定されている役職を追加
    public List<ROLLTYPE> ComingOutButtonList = new List<ROLLTYPE>();
    public GameObject BegginerGuideObj;


    public void GameManagerSetUp() {
        //BGM開始
        StartCoroutine(AudioManager.instance.PlayBGM(AudioManager.BGM_TYPE.GAME));

        //部屋を作った人は初めての人なのでこの処理はない
        //二人目以降の人が値を取得する
        //num = GetNum();
        num = NetworkManager.instance.GetCustomPropertesOfRoom<int>("num");
        if (DebugManager.instance.isTimeController && PhotonNetwork.IsMasterClient) {
            num = DebugManager.instance.num;
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
        };

        //カスタムプロパティに更新した変数をセットする
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);

        SetRoomData();

        //自分のBanListを一つのstringにする
        myBanListStr = GetStringBanList();

        //PhotonのPlayerクラスに新しい情報を追加
        PhotonNetwork.LocalPlayer.NickName = PlayerManager.instance.playerName;
        ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
            {"myBanListStr", myBanListStr}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customPlayerProperties);

        //各プレイヤーから受け取ったmyBanListStrをつなげて一つのstringにする
        string banListStr = GetStringMasterBanList();
        var customRoomBanListProperties = new ExitGames.Client.Photon.Hashtable {
            {"banListStr",banListStr }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomBanListProperties);

        //BanListの作成
        CreateBanList();

        //内部通貨を更新する
        UpdateCurrencyText();

        //部屋が満室なら部屋を閉じる、そうでなければ開放する
        if (PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers) {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            if(PlayerPrefs.GetString(PlayerManager.ID_TYPE.begginer.ToString()) != "初心者ガイドをみました") {
                Instantiate(BegginerGuideObj);
            }
        }
        isCheckEnteredRoom = true;

        //シーン遷移関連
        Scene gameScene = SceneManager.GetSceneByName(SCENE_TYPE.GAME.ToString());
        SceneManager.SetActiveScene(gameScene);
    }

    
    /// <summary>
    /// 入室と退室を管理
    /// 人数設定と入室者が同じ数になるとゲーム参加再確認を行う
    /// </summary>
    void Update() {

        //入室チェック中は実行しない
        if (!isCheckEnteredRoom) {
            return;
        }
        //ルームにいない場合処理しない
        if (!PhotonNetwork.InRoom) {
            return;
        }
        //ゲーム開始したら処理しない
        if(gameStart) {
            return;
        }

        if (!gameStart) {
            gameStart = NetworkManager.instance.GetCustomPropertesOfRoom<bool>("gameStart");

            if(gameStart) {
                PraparateGameStart();
                gameMasterChatManager.timeSavingButton.interactable = false;

            }
        }

        //人数を確認する
        CountNum();

        //以下はマスターのみの処理
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }

        //人数が揃ったら役職セット
        if (!isSetRoll && NetworkManager.instance.GetCustomPropertesOfRoom<int>("num") == numLimit) {
            isSetRoll = true;
            SetRoll();
            gameMasterChatManager.timeSavingButton.interactable = false;
        }
    }

    /////////////////////////////////
    ///メソッド関連
    /////////////////////////////////

   
    /// <summary>
    /// テスト、通常時両方を含めたメソッド
    /// 役職をセットします。
    /// </summary>
    public void SetRoll() {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        StartCoroutine(SetRandomRoll());
    }

    /// <summary>
    /// 部屋の参加人数を確認します。
    /// </summary>
    public void CountNum() {
        //1秒ごとに部屋参加人数を確認する
        checkTimer += Time.deltaTime;

        if (checkTimer >= 1 && NetworkManager.instance.GetCustomPropertesOfRoom<int>("num") <= numLimit) {
            checkTimer = 0;
            num = NetworkManager.instance.GetCustomPropertesOfRoom<int>("num");
            NumText.text = num + "/" + numLimit;
        }
    }

    /// <summary>
    /// ゲームがスタートするとそれに合わせて必要なボタンをONにする
    /// </summary>
    private void SetUpButton() {
        firstDayButton.interactable = true;
        prevDayButton.interactable = true;
        nextDayButton.interactable = true;
        lastDayButton.interactable = true;
        inputView.filterButton.interactable = true;
    }

    /// <summary>
    /// ルームデータで決められた役職をセットし、それに合わせてボタンのセットとランダムに配布します。
    /// </summary>
    /// <returns></returns>
    private IEnumerator SettingRondomRollType() {
        List<ROLLTYPE> randomRollTypeList = new List<ROLLTYPE>();
        int testIndex = 0;

        //ルームデータから取得する
        if (!DebugManager.instance.isTestPlay) {

            //debugモードでなければ、ルームデータから役職数を取得して、ランダムに役職リストを登録
            for (int x = 0; x < RoomData.instance.rollList.Count; x++) {
                if (RoomData.instance.rollList[x] > 0) {
                    for (int i = 0; i < RoomData.instance.rollList[x]; i++) {
                        randomRollTypeList.Add((ROLLTYPE)x);
                    }
                }
            }
        } else {
            foreach (ROLLTYPE rollObj in DebugManager.instance.testRollTypeList) {
                randomRollTypeList.Add(rollObj);
            }
        }

        //参加人数分だけランダムに
        //PhotonNetwork.PlayerList　=　今部屋に参加しているプレイヤーのリスト9人ぶん
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable();

            if (!DebugManager.instance.isTestPlay) {
                int randomValue = Random.Range(0, randomRollTypeList.Count);
                customPlayerProperties.Add("roll", randomRollTypeList[randomValue]);
                
                randomRollTypeList.Remove(randomRollTypeList[randomValue]);
            } else {
                customPlayerProperties.Add("roll", randomRollTypeList[testIndex]);
                testIndex++;
            }
            //プレイヤーのカスタムプロパティに役職を登録しておく
            player.SetCustomProperties(customPlayerProperties);
        }

        //この待つ処理は問題なし（確認済み）
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// 役職をランダムに配布
    /// </summary>
    private IEnumerator SetRandomRoll() {
        //マスターはゲームに使用する役職を用意する
        //yield return StartCoroutineはそのメソッドの処理が終わるまで次の処理へと進まない
        yield return StartCoroutine(SettingRondomRollType());

        gameStart = true;
        NetworkManager.instance.SetCustomPropertesOfRoom<bool>("gameStart", gameStart);
        PraparateGameStart();
    }

    private void PraparateGameStart() {
        StartCoroutine(StartGame());
        num = NetworkManager.instance.GetCustomPropertesOfRoom<int>("num");
        NumText.text = num + "/" + numLimit;
    }

    /// <summary>
    /// Player作成　ゲームスタート
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartGame() {
        //各自が自分の分のプレイヤーを作る
        StartCoroutine(SetUpMyRollType());

        //自分が参加者全員のプレイヤーをもらってリストにする
        yield return StartCoroutine(SetPlayerData());

        //PhotonNetwork.PlayerListは配列だからLengthで対応する
        //参加者全員がPlayerのリストを作りおわるまで（上の処理が終わるまで）待機する
        //WatiUntilは条件を満たすまで待機（Trueになるまで）
        //CheckPlayerInGame()で取得できるReadyのフラグはネットワークで共有化されている情報
        //よって参加者全員からリストを作り終わるまで次の処理に行かない
        yield return new WaitUntil(() => PhotonNetwork.PlayerList.Length == CheckPlayerInGame());

        //PlayerButtonが生成されるのを待つ
        yield return StartCoroutine(CeackPlayerButton());

        //取得したPlayerButtonに役職をセットしてListに追加
        yield return StartCoroutine(SetPlayerButtonList());



        //ボタンの設定を変更する
        SetUpButton();

        //Startで反応しない場合は処理中に書くとよい
        comingOut.ComingOutSetUp(ComingOutButtonList);
        StartCoroutine(timeController.Init());
        chatListManager.PlayerListSetUp(chatSystem.myPlayer.wolfChat);
        rollBtn.interactable = true;
        liveNum = PhotonNetwork.PlayerList.Length;

        if (PhotonNetwork.IsMasterClient) {
            NetworkManager.instance.SetCustomPropertesOfRoom<int>("liveNum", liveNum);
            //SetLiveNum();
        }

        //自分が狼チャットが使えるなら
        if (chatSystem.myPlayer.wolfChat) {

            inputView.wolfBtnImage.color = inputView.btnColor[2];
            inputView.wolfModeButtonText.text = "狼";
            inputView.wolfModeButton.interactable = false;
            inputView.wolfMode = true;
        }
        

        PlayerManager.instance.roomName = NetworkManager.instance.roomName;
        PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.roomName, PlayerManager.ID_TYPE.roomName);

        //突然死用のフラグを保存する
        PlayerManager.instance.SetStringSuddenDeathTypeForPlayerPrefs(PlayerManager.SuddenDeath_TYPE.ゲーム開始);
    }

    /// <summary>
    /// PlayerObjの作成
    /// </summary>
    private IEnumerator SetUpMyRollType() {

        //Debug用　一人でDebugしたときに処理する
        if(DebugManager.instance.numLimit == 1) {
            yield return StartCoroutine(WaitCreatePlayer());
        }

        //取得した自分の番号と同じ番号を探して役職を設定する
        foreach (Photon.Realtime.Player playerData in PhotonNetwork.PlayerList) {
            if (chatSystem.myPlayer.playerID == playerData.ActorNumber) {
                chatSystem.myPlayer.rollType = (ROLLTYPE)playerData.CustomProperties["roll"];
                titleRollText.text = chatSystem.myPlayer.rollType.ToString();
                break;
            }
        }
        chatSystem.myPlayer.PlayerSetUp(this,voteCount,timeController);
    }
    /// <summary>
    /// Debug用　一人でdebugするときに処理する
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitCreatePlayer() {
        while (chatSystem.myPlayer == null) {
            yield return null;
        }
    }
    /// <summary>
    /// 各プレイヤーのプレイヤークラスをもらってリストにする
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetPlayerData() {
        GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");

        //whileの条件式を満たしている間は、繰り返す。
        //参加している人数と探してきた人数が一致しているかどうか
        //条件式をクリアするまで繰り返され、抜けない
        while (playerObjs.Length != PhotonNetwork.PlayerList.Length) {
            playerObjs = GameObject.FindGameObjectsWithTag("Player");
            yield return null;
        }

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
            RoomData.instance.roomInfo.roomSelection = (ROOMSELECTION)PhotonNetwork.CurrentRoom.CustomProperties["roomSelect"];
            string roll = (string)PhotonNetwork.CurrentRoom.CustomProperties["numListStr"];
            int[] intArray = roll.Split(',').Select(int.Parse).ToArray();
            RoomData.instance.rollList = intArray.ToList();
        }
        //全員の世界にComingOuottonと役職説明用のボタンを追加する
        for(int i = 0; i < RoomData.instance.rollList.Count; i++) {
            ComingOutButtonList.Add((ROLLTYPE)i);
            //役職説明のボタンを追加している
            rollTypeList.Add((ROLLTYPE)i);
        }

        rollExplanation.RollExplanationSetUp(rollTypeList);
        rollTypeList.Clear();

        if (DebugManager.instance.isTimeController) {
            RoomData.instance.roomInfo.mainTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["testMainTime"];
            RoomData.instance.roomInfo.nightTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["testNightTime"];
        }
    }

    /// <summary>
    /// ボタンが作成されるまで待機する
    /// ボタンが生成されてからでないと役職をButtonに登録できないから
    /// </summary>
    /// <returns></returns>
    private IEnumerator CeackPlayerButton() {
        bool isCheck = false;
        while (!isCheck) {
            GameObject[] objs = GameObject.FindGameObjectsWithTag("PlayerButton");
            if (objs.Length == PhotonNetwork.PlayerList.Length) {
                isCheck = true;
            } else {
                yield return null;
            }
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

    /// <summary>
    /// マスタークライアント以外がする処理
    /// 自分のBanListを一つの文字列にする
    /// </summary>
    /// <returns></returns>
    public string GetStringBanList() {
        string banListStr = "";
        foreach (string str in PlayerManager.instance.banUniqueIDList) {
            if (str == null) {
                continue;
            }
            banListStr += str + ",";
        }
        if (banListStr != "") {
            return banListStr;
        } else {
            return "";
        }
    }

    /// <summary>
    /// マスタークライアントが管理する処理
    /// 参加プレイヤーのBanListを一つの文字列にする
    /// </summary>
    /// <returns></returns>
    public string GetStringMasterBanList() {
        string banListStr = "";
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            banListStr += (string)player.CustomProperties["myBanListStr"];
        }
        if (banListStr != "") {
            //最後のカンマを取り除く
            return banListStr.Substring(0, banListStr.Length - 1);
        } else {
            return "";
        }
    }

    /// <summary>
    /// 起動時にBanListを作成する
    /// </summary>
    public void CreateBanList() {

        ////BanListがないなら実行しない
        if (PlayerManager.instance.banListIndex <= 0) {
            return;
        }


        //BanList作成
        for (int i = 0; i < PlayerManager.instance.banUniqueIDList.Count; i++) {
            BanPlayer banPlayer = Instantiate(banPlayerPrefab, banListTran, false);
            banPlayer.SetUp(PlayerManager.instance.banUniqueIDList[i], PlayerManager.instance.banUserNickNameList[i]);
        }
    }

    //=======================
    //課金
    //=======================
    /// <summary>
    /// 内部通貨を更新します。
    /// </summary>
    public void UpdateCurrencyText() {
        //課金額表示
        currencyText.text = PlayerManager.instance.currency.ToString();
    }

    /// <summary>
    /// ゲーム内通貨の利用に関するテキストを表示する
    /// </summary>
    public void InstantiateCurrencyTextPopUP(string warning) {
        if (warning == "superChatStr" && !PlayerPrefs.HasKey(PlayerManager.ID_TYPE.superChat.ToString())) {
            InstantiatePopUp(warning);
        } else if(warning == "exitStr" && !PlayerPrefs.HasKey(PlayerManager.ID_TYPE.exit.ToString())) {
            InstantiatePopUp(warning);
        } else {
            gameMasterChatManager.gameManager.showPopUp = true;
        }
    }

    void InstantiatePopUp(string warning) {
        GameObject gameCanvas = GameObject.FindGameObjectWithTag("GameCanvas");
        CurrencyTextPopUp obj = Instantiate(currencyTextPopUpObj, gameCanvas.transform, false);
        obj.warningStr = warning;
    }

    ///////////////////////////////
    /////カスタムプロパティ関連
    ///////////////////////////////


    ///// <summary>
    ///// 入場者数をチェックする
    ///// </summary>
    //public int GetNum() {
    //    //ルームに保存されているnumという情報があったら、それをキャストして変数に入れる
    //    //numというKeyがセットされていて、numがあった場合
    //    //outとしてobject型のnumObjが作られる。
    //    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("num", out object numObj)) {
    //        //numObjがobject型なのでintにキャスト
    //        num = (int)numObj;
    //    }
    //    return num;
    //}
    ///// <summary>
    ///// 生存人数をセットします
    ///// </summary>
    //public void SetLiveNum() {
    //    ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
    //        {"liveNum", liveNum}
    //    };
    //    PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    //}

    ///// <summary>
    ///// 生存人数をもらいます
    ///// </summary>
    ///// <returns></returns>
    //public int GetLiveNum() {
    //    int value = 0;
    //    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("liveNum", out object liveNumObj)) {
    //        value = (int)liveNumObj;
    //    }
    //    return value;
    //}

    //private bool GetGameStart() {
    //    if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameStart", out object gameStartObj)) {
    //        gameStart = (bool)gameStartObj;
    //    }
    //    return gameStart;
    //}
}

//変数

//public int enterNum;//参加希望人数
//public float enterNumTime;//30秒タイマーの残り時間（ゲーム参加再確認の時間

//public GameObject confirmationImage;//確認Image
//public Text confirmationTimeText;//30秒タイマー
//public Text confirmationNumText;//参加人数
//public Text confirmationEnterButtonText;//参加ボタン
//public bool confirmation;//確認画面開始の判定　trueなら開始されている
//public bool isConfirmation;//確認画面中ならtrue
//private bool isJoined = false;//参加不参加の確認用
//public bool isExit;//確認画面で退出時に使われる


////一人以上のプレイヤーが退出した場合isJoinedisExitの値をリセットして確認PopUPを削除する
//if (GetIsExit()) {
//    confirmationImage.SetActive(false);
//    PhotonNetwork.CurrentRoom.IsOpen = true;
//    ResetButton();
//    confirmation = false;
//    isJoined = false;
//    isExit = false;
//    enterNumTime = 25.0f;
//    SetEnterNumTime();
//    SetIsJoin();
//    SetIsExit();
//}



//Update



////確認PopUP表示して参加確認を行う
//if (GetNum() == numLimit && GetEnterNum() != numLimit) {
//    isConfirmation = true;
//    //参加意思表示の人数確認
//    CountDownEnterNumTime();
//    //参加意思表示の確認の時間切れ
//    TimeUpEnterNumTime();
//    //左上の参加人数記載
//    NumText.text = GetNum() + "/" + numLimit;
//    confirmationNumText.text = enterNum + "/" + numLimit;
//    confirmationTimeText.text = (int)enterNumTime + "秒";
//}

////参加意思表示確認画面の監視
//if (timeController.timeType == TIME.開始前 && numLimit == GetNum()) {
//    CheckEnterNum();
//}

//if(GetEnterNum() != numLimit) {
//    return;
//}

///// <summary>
///// 参加人数をリセットする
///// </summary>
//private void ResetJoin() {

//    num = PhotonNetwork.PlayerList.Length;
//    SetNum();

//    //enterNumもりセット
//    enterNum = 0;
//    //トータルの参加人数を更新して、カスタムプロパティに保存する
//    SetEnterNum();
//}



//ボタン



///// <summary>
///// 参加確認Imageにて参加orキャンセルする
///// </summary>
//public void EnterButton() {
//    //時間が0になったらボタンの無効化
//    if (GetEnterNumTime() <= 0) {
//        return;
//    }
//    //ネットワーク上に保存されているキーがあるかを確認

//    switch (confirmationEnterButtonText.text) {
//        case "参加":
//            //参加するなら
//            isJoined = true;
//            confirmationEnterButtonText.text = "キャンセル";
//            break;
//        case "キャンセル":
//            //不参加なら
//            isJoined = false;
//            confirmationEnterButtonText.text = "参加";
//            break;
//    }
//    //isJoinedを各プレイヤーごとに更新（LocalPlayer
//    SetIsJoin();
//}

///// <summary>
///// 参加確認Imageにて退出する
///// </summary>
//public void ExitButton() {
//    //時間が0になったら時間の無効化
//    if (GetEnterNumTime() <= 0) {
//        return;
//    }

//    num--;
//    enterNum = 0;

//    SetNum();
//    SetEnterNum();

//    //誰かが退出したことを知らせるためのbool型
//    isExit = true;
//    SetIsExit();
//    PhotonNetwork.CurrentRoom.IsOpen = true;
//    NetworkManager.instance.LeaveRoom();

//}


//メソッド


///// <summary>
///// 参加意思表示を確認する時間をカウントダウンします。
///// </summary>
//public void CountDownEnterNumTime() {
//    //参加確認中の挙動を制御
//    if (!confirmation) {
//        confirmation = true;
//        savingText.text = "時短";
//        damyObj.SetActive(true);
//        confirmationImage.SetActive(true);

//        firstDayButton.interactable = false;
//        prevDayButton.interactable = false;
//        nextDayButton.interactable = false;
//        lastDayButton.interactable = false;
//        inputView.filterButton.interactable = false;
//        inputView.foldingButton.interactable = false;
//        chatSystem.chatInputField.interactable = false;
//        inputView.superChatButton.interactable = false;
//    }
//    if (gameMasterChatManager.timeSavingButton.interactable) {
//        gameMasterChatManager.timeSavingButton.interactable = false;
//    }

//    //マスターだけ時間を書き込む
//    if (PhotonNetwork.IsMasterClient) {

//        //人数が揃ったらenterNumTimeをセットする
//        if (!isNumComplete) {
//            isNumComplete = true;
//            enterNumTime = 25.0f;
//            SetEnterNumTime();
//        }

//        //カウントダウン
//        checkEnterTimer += Time.deltaTime;
//        if (checkEnterTimer >= 1) {
//            checkEnterTimer = 0;
//            enterNumTime--;
//            SetEnterNumTime();
//        }

//        //マスター以外の処理
//    } else {
//        enterNumTime = GetEnterNumTime();
//    }
//}

///// <summary>
///// 参加意思表示の確認時間が切れたら行われる処理
///// </summary>
//public void TimeUpEnterNumTime() {
//    //参加確認の時間が切れたら ゲームマスターのみ
//    if (GetEnterNumTime() < 1 && PhotonNetwork.IsMasterClient && !GetIsTimeUp()) {
//        isTimeUp = true;
//        SetIsTimeUp();
//    }

//    //時間切れになったらマスターから時間をもらう
//    if (GetIsTimeUp()) {
//        //参加意思表示ないプレイヤーのみ強制退出
//        if (!(bool)PhotonNetwork.LocalPlayer.CustomProperties["isJoined"]) {

//            NetworkManager.instance.LeaveRoom();
//        } else {
//            //意思表示があるプレイヤーは一度isJoinedをリセット
//            isJoined = false;
//            ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
//                {"isJoined", isJoined }
//            };
//            PhotonNetwork.LocalPlayer.SetCustomProperties(customPlayerProperties);
//        }

//        damyObj.SetActive(false);
//        confirmationImage.SetActive(false);
//        confirmation = false;
//        timeController.savingButton.interactable = true;
//        isConfirmation = false;
//        //マスターに管理(リセット
//        if (PhotonNetwork.IsMasterClient) {
//            //部屋開放する
//            PhotonNetwork.CurrentRoom.IsOpen = true;
//            //リセット
//            enterNumTime = setEnterNumTime;
//            ResetButton();
//            isTimeUp = false;
//            SetIsTimeUp();
//            SetEnterNumTime();
//            ResetJoin();
//        }
//    }

//}

///// <summary>
///// 確認画面にてプレイヤーが退出したときにボタン情報をリセットする
///// </summary>
//private void ResetButton() {
//    gameMasterChatManager.timeSavingButtonText.text = "退出";
//    gameMasterChatManager.timeSavingButton.interactable = true;
//    inputView.filterButton.interactable = true;
//    inputView.foldingButton.interactable = true;
//    chatSystem.chatInputField.interactable = true;
//    inputView.superChatButton.interactable = true;
//}

///// <summary>
///// マスターだけが参加意思表示あるプレイヤーを監視します。
///// 参加意思があるならenterNumを加算します。
///// </summary>
//public void CheckEnterNum() {
//    checkTimer += Time.deltaTime;
//    if (checkTimer >= 1) {
//        checkTimer = 0;
//        int num = 0;
//        enterNum = 0;
//        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
//            if(player.CustomProperties.TryGetValue("isJoined", out object isJoinedObj)) {
//                if ((bool)player.CustomProperties["isJoined"]) {
//                    num++;
//                }
//            }
//        }
//        enterNum = num;
//        SetEnterNum();
//    }
//}


//カスタムプロパティ



///// <summary>
///// ゲーム参加確認画面の人数をもらう
///// </summary>
//public int GetEnterNum() {
//    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("enterNum", out object enterNumObj)) {
//        enterNum = (int)enterNumObj;
//    }
//    return enterNum;
//}

///// <summary>
///// ゲーム参加確認画面の人数をセットする
///// </summary>
//public  void SetEnterNum() {
//    var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
//        {"enterNum", enterNum }
//    };
//    PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
//}

///// <summary>
///// ゲーム参加確認画面の人数チェック中の時間管理
///// </summary>
//private float GetEnterNumTime() {
//    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("enterNumTime", out object enterNumTimeObj)) {
//        enterNumTime = (float)enterNumTimeObj;
//    }
//    return enterNumTime;
//}

///// <summary>
///// EnterNumTimeをセットします。
///// </summary>
//private void SetEnterNumTime() {
//    ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
//        {"enterNumTime", enterNumTime }
//      };
//    PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
//}


///// <summary>
///// 確認画面にて誰が退出した場合に使われる
///// </summary>
//private bool GetIsExit() {
//    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("isExit", out object isExitObj)) {
//        isExit = (bool)isExitObj;
//    }
//    return isExit;
//}
///// <summary>
///// 確認画面にて誰が退出した場合に使われる
///// </summary>
//private void SetIsExit() {
//    ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
//                {"isExit", isExit }
//            };
//    PhotonNetwork.CurrentRoom.SetCustomProperties(customPlayerProperties);
//}

///// <summary>
///// 参加者の意思表示のbool型をセットする
///// </summary>
//private void SetIsJoin() {
//    ExitGames.Client.Photon.Hashtable customPlayerProperties = new ExitGames.Client.Photon.Hashtable {
//                {"isJoined", isJoined }
//            };
//    PhotonNetwork.LocalPlayer.SetCustomProperties(customPlayerProperties);
//}