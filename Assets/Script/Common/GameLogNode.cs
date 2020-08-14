using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// チャットログのNode
/// </summary>
public class GameLogNode : MonoBehaviour
{

    public int roomNum;
    public Button gameLogBtn;
    public Text resultText;//勝敗
    public Text rollListText;//役職一覧
    public GameObject chatLogCanvasPrefab;
    public PlayerInfoPopUp playerInfoPopUp;

    private void Start() {
        gameLogBtn.onClick.AddListener(() => GetSaveLogButton(roomNum));
    }

    /// <summary>
    /// ボタンを押すとRoomNumに応じてログを取得する
    /// </summary>
    private void GetSaveLogButton(int roomNum) {
        playerInfoPopUp.mainCanvas.SetActive(false);
        playerInfoPopUp.underBarCanvas.SetActive(false);
        GameObject playerInfoPopUpObj = GameObject.FindGameObjectWithTag("PlayerInfoPopUp").gameObject;
        playerInfoPopUpObj.SetActive(false);
        CanvasGroup chatLogCanvas = GameObject.FindGameObjectWithTag("ChatLog").GetComponent<CanvasGroup>();
        chatLogCanvas.alpha = 1;
        chatLogCanvas.blocksRaycasts = true;

        PlayerManager.instance.GetGameChatLog(roomNum);
        
    }

}
