using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;
using System;


/// <summary>
/// タイムコントロール　夜、昼、投票を決定　時短処理など
/// </summary>
public class TimeController : MonoBehaviourPunCallbacks {


    //class
    public GameManager gameManager;
    public ChatSystem chatSystem;
    public VoteCount voteCount;
    public GameOver gameOver;
    public GameMasterChatManager gameMasterChatManager;
    public InputView inputView;
    public Fillter fillter;
    public TIME timeType;
    public DayOrderButton dayOrderButton;

    //main
    public Text timerText;//メインタイマーテキスト
    public float totalTime;
    public float mainTime;　　　//昼の時間
    public float votingTime;   //投票時間
    public float nightTime;   //夜の時間
    public float executionTime;//処刑時間
    public float checkGameOverTime;//ゲームオーバー確認時間
    public float resultTime;
    public float intervalTime;
    public bool isDisplay;//時間を表示するか否か　trueなら表示
    public bool isPlaying;　　//gameが動いているかの判定
    public bool gameReady;//ゲーム待機状態か否か
    public bool isSpeaking;//喋ったか否かtrueならしゃべった
    public bool setSuddenDeath;
    public bool isPlay;//falseならゲームオーバー
    private float chekTimer;//1秒ごとに時間を管理する
    public bool isVotingCompleted;
    public bool isCeackFlag;//PlayStateのフラグ確認
    public bool isCeackInterval;
    //ボタン・Input関連
    public Button savingButton;//時短ボタン
    
    public Button COButton;
    public Button superChatButton;
    public Button wolfButton;
    public InputField inputField;//文字入力部分

    //PoPUp
    public TimeContollerPopUp timeControllerPopUpPrefab;//一定の時間を迎えたときに案内として出す
    public TimeContollerPopUp timeContollerPopUpObj;
    public Transform mainCanvasTran;

    //課金関連のPopUp
    public GameObject currencyPopUP;//課金が必要な場面で課金についての説明文を入れる
    public bool exitCurrency;//退出用の課金PopUPを出すか否か

    //x日　GMのチャット追加
    public GameObject chatContent;
    public int day;
    public NextDay dayPrefab;
    public bool firstDay;

    public enum PlayState {
        Play,
        Stop,
        Interval,
        Buffer
    }
    public PlayState playState;


    ////////////////////////
    ///SetUp関連
    ////////////////////////

    /// <summary>
    /// 各ボタンの制御,
    /// </summary>
    public IEnumerator Init() {
        firstDay = true;
        isPlay = true;
        timeType = TIME.処刑後チェック;
        playState = PlayState.Play;
        //savingButton.interactable = true;


        //Debug用
        if (DebugManager.instance.isTimeController) {
            votingTime = DebugManager.instance.testVotingTime;
            executionTime = DebugManager.instance.testExecutionTime;
            checkGameOverTime = DebugManager.instance.testCheckGameOverTime;
            resultTime = DebugManager.instance.testResultTime;
            timeType = DebugManager.instance.timeType;
        }

        wolfButton.interactable = false;
        inputField.interactable = false;
        ////狼の場合
        //if (chatSystem.myPlayer.wolfChat) {
        //    wolfButton.interactable = true;
        //    inputField.interactable = true;
        //} else {
            
        //}

        superChatButton.interactable = false;
        COButton.interactable = false;
        mainTime = RoomData.instance.roomInfo.mainTime;
        nightTime = RoomData.instance.roomInfo.nightTime;
        

        //マスターだけトータルタイムとIsPlayingを取得する
        if (PhotonNetwork.IsMasterClient) {
            //isPlaying = true;
            //gameReady = true;
            //playState = PlayState.Play;
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                { "totalTime", totalTime },
                {"playState", PlayState.Stop.ToString() },
                {"gameReady",true },
                {"timeType",timeType }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);

        }

        // 本当の姿を表示する
        gameMasterChatManager.TrueCharacter();

        yield return new WaitForSeconds(2.0f);

        //playState = GetPlayState();

        //ゲームスタート
        gameReady = GetGameReady();
    }


    /// <summary>
    /// カウントダウンタイマー
    /// </summary>
    void Update() {

        //ゲーム終了したら実行しない
        if (!isPlay) {
            return;
        }

        //ゲーム待機中かどうかを確認する
        if (!gameReady) {
            return;
        }

        //ゲームがスタートしてないなら実行しない
        if (!gameManager.gameStart) {
            return;
        }

        //ゲーム開始したら以下のUpdateを通過する
        //ゲームが終了したらUpdateを止める
        if (timeType == TIME.終了) {
            timerText.text = string.Empty;
            gameManager.gameMasterChatManager.timeSavingButtonText.text = "退出";
            return;
        }




        //カウントダウン処理
        if (playState == PlayState.Play) {

            if (!isCeackFlag) {
                isCeackFlag = true;
                isCeackInterval = false;
                ChangeIsCheckPlayState_Interval(false);
            }

            //マスターだけトータルタイムを管理する
            if (PhotonNetwork.IsMasterClient) {
                chekTimer += Time.deltaTime;
                if (chekTimer >= 1) {
                    chekTimer = 0;
                    totalTime--;
                    SetGameTime();
                }
                //マスター以外はトータルタイムをもらう
            } else {
                totalTime = GetGameTime();
            }

            //トータルタイム表示
            //トータルタイムを受け取る側はー1秒から始まるのでその調整用
            if (totalTime >= 0 && isDisplay) {
                timerText.text = totalTime.ToString("F0");
            }

            //0秒になったら次のシーンへ移行する
            //時短が成立しても実行される


            //マスターの処理
            //トータルタイムが0秒になったらPlayStateをStopにする
            //Stopを受け取りPlayStateをインターバルにした後　←問題
            //スタートインターバル、エンドインターバル開始
            //PlayStateをPlayに戻す

            //マスターだけの処理
            if ((totalTime < 0 || gameManager.gameMasterChatManager.GetIsTimeSaving()) && PhotonNetwork.IsMasterClient) {
                Debug.Log("SetPlayState");
                SetPlayState(PlayState.Interval);

                gameManager.gameMasterChatManager.isTimeSaving = false;
                gameManager.gameMasterChatManager.SetIsTimeSaving();
            }

            //マスター以外の処理
            if (totalTime < 0 || gameManager.gameMasterChatManager.GetIsTimeSaving()) {
                Debug.Log("isCeackPlayState True");
                ChangeIsCheckPlayState_Interval(true);

                if (PhotonNetwork.PlayerList.Length != IsCheckPlayState_Interval()) {
                    return;
                }

                GetPlayState();
            }


            //次のシーンへ移行する処理
        //} else if (playState == PlayState.Stop) {
        //    Debug.Log("SetPlayState2");
        //    timerText.text = string.Empty;


        //    if (PhotonNetwork.IsMasterClient && !isCeackStop) {
        //        isCeackStop = true;
        //        SetPlayState(PlayState.Interval);
        //    }

        //    //ChangeIsCheckPlayState(true);

        //    //全員がPlayStateをもらえる状態まで待つ
        //    if (PhotonNetwork.PlayerList.Length != IsCheckPlayState()) {
        //        return;
        //    }

        //    GetPlayState();

        } else if (playState == PlayState.Interval && !isCeackInterval) {
            isCeackInterval = true;
            isCeackFlag = false;
            ChangeIsCheckPlayState_Play(false);
            StartInterval();
            Debug.Log(timeType);
        }

        //全員の投票が完了したら
        if (!GetIsVotingCompleted() || !PhotonNetwork.IsMasterClient) {
            return;
        }else if (GetIsVotingCompleted() && PhotonNetwork.IsMasterClient) {
            isVotingCompleted = false;
            SetIsVotingCompleted();
            totalTime = 0;
            SetGameTime();
        }
    }


    ///////////////////////////
    ///メソッド関連
    ///////////////////////////

    /// <summary>
    /// インターバルを決定する
    /// </summary>
    public void StartInterval() {
        Debug.Log("StartInterval");

        //if (PhotonNetwork.IsMasterClient) {
        //    isCeackStop = false;
        //}

        switch (timeType) {
            //お昼
            case TIME.結果発表後チェック:
                timeType = TIME.昼;
                isDisplay = true;
                totalTime = mainTime;

                ChangeSecene();

                //死亡している場合時短or退出ボタンを退出にする
                if (chatSystem.myPlayer.live == false) {
                    gameManager.gameMasterChatManager.timeSavingButtonText.text = "退出";
                    gameManager.gameMasterChatManager.timeSavingButton.interactable = true;
                } else {
                    Debug.Log("時短");
                    gameManager.gameMasterChatManager.timeSavingButtonText.text = "時短";
                }


                //初期化
                chatSystem.coTimeLimit = 0;
                    

                //GMチャットなど
                if (!firstDay) {
                    StartCoroutine(NextDay());
                }
                StartCoroutine(GameMasterChat());
                StartCoroutine(DownInputView());
                break;

            //投票時間
            case TIME.昼:
                timeType = TIME.投票時間;

                isDisplay = true;
                totalTime = votingTime;

                ChangeSecene();
                //一日ごとに突然死チェックするのでリセット
                isSpeaking = false;

                //GMチャットなど
                TimesavingControllerFalse();
                StartCoroutine(GameMasterChat());
                StartCoroutine(UpInputView());
                break;

            //処刑
            case TIME.投票時間:
                timeType = TIME.処刑;
                isDisplay = false;
                totalTime = executionTime;

                //初期化
                isVotingCompleted = false;

                //処刑処理　DebugManagerでチェック入っていると走らない
                if (!DebugManager.instance.isVoteCount) {
                    voteCount.Execution();
                    gameManager.gameMasterChatManager.ExecutionChat();
                }

                //自分の世界で突然死したプレイヤーの生存情報をfalseにする
                if (!DebugManager.instance.isCheckSuddenDeath) {

                    GameObject[] objs = GameObject.FindGameObjectsWithTag("PlayerButton");

                    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {

                        //突然死していないプレイヤーは通過
                        if (GetSuddenDeath(player)) {
                            Debug.Log("player.ActorNumber"+ player.ActorNumber);
                            continue;
                        }
                        //Player生きていたら
                        //PlayerListのLiveをfalseにする

                        foreach(Player playerObject in chatSystem.playersList) {

                            //各プレイヤーを突然死させる
                            if (player.ActorNumber == playerObject.playerID && playerObject.live) {
                                playerObject.live = false;
                                gameManager.gameMasterChatManager.SuddenDeath(player);

                                //突然死するプレイヤーが自分の場合不参加のフラグを記録する
                                if(playerObject == chatSystem.myPlayer) {
                                    PlayerManager.instance.SetStringSuddenDeathTypeForPlayerPrefs(PlayerManager.SuddenDeath_TYPE.不参加);
                                }

                                //PlayerButtonの処理
                                foreach (GameObject obj in objs) {
                                    PlayerButton playerObj = obj.GetComponent<PlayerButton>();
                                    if (player.ActorNumber == playerObj.playerID) {
                                        playerObj.live = false;
                                        playerObj.playerInfoText.text += day + "日目突然死";
                                        break;
                                    }
                                }
                                break;
                            }
                        }

                    }

                }

                //死亡したプレイヤーの処理
                DeathPlayer();

                FalseInputViewButton();
                break;

            //処刑後チェック
            case TIME.処刑:
                timeType = TIME.処刑後チェック;
                chatSystem.myPlayer.isVoteFlag = false;
                isDisplay = false;
                totalTime = executionTime;

                //初期化
                if (PhotonNetwork.IsMasterClient) {
                    //生き残ったプレイヤーのVoteCountを０にする
                    foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                        var properties = new ExitGames.Client.Photon.Hashtable {
                        {"voteNum", 0 },
                        {"voteName", ""},
                        {"votingCompleted",false }
                    };
                        player.SetCustomProperties(properties);
                    }
                }
                //突然死の初期化
                setSuddenDeath = false;
                SetSuddenDeath();

                //生存者数を取得
                gameManager.liveNum = gameManager.GetLiveNum();
                if (!DebugManager.instance.isGameOver) {
                    gameOver.CheckGameOver();
                }

                break;

            //夜の行動
            case TIME.処刑後チェック:
                timeType = TIME.夜の行動;

                //狼ならチャット開放する
                if (chatSystem.myPlayer.wolfChat) {
                    inputField.interactable = true;
                    inputView.wolfMode = true;
                    inputView.wolfModeButtonText.text = "狼";
                    inputView.wolfModeButton.interactable = false;
                }

                isDisplay = true;
                totalTime = nightTime;
                ChangeSecene();


                //初期化
                voteCount.mostVotes = 0;
                voteCount.ExecutionPlayerList.Clear();
                    
                //GMチャットなど
                if (firstDay) {
                    StartCoroutine(NextDay());
                    StartCoroutine(UpInputView());
                }
                StartCoroutine(GameMasterChat());
                StartCoroutine(gameMasterChatManager.OpeningDayFortune());
                if (!firstDay) {
                    StartCoroutine(PsychicAction());
                }
                break;

            //夜の行動の結果発表
            case TIME.夜の行動:
                timeType = TIME.夜の結果発表;

                //狼チャットできる人だけチャットfalseにする
                if (chatSystem.myPlayer.wolfChat) {
                    inputField.interactable = false;
                }

                isDisplay = false;
                totalTime = resultTime;

                //死亡している場合時短or退出ボタンを退出にする
                if (chatSystem.myPlayer.live == false) {
                    gameManager.gameMasterChatManager.timeSavingButtonText.text = "退出";
                    gameManager.gameMasterChatManager.timeSavingButton.interactable = true;
                }

                //初期化
                chatSystem.myPlayer.isRollAction = false;

                //GMチャットなど
                if (!firstDay) {
                    gameMasterChatManager.MorningResults();
                } else {
                    firstDay = false;
                }
                break;

            //結果発表チェック
            case TIME.夜の結果発表:
                timeType = TIME.結果発表後チェック;
                isDisplay = false;
                totalTime = checkGameOverTime;

                //初期化噛みと守り先の初期化
                if (PhotonNetwork.IsMasterClient) {
                 
                    gameManager.gameMasterChatManager.bitedID = 9999;
                    gameManager.gameMasterChatManager.protectedID = 9999;
                    gameManager.gameMasterChatManager.SetProtectedPlayerID();
                    gameManager.gameMasterChatManager.SetBitedPlayerID();
                }

                if (!DebugManager.instance.isGameOver) {
                    gameOver.CheckGameOver();
                }
                DeathPlayer();
                break;
        }

        StartCoroutine(EndInterval(timeType));
    }

    /// <summary>
    /// GameMasterChatの制御
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameMasterChat() {
        //yield return new WaitForSeconds(0);
        yield return new WaitForSeconds(intervalTime + 0.2f);
        gameMasterChatManager.TimeManagementChat();
    }

    /// <summary>
    /// 翌日分のObjをInstatiateする
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextDay() {
        yield return new WaitForSeconds(intervalTime + 0.1f);
        day++;
        chatSystem.id++;
        
        NextDay dayObj = Instantiate(dayPrefab, chatContent.transform, false);

        dayObj.day = day;
        dayObj.nextDayText.text = day + "日目";
        dayOrderButton.nextDaysList.Add(dayObj.gameObject);
        ChatData chatData = new ChatData("", 7777, 7777, SPEAKER_TYPE.NULL.ToString(), ROLLTYPE.ETC);
        PlayerManager.instance.saveChatLog += PlayerManager.instance.ConvertStringToChatData(chatData) + "%";
    }


    /// <summary>
    /// 霊能者結果の表示に遅延を与える
    /// </summary>
    /// <returns></returns>
    private IEnumerator PsychicAction() {
        yield return new WaitForSeconds(intervalTime + 0.3f);
        gameMasterChatManager.PsychicAction();
    }

    /// <summary>
    ///ゲーム終了時でないときにPlayerが死亡した場合　課金して退出できるシステムを紹介する説明文を表示
    /// </summary>
    private void ExitCurrencyPopUp() {
        if(!gameManager.chatSystem.myPlayer.live && !gameOver.isGameOver && !exitCurrency) {
            exitCurrency = true;
            Instantiate(currencyPopUP, mainCanvasTran.transform, false);
        }
    }


    /// <summary>
    /// ゲームシーンが変更されると時間の案内表示が出る
    /// </summary>
    private void ChangeSecene() {
        timeContollerPopUpObj = Instantiate(timeControllerPopUpPrefab, mainCanvasTran, false);

        switch (timeType) {
            case TIME.昼:
                timeContollerPopUpObj.text.text = "お昼の時間です。";
                break;
            case TIME.投票時間:
                timeContollerPopUpObj.text.text = "投票の時間です。";
                break;
            case TIME.夜の行動:
                timeContollerPopUpObj.text.text = "夜の行動。"; 
                break;
        }
        timeContollerPopUpObj.image.color = new Color(255, 255, 255, 0);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(timeContollerPopUpObj.image.DOFade(1, 1.0f));
        sequence.AppendInterval(1.0f);
        sequence.Append(timeContollerPopUpObj.image.DOFade(0, 1.0f))
            .OnComplete(() => {
                Destroy(timeContollerPopUpObj.gameObject);
            });
    }
    /// <summary>
    /// 昼、夜、投票後にあるインターバル時間を設定
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndInterval(TIME nowTimeType) {

        //役職に合わせてボタンなどを変更する
        TimesavingControllerTrue();

        //次の時間へ移行する
        if (PhotonNetwork.IsMasterClient) {
            //isPlaying = true;
            //playState = PlayState.Play;
            //SetTimeType(nowTimeType);
            Debug.Log("PlayState-Play");
            SetPlayState(PlayState.Play);
        }

        ChangeIsCheckPlayState_Play(true);
        yield return new WaitUntil(() => PhotonNetwork.PlayerList.Length == IsCheckPlayState_Play());


        playState = GetPlayState();
        Debug.Log("playState" + playState);
        

        //while (playState == PlayState.Interval) {
        //    playState = GetPlayState();
        //    yield return null;
        //}
        //違うタイムタイプなら訂正する
        //if(timeType != GetTimeType()) {
        //    timeType = GetTimeType();
        //}
    }

    /// <summary>
    /// InPutViewを上げます
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpInputView() {
        //fillter.folding = true;
        inputView.foldingButton.interactable = false;
        yield return new WaitForSeconds(intervalTime + 0.3f);
        inputView.inputRectTransform.DOLocalMoveY(0, 0.5f);
        //inputView.viewport.DOLocalMoveY(72, 0.5f);
        inputView.viewport.DOSizeDelta(new Vector2(202f, 258f), 0.5f);
        inputView.menberViewPopUpObj.SetActive(true);
        //inputView.foldingText.text = "↓";
        inputView.folding = false;
        inputView.foldingImage.sprite = inputView.downBtnSprite;

    }

    /// <summary>
    /// InPutViewを下げます
    /// </summary>
    /// <returns></returns>
    public IEnumerator DownInputView() {
        //fillter.folding = false;
        inputView.foldingButton.interactable = true;
        yield return new WaitForSeconds(intervalTime + 0.3f);
        inputView.inputRectTransform.DOLocalMoveY(-70, 0.5f);
        inputView.viewport.DOSizeDelta(new Vector2(202f, 330f), 0.5f);
        StartCoroutine(inputView.PopUpFalse());
        inputView.folding = true;
        //inputView.foldingText.text = "↑";
        inputView.foldingImage.sprite = inputView.upBtnSprite;

    }

    /// <summary>
    /// ButtonなどをRollに応じてtrueにする
    /// </summary>
    public void TimesavingControllerTrue() {
        //昼にそれぞれのボタンをtrueにする
        if (timeType == TIME.昼) {
            savingButton.interactable = true;
            //狼プレイヤー
            if (chatSystem.myPlayer.wolfChat) {
                if (chatSystem.myPlayer.live) {
                    wolfButton.interactable = true;
                } else {
                    wolfButton.interactable = false;
                }
                //狼以外のプレイヤー
            }else {
                wolfButton.interactable = false;
            }
            //生存者
            if (chatSystem.myPlayer.live) {
                COButton.interactable = true;
                superChatButton.interactable = true;
                //死亡者
            } else {
                COButton.interactable = false;
                superChatButton.interactable = false;
            }
            
            inputField.interactable = true;
        }
    }


    /// <summary>
    /// ButtonなどをRollに応じてfalseにする
    /// </summary>
    public void TimesavingControllerFalse() {
        if(chatSystem.myPlayer.live) {
            savingButton.interactable = false;
            COButton.interactable = false;
        }


        //if (!chatSystem.myPlayer.wolfChat) {
        //    ////WolfChatしゃべれないプレイヤーの処理
        //    wolfButton.interactable = false;
        //    //inputField.interactable = false;
        //}
    }

    /// <summary>
    /// 狼チャットを打つことができないプレイヤーは
    /// </summary>
    public void FalseInputViewButton() {
        //if (!chatSystem.myPlayer.wolfChat) {
            ////WolfChatしゃべれないプレイヤーの処理
            wolfButton.interactable = false;
            inputField.interactable = false;
        //}

        //狼出ないプレイヤーはsuperチャットだけfalseにする
        superChatButton.interactable = false;

    }

    /// <summary>
    /// 死亡したプレイヤーの処理
    /// </summary>
    private void DeathPlayer() {
        //自分のプレイヤーが死亡した場合の処理をする
        if (!chatSystem.myPlayer.live) {
            //狼モード、superチャット、COのボタンを無効
            inputView.wolfModeButton.interactable = false;
            inputView.comingOutButton.interactable = false;
            inputView.superChatButton.interactable = false;
            if (chatSystem.myPlayer.wolfChat) {
                gameManager.chatListManager.OffWolfMode();
                inputView.wolfModeButtonText.text = "市民";
            }
        }
    }




    ///////////////////////////////
    ///カスタムプロパティ関連
    ///////////////////////////////

    /// <summary>
    /// gamereadyを取得する
    /// </summary>
    /// <returns></returns>
    private bool GetGameReady() {
        gameReady = false;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameReady", out object gameReadyObj)) {
            gameReady = (bool)gameReadyObj;
        }
        return gameReady;
    }

    /// <summary>
    /// ゲーム中の時間のオンライン化
    /// </summary>
    public void SetGameTime() {
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"totalTime", totalTime }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    }


    /// <summary>
    /// トータルタイムを取得する
    /// </summary>
    /// <returns></returns>
    private float GetGameTime() {
        float time = 0;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("totalTime", out object totalTimeObj)) {
            time = (float)totalTimeObj;
        }
        return time;
    }


    /// <summary>
    /// isPlayingのオンライン化をbool型で返す
    /// </summary>
    /// <returns></returns>
    private PlayState  GetPlayState() {


        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("playState", out object isPlayingeObj)) {
            playState = (PlayState)Enum.Parse(typeof(PlayState),isPlayingeObj.ToString());
            Debug.Log("PlayState" + playState);
        }

        //ChangeIsCheckPlayState(false);


        return playState;
    }

    /// <summary>
    /// bool型IsPlayingをRoomPropertiesに保存する
    /// </summary>
    /// <returns></returns>
    private void SetPlayState(PlayState nowPlayState) {

        Debug.Log("nowPlayState" + nowPlayState);
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"playState",nowPlayState.ToString() }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        //foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
        //    //player.CustomProperties["playState"] = nowPlayState.ToString();
        //    player.SetCustomProperties(customRoomProperties);
        //}
        Debug.Log("SetPlayState" + (PlayState)Enum.Parse(typeof(PlayState),PhotonNetwork.CurrentRoom.CustomProperties["playState"].ToString()));
    }

    /// <summary>
    /// isCeackPlayStateのBoolを返す　 全員がPlayStateを取れる状態化の確認を取る用
    /// </summary>
    /// <param name="isCheackPlayState"></param>
    bool ChangeIsCheckPlayState_Interval(bool isCheackPlayState) {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"isCeackPlayState-Interval",isCheackPlayState }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customRoomProperties);
        Debug.Log("isCeackPlayState-Interval" + PhotonNetwork.LocalPlayer.CustomProperties["isCeackPlayState-Interval"]);
        return isCheackPlayState;
    }

    /// <summary>
    /// isCeackPlayStateのBoolを返す　 全員がPlayStateを取れる状態化の確認を取る用
    /// </summary>
    /// <param name="isCheackPlayState"></param>
    bool ChangeIsCheckPlayState_Play(bool isCheackPlayState) {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"isCeackPlayState-Play",isCheackPlayState }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customRoomProperties);
        Debug.Log("isCeackPlayState-Play" + PhotonNetwork.LocalPlayer.CustomProperties["isCeackPlayState-Play"]);
        return isCheackPlayState;
    }

    /// <summary>
    /// 全員がPlayStateを取れる状態化の確認を取る
    /// </summary>
    /// <returns></returns>
    int IsCheckPlayState_Interval() {
        int retReadyPlayerCount = 0;
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if (player.CustomProperties["isCeackPlayState-Interval"] != null && (bool)player.CustomProperties["isCeackPlayState-Interval"]) {
                retReadyPlayerCount++;
            }
        }
        Debug.Log("CheckPlayState" + retReadyPlayerCount);
        return retReadyPlayerCount;
    }

    /// <summary>
    /// 全員がPlayStateを取れる状態化の確認を取る
    /// </summary>
    /// <returns></returns>
    int IsCheckPlayState_Play() {
        int retReadyPlayerCount = 0;
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if (player.CustomProperties["isCeackPlayState-Play"] != null && (bool)player.CustomProperties["isCeackPlayState-Play"]) {
                retReadyPlayerCount++;
            } 
        }
        Debug.Log("CheckPlayState" + retReadyPlayerCount);
        return retReadyPlayerCount;
    }

    /// <summary>
    /// 全員が投票完了したらtrue
    /// </summary>
    public bool GetIsVotingCompleted() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("isVotingCompleted", out object isVotingCompletedObj)) {
            isVotingCompleted = (bool)isVotingCompletedObj;
        }
        return isVotingCompleted;
    }

    public void SetIsVotingCompleted() {
        var propertis = new ExitGames.Client.Photon.Hashtable {
            {"isVotingCompleted",isVotingCompleted }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(propertis);
    }

    /// <summary>
    /// 突然死したらフラグを付ける
    /// </summary>
    public void SetSuddenDeath() {
        var properties = new ExitGames.Client.Photon.Hashtable {
            {"setSuddenDeath",setSuddenDeath},
        };
        Debug.Log("setSuddenDeath" + setSuddenDeath);
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }

    /// <summary>
    /// 突然死したプレイヤーのフラグをもらう
    /// </summary>
    /// <returns></returns>
    private bool GetSuddenDeath(Photon.Realtime.Player player) {
        if (player.CustomProperties.TryGetValue("setSuddenDeath", out object setSuddenDeathObj)) {
            setSuddenDeath = (bool)setSuddenDeathObj;
        }
        Debug.Log("GetsetSuddenDeath"+ setSuddenDeath);
        return setSuddenDeath;
    }

}


