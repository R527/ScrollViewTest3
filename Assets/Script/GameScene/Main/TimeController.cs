using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using DG.Tweening;



/// <summary>
/// タイムコントロール　夜、昼、投票を決定　時短処理など
/// </summary>
public class TimeController : MonoBehaviourPunCallbacks {


    //class
    public GameManager gameManager;
    public NextDay nextDay;
    public ChatSystem chatSystem;
    public VoteCount voteCount;
    public RollAction rollAction;
    public GameOver gameOver;
    public GameMasterChatManager gameMasterChatManager;
    public InputView inputView;
    public Fillter fillter;

    //main
    public Text timerText;
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
    //public string isIntervalFlag;   //mainTime or intervalTimeが動いているかの判定
    private int seconds;
    public Button savingButton;//時短ボタン
    public Text savingText;
    public Button COButton;
    public Button callOutButton;
    public Button wolfButton;
    public InputField inputField;//文字入力部分
    public GameObject mainPopup;
    public GameObject votingPopup;
    public GameObject nightPopup;
    public TIME timeType;
    public bool isGameOver;//falseならゲームオーバー
    private float cheakTimer;//1秒ごとに時間を管理する
    public bool isVotingCompleted;

    //x日　GMのチャット追加
    public GameObject chatContent;
    public int day;
    public NextDay dayPrefab;
    public bool firstDay;
    public List<NextDay> nextDayList = new List<NextDay>();

    /// <summary>
    /// 各ボタンの制御,
    /// </summary>
    public void Init(bool isOffline) {
        firstDay = true;
        isGameOver = true;
        timeType = TIME.処刑後チェック;
        savingButton.interactable = true;


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

        callOutButton.interactable = false;
        COButton.interactable = false;
        mainTime = RoomData.instance.roomInfo.mainTime;
        nightTime = RoomData.instance.roomInfo.nightTime;
        

        //マスターだけトータルタイムとIsPlayingを取得する
        if (!isOffline && PhotonNetwork.IsMasterClient) {
            isPlaying = true;
            gameReady = true;
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                { "totalTime", totalTime },
                {"isPlaying", isPlaying },
                {"gameReady",gameReady },
                {"timeType",timeType }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
            //Debug.Log((float)PhotonNetwork.CurrentRoom.CustomProperties["totalTime"]);
            //Debug.Log((bool)PhotonNetwork.CurrentRoom.CustomProperties["isPlaying"]);
            //Debug.Log((bool)PhotonNetwork.CurrentRoom.CustomProperties["gameReady"]);
            Debug.Log((TIME)PhotonNetwork.CurrentRoom.CustomProperties["timeType"]);
        }

        // 本当の姿を表示する
        gameMasterChatManager.TrueCharacter();


       //StartInterval();
    }


    /// <summary>
    /// カウントダウンタイマー
    /// </summary>
    void Update() {
        //全員の投票が完了したら
        if (GetIsVotingCompleted() && PhotonNetwork.IsMasterClient) {
            isVotingCompleted = false;
            SetIsVotingCompleted();
            totalTime = 0;
            SetGameTime();
        }
        //ゲーム待機中かどうかを確認する
        if (!GetGameReady()) {
            return;
        } else { 
        //ゲーム開始したら以下のUpdateを通過する
        
            //ゲームが終了したらUpdateを止める
            if (timeType == TIME.終了) {
                timerText.text = string.Empty;
                gameManager.gameMasterChatManager.timeSavingButtonText.text = "退出";
                return;

                //GetPlayStateはtrueが返ってきたらOK
            } else if (GetPlayState() && gameManager.gameStart) {

                //マスターだけトータルタイムを管理する
                if (PhotonNetwork.IsMasterClient) {
                    cheakTimer += Time.deltaTime;
                    if (cheakTimer >= 1) {
                        cheakTimer = 0;
                        totalTime--;

                        SetGameTime();
                    }
                    //マスター以外はトータルタイムをもらう
                } else {
                    //Debug.Log(totalTime);
                    totalTime = GetGameTime();
                }
                //トータルタイム表示
                seconds = (int)totalTime;
                timerText.text = totalTime.ToString("F0");
                //トータルタイムが0未満になったら
                if (totalTime < 0) {
                    //マスターだけがisPlayingをfalseに
                    if (PhotonNetwork.IsMasterClient) {
                        isPlaying = false;
                        SetPlayState(isPlaying);
                    }
                }

                //isPlayingがfalseでかつトータルタイムが0以下ならStartIntervalへ
            } else if (!GetPlayState() && totalTime <= 0) {
                timerText.text = string.Empty;
                //Debug.Log(!GetPlayState());
                //Debug.Log(totalTime);

                //Debug.Log(timeType);
                StartInterval();
                Debug.Log(timeType);
            }
        }
        
    }


    /// <summary>
    /// bool型IsPlayingをRoomPropertiesに保存する
    /// </summary>
    /// <returns></returns>
    private void SetPlayState(bool isPlayState) {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                    {"isPlaying",isPlayState }
                };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);

        //Debug.Log("isPlaying:セット完了");
    }

    ///// <summary>
    ///// bool型gameReadyをRoomPropertiesに保存する
    ///// </summary>
    ///// <param name="isPlayState"></param>
    //private void SetGameReady(bool gameReady) {
    //    var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
    //                {"gameReady",gameReady }
    //            };
    //    PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);

    //    Debug.Log("gameReady:セット完了");
    //}

    /// <summary>
    /// gamereadyを取得する
    /// </summary>
    /// <returns></returns>
    private bool GetGameReady() {
        gameReady = false;
        if (!gameManager.isOffline && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameReady", out object gameReadyObj)) {
            gameReady = (bool)gameReadyObj;
        }
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
        //Debug.Log((float)PhotonNetwork.CurrentRoom.CustomProperties["totalTime"]);
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
    private bool GetPlayState() {
        isPlaying = false;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("isPlaying", out object isPlayingeObj)) {
            isPlaying = (bool)isPlayingeObj;
        }
        return isPlaying;
    }


    /// <summary>
    /// TimeTypeをもらう
    /// </summary>
    /// <returns></returns>
    private TIME GetTimeType() {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("timeType", out object timeTypeObj)) {
            timeType = (TIME)timeTypeObj;
        }
        return timeType;
    }


    /// <summary>
    /// TimeTypeをセットする
    /// </summary>
    private void SetTimeType() {
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                            {"timeType", timeType }
                        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log((TIME)PhotonNetwork.CurrentRoom.CustomProperties["timeType"]);
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
    /// インターバルを決定する
    /// </summary>
    public void StartInterval() {
        Debug.Log("StartInterval:開始");

        ////TimeTypeをもらう
        //if (PhotonNetwork.IsMasterClient) {
        //    SetTimeType();
        //    Debug.Log(timeType);
        //} else {
        //    timeType = GetTimeType();
        //    Debug.Log(timeType);
        //}

        if (!isPlaying && gameManager.gameStart) {
            switch (timeType) {
                //お昼
                case TIME.結果発表後チェック:
                    timeType = TIME.昼;

                    mainPopup.SetActive(true);
                    totalTime = mainTime;
                    chatSystem.coTimeLimit = 0;
                    chatSystem.calloutTimeLimit = 0;
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
                    TimesavingControllerFalse();
                    StartCoroutine(GameMasterChat());
                    StartCoroutine(UpInputView());
                    break;

                //処刑
                case TIME.投票時間:
                    timeType = TIME.処刑;
                    isVotingCompleted = false;
                    totalTime = executionTime;
                    voteCount.Execution();
                    gameManager.gameMasterChatManager.ExecutionChat();
                    break;

                //処刑後チェック
                case TIME.処刑:
                    timeType = TIME.処刑後チェック;
                    chatSystem.myPlayer.isVoteFlag = false;

                    if (PhotonNetwork.IsMasterClient) {
                        //生き残ったプレイヤーのVoteCountを０にする
                        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
                            var properties = new ExitGames.Client.Photon.Hashtable {
                            {"voteNum", 0 },
                            {"VotingCompletedNum",false }
                        };
                            Debug.Log(player.CustomProperties["voteNum"]);
                            Debug.Log(player.CustomProperties["VotingCompletedNum"]);
                            player.SetCustomProperties(properties);
                        }
                    }


                    totalTime = checkGameOverTime;
                    //生存者数を取得
                    gameManager.liveNum = gameManager.GetLiveNum();
                    //gameOver.CheckGameOver();
                    break;

                //夜の行動
                case TIME.処刑後チェック:
                    timeType = TIME.夜の行動;

                    nightPopup.SetActive(true);
                    voteCount.mostVotes = 0;
                    voteCount.ExecutionPlayerList.Clear();
                    totalTime = nightTime;
                    if (firstDay) {
                        StartCoroutine(NextDay());
                        StartCoroutine(UpInputView());
                    }
                    StartCoroutine(GameMasterChat());
                    if (!firstDay) {
                        StartCoroutine(PsychicAction());
                    }
                    break;

                //夜の行動の結果発表
                case TIME.夜の行動:
                    timeType = TIME.夜の結果発表;
                    totalTime = resultTime;
                    chatSystem.myPlayer.isRollAction = false;
                    //gameMasterChatManager.MorningResults();
                    if (!firstDay) {
                        //gameMasterChatManager.MorningResults();
                    } else {
                        firstDay = false;
                    }
                    
                    break;

                //結果発表チェック
                case TIME.夜の結果発表:
                    timeType = TIME.結果発表後チェック;
                    totalTime = checkGameOverTime;
                    //gameOver.CheckGameOver();
                    break;
            }

            ////TimeTypeをもらう
            //if (PhotonNetwork.IsMasterClient) {
            //    SetTimeType();
            //Debug.Log(timeType);
            //} else {
            //    timeType = GetTimeType();
            //Debug.Log(timeType);
            //}


            StartCoroutine(EndInterval());
        }
    }

    /// <summary>
    /// GameMasterChatの制御
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameMasterChat() {
        yield return new WaitForSeconds(intervalTime + 0.2f);
        //chatSystem.GameMasterChatNode();
        gameMasterChatManager.TimeManagementChat();
    }

    /// <summary>
    /// 翌日分のObjをInstatiateする
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextDay() {
        //Debug.Log("oldday" + day);
        yield return new WaitForSeconds(intervalTime + 0.1f);
        day++;
        chatSystem.id++;
        //nextDay.nextDayText.text = day + "日目";
        //NextDayクラスのdayにTimeController.dayを代入
        //nextDay.day = day;
        NextDay dayObj = Instantiate(dayPrefab, chatContent.transform, false);
        dayObj.day = day;
        dayObj.nextDayText.text = day + "日目";
        nextDayList.Add(dayObj);
        //Debug.Log("newDay" + day);
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
    private IEnumerator EndInterval() {
        yield return new WaitForSeconds(intervalTime);//コルーチンでインターバル時間を設ける
        mainPopup.SetActive(false);
        votingPopup.SetActive(false);
        nightPopup.SetActive(false);
        TimesavingControllerTrue();

        if (PhotonNetwork.IsMasterClient) {
            isPlaying = true;
            SetPlayState(isPlaying);
        }

        //Debug.Log("EndInterval: 終了");
    }

    public IEnumerator UpInputView() {
        fillter.folding = true;
        inputView.foldingButton.interactable = false;
        yield return new WaitForSeconds(intervalTime + 0.3f);
        inputView.inputRectTransform.DOLocalMoveY(0, 0.5f);
        inputView.mainRectTransform.DOLocalMoveY(72, 0.5f);
        inputView.menberViewPopUpObj.SetActive(true);
        inputView.foldingText.text = "↓";
    }

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
        if (timeType == TIME.昼) {
            savingButton.interactable = true;
            if (chatSystem.myPlayer.wolfChat) {
                wolfButton.interactable = true;
            }
            callOutButton.interactable = true;
            COButton.interactable = true;
            inputField.interactable = true;
        }
    }


    /// <summary>
    /// ButtonなどをRollに応じてfalseにする
    /// </summary>
    public void TimesavingControllerFalse() {
        savingButton.interactable = false;
        callOutButton.interactable = false;
        COButton.interactable = false;

        //WolfChatが使えるプレイヤーの場合
        if (chatSystem.myPlayer.wolfChat) {
            //変更なくしゃべれる
        } else {
            //しゃべれないプレイヤーの処理
            wolfButton.interactable = false;
            inputField.interactable = false;
        }
    }

    ///// <summary>
    ///// 投票時PlayerButtonをRollに応じてOFFにする((フィルターにも対応させる必要があるから別の方法を考える
    ///// /// </summary>
    //public void OffVotingButton() {
    //    foreach (Player playerObj in chatSystem.playersList) {
    //        if (chatSystem.myPlayer.playerID == playerObj.playerID || !playerObj.live) {
    //            playerObj.GetComponent<Button>().interactable = false;
    //        }
    //    }
    //}
}


