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
    public GameObject menberShipTextObj;

    // Start is called before the first frame update
    void Start()
    {
        playerNameText.text = PlayerManager.instance.playerName;
        UpdateCurrencyText();

        //さぶすく中ならメンバーシップ用のテキストを表示する
        if(!PlayerManager.instance.subscribe) {
            UpdateMenberShipText();
        }
    }


    /// <summary>
    /// ゲーム内通貨のテキスト更新
    /// </summary>
    public void UpdateCurrencyText() {
        currencyText.text = PlayerManager.instance.currency.ToString();

    }

    /// <summary>
    /// メンバーシップ用のテキスト表示
    /// </summary>
    public void UpdateMenberShipText() {
        menberShipTextObj.SetActive(true);
    }
}
