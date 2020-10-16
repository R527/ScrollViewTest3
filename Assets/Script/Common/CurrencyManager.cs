using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 課金に関するイベントなどを管理する
/// </summary>
public class CurrencyManager : MonoBehaviour
{

    public static CurrencyManager instance;


    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }


    //課金用のボタンに取り付けているテスト中
    public void CheckMoneyTest() {
        Debug.Log("100水晶獲得しました");

        int currency = PlayerPrefs.GetInt(PlayerManager.ID_TYPE.currency.ToString(), 0);
        currency += 100;
        Debug.Log("currency" + currency);
        PlayerManager.instance.SetIntForPlayerPrefs(currency, PlayerManager.ID_TYPE.currency);
    }

    /// <summary>
    /// ゲーム内通貨を利用する
    /// </summary>
    public void UseCurrency(int useCurrency) {

        int currency = PlayerPrefs.GetInt(PlayerManager.ID_TYPE.currency.ToString(), 0);

        currency -= useCurrency;
        PlayerManager.instance.SetIntForPlayerPrefs(currency, PlayerManager.ID_TYPE.currency);
        Debug.Log("currency" + currency);
    }
}
