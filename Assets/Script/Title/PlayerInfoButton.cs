using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// タイトル画面のPlayer情報ボタンの管理
/// </summary>
public class PlayerInfoButton : MonoBehaviour
{

    public Text playerNameText;
    public Text currencyText;

    // Start is called before the first frame update
    void Start()
    {
        playerNameText.text = PlayerManager.instance.playerName;
        UpdateCurrencyText();
    }


    /// <summary>
    /// ゲーム内通貨のテキスト更新
    /// </summary>
    public void UpdateCurrencyText() {
        currencyText.text = PlayerManager.instance.currency.ToString();

    }
}
