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



    private void Start() {
        backButton.onClick.AddListener(OnDestroy);
        maskButton.onClick.AddListener(OnDestroy);
        totalNumberOfMatchesText.text = PlayerManager.instance.totalNumberOfMatches + "回";
        totalNumberOfWinsText.text = PlayerManager.instance.totalNumberOfWins + "回";
        totalNumberOfLosesText.text = PlayerManager.instance.totalNumberOfLoses + "回";
        totalNumberOfSuddenDeathText.text = PlayerManager.instance.totalNumberOfSuddenDeath + "回";
    }


    public void OnDestroy() {
        Destroy(playerInfoPopUpObj);
    }


}
