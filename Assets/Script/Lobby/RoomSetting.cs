using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 部屋ルールの設定（ロールは別にある
/// </summary>
public class RoomSetting : MonoBehaviour
{

    //class
    public FORTUNETYPE fortuneType;
    public RoomNode roomNode;
    public RollSetting rollSetting;
    public ROOMSELECTION roomSelection;//部屋の難易度
    //main
    public string firstDayFortune;
    public int mainTime;//お昼の時間
    public int nightTime;//夜の時間
    public VOTING openVoting;//投票開示するか否か
    public string title;//部屋のタイトル
    public Text firstDayFrotuneText;//各設定のテキスト
    public Text roomLevelText;
    public Text mainTimeText;
    public Text nightTimeText;
    public Text openVotingText;//各設定のテキスト
    public InputField titleText;//タイトル入力部分
    public RoomNode RoomNodePrefab;//RoomNodePrefab
    public GameObject content;//Lobby画面のScrollViewについているコンテント
    public GameObject roomSelectCanvas;//部屋選択画面Canvas
    public GameObject roomSettingCanvas;//ルーム設定Canvas
    public List<RoomNode> roomNodeList = new List<RoomNode>();//検索用に用意したRoomNodeList
    public Button createRoomButton;//部屋作成用ボタン
    public Button backButton;
 

    [System.Serializable]
    public class RoomSettingButton {
        public Button rightButton;
        public Button leftButton;
    }

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


        firstDayFrotuneText.text = firstDayFortune;
        roomLevelText.text = roomSelection.ToString();
        mainTimeText.text = mainTime.ToString();
        nightTimeText.text = nightTime.ToString();
        openVotingText.text = openVoting.ToString();

        //ボタン設定
        nightTimeButton.leftButton.interactable = false;//夜の時間が最低値なのでfalseで制御

        createRoomButton.onClick.AddListener(CreateRoomNode);
        
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

        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);

        //タイトルも字数制限を監視
        if (titleText.text.Length >= 13) {
            rollSetting.wrongPopUpObj.SetActive(true);
            rollSetting.wrongPopUp.wrongText.text = "タイトルの文字数が多すぎます。";
            return;
        }
        titleText.text = "";

        //部屋をインスタンスと同時に部屋情報を渡す。
        RoomNode room = Instantiate(RoomNodePrefab, content.transform, false);
        //と同時に部屋情報を渡す。
        if (DebugManager.instance.isTimeController) {
            openVoting = DebugManager.instance.openVoting;
            fortuneType = DebugManager.instance.fortuneType;
        }
        RoomInfo roomInfo = new RoomInfo(openVoting, title, fortuneType, mainTime, nightTime, roomSelection);
        room.InitRoomNode(roomInfo, rollSetting.NumList,rollSetting.numLimit);

        //List管理
        roomNodeList.Add(room);

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
    /// タイトルが無いときは別のテキストを入れる
    /// </summary>
    public void SettingTitle() {
        if(titleText.text.Trim().Length < 1) {
            titleText.text = "初心者歓迎！";
        }
        title = titleText.text;
    }

    /// <summary>
    /// 初日占いの設定　右
    /// </summary>
    public void FirstDayFortuneRight() {
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

        roomSettingCanvas.SetActive(false);
        roomSelectCanvas.SetActive(true);

    }

}

[System.Serializable]
public class RoomInfo {
    public VOTING openVoting;
    public string title;
    public FORTUNETYPE fortuneType;
    public int mainTime;
    public int nightTime;
    public ROOMSELECTION roomSelection;

    public RoomInfo(VOTING openVoting, string title, FORTUNETYPE fortuneType,int mainTime,int nightTime, ROOMSELECTION roomSelection) {
        this.openVoting = openVoting;
        this.title = title;
        this.fortuneType = fortuneType;
        this.mainTime = mainTime;
        this.nightTime = nightTime;
        this.roomSelection = roomSelection;
    }
}