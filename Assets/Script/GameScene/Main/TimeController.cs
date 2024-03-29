﻿using System.Collections;
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
    public bool isNextInterval;
    public bool intervalState;

    public bool gameReady;//ゲーム待機状態か否か
    public bool isSpeaking;//喋ったか否かtrueならしゃべった
    public bool setSuddenDeath;
    public bool isPlay;//falseならゲームオーバー
    private float checkTimer;//1秒ごとに時間を管理する
    public bool isVotingCompleted;
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
        Stop_play
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
        superChatButton.interactable = false;
        COButton.interactable = false;
        mainTime = RoomData.instance.roomInfo.mainTime;
        nightTime = RoomData.instance.roomInfo.nightTime;

        //マスターだけトータルタイムとIsPlayingを取得する
        if (PhotonNetwork.IsMasterClient) {
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
                {"totalTime", totalTime },
                {"gameReady",true },
                {"playState", PlayState.Stop_play.ToString() },
                {"timeType",timeType }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        }

        // 本当の姿を表示する
        gameMasterChatManager.TrueCharacter();

        //ゲームスタート
        //GetGameReady
        yield return new WaitUntil(() => NetworkManager.instance.GetCustomPropertesOfRoom<bool>("gameReady"));
        gameReady = NetworkManager.instance.GetCustomPropertesOfRoom<bool>("gameReady");
    }


    /// <summary>
    /// カウントダウンタイマー
    /// </summary>
    void FixedUpdate() {
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

        //testText.text = "isNextInterval:" + isNextInterval;
        if (!isNextInterval) {
            //マスターだけトータルタイムを管理する
            if (PhotonNetwork.IsMasterClient) {
                checkTimer += Time.deltaTime;
                if (checkTimer >= 1) {
                    checkTimer = 0;
                    totalTime--;
                    NetworkManager.instance.SetCustomPropertesOfRoom("totalTime",totalTime);
                    //SetGameTime();
                }
                //マスター以外はトータルタイムをもらう
            } else {
                //GetGameTime()
                totalTime = NetworkManager.instance.GetCustomPropertesOfRoom<float>("totalTime");
            }

            //トータルタイム表示
            //トータルタイムを受け取る側はー1秒から始まるのでその調整用
            if (totalTime > 0 && isDisplay) {
                timerText.text = totalTime.ToString("F0");
            }

            //マスターだけ時短処理のフラグを受け取る
            if (PhotonNetwork.IsMasterClient) {
                checkTimer += Time.deltaTime;
                if (checkTimer >= 1) {
                    checkTimer = 0;
                    NetworkManager.instance.GetCustomPropertesOfRoom<bool>("isTimeSaving");
                    //gameManager.gameMasterChatManager.GetIsTimeSaving();
                }
            }
            //マスターだけが0秒もしくは時短成立の確認を取れたら全員に次のシーンへと行くフラグを送信する
            if ((totalTime < 0 || gameManager.gameMasterChatManager.isTimeSavingOfOnLine) && PhotonNetwork.IsMasterClient) {
                totalTime = -1;
                //SetGameTime();
                totalTime = NetworkManager.instance.GetCustomPropertesOfRoom<float>("totalTime");
                intervalState = true;
                gameManager.gameMasterChatManager.isTimeSavingOfOnLine = false;
                NetworkManager.instance.SetCustomPropertesOfRoom("isTimeSaving", gameManager.gameMasterChatManager.isTimeSavingOfOnLine);

                //gameManager.gameMasterChatManager.SetIsTimeSaving();
                //SetIntervalState();
                NetworkManager.instance.SetCustomPropertesOfRoom("intervalState", true);
            }


            //時短もしくは時間が0秒になったら次のシーンへ以降するフラグを受け取る
            if (totalTime < 0) {
                intervalState = NetworkManager.instance.GetCustomPropertesOfRoom<bool>("intervalState");
                //GetIntervalState();
            }

            //フラグを受け取るとスタートインターバルを走らせる
            //intervalStateはオンライン用、IsNextIntervalはOFFLine用のフラグ
            if (intervalState  && !isNextInterval) {
                isNextInterval = true;
                StartInterval();
            }
        }

        //全員の投票が完了したら
        //GetIsVotingCompleted()
        if (!NetworkManager.instance.GetCustomPropertesOfRoom<bool>("isVotingCompleted") || !PhotonNetwork.IsMasterClient) {
            return;
        }else if (NetworkManager.instance.GetCustomPropertesOfRoom<bool>("isVotingCompleted") && PhotonNetwork.IsMasterClient) {
            isVotingCompleted = false;
            NetworkManager.instance.SetCustomPropertesOfRoom("isVotingCompleted", isVotingCompleted);
            //SetIsVotingCompleted();
            totalTime = 0;
            //SetGameTime();
            totalTime = NetworkManager.instance.GetCustomPropertesOfRoom<float>("totalTime");
        }
    }


    ///////////////////////////
    ///メソッド関連
    ///////////////////////////

    /// <summary>
    /// インターバルを決定する
    /// </summary>
    public void StartInterval() {
        //タイムタイプに違いがある場合修正する
        if(timeType != NetworkManager.instance.GetCustomPropertesOfRoom<TIME>("timeType")) {
            timeType = NetworkManager.instance.GetCustomPropertesOfRoom<TIME>("timeType");
        }

        //一度時間を非表示にする
        timerText.text = string.Empty;
        switch (timeType) {
            //お昼
            case TIME.結果発表後チェック:
                timeType = TIME.昼;

                StartCoroutine(AudioManager.instance.PlayBGM(AudioManager.BGM_TYPE.お昼));
                isDisplay = true;
                totalTime = mainTime;
                ChangeSecene();

                if (chatSystem.myPlayer.live) {
                    gameManager.gameMasterChatManager.timeSavingButtonText.text = "時短";
                }

                //Co制限初期化
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

                StartCoroutine(AudioManager.instance.PlayBGM(AudioManager.BGM_TYPE.投票時間));
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

                AudioManager.instance.StopBGM();
                timeType = TIME.処刑;
                isDisplay = false;
                totalTime = executionTime;
                Destroy(gameMasterChatManager.destoryedObj);
                //初期化
                isVotingCompleted = false;
                //時短リセット処理
                gameMasterChatManager.timeSaving = false;
                NetworkManager.instance.SetCustomPropertesOfRoom("timeSavingNum", 0);

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
                        if (NetworkManager.instance.GetCustomPropertesOfPlayer<bool>("setSuddenDeath", player)) {
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
                                        playerObj.playerInfoText.text = day + "日目\n\r突然死";

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
                totalTime = checkGameOverTime;

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
                NetworkManager.instance.SetCustomPropertesOfPlayer("setSuddenDeath", setSuddenDeath, PhotonNetwork.LocalPlayer);

                //生存者数を取得
                gameManager.liveNum = NetworkManager.instance.GetCustomPropertesOfRoom<int>("liveNum");
                if (!DebugManager.instance.isGameOver) {
                    gameOver.CheckGameOver();
                }
                break;

            //夜の行動
            case TIME.処刑後チェック:
                timeType = TIME.夜の行動;
                StartCoroutine(AudioManager.instance.PlayBGM(AudioManager.BGM_TYPE.夜の行動));

                //狼ならチャット開放する
                if (chatSystem.myPlayer.wolfChat) {
                    inputField.interactable = true;
                    inputView.wolfMode = true;
                    inputView.wolfModeButtonText.text = "狼";
                    inputView.wolfModeButton.interactable = false;
                }

                //死亡している場合時短or退出ボタンを退出にする
                if (!chatSystem.myPlayer.live) {
                    gameManager.gameMasterChatManager.timeSavingButtonText.text = "退出";
                    gameManager.gameMasterChatManager.timeSavingButton.interactable = true;
                }

                isDisplay = true;
                totalTime = nightTime;
                ChangeSecene();

                //初期化
                if (PhotonNetwork.IsMasterClient) {
                    voteCount.mostVotes = 0;
                    voteCount.ExecutionPlayerList.Clear();
                }

                    
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

                AudioManager.instance.StopBGM();
                Destroy(gameMasterChatManager.destoryedObj);
                //狼チャットできる人だけチャットfalseにする
                if (chatSystem.myPlayer.wolfChat) {
                    inputField.interactable = false;
                }

                isDisplay = false;
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
                isDisplay = false;
                totalTime = checkGameOverTime;

                //初期化噛みと守り先の初期化
                if (PhotonNetwork.IsMasterClient) {
                 
                    gameManager.gameMasterChatManager.bitedID = 9999;
                    gameManager.gameMasterChatManager.protectedID = 9999;
                    NetworkManager.instance.SetCustomPropertesOfRoom("protectedID", gameManager.gameMasterChatManager.protectedID);
                    NetworkManager.instance.SetCustomPropertesOfRoom("bitedID", gameManager.gameMasterChatManager.bitedID);
                    //gameManager.gameMasterChatManager.SetProtectedPlayerID();
                    //gameManager.gameMasterChatManager.SetBitedPlayerID();
                }

                if (!DebugManager.instance.isGameOver) {
                    gameOver.CheckGameOver();
                }
                DeathPlayer();
                break;
        }
        StartCoroutine(EndInterval());
    }

    /// <summary>
    /// 昼、夜、投票後にあるインターバル時間を設定
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndInterval() {

        //役職に合わせてボタンなどを変更する
        TimesavingControllerTrue();

        //SetEndIntervalPassCount(true);
        NetworkManager.instance.SetCustomPropertesOfPlayer("endIntervalPass",true,PhotonNetwork.LocalPlayer);
        if(PhotonNetwork.IsMasterClient) {
            yield return  new WaitUntil (() => PhotonNetwork.PlayerList.Length == GetEndIntervalPassCount()) ;
            intervalState = false;
            //SetIntervalState();
            NetworkManager.instance.SetCustomPropertesOfRoom("intervalState", false);
        }

        //Debug用　人数が一人の時UpDateの処理に問題があるので1秒待つ
        if (DebugManager.instance.numLimit == 1) {
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitUntil(() => !NetworkManager.instance.GetCustomPropertesOfRoom<bool>("intervalState"));
        intervalState = NetworkManager.instance.GetCustomPropertesOfRoom<bool>("intervalState");

        //マスターだけTimeTypeをセットする
        if (PhotonNetwork.IsMasterClient) {
            NetworkManager.instance.SetCustomPropertesOfRoom("timeType",timeType);
        }
        NetworkManager.instance.SetCustomPropertesOfPlayer("endIntervalPass", false, PhotonNetwork.LocalPlayer);
        //SetEndIntervalPassCount(false);
        isNextInterval = false;
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
        yield return new WaitForSeconds(intervalTime + 0.1f);
        day++;
        chatSystem.id++;
        
        NextDay dayObj = Instantiate(dayPrefab, chatContent.transform, false);
        dayObj.day = day;
        dayObj.nextDayText.text = day + "日目";
        dayOrderButton.nextDaysList.Add(dayObj.gameObject);
        ChatData chatData = new ChatData("", 7777, 7777, SPEAKER_TYPE.NULL.ToString(), ROLLTYPE.ETC,7777);
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
        Sequence sequence = DOTween.Sequence();
        sequence.Append(timeContollerPopUpObj.canvasGroup.DOFade(1, 1.0f));
        sequence.AppendInterval(1.0f);
        sequence.Append(timeContollerPopUpObj.canvasGroup.DOFade(0, 1.0f))
            .OnComplete(() => {
                Destroy(timeContollerPopUpObj.gameObject);
            });
    }
    

    /// <summary>
    /// InPutViewを上げます
    /// </summary>
    /// <returns></returns>
    public IEnumerator UpInputView() {
        inputView.foldingButton.interactable = false;
        yield return new WaitForSeconds(intervalTime + 0.3f);
        inputView.inputRectTransform.DOLocalMoveY(0, 0.5f);
        inputView.viewport.DOSizeDelta(new Vector2(202f, 258f), 0.5f);
        inputView.menberViewPopUpObj.SetActive(true);
        inputView.folding = false;
        inputView.foldingImage.sprite = inputView.downBtnSprite;
    }

    /// <summary>
    /// InPutViewを下げます
    /// </summary>
    /// <returns></returns>
    public IEnumerator DownInputView() {
        inputView.foldingButton.interactable = true;
        yield return new WaitForSeconds(intervalTime + 0.3f);
        inputView.inputRectTransform.DOLocalMoveY(-70, 0.5f);
        inputView.viewport.DOSizeDelta(new Vector2(202f, 330f), 0.5f);
        StartCoroutine(inputView.PopUpFalse());
        inputView.folding = true;
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
    }

    /// <summary>
    /// 狼チャットを打つことができないプレイヤーは
    /// </summary>
    public void FalseInputViewButton() {
        wolfButton.interactable = false;
        inputField.interactable = false;
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
                inputView.wolfBtnImage.color = inputView.btnColor[0];

            }
        }
    }

    ///////////////////////////////
    ///カスタムプロパティ関連
    ///////////////////////////////

    int GetEndIntervalPassCount() {
        int endIntervalPassCount = 0;

        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if (player.CustomProperties.TryGetValue("endIntervalPass", out object endIntervalPassObj)) {
                bool endIntervalPass = (bool)endIntervalPassObj;
                if (endIntervalPass) {
                    endIntervalPassCount++;
                }
            }
        }
        return endIntervalPassCount;
    }

    ///// <summary>
    ///// gamereadyを取得する
    ///// </summary>
    ///// <returns></returns>
    //private bool GetGameReady() {
    //    gameReady = false;
    //    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameReady", out object gameReadyObj)) {
    //        gameReady = (bool)gameReadyObj;
    //    }
    //    return gameReady;
    //}

    ///// <summary>
    ///// ゲーム中の時間のオンライン化
    ///// </summary>
    //public void SetGameTime() {
    //    ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
    //        {"totalTime", totalTime }
    //    };
    //    PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    //}


    ///// <summary>
    ///// トータルタイムを取得する
    ///// </summary>
    ///// <returns></returns>
    //private float GetGameTime() {
    //    float time = 0;
    //    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("totalTime", out object totalTimeObj)) {
    //        time = (float)totalTimeObj;
    //    }
    //    return time;
    //}

    ///// <summary>
    ///// 次のインターバルへ以降するフラグをセットする
    ///// </summary>
    //void SetIntervalState() {
    //    ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
    //        {"intervalState", intervalState }
    //    };
    //    PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
    //}

    ///// <summary>
    ///// 次のインターバルへ以降するフラグを受け取る
    ///// </summary>
    //bool GetIntervalState() {
    //    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("intervalState", out object intervalStateObj)) {
    //        intervalState = (bool)intervalStateObj;
    //    }
    //    return intervalState;
    //}

    //void SetEndIntervalPassCount(bool endIntervalPass) {
    //    ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable {
    //        {"endIntervalPass", endIntervalPass }
    //    };
    //    PhotonNetwork.LocalPlayer.SetCustomProperties(customRoomProperties);
    //}

    ///// <summary>
    ///// 全員が投票完了したらtrue
    ///// </summary>
    //public bool GetIsVotingCompleted() {
    //    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("isVotingCompleted", out object isVotingCompletedObj)) {
    //        isVotingCompleted = (bool)isVotingCompletedObj;
    //    }
    //    return isVotingCompleted;
    //}

    //public void SetIsVotingCompleted() {
    //    var propertis = new ExitGames.Client.Photon.Hashtable {
    //        {"isVotingCompleted",isVotingCompleted }
    //    };
    //    PhotonNetwork.CurrentRoom.SetCustomProperties(propertis);
    //}

    ///// <summary>
    ///// 突然死したらフラグを付ける
    ///// </summary>
    //public void SetSuddenDeath() {
    //    var properties = new ExitGames.Client.Photon.Hashtable {
    //        {"setSuddenDeath",setSuddenDeath},
    //    };
    //    PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    //}

    ///// <summary>
    ///// 突然死したプレイヤーのフラグをもらう
    ///// </summary>
    ///// <returns></returns>
    //private bool GetSuddenDeath(Photon.Realtime.Player player) {
    //    if (player.CustomProperties.TryGetValue("setSuddenDeath", out object setSuddenDeathObj)) {
    //        setSuddenDeath = (bool)setSuddenDeathObj;
    //    }
    //    return setSuddenDeath;
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //void SetTimeType() {
    //    var properties = new ExitGames.Client.Photon.Hashtable {
    //        {"timeType",timeType},
    //    };
    //    PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    //}

    ///// <summary>
    ///// 突然死したプレイヤーのフラグをもらう
    ///// </summary>
    ///// <returns></returns>
    //TIME GetTimeType() {
    //    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("timeType", out object timeTypeObj)) {
    //        timeType = (TIME)Enum.Parse(typeof(TIME), timeTypeObj.ToString());
    //    }
    //    return timeType;
    //}
}


