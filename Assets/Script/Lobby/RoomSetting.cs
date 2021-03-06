using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 部屋ルールの設定（ロールは別にある
/// </summary>
public class RoomSetting : MonoBehaviour
{

    //class
    public RollSetting rollSetting;
    //main
    private int mainTime = 400;//お昼の時間
    private int nightTime = 30;//夜の時間

    public VOTING openVoting;//投票開示するか否か
    public FORTUNETYPE fortuneType;
    public SUDDENDEATH_TYPE suddenDeath_Type;
    public ROOMSELECTION roomSelection;//部屋の難易度

    private string title;//部屋のタイトル
    public Text firstDayFrotuneText;//各設定のテキスト
    public Text roomLevelText;
    public Text mainTimeText;
    public Text nightTimeText;
    public Text openVotingText;//各設定のテキスト
    public Text suddenDeathText;
    public InputField titleInputField;//タイトル入力部分
    public RoomNode RoomNodePrefab;//RoomNodePrefab
    public GameObject content;//Lobby画面のScrollViewについているコンテント
    public GameObject rollSettingCanvas;//部屋選択画面Canvas
    public GameObject roomSettingCanvas;//ルーム設定Canvas
    public Button createRoomButton;//部屋作成用ボタン
    public Button backButton;
 

    [System.Serializable]
    public class RoomSettingButton {
        public Button rightButton;
        public Button leftButton;
    }

    public RoomSettingButton suddenDeathButton;
    public RoomSettingButton roomLevelButton;
    public RoomSettingButton firstFortuneButton;
    public RoomSettingButton mainTimeButton;
    public RoomSettingButton nightTimeButton;
    public RoomSettingButton openVotingButton;

    private void Start() {

        //Lobby入室確認
        if (NetworkManager.instance.isCheckJoinLobby) {
            createRoomButton.interactable = true;
        }

        //突然死数を見て設定を設ける
        if(PlayerManager.instance.totalNumberOfMatches < 25 && PlayerManager.instance.totalNumberOfSuddenDeath == 0) {
            suddenDeath_Type = SUDDENDEATH_TYPE._制限なし;
            suddenDeathText.text = SUDDENDEATH_TYPE._制限なし.ToString().Trim('_');
        } else if(PlayerManager.instance.totalNumberOfSuddenDeath == 0) {
            suddenDeath_Type = SUDDENDEATH_TYPE._0回;
            suddenDeathText.text = SUDDENDEATH_TYPE._0回.ToString().Trim('_');
        }else if (PlayerManager.instance.totalNumberOfSuddenDeath == 1) {
            suddenDeath_Type = SUDDENDEATH_TYPE._1回以下;
            suddenDeathText.text = SUDDENDEATH_TYPE._1回以下.ToString().Trim('_');
        } else {
            suddenDeath_Type = SUDDENDEATH_TYPE._2回以上;
            suddenDeathText.text = SUDDENDEATH_TYPE._2回以上.ToString().Trim('_');
        }


        firstDayFrotuneText.text = fortuneType.ToString();
        roomLevelText.text = roomSelection.ToString();
        mainTimeText.text = mainTime.ToString();
        nightTimeText.text = nightTime.ToString();
        openVotingText.text = openVoting.ToString();

        //ボタン設定
        nightTimeButton.leftButton.interactable = false;//夜の時間が最低値なのでfalseで制御

        createRoomButton.onClick.AddListener(CreateRoomNode);

        suddenDeathButton.rightButton.onClick.AddListener(SuddenDeathLimitRight);
        suddenDeathButton.leftButton.onClick.AddListener(SuddenDeathLimitLeft);
        roomLevelButton.rightButton.onClick.AddListener(DifficultySelectionRight);
        roomLevelButton.leftButton.onClick.AddListener(DifficultySelectionLeft);
        firstFortuneButton.rightButton.onClick.AddListener(FirstDayFortuneRight);
        firstFortuneButton.leftButton.onClick.AddListener(FirstDayFortuneLeft);
        mainTimeButton.rightButton.onClick.AddListener(MainTimeRight);
        mainTimeButton.leftButton.onClick.AddListener(MainTimeLeft);
        nightTimeButton.rightButton.onClick.AddListener(NightTimeRight);
        nightTimeButton.leftButton.onClick.AddListener(NightTimeLeft);
        openVotingButton.rightButton.onClick.AddListener(OpenVoting);
        openVotingButton.leftButton.onClick.AddListener(OpenVoting);
        backButton.onClick.AddListener(BackButton);
    }



    //部屋作成
    /// <summary>
    /// 部屋作成ボタンの制御,部屋設定終了後次へのボタン
    /// </summary>
    public void CreateRoomNode() {


        //タイトルも字数制限を監視
        if (titleInputField.text.Length >= 13) {
            rollSetting.wrongPopUpObj.SetActive(true);
            rollSetting.wrongPopUp.wrongText.text = "タイトルの文字数が多すぎます。";
            return;
        }

        if (titleInputField.text.Trim().Length < 1) {

            if (roomSelection == ROOMSELECTION.初心者) {
                titleInputField.text = "初心者歓迎！";

            } else {
                Debug.Log("一般");

                titleInputField.text = "一般部屋";
            }
        }
        title = titleInputField.text;
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);

        //部屋をインスタンスと同時に部屋情報を渡す。
        RoomNode room = Instantiate(RoomNodePrefab, content.transform, false);
        //と同時に部屋情報を渡す。
        if (DebugManager.instance.isTimeController) {
            openVoting = DebugManager.instance.openVoting;
            fortuneType = DebugManager.instance.fortuneType;
        }
        room_information roomInfo = new room_information(openVoting, title, fortuneType, mainTime, nightTime, roomSelection, suddenDeath_Type);
        room.InitRoomNode(roomInfo, rollSetting.NumList,rollSetting.numLimit);


        //RoomDataにデータ保存
        RoomData.instance.roomInfo = roomInfo;
        RoomData.instance.rollList = rollSetting.NumList;

        if (DebugManager.instance.isTimeController) {
            RoomData.instance.numLimit = DebugManager.instance.numLimit;
        } else {
            RoomData.instance.numLimit = rollSetting.numLimit;
        }
        

        //一旦SetActive（false);にしておく→更新や、難易度変更で後程制御
        room.gameObject.SetActive(false);


        //サーバーに部屋情報を渡す。
        NetworkManager.instance.PreparateCreateRoom(RoomData.instance.numLimit, room);

    }


    /// <summary>
    /// 突然死数制限する 右
    /// </summary>
    public void SuddenDeathLimitRight() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);

        if (PlayerManager.instance.totalNumberOfSuddenDeath == 0 && PlayerManager.instance.totalNumberOfMatches >= 25) {
            Debug.Log("0ban");
            switch (suddenDeath_Type) {
                case SUDDENDEATH_TYPE._0回:
                    suddenDeath_Type = SUDDENDEATH_TYPE._1回以下;
                    break;
                case SUDDENDEATH_TYPE._1回以下:
                    suddenDeath_Type = SUDDENDEATH_TYPE._2回以上;
                    break;
                case SUDDENDEATH_TYPE._2回以上:
                    suddenDeath_Type = SUDDENDEATH_TYPE._制限なし;
                    break;
                case SUDDENDEATH_TYPE._制限なし:
                    suddenDeath_Type = SUDDENDEATH_TYPE._0回;
                    break;

                
                
            } 
            
        } else if (PlayerManager.instance.totalNumberOfSuddenDeath == 1) {

            if(PlayerManager.instance.totalNumberOfMatches >= 25) {
                switch (suddenDeath_Type) {

                    case SUDDENDEATH_TYPE._1回以下:
                        suddenDeath_Type = SUDDENDEATH_TYPE._制限なし;
                        break;
                    case SUDDENDEATH_TYPE._制限なし:
                        suddenDeath_Type = SUDDENDEATH_TYPE._2回以上;
                        break;
                    case SUDDENDEATH_TYPE._2回以上:
                        suddenDeath_Type = SUDDENDEATH_TYPE._1回以下;
                        break;

                }
            } else {
                switch (suddenDeath_Type) {
                    case SUDDENDEATH_TYPE._1回以下:
                        suddenDeath_Type = SUDDENDEATH_TYPE._2回以上;
                        break;
                    case SUDDENDEATH_TYPE._2回以上:
                        suddenDeath_Type = SUDDENDEATH_TYPE._1回以下;
                        break;
                }
            }
        } else {
            return;
        }
        suddenDeathText.text = suddenDeath_Type.ToString().Trim('_');
    }

    /// <summary>
    /// 突然死数制限する 左
    /// </summary>
    public void SuddenDeathLimitLeft() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        if (PlayerManager.instance.totalNumberOfSuddenDeath == 0 && PlayerManager.instance.totalNumberOfMatches >= 25) {
            switch (suddenDeath_Type) {
                case SUDDENDEATH_TYPE._0回:
                    suddenDeath_Type = SUDDENDEATH_TYPE._制限なし;
                    break;
                case SUDDENDEATH_TYPE._制限なし:
                    suddenDeath_Type = SUDDENDEATH_TYPE._2回以上;
                    break;
                case SUDDENDEATH_TYPE._2回以上:
                    suddenDeath_Type = SUDDENDEATH_TYPE._1回以下;
                    break;
                case SUDDENDEATH_TYPE._1回以下:
                    suddenDeath_Type = SUDDENDEATH_TYPE._0回;
                    break;
            }
        } else if (PlayerManager.instance.totalNumberOfSuddenDeath == 1) {

            if(PlayerManager.instance.totalNumberOfMatches >= 25) {
                switch (suddenDeath_Type) {

                    case SUDDENDEATH_TYPE._制限なし:
                        suddenDeath_Type = SUDDENDEATH_TYPE._1回以下;
                        break;
                    case SUDDENDEATH_TYPE._1回以下:
                        suddenDeath_Type = SUDDENDEATH_TYPE._2回以上;
                        break;
                    case SUDDENDEATH_TYPE._2回以上:
                        suddenDeath_Type = SUDDENDEATH_TYPE._制限なし;
                        break;


                }
            }else {
                switch (suddenDeath_Type) {

                    case SUDDENDEATH_TYPE._2回以上:
                        suddenDeath_Type = SUDDENDEATH_TYPE._1回以下;
                        break;
                    case SUDDENDEATH_TYPE._1回以下:
                        suddenDeath_Type = SUDDENDEATH_TYPE._2回以上;
                        break;

                }
            }
        } else {
            return;
        }
        suddenDeathText.text = suddenDeath_Type.ToString().Trim('_');
    }

    /// <summary>
    /// 初日占いの設定　右
    /// </summary>
    public void FirstDayFortuneRight() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        switch (fortuneType) {
            case FORTUNETYPE.ランダム白:
                fortuneType = FORTUNETYPE.あり;
                break;
            case FORTUNETYPE.あり:
                fortuneType = FORTUNETYPE.なし;
                break;
            case FORTUNETYPE.なし:
                fortuneType = FORTUNETYPE.ランダム白;
                break;
        }
        firstDayFrotuneText.text = fortuneType.ToString();
    }

    /// <summary>
    /// 初日占いの設定　左
    /// </summary>
    public void FirstDayFortuneLeft() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        switch (fortuneType) {
            case FORTUNETYPE.ランダム白:
                fortuneType = FORTUNETYPE.なし;
                break;
            case FORTUNETYPE.なし:
                fortuneType = FORTUNETYPE.あり;
                break;
            case FORTUNETYPE.あり:
                fortuneType = FORTUNETYPE.ランダム白;
                break;
        }
        firstDayFrotuneText.text = fortuneType.ToString();
    }

    /// <summary>
    /// 募集条件　右
    /// </summary>
    public void DifficultySelectionRight() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        switch (roomSelection) {
            case ROOMSELECTION.初心者:
                roomSelection = ROOMSELECTION.一般;
                break;
            case ROOMSELECTION.一般:
                roomSelection = ROOMSELECTION.初心者;
                break;
        }
        roomLevelText.text = roomSelection.ToString();
    }

    /// <summary>
    /// 募集条件　左
    /// </summary>
    public void DifficultySelectionLeft() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        switch (roomSelection) {
            case ROOMSELECTION.初心者:
                roomSelection = ROOMSELECTION.一般;
                break;
            case ROOMSELECTION.一般:
                roomSelection = ROOMSELECTION.初心者;
                break;
        }
        roomLevelText.text = roomSelection.ToString();
    }

    /// <summary>
    /// 昼の時間設定　右
    /// </summary>
    public void MainTimeRight() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        mainTime = mainTime + 100;
        switch (mainTime) {
            case 400:
                mainTimeButton.leftButton.interactable = true;
                break;
            case 900:
                mainTimeButton.rightButton.interactable = false;
                break;
        }
        mainTimeText.text = mainTime.ToString();
    }

    /// <summary>
    /// 昼の時間設定　左
    /// </summary>
    public void MainTimeLeft() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        mainTime = mainTime - 100;
        switch (mainTime) {
            case 300:
                mainTimeButton.leftButton.interactable = false;
                break;
            case 800:
                mainTimeButton.rightButton.interactable = true;
                break;
        }
        mainTimeText.text = mainTime.ToString();
    }

    /// <summary>
    /// 夜の時間設定　右
    /// </summary>
    public void NightTimeRight() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        nightTime = nightTime + 30;
        switch (nightTime) {
            case 60:
                nightTimeButton.leftButton.interactable = true;
                break;
            case 90:
                nightTimeButton.rightButton.interactable = false;
                break;
        }
        nightTimeText.text = nightTime.ToString();
    }


    /// <summary>
    /// 夜の時間設定　左
    /// </summary>
    public void NightTimeLeft() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        nightTime = nightTime - 30;
        switch (nightTime) {
            case 30:
                nightTimeButton.leftButton.interactable = false;
                break;
            case 60:
                nightTimeButton.rightButton.interactable = true;
                break;
        }
        nightTimeText.text = nightTime.ToString();
    }

    /// <summary>
    /// 投票開示設定　右
    /// </summary>
    public void OpenVoting() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        switch (openVoting) {
            case VOTING.開示しない:
                openVoting = VOTING.開示する;
                break;
            case VOTING.開示する:
                openVoting = VOTING.開示しない;
                break;
        }
        openVotingText.text = openVoting.ToString();
    }

    public void BackButton() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.NG);
        rollSettingCanvas.SetActive(true);
        roomSettingCanvas.gameObject.SetActive(false);
    }

}

[System.Serializable]
public class room_information {
    public VOTING openVoting;
    public string title;
    public FORTUNETYPE fortuneType;
    public int mainTime;
    public int nightTime;
    public ROOMSELECTION roomSelection;
    public SUDDENDEATH_TYPE suddenDeath_Type;

    public room_information(VOTING openVoting, string title, FORTUNETYPE fortuneType,int mainTime,int nightTime, ROOMSELECTION roomSelection, SUDDENDEATH_TYPE suddenDeath_Type) {
        this.openVoting = openVoting;
        this.title = title;
        this.fortuneType = fortuneType;
        this.mainTime = mainTime;
        this.nightTime = nightTime;
        this.roomSelection = roomSelection;
        this.suddenDeath_Type = suddenDeath_Type;
    }
}