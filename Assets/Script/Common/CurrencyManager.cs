using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 課金に関するイベントなどを管理する
/// </summary>
public class CurrencyManager : MonoBehaviour
{

    /// <summary>
    /// ゲーム内通貨を購入
    /// </summary>
    public void BuyCurrency(int buyCurrency) {
        Debug.Log(buyCurrency + "水晶獲得しました");

        //PlayerManager.instance.currency = PlayerPrefs.GetInt(PlayerManager.ID_TYPE.currency.ToString(), 0);
        PlayerManager.instance.currency += buyCurrency;
        Debug.Log("currency" + PlayerManager.instance.currency);
        PlayerManager.instance.SetIntForPlayerPrefs(PlayerManager.instance.currency, PlayerManager.ID_TYPE.currency);

    }


}
