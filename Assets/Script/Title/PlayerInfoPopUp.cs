using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// プレイヤーPoPUpを管理する
/// </summary>
public class PlayerInfoPopUp : MonoBehaviour
{
    public GameObject playerInfoPopUpObj;
    public Button maskButton;
    public Button backButton;

    //Test
    public Text totalNumberOfMatchesText;
    public Text totalNumberOfWinsText;
    public Text totalNumberOfLosesText;
    public Text totalNumberOfSuddenDeathText;

    //GameLog
    public GameLogNode gameLogPrefab;
    public Transform logTran;
    public GameObject mainCanvas;
    public GameObject underBarCanvas;



    private void Start() {
        backButton.onClick.AddListener(ClosePopUp);
        maskButton.onClick.AddListener(ClosePopUp);
        totalNumberOfMatchesText.text = PlayerManager.instance.totalNumberOfMatches + "回";
        totalNumberOfWinsText.text = PlayerManager.instance.totalNumberOfWins + "回";
        totalNumberOfLosesText.text = PlayerManager.instance.totalNumberOfLoses + "回";
        totalNumberOfSuddenDeathText.text = PlayerManager.instance.totalNumberOfSuddenDeath + "回";

        
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
        if(PlayerManager.instance.getChatLogList.Count <= 0) {
            Debug.Log("CreateGameLog Return");
            return;
        }
        for(int i = 0; i < PlayerManager.instance.getChatLogList.Count; i++) {
            GameLogNode gameLogNode = Instantiate(gameLogPrefab, logTran, false);
            gameLogNode.roomNum = i;
            gameLogNode.playerInfoPopUp = this;
        }
    }
}
