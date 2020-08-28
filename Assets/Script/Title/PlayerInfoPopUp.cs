using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


/// <summary>
/// プレイヤーPoPUpを管理する
/// </summary>
public class PlayerInfoPopUp : MonoBehaviour
{
    public GameObject playerInfoPopUpObj;
    public Button maskButton;
    public Button backButton;

    //Text
    public Text totalNumberOfMatchesText;
    public Text totalNumberOfWinsText;
    public Text totalNumberOfLosesText;
    public Text totalNumberOfwinRateText;
    public Text totalNumberOfSuddenDeathText;

    public Text begginerOfMatchesText;
    public Text begginerOfWinsText;
    public Text begginerOfwinRateText;
    public Text begginerOfLosesText;

    public Text generalOfMatchesText;
    public Text generalOfWinsText;
    public Text generalOfwinRateText;
    public Text generalOfLosesText;


    //GameLog
    public GameLogNode gameLogPrefab;
    public Transform logTran;
    public GameObject mainCanvas;
    public GameObject underBarCanvas;

    public List<int> rollNumList = new List<int>();

    private void Start() {
        backButton.onClick.AddListener(ClosePopUp);
        maskButton.onClick.AddListener(ClosePopUp);
        totalNumberOfMatchesText.text = PlayerManager.instance.totalNumberOfMatches + "回";
        totalNumberOfWinsText.text = PlayerManager.instance.totalNumberOfWins + "回";
        totalNumberOfLosesText.text = PlayerManager.instance.totalNumberOfLoses + "回";

        if (PlayerManager.instance.totalNumberOfWins == 0) {
            totalNumberOfwinRateText.text = "0%";
        } else {
            totalNumberOfwinRateText.text = 100 * (PlayerManager.instance.totalNumberOfWins / PlayerManager.instance.totalNumberOfWins) + "%";
        }

        totalNumberOfSuddenDeathText.text = PlayerManager.instance.totalNumberOfSuddenDeath + "回";

        begginerOfMatchesText.text = PlayerManager.instance.beginnerTotalNumberOfMatches + "回";
        begginerOfWinsText.text = PlayerManager.instance.beginnerTotalNumberOfWins + "回";
        begginerOfLosesText.text = PlayerManager.instance.beginnerTotalNumberOfLoses + "回";
        if(PlayerManager.instance.beginnerTotalNumberOfMatches == 0) {
            begginerOfwinRateText.text = "0%";
        } else {
            begginerOfwinRateText.text = 100 * (PlayerManager.instance.beginnerTotalNumberOfWins / PlayerManager.instance.beginnerTotalNumberOfMatches) + "%";
        }

        generalOfMatchesText.text = PlayerManager.instance.generalTotalNumberOfMatches + "回";
        generalOfWinsText.text = PlayerManager.instance.generalTotalNumberOfWins + "回";
        generalOfLosesText.text = PlayerManager.instance.generalTotalNumberOfLoses + "回";
        if (PlayerManager.instance.generalTotalNumberOfMatches == 0) {
            generalOfwinRateText.text = "0%";
        } else {
            generalOfwinRateText.text = 100 * (PlayerManager.instance.generalTotalNumberOfWins / PlayerManager.instance.generalTotalNumberOfMatches) + "%";
        }
        PlayerManager.instance.GetSaveRoomData();
        CreateGameLog();
    }


    public void ClosePopUp() {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ゲームデータの数だけインスタンスします。
    /// </summary>
    public void CreateGameLog() {

        //チャットログがない場合
        //生成しない
        if(PlayerManager.instance.getChatLogList.Count <= 0) {
            Debug.Log("CreateGameLog Return");
            return;
        }

        
        

        //ちゃっとログがある分部屋を生成する
        for(int i = 0; i < PlayerManager.instance.getChatLogList.Count; i++) {

            //チャットログの部屋情報だけを抜き取り
            string[] result = PlayerManager.instance.getChatLogList[i].Substring(0, PlayerManager.instance.getChatLogList[i].Length - 1).Split('%').ToArray<string>();

            //部屋生成
            GameLogNode gameLogNode = Instantiate(gameLogPrefab, logTran, false);
            gameLogNode.roomNum = i;
            gameLogNode.playerInfoPopUp = this;

            //勝敗の表示
            if(result[0] == "False") {
                gameLogNode.resultText.text = "敗北";
            } else {
                gameLogNode.resultText.text = "勝利";
            }

            //役職一覧
            rollNumList = result[1].Split(',').Select(int.Parse).ToList();
            DisplayRollList(rollNumList, gameLogNode);
        }
    }

    /// <summary>
    /// 役職のテキスト表示
    /// </summary>
    /// <param name="numList"></param>
    private void DisplayRollList(List<int> rollNumList,GameLogNode gameLogNode) {
        gameLogNode.rollListText.text = string.Empty;
        //役職テキスト
        int num = 0;
        for (int i = 0; i < rollNumList.Count; i++) {
            if (rollNumList[i] != 0) {
                string emptyStr = "";
                num++;
                if (num != 0 && num % 3 == 1) {
                    emptyStr = "\r\n";
                }
                string str = ((ROLLTYPE)i) + ": " + rollNumList[i];
                gameLogNode.rollListText.text += emptyStr + str;
            }
        }
    }
}
