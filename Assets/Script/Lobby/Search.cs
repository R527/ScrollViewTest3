using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 部屋検索、部屋を設定ごとに絞る
/// </summary>
public class Search : MonoBehaviour
{
    //class
    public RoomSetting roomSetting;
    public FORTUNETYPE searchFortuneType;
    public ROOMSELECTION searchRoomSelection;
    public VOTING searchOpenVoting;

    //Lobby
    public Text roomSelectionText;

    //検索
    public Text firstDayFrotuneText;
    public Text openVotingText;
    public Text joinNumText;
    public Text searchRoomSelectText;
    public GameObject searchPopUpObj;
    public Button searchPopUPButton;
    public Button selectionButtonLeftButton;
    public Button selectionButtonRightButton;
    public Button searchJoinPlusButton;
    public Button searchJoinMinusButton;
    public Button initSearchButton;//検索初期化
    public Button upDateButton;//更新ボタン
    public int searchJoinNum;
    public bool join;//参加人数未設定か否かの判定


    private void Start() {
        roomSelectionText.text = searchRoomSelection.ToString();
        firstDayFrotuneText.text = searchFortuneType.ToString();
        openVotingText.text = searchOpenVoting.ToString();
        searchRoomSelectText.text = searchRoomSelection.ToString();
        joinNumText.text = "未設定";

        //button
        initSearchButton.onClick.AddListener(InitSearch);
        upDateButton.onClick.AddListener(UpDateButton);
        searchPopUPButton.onClick.AddListener(SearchPopUP);
        selectionButtonLeftButton.onClick.AddListener(SelectionButtonLeft);
        selectionButtonRightButton.onClick.AddListener(SelectionButtonRight);
    }

    /// <summary>
    /// 部屋検索
    /// </summary>
    public void SearchRoomNode() {
        //一度falseに
        foreach (RoomNode roomObj in roomSetting.roomNodeList) {
            roomObj.gameObject.SetActive(false);
        }


        foreach (RoomNode roomObj in roomSetting.roomNodeList) {

            //未設定を処理する
            FORTUNETYPE lastSearchFortuneType = searchFortuneType;
            VOTING lastSearchOpenVoting = searchOpenVoting;
            int lastSearchJoinNum = searchJoinNum;
            if (searchFortuneType == FORTUNETYPE.未設定) {
                searchFortuneType = roomObj.fortuneType;
            }
            if (searchOpenVoting == VOTING.未設定) {
                searchOpenVoting = roomObj.openVoting;
            }

            //フィルターにかける
            //人数設定が未設定
            if(join == true) {
                if (roomObj.fortuneType == searchFortuneType && roomObj.openVoting == searchOpenVoting &&  roomObj.settingNum > searchJoinNum　&& searchRoomSelection == roomObj.roomSelection) {
                    roomObj.gameObject.SetActive(true);
                }
            //人数設定が設定されている場合
            } else {
                if (roomObj.fortuneType == searchFortuneType && roomObj.openVoting == searchOpenVoting && roomObj.settingNum == searchJoinNum && searchRoomSelection == roomObj.roomSelection) {
                    roomObj.gameObject.SetActive(true);
                }
            }

            //未設定処理初期化
            searchFortuneType = lastSearchFortuneType;
            searchOpenVoting = lastSearchOpenVoting;
        }
    }

    //Lobby上部の難易度変更
    /// <summary>
    /// 難易度選択ボタン 右
    /// </summary>
    public void SelectionButtonRight() {
        switch (searchRoomSelection) {
            case ROOMSELECTION.初心者:
                searchRoomSelection = ROOMSELECTION.一般;
                searchRoomSelectText.text = searchRoomSelection.ToString();
                SearchRoomNode();
                break;
            case ROOMSELECTION.一般:
                searchRoomSelection = ROOMSELECTION.観戦; 
                searchRoomSelectText.text = searchRoomSelection.ToString();
                SearchRoomNode();
                break;
            case ROOMSELECTION.観戦:
                searchRoomSelection = ROOMSELECTION.初心者;
                searchRoomSelectText.text = searchRoomSelection.ToString();
                SearchRoomNode();
                break;
        }
        roomSelectionText.text = searchRoomSelection.ToString();
    }

    /// <summary>
    /// 難易度選択ボタン 右
    /// </summary>
    public void SelectionButtonLeft() {
        switch (searchRoomSelection) {
            case ROOMSELECTION.初心者:
                searchRoomSelection = ROOMSELECTION.観戦;
                searchRoomSelectText.text = searchRoomSelection.ToString();
                SearchRoomNode();
                break;
            case ROOMSELECTION.観戦:
                searchRoomSelection = ROOMSELECTION.一般;
                searchRoomSelectText.text = searchRoomSelection.ToString();
                SearchRoomNode();
                break;
            case ROOMSELECTION.一般:
                searchRoomSelection = ROOMSELECTION.初心者;
                searchRoomSelectText.text = searchRoomSelection.ToString();
                SearchRoomNode();
                break;
        }
        roomSelectionText.text = searchRoomSelection.ToString();
    }

    /// <summary>
    /// 更新ボタン　部屋情報を更新する
    /// </summary>
    public void UpDateButton() {
        SearchRoomNode();
    }

    /// <summary>
    /// 検索PopUpアクティブ化
    /// </summary>
    public void SearchPopUP() {
        searchPopUpObj.SetActive(true);
    }

    /// <summary>
    /// 検索初期化
    /// </summary>
    public void InitSearch() {
        searchFortuneType　= FORTUNETYPE.未設定;
        searchOpenVoting = VOTING.未設定;
        searchJoinNum = 3;
        joinNumText.text = "未設定";
        openVotingText.text = "未設定";
        firstDayFrotuneText.text = "未設定";
        join = true;
    }
    /// <summary>
    /// 募集条件
    /// </summary>
    public void DifficultySelectionRight() {
        switch (searchRoomSelection) {
            case ROOMSELECTION.観戦:
                searchRoomSelection = ROOMSELECTION.初心者;
                break;
            case ROOMSELECTION.初心者:
                searchRoomSelection = ROOMSELECTION.一般;
                break;
            case ROOMSELECTION.一般:
                searchRoomSelection = ROOMSELECTION.観戦;
                break;

        }
        searchRoomSelectText.text = searchRoomSelection.ToString();
        roomSelectionText.text = searchRoomSelection.ToString();
    }

    /// <summary>
    /// 募集条件
    /// </summary>
    public void DifficultySelectionLeft() {
        switch (searchRoomSelection) {
            case ROOMSELECTION.初心者:
                searchRoomSelection = ROOMSELECTION.観戦;
                break;
            case ROOMSELECTION.観戦:
                searchRoomSelection = ROOMSELECTION.一般;
                break;
            case ROOMSELECTION.一般:
                searchRoomSelection = ROOMSELECTION.初心者;
                break;

        }
        roomSelectionText.text = searchRoomSelection.ToString();
        searchRoomSelectText.text = searchRoomSelection.ToString();
    }
    /// <summary>
    /// 初日占い右
    /// </summary>
    public void FirstDayFortuneRight() {
        switch (searchFortuneType) {
            case FORTUNETYPE.ランダム白:
                searchFortuneType = FORTUNETYPE.あり;
                break;
            case FORTUNETYPE.あり:
                searchFortuneType = FORTUNETYPE.なし;
                break;
            case FORTUNETYPE.なし:
                searchFortuneType = FORTUNETYPE.未設定;
                break;
            case FORTUNETYPE.未設定:
                searchFortuneType = FORTUNETYPE.ランダム白;
                break;
        }
        firstDayFrotuneText.text = searchFortuneType.ToString();
    }
    /// <summary>
    /// 初日占い　左
    /// </summary>
    public void FirstDayFortuneLeft() {
        switch (searchFortuneType) {
            case FORTUNETYPE.ランダム白:
                searchFortuneType = FORTUNETYPE.未設定;
                break;
            case FORTUNETYPE.未設定:
                searchFortuneType = FORTUNETYPE.なし;
                break;
            case FORTUNETYPE.なし:
                searchFortuneType = FORTUNETYPE.あり;
                break;
            case FORTUNETYPE.あり:
                searchFortuneType = FORTUNETYPE.ランダム白;
                break;
        }
        firstDayFrotuneText.text = searchFortuneType.ToString();
    }

    /// <summary>
    /// 投票開示右
    /// </summary>
    public void OpenVotingRight() {
        switch (searchOpenVoting) {
            case VOTING.開示しない:
                searchOpenVoting = VOTING.開示する;
                break;
            case VOTING.開示する:
                searchOpenVoting = VOTING.未設定;
                break;
            case VOTING.未設定:
                searchOpenVoting = VOTING.開示しない;
                break;
        }
        openVotingText.text = searchOpenVoting.ToString();
    }
    /// <summary>
    /// 投票開示設定　左
    /// </summary>
    public void OpenVotingLeft() {
        switch (searchOpenVoting) {
            case VOTING.開示する:
                searchOpenVoting = VOTING.開示しない;
                break;
            case VOTING.開示しない:
                searchOpenVoting = VOTING.未設定;
                break;
            case VOTING.未設定:
                searchOpenVoting = VOTING.開示する;
                break;
        }
        openVotingText.text = searchOpenVoting.ToString();
    }
    /// <summary>
    /// 人数設定　プラスボタン
    /// </summary>
    public void SumNumberPlusButton() {
        searchJoinNum++;
        if(searchJoinNum == 3) {
            joinNumText.text = "未設定";
            join = true;
        } else {
            joinNumText.text = searchJoinNum.ToString();
            join = false;
        }

        if (searchJoinNum == 15) {
            searchJoinPlusButton.interactable = false;
        } else if (searchJoinNum == 4) {
            searchJoinMinusButton.interactable = true;
        }
    }

    /// <summary>
    /// 人数設定　マイナスボタン
    /// </summary>
    public void SumNumberMinusButton() {
        searchJoinNum--;
        if (searchJoinNum == 3) {
            joinNumText.text = "未設定";
            join = true;
        } else {
            joinNumText.text = searchJoinNum.ToString();
            join = false;
        }

        if (searchJoinNum == 14) {
            searchJoinPlusButton.interactable = true;
        } else if (searchJoinNum == 3) {
            searchJoinMinusButton.interactable = false;
        }
    }
}
