using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/// <summary>
/// 課金に関するイベントなどを管理する
/// GameCanvas内の課金Imageに取り付けている
/// </summary>
public class CurrencyManager : MonoBehaviour
{

    public GameManager gameManager;

    public Button closeBtn;
    public Button maskBtn;

    void Start() {
        if(closeBtn != null) 
            closeBtn.onClick.AddListener(closePopUp);
        
        if(maskBtn != null)
            maskBtn.onClick.AddListener(closePopUp);
    }
    /// <summary>
    /// ゲーム内通貨を購入
    /// </summary>
    public void BuyCurrency(int buyCurrency) {
        Debug.Log(buyCurrency + "水晶獲得しました");

        //PlayerManager.instance.currency = PlayerPrefs.GetInt(PlayerManager.ID_TYPE.currency.ToString(), 0);
        PlayerManager.instance.currency += buyCurrency;
        Debug.Log("currency" + PlayerManager.instance.currency);
        PlayerManager.instance.SetIntForPlayerPrefs(PlayerManager.instance.currency, PlayerManager.ID_TYPE.currency);

        //GameObject gameCanvas = GameObject.FindGameObjectWithTag("GameCanvas");
        if(SceneManager.GetActiveScene().name == "Game") {
            gameManager.UpdateCurrencyText();
        }
    }


    /// <summary>
    /// 定期購入
    /// </summary>
    public void BuySubscribe() {
        //PlayerManager.instance.subscribe = true;
        Debug.Log("購入" + PlayerManager.instance.SetSubscribe());
    }

    public void FailedSubscribe() {
        Debug.Log("購入失敗");
    }

    void closePopUp() {
        gameObject.SetActive(false);
        GraphicRaycastersManager.instance.SwitchGraphicRaycasters(true);
    }


}
