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



    private void Start() {
        backButton.onClick.AddListener(OnDestroy);
        maskButton.onClick.AddListener(OnDestroy);
    }


    public void OnDestroy() {
        Destroy(playerInfoPopUpObj);
    }


}
