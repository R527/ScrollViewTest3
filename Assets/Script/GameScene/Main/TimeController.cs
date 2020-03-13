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
    
    //main
    public Text timerText;
    public  float totalTime;
    public float mainTime;　　　//昼の時間
    public float votingTime;   //投票時間
    public float nightTime;   //夜の時間
    public float executionTime;//処刑時間
    public float moningTime;//朝の結果発表
    public float rollActionTime;
    public float intervalTime;
    public bool isPlaying;　　//gameが動いているかの判定
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
    public bool isGameOver;
    private float cheakTimer;//1秒ごとに時間を管理する

    //x日　GMのチャット追加
    public GameObject content;
    public int day;
    public GameObject dayPrefab;
    public bool startGame;
    public List<GameObject> nextDayList = new List<GameObject>();

    /// <summary>
    /// 各ボタンの制御,
    /// </summary>
    public void Init(bool isOffline) {
        startGame = true;
        isGameOver = true;
        timeType = TIME.夜の行動;
        savingButton.interactable = true;
        wolfButton.interactable = false;
        callOutButton.interactable = false;
        COButton.interactable = false;
        mainTime = RoomData.instance.roomInfo.mainTime;
        nightTime = RoomData.instance.roomInfo.nightTime;
        totalTime = nightTime;
        if (!isOffline && PhotonNetwork.IsMasterClient) {
            isPlaying = true;
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                { "totalTime", totalTime },
                {"isPlaying", isPlaying }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
            Debug.Log((float)PhotonNetwork.CurrentRoom.CustomProperties["totalTime"]);
            Debug.Log((bool)PhotonNetwork.CurrentRoom.CustomProperties["isPlaying"]);
        }
    }



    //カウントダウンタイマー
    void Update() {
        //GetPlayStateはtrueが返ってきたらOK
        if (GetPlayState() && gameManager.gameStart == true) {
            if (PhotonNetwork.IsMasterClient) {
                cheakTimer += Time.deltaTime;
                if (cheakTimer >= 1) {
                    cheakTimer = 0;
                    totalTime--;
                    SetGameTime();
                }
            } else {
                Debug.Log(totalTime);
                totalTime = GetGameTime();
            }
            //totalTime -= Time.deltaTime;
            seconds = (int)totalTime;
            timerText.text = totalTime.ToString("F0");
            if (totalTime < 1) {
                isPlaying = false;
                StartInterval();
            }
        }
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
        if (isPlaying == false && isGameOver == true) {
            switch (timeType) {
                //お昼
                case TIME.夜の結果発表:
                    timeType = TIME.昼;
                    mainPopup.SetActive(true);
                    totalTime = mainTime;
                    chatSystem.coTimeLimit = 0;
                    chatSystem.calloutTimeLimit = 0;
                    if (startGame == false) {
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
                    timeType = TIME.処刑後チェック;
                    //voteCount.Execution();
                    totalTime = executionTime;
                    break;

                //処刑後チェック
                case TIME.処刑後チェック:
                    //gameOver.CheckGameOver();
                    timeType = TIME.処刑;
                    break;

                //夜の行動
                case TIME.処刑:
                    timeType = TIME.夜の行動;
                    nightPopup.SetActive(true);
                    //霊能
                    //rollAction.PsychicAction();
                    voteCount.mostVotes = 0;
                    voteCount.ExecutionPlayerList.Clear();
                    totalTime = nightTime;
                    break;

                //夜の結果発表
                case TIME.夜の行動:
                    totalTime = rollActionTime;
                    if (startGame == true) {
                        StartCoroutine(NextDay());
                        startGame = false;
                    }
                    StartCoroutine(GameMasterChat());
                    timeType = TIME.結果発表後チェック;
                    break;

                //結果発表チェック
                case TIME.結果発表後チェック:
                    timeType = TIME.夜の結果発表;
                    //gameOver.CheckGameOver();
                    break;
            }
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
        isPlaying = true;
        TimesavingControllerTrue();
    }

    /// <summary>
    /// 時短、狼モード、
    /// </summary>
    public void TimesavingControllerTrue() {
        if(timeType == TIME.昼) {
            savingButton.interactable = true;
            if(chatSystem.myPlayer.rollType == ROLLTYPE.人狼) {
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

    ///// <summary>
    ///// 時短.退出ボタン
    ///// </summary>
    //public void SavingButton() {
    //    if (savingText.text == "退出") {
    //        if (gameManager.isOffline) {
    //            SceneStateManager.instance.NextScene(SCENE_TYPE.LOBBY);
    //        } else {
    //            NetworkManager.instance.LeaveRoom();
    //        }
    //    }
    //        } else {
    //            totalTime = 0;
    //        }
    //    }
}


