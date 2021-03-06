using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;



/// <summary>
/// 部屋検索、部屋を設定ごとに絞る
/// </summary>
public class Search : MonoBehaviour {
    //class
    public FORTUNETYPE searchFortuneType;
    public ROOMSELECTION searchRoomSelection;
    public VOTING searchOpenVoting;
    public SUDDENDEATH_TYPE suddenDeath_Type;

    //Lobby
    public Text roomSelectionText;

    //検索
    public Text firstDayFrotuneText;
    public Text openVotingText;
    public Text joinNumText;
    public Text searchRoomSelectText;
    public Text suddenDeath_TypeText;
    public GameObject searchPopUpObj;


    public Button searchPopUPButton;
    public Button selectionButtonLeftButton;
    public Button selectionButtonRightButton;
    public Button suddenDeath_TypeLeftBtn;
    public Button suddenDeath_TypeRightBtn;
    public Button searchJoinPlusButton;
    public Button searchJoinMinusButton;
    public Button submitSearch;//検索をかけるボタン
    public Button initSearchButton;//検索初期化
    public Button upDateButton;//更新ボタン


    public int searchJoinNum;
    public bool isNumLimit;//参加人数未設定か否かの判定
    private Dictionary<string, RoomNode> activeEntriesList = new Dictionary<string, RoomNode>();
    private bool isSearch;



    private void Start() {
        roomSelectionText.text = searchRoomSelection.ToString();
        firstDayFrotuneText.text = searchFortuneType.ToString();
        openVotingText.text = searchOpenVoting.ToString();
        searchRoomSelectText.text = searchRoomSelection.ToString();
        joinNumText.text = "未設定";
        suddenDeath_TypeText.text = "未設定";

        //button
        initSearchButton.onClick.AddListener(InitSearch);
        //upDateButton.onClick.AddListener(UpDateButton);
        searchPopUPButton.onClick.AddListener(SearchPopUP);
        selectionButtonLeftButton.onClick.AddListener(SelectionButtonLeft);
        selectionButtonRightButton.onClick.AddListener(SelectionButtonRight);
        submitSearch.onClick.AddListener(SearchRoomNode);
        suddenDeath_TypeLeftBtn.onClick.AddListener(SuddenDeath_TypeLeft);
        suddenDeath_TypeRightBtn.onClick.AddListener(SuddenDeath_TypeRight);
    }

    /// <summary>
    /// 部屋検索
    /// </summary>
    public void SearchRoomNode() {
        //更新が終了するまで押せなくする
        if (isSearch) {
            return;
        }
        isSearch = true;
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);


        //Listを取得する
        activeEntriesList = NetworkManager.instance.GetActiveEntries();

        //一度falseに
        foreach (RoomNode Obj in activeEntriesList.Values) {
            Obj.gameObject.SetActive(false);
        }

        foreach (RoomNode Obj in activeEntriesList.Values) {
            //BanListをチェックする
            if(Obj.isCheckBanList == true) {
                continue;
            }

            //未設定を処理する
            FORTUNETYPE lastSearchFortuneType = searchFortuneType;
            VOTING lastSearchOpenVoting = searchOpenVoting;
            int lastSearchJoinNum = searchJoinNum;
            if (searchFortuneType == FORTUNETYPE.未設定) {
                searchFortuneType = Obj.fortuneType;
            }
            if (searchOpenVoting == VOTING.未設定) {
                searchOpenVoting = Obj.openVoting;
            }
            if(suddenDeath_Type == SUDDENDEATH_TYPE._未設定) {
                suddenDeath_Type = Obj.suddenDeath_Type;
            }

            //フィルターにかける
            //人数設定が未設定

            if (isNumLimit == true) {
                if (Obj.fortuneType == searchFortuneType && Obj.openVoting == searchOpenVoting && searchRoomSelection == Obj.roomSelection && CheckSuddenDeath(Obj) && Obj.suddenDeath_Type == suddenDeath_Type) {
                    Obj.gameObject.SetActive(true);
                }
            //人数設定が設定されている場合
            } else {
                if (Obj.fortuneType == searchFortuneType && Obj.openVoting == searchOpenVoting && Obj.settingNum == searchJoinNum && searchRoomSelection == Obj.roomSelection && CheckSuddenDeath(Obj) && Obj.suddenDeath_Type == suddenDeath_Type) {
                    Obj.gameObject.SetActive(true);
                }
            }

            //未設定処理初期化
            searchFortuneType = lastSearchFortuneType;
            searchOpenVoting = lastSearchOpenVoting;
        }

        isSearch = false;
    }

    /// <summary>
    /// 突然死数を見て部屋の表示を決定する
    /// </summary>
    public bool CheckSuddenDeath(RoomNode Obj) {
        bool isCheck = false;

        //凸1回以下　
        //(Obj.suddenDeath_Type == SUDDENDEATH_TYPE._1回以下 || Obj.suddenDeath_Type == SUDDENDEATH_TYPE._2回以上 ||
        if (PlayerManager.instance.totalNumberOfSuddenDeath == 1 &&  Obj.suddenDeath_Type == SUDDENDEATH_TYPE._1回以下 && PlayerManager.instance.totalNumberOfMatches >= 25) {
            isCheck = true;

        //凸2回以上
        } else if (PlayerManager.instance.totalNumberOfSuddenDeath > 1 && Obj.suddenDeath_Type == SUDDENDEATH_TYPE._2回以上) {
            isCheck = true;

        //凸0回以下
        } else if(PlayerManager.instance.totalNumberOfSuddenDeath == 0 && PlayerManager.instance.totalNumberOfMatches >= 25 && Obj.suddenDeath_Type == SUDDENDEATH_TYPE._0回) {
            isCheck = true;

        //新規プレイヤー受け入れ
        }else if(PlayerManager.instance.totalNumberOfSuddenDeath == 0 && Obj.suddenDeath_Type == SUDDENDEATH_TYPE._制限なし) {
            isCheck = true;
        }
        return isCheck;
    }

    //Lobby上部の難易度変更
    /// <summary>
    /// 難易度選択ボタン 右
    /// </summary>
    public void SelectionButtonRight() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);

        switch (searchRoomSelection) {
            case ROOMSELECTION.初心者:
                searchRoomSelection = ROOMSELECTION.一般;
                SearchRoomNode();
                break;
            case ROOMSELECTION.一般:
                searchRoomSelection = ROOMSELECTION.初心者; 
                SearchRoomNode();
                break;
            //case ROOMSELECTION.観戦:
            //    searchRoomSelection = ROOMSELECTION.初心者;
            //    searchRoomSelectText.text = searchRoomSelection.ToString();
            //    SearchRoomNode();
            //    break;
        }
        searchRoomSelectText.text = searchRoomSelection.ToString();

        roomSelectionText.text = searchRoomSelection.ToString();
    }

    /// <summary>
    /// 難易度選択ボタン 右
    /// </summary>
    public void SelectionButtonLeft() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);

        switch (searchRoomSelection) {
            case ROOMSELECTION.初心者:
                searchRoomSelection = ROOMSELECTION.一般;
                SearchRoomNode();
                break;
            //case ROOMSELECTION.観戦:
            //    searchRoomSelection = ROOMSELECTION.一般;
            //    searchRoomSelectText.text = searchRoomSelection.ToString();
            //    SearchRoomNode();
            //    break;
            case ROOMSELECTION.一般:
                searchRoomSelection = ROOMSELECTION.初心者;
                SearchRoomNode();
                break;
        }
        searchRoomSelectText.text = searchRoomSelection.ToString();
        roomSelectionText.text = searchRoomSelection.ToString();
    }

    /// <summary>
    /// 更新ボタン　部屋情報を更新する
    /// </summary>
    public void UpDateButton() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        SearchRoomNode();
    }

    /// <summary>
    /// 検索PopUpアクティブ化
    /// </summary>
    public void SearchPopUP() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);

        searchPopUpObj.SetActive(true);
    }

    /// <summary>
    /// 検索初期化
    /// </summary>
    public void InitSearch() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        searchFortuneType = FORTUNETYPE.未設定;
        searchOpenVoting = VOTING.未設定;
        searchJoinNum = 3;
        joinNumText.text = "未設定";
        openVotingText.text = "未設定";
        firstDayFrotuneText.text = "未設定";
        suddenDeath_TypeText.text = "未設定";
        isNumLimit = true;
    }

    
    /// <summary>
    /// 募集条件
    /// </summary>
    public void DifficultySelectionRight() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        switch (searchRoomSelection) {
            //case ROOMSELECTION.観戦:
            //    searchRoomSelection = ROOMSELECTION.初心者;
            //    break;
            case ROOMSELECTION.初心者:
                searchRoomSelection = ROOMSELECTION.一般;
                break;
            case ROOMSELECTION.一般:
                searchRoomSelection = ROOMSELECTION.初心者;
                break;

        }
        searchRoomSelectText.text = searchRoomSelection.ToString();
        roomSelectionText.text = searchRoomSelection.ToString();
    }

    /// <summary>
    /// 募集条件
    /// </summary>
    public void DifficultySelectionLeft() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        switch (searchRoomSelection) {
            //case ROOMSELECTION.初心者:
            //    searchRoomSelection = ROOMSELECTION.観戦;
            //    break;
            case ROOMSELECTION.初心者:
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
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
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
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
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
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
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
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
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
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        searchJoinNum++;
        if(searchJoinNum == 3) {
            joinNumText.text = "未設定";
            isNumLimit = true;
        } else {
            joinNumText.text = searchJoinNum.ToString();
            isNumLimit = false;
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
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        searchJoinNum--;
        if (searchJoinNum == 3) {
            joinNumText.text = "未設定";
            isNumLimit = true;
        } else {
            joinNumText.text = searchJoinNum.ToString();
            isNumLimit = false;
        }

        if (searchJoinNum == 14) {
            searchJoinPlusButton.interactable = true;
        } else if (searchJoinNum == 3) {
            searchJoinMinusButton.interactable = false;
        }
    }

    /// <summary>
    /// 突然死設定　左
    /// </summary>
    public void SuddenDeath_TypeLeft() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        //凸0回のプレイヤー
        if (PlayerManager.instance.totalNumberOfSuddenDeath == 0 && PlayerManager.instance.totalNumberOfMatches >= 25) {
            switch (suddenDeath_Type) {
                case SUDDENDEATH_TYPE._未設定:
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
                case SUDDENDEATH_TYPE._0回:
                    suddenDeath_Type = SUDDENDEATH_TYPE._未設定;
                    break;
            }

        //凸1回のプレイヤー
        } else if (PlayerManager.instance.totalNumberOfSuddenDeath == 1 && PlayerManager.instance.totalNumberOfMatches >= 25) {
            switch (suddenDeath_Type) {

                case SUDDENDEATH_TYPE._未設定:
                    suddenDeath_Type = SUDDENDEATH_TYPE._制限なし;
                    break;
                case SUDDENDEATH_TYPE._制限なし:
                    suddenDeath_Type = SUDDENDEATH_TYPE._2回以上;
                    break;
                case SUDDENDEATH_TYPE._2回以上:
                    suddenDeath_Type = SUDDENDEATH_TYPE._1回以下;
                    break;
                case SUDDENDEATH_TYPE._1回以下:
                    suddenDeath_Type = SUDDENDEATH_TYPE._未設定;
                    break;
            }
        } else {
            return;
        }
        suddenDeath_TypeText.text = suddenDeath_Type.ToString().Trim('_');
    }
    /// <summary>
    /// 突然死設定　右
    /// </summary>
    public void SuddenDeath_TypeRight() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        //凸0回のプレイヤー
        if (PlayerManager.instance.totalNumberOfSuddenDeath == 0 && PlayerManager.instance.totalNumberOfMatches >= 25) {
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
                    suddenDeath_Type = SUDDENDEATH_TYPE._未設定;
                    break;
                case SUDDENDEATH_TYPE._未設定:
                    suddenDeath_Type = SUDDENDEATH_TYPE._0回;
                    break;
            }

            //凸1回のプレイヤー
        } else if (PlayerManager.instance.totalNumberOfSuddenDeath == 1 && PlayerManager.instance.totalNumberOfMatches >= 25) {
            switch (suddenDeath_Type) {

                case SUDDENDEATH_TYPE._1回以下:
                    suddenDeath_Type = SUDDENDEATH_TYPE._2回以上;
                    break;
                case SUDDENDEATH_TYPE._2回以上:
                    suddenDeath_Type = SUDDENDEATH_TYPE._制限なし;
                    break;
                case SUDDENDEATH_TYPE._制限なし:
                    suddenDeath_Type = SUDDENDEATH_TYPE._未設定;
                    break;
                case SUDDENDEATH_TYPE._未設定:
                    suddenDeath_Type = SUDDENDEATH_TYPE._1回以下;
                    break;
            }

            //新規プレイヤー
        } else { 
            return;
        }
        suddenDeath_TypeText.text = suddenDeath_Type.ToString().Trim('_');

    }
}
