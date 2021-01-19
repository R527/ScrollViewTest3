using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ゲーム内通貨の利用時のテキスト表示とPopUPの制御
/// </summary>
public class CurrencyTextPopUp : BasePopUP
{

    public Toggle checkBox;//このPopUpを二度と出さないようにするチェックボックス
    public Text warningText;
    public string warningStr;//退出と青チャットどちらの警告文化を分ける

    public GameManager gameManager;


    protected override void Start() {
        base.Start();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        //退出と青チャットどちらの警告文化を分ける
        switch (warningStr) {
            case "superChatStr":
                warningText.text = "水晶を" + gameManager.superChatCurrency + "消費します" + "\n\r\n\r(メンバーは1ゲーム3回まで無料です)";
                break;
            case "exitStr":
                warningText.text = "水晶を" + gameManager.extitCurrency + "消費します";
                break;
        }
    }
    public override void DestroyPopUP() {

        gameManager.showPopUp = true;
        Debug.Log("DestroyPopUp");
        //チェックボックスがOnなら次回以降このPopUpを表示しない
        if (checkBox.isOn) {

            switch (warningStr) {
                case "superChatStr":
                    Debug.Log("superChatStr");
                    PlayerManager.instance.SetStringForPlayerPrefs("非表示", PlayerManager.ID_TYPE.superChat);
                    break;
                case "exitStr":
                    Debug.Log("exitStr");
                    PlayerManager.instance.SetStringForPlayerPrefs("非表示", PlayerManager.ID_TYPE.exit);
                    break;
            }
        }

        base.DestroyPopUP();
    }
}
