using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// カミングアウト用のボタンに取り付けている
/// </summary>
public class ComingOutButton : MonoBehaviourPunCallbacks {

    public Text comingOutText;
    public Button comingOutButton;
    public ChatSystem chatSystem;

    private void Start() {
        chatSystem = GameObject.FindGameObjectWithTag("ChatSystem").GetComponent<ChatSystem>();
        comingOutButton = gameObject.GetComponent<Button>();
        //if (comingOutText.text == "スライド") {
        //    comingOutText.text = string.Empty;
        //}
        comingOutButton.onClick.AddListener(ComingOut);
    }

    /// <summary>
    /// カミングアウト用のチャットボタン
    /// </summary>
    public void ComingOut() {
        SetComingOutText();
        chatSystem.chatInputField.text = "";
        chatSystem.CreateChatNode(true, SPEAKER_TYPE.UNNKOWN);
    }

    private void SetComingOutText() {
        var propertis = new ExitGames.Client.Photon.Hashtable {
            {"comingOutText",comingOutText.text }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(propertis);
        Debug.Log("comingOutText" + (string)PhotonNetwork.LocalPlayer.CustomProperties["comingOutText"]);
    }
}
