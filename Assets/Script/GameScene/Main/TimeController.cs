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
    public bool isPlaying;　　//gameが動いているかの判定
    public bool gameReady;//ゲーム待機状態か否か
    private int seconds;
    public bool isGameOver;//falseならゲームオーバー
    private float chekTimer;//1秒ごとに時間を管理する
    public bool isVotingCompleted;

    //ボタン・Input関連
    public Button savingButton;//時短ボタン
    
    public Button COButton;
    public Button superChatButton;
    public Button wolfButton;
    public InputField inputField;//文字入力部分

    //PoPUp
    public GameObject mainPopup;
    public GameObject votingPopup;
    public GameObject nightPopup;

    //x日　GMのチャット追加
    public GameObject chatContent;
    public int day;
    public NextDay dayPrefab;
    public bool firstDay;
    public List<NextDay> nextDayList = new List<NextDay>();

    public enum PlayState {
        Play,
        Stop,
        Interval
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
        isGameOver = true;
        timeType = TIME.処刑後チェック;
        //savingButton.interactable = true;


        //Debug用
        if (DebugManager.instance.isDebug) {
            votingTime = DebugManager.instance.testVotingTime;
            executionTime = DebugManager.instance.testExecutionTime;
            checkGameOverTime = DebugManager.instance.testCheckGameOverTime;
            resultTime = DebugManager.instance.testResultTime;
            timeType = DebugManager.instance.timeType;
        }

        //狼の場合
        if (chatSystem.myPlayer.wolfChat) {
            wolfButton.interactable = true;
            inputField.interactable = true;
        } else {
            wolfButton.interactable = false;
            inputField.interactable = false;
        }

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
            Debug.Log((TIME)PhotonNetwork.CurrentRoom.CustomProperties["timeType"]);
        }

        // 本当の姿を表示する
        gameMasterChatManager.TrueCharacter();

        yield return new WaitForSeconds(2.0f);

        playState = GetPlayState();

        //ゲームスタート
        gameReady = GetGameReady();
    }


    /// <summary>
    /// カウントダウンタイマー
    /// </summary>
    void Update() {

        //ゲーム終了したら実行しない
        if (!isGameOver) {
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
            if(totalTime >= 0) {
                timerText.text = totalTime.ToString("F0");
            }
                
            //0秒になったら次のシーンへ移行する
            if (totalTime < 0 && PhotonNetwork.IsMasterClient) {
                //マスターだけがisPlayingをfalseに
                //isPlaying = false;
                //playState = PlayState.Stop;
                SetPlayState(PlayState.Stop);
            } //else if(totalTime < 0 && !PhotonNetwork.IsMasterClient) {

            GetPlayState();
            //}

            //次のシーンへ移行する処理
        } else if (playState == PlayState.Stop){
            playState = PlayState.Interval;
            timerText.text = string.Empty;
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
        Debug.Log("StartInterval:開始");
        //!isPlaying &&
        //if (gameManager.gameStart) {

            switch (timeType) {
                //お昼
                case TIME.結果発表後チェック:
                    timeType = TIME.昼;
                    totalTime = mainTime;
                    mainPopup.SetActive(true);
                    
                    //初期化
                    chatSystem.coTimeLimit = 0;
                    chatSystem.calloutTimeLimit = 0;

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
                    votingPopup.SetActive(true);
                    totalTime = votingTime;

                    //GMチャットなど
                    TimesavingControllerFalse();
                    StartCoroutine(GameMasterChat());
                    StartCoroutine(UpInputView());
                    break;

                //処刑
                case TIME.投票時間:
                    timeType = TIME.処刑;
                    totalTime = executionTime;

                    //初期化
                    isVotingCompleted = false;

                    voteCount.Execution();
                    gameManager.gameMasterChatManager.ExecutionChat();
                    DeathPlayer();
                    break;

                //処刑後チェック
                case TIME.処刑:
                    timeType = TIME.処刑後チェック;
                    chatSystem.myPlayer.isVoteFlag = false;
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
                            Debug.Log((int)player.CustomProperties["voteNum"]);
                            Debug.Log((string)player.CustomProperties["voteName"]);
                            Debug.Log((bool)player.CustomProperties["votingCompleted"]);
                        }

                    }

                    //生存者数を取得
                    gameManager.liveNum = gameManager.GetLiveNum();
                    //gameOver.CheckGameOver();
                    break;

                //夜の行動
                case TIME.処刑後チェック:
                    timeType = TIME.夜の行動;
                    totalTime = nightTime;
                    nightPopup.SetActive(true);

                    //初期化
                    voteCount.mostVotes = 0;
                    voteCount.ExecutionPlayerList.Clear();
                    
                    //GMチャットなど
                    if (firstDay) {
                        StartCoroutine(NextDay());
                        StartCoroutine(UpInputView());
                        Debug.Log("firstDay");
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
                    totalTime = resultTime;

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
                    totalTime = checkGameOverTime;

                    //gameOver.CheckGameOver();
                    DeathPlayer();
                    break;
            }

            StartCoroutine(EndInterval(timeType));
        //}
    }

    /// <summary>
    /// GameMasterChatの制御
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameMasterChat() {
        yield return new WaitForSeconds(intervalTime + 0.2f);
        gameMasterChatManager.TimeManagementChat();
    }

    /// <summary>
    /// 翌日分のObjをInstatiateする
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextDay() {
        Debug.Log("NextDay" + day);
        yield return new WaitForSeconds(intervalTime + 0.1f);
        day++;
        chatSystem.id++;
        
        NextDay dayObj = Instantiate(dayPrefab, chatContent.transform, false);
        dayObj.day = day;
        dayObj.nextDayText.text = day + "日目";
        nextDayList.Add(dayObj);
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
    /// 昼、夜、投票後にあるインターバル時間を設定
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndInterval(TIME nowTimeType) {
        yield return new WaitForSeconds(intervalTime);//コルーチンでインターバル時間を設ける
        mainPopup.SetActive(false);
        votingPopup.SetActive(false);
        nightPopup.SetActive(false);
        TimesavingControllerTrue();

        if (PhotonNetwork.IsMasterClient) {
            //isPlaying = true;
            //playState = PlayState.Play;
            SetTimeType(nowTimeType);
            SetPlayState(PlayState.Play);
        }

        yield return new WaitForSeconds(2.0f);
        playState = GetPlayState();
        //while (playState == PlayState.Interval) {
        //    playState = GetPlayState();
        //    yield return null;
        //}
        //違うタイムタイプなら訂正する
        if(timeType != GetTimeType()) {
            timeType = GetTimeType();
        }
    }

    /// <summary>
    /// InPutViewを上げます
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpInputView() {
        fillter.folding = true;
        inputView.foldingButton.interactable = false;
        yield return new WaitForSeconds(intervalTime + 0.3f);
        inputView.inputRectTransform.DOLocalMoveY(0, 0.5f);
        inputView.mainRectTransform.DOLocalMoveY(72, 0.5f);
        inputView.menberViewPopUpObj.SetActive(true);
        inputView.foldingText.text = "↓";
    }

    /// <summary>
    /// InPutViewを下げます
    /// </summary>
    /// <returns></returns>
    public IEnumerator DownInputView() {
        fillter.folding = false;
        inputView.foldingButton.interactable = true;
        yield return new WaitForSeconds(intervalTime + 0.3f);
        inputView.inputRectTransform.DOLocalMoveY(-67, 0.5f);
        inputView.mainRectTransform.DOLocalMoveY(0, 0.5f);
        StartCoroutine(inputView.PopUpFalse());
        inputView.foldingText.text = "↑";
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
        savingButton.interactable = false;
        superChatButton.interactable = false;
        COButton.interactable = false;

        //WolfChatが使えるプレイヤーの場合
        if (!chatSystem.myPlayer.wolfChat) {
            //しゃべれないプレイヤーの処理
            wolfButton.interactable = false;
            inputField.interactable = false;
        }
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
        Debug.Log("gameReady" + gameReady);
        return gameReady;
    }

    /// <summary>
    /// ゲーム中の時間のオンライン化
    /// </summary>
    private void SetGameTime() {
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
        //isPlaying = false;
        //playState = PlayerState.Stop;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("playState", out object isPlayingeObj)) {
            playState = (PlayState)Enum.Parse(typeof(PlayState),isPlayingeObj.ToString());
            Debug.Log(playState);
        }
        return playState;
    }

    /// <summary>
    /// bool型IsPlayingをRoomPropertiesに保存する
    /// </summary>
    /// <returns></returns>
    private void SetPlayState(PlayState nowPlayState) {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
            {"playState",nowPlayState.ToString() }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log("SetPlayState" + (PlayState)Enum.Parse(typeof(PlayState),PhotonNetwork.CurrentRoom.CustomProperties["playState"].ToString()));
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
    /// TimeTypeをもらう
    /// </summary>
    /// <returns></returns>
    private TIME GetTimeType() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("timeType", out object timeTypeObj)) {
            timeType = (TIME)Enum.Parse(typeof(TIME),timeTypeObj.ToString());
        }
        return timeType;
    }


    /// <summary>
    /// TimeTypeをセットする
    /// </summary>
    private void SetTimeType(TIME nowTimeType) {
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                            {"timeType", nowTimeType.ToString() }
                        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    }
}


