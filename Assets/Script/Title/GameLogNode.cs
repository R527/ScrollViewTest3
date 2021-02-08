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
    public ChatLog chatLogCanvasPrefab;
    public PlayerInfoPopUp playerInfoPopUp;

    private void Start() {
        gameLogBtn.onClick.AddListener(() => GetSaveLogButton(roomNum));
    }

    /// <summary>
    /// ボタンを押すとRoomNumに応じてログを取得する
    /// </summary>
    private void GetSaveLogButton(int roomNum) {
        GameObject playerInfoPopUpObj = GameObject.FindGameObjectWithTag("PlayerInfoPopUp").gameObject;
        playerInfoPopUpObj.SetActive(false);
        GameObject titleCanvas = GameObject.FindGameObjectWithTag("TitleCanvas").gameObject;
        titleCanvas.SetActive(false);
        GameObject underCanvas = GameObject.FindGameObjectWithTag("UnderCanvas").gameObject;
        underCanvas.SetActive(false);

        GameObject chatLogCanvas = GameObject.FindGameObjectWithTag("ChatLogCanvas").gameObject;
        ChatLog chatLogObj = Instantiate(chatLogCanvasPrefab, chatLogCanvas.transform, false);
        chatLogObj.titleCanvas = titleCanvas;
        chatLogObj.underCanvas = underCanvas;

        PlayerManager.instance.GetGameChatLog(roomNum);
    }

}
