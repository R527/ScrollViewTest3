using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;



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

    //main
    public Text timerText;
    public float totalTime;
    public float mainTime;　　　//昼の時間
    public float votingTime;   //投票時間
    public float nightTime;   //夜の時間
    public float executionTime;//処刑時間
    public float moningTime;//朝の結果発表
    public float rollActionTime;
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
    public GameObject mainPopup;
    public GameObject votingPopup;
    public GameObject nightPopup;
    public TIME timeType;
    public bool isGameOver;//falseならゲームオーバー
    private float cheakTimer;//1秒ごとに時間を管理する

    //x日　GMのチャット追加
    public GameObject content;
    public int day;
    public GameObject dayPrefab;
    public bool firstDay;
    public List<GameObject> nextDayList = new List<GameObject>();

    /// <summary>
    /// 各ボタンの制御,
    /// </summary>
    public void Init(bool isOffline) {
        firstDay = true;
        isGameOver = true;
        timeType = TIME.処刑後チェック;
        savingButton.interactable = true;
        wolfButton.interactable = false;
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
                {"gameReady",gameReady }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
            Debug.Log((float)PhotonNetwork.CurrentRoom.CustomProperties["totalTime"]);
            Debug.Log((bool)PhotonNetwork.CurrentRoom.CustomProperties["isPlaying"]);
            Debug.Log((bool)PhotonNetwork.CurrentRoom.CustomProperties["gameReady"]);
        }
        Debug.Log("Init時のTimeType"　+ timeType);

        // 本当の姿を表示する
        gameMasterChatManager.TrueCharacter();


        StartInterval();
    }


    /// <summary>
    /// カウントダウンタイマー
    /// </summary>
    void Update() {
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
                    Debug.Log(totalTime);
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
                Debug.Log(!GetPlayState());
                Debug.Log(totalTime);

                Debug.Log(timeType);
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

        Debug.Log("isPlaying:セット完了");
    }

    /// <summary>
    /// bool型gameReadyをRoomPropertiesに保存する
    /// </summary>
    /// <param name="isPlayState"></param>
    private void SetGameReady(bool gameReady) {
        var customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                    {"gameReady",gameReady }
                };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);

        Debug.Log("gameReady:セット完了");
    }

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
    private void SetGameTime() {
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                            {"totalTime", totalTime }
                        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        Debug.Log((float)PhotonNetwork.CurrentRoom.CustomProperties["totalTime"]);
    }

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
    /// インターバルを決定する
    /// </summary>
    public void StartInterval() {
        Debug.Log("StartInterval:開始");
        if (!isPlaying && gameManager.gameStart) {
            switch (timeType) {
                //お昼
                case TIME.夜の結果発表:
                    timeType = TIME.昼;

                    mainPopup.SetActive(true);
                    totalTime = mainTime;
                    chatSystem.coTimeLimit = 0;
                    chatSystem.calloutTimeLimit = 0;
                    if (firstDay == false) {
                        StartCoroutine(NextDay());
                    }
                    StartCoroutine(GameMasterChat());
                    break;

                //投票時間
                case TIME.昼:
                    timeType = TIME.投票時間;

                    votingPopup.SetActive(true);
                    totalTime = votingTime;
                    voteCount.isVoteFlag = false;
                    TimesavingControllerFalse();
                    StartCoroutine(GameMasterChat());
                    break;

                //処刑
                case TIME.投票時間:
                    timeType = TIME.処刑;

                    totalTime = executionTime;
                    voteCount.Execution();
                    gameManager.gameMasterChatManager.ExecutionChat();
                    break;

                //処刑後チェック
                case TIME.処刑:
                    timeType = TIME.処刑後チェック;

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
                    //霊能結果の表示
                    gameMasterChatManager.PsychicAction();
                    if (firstDay == true) {
                        StartCoroutine(NextDay());
                        firstDay = false;
                    } 
                    StartCoroutine(GameMasterChat());
                    break;

                //夜の行動の結果発表
                case TIME.夜の行動:
                    timeType = TIME.結果発表後チェック;
                    chatSystem.myPlayer.isRollAction = false;
                    gameMasterChatManager.MorningResults();
                    totalTime = rollActionTime;
                    break;

                //結果発表チェック
                case TIME.結果発表後チェック:
                    timeType = TIME.夜の結果発表;
                    //gameOver.CheckGameOver();
                    break;
            }
            Debug.Log(timeType);
            StartCoroutine(EndInterval());
        }
    }

    /// <summary>
    /// GameMasterChatの制御
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameMasterChat() {
        yield return new WaitForSeconds(intervalTime + 0.2f);
        chatSystem.GameMasterChatNode();
    }

    /// <summary>
    /// 翌日分のObjをInstatiateする
    /// </summary>
    /// <returns></returns>
    private IEnumerator NextDay() {
        yield return new WaitForSeconds(intervalTime + 0.1f);
        day++;
        chatSystem.id++;
        nextDay.nextDayText.text = day + "日目";
        //NextDayクラスのdayにTimeController.dayを代入
        nextDay.day = day;
        GameObject dayObj = Instantiate(dayPrefab, content.transform, false);
        nextDayList.Add(dayObj);
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

        Debug.Log("EndInterval: 終了");
    }

    /// <summary>
    /// 時短、狼モード、
    /// </summary>
    public void TimesavingControllerTrue() {
        if (timeType == TIME.昼) {
            savingButton.interactable = true;
            if (chatSystem.myPlayer.rollType == ROLLTYPE.人狼) {
                wolfButton.interactable = true;
            }
            callOutButton.interactable = true;
            COButton.interactable = true;
        }
    }

    public void TimesavingControllerFalse() {
        savingButton.interactable = false;
        wolfButton.interactable = false;
        callOutButton.interactable = false;
        COButton.interactable = false;
    }


}


