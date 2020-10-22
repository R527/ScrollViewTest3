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


    public override void DestroyPopUP() {

        //チェックボックスがOnなら次回以降このPopUpを表示しない
        if (checkBox.isOn) {
            PlayerManager.instance.SetStringForPlayerPrefs("非表示",PlayerManager.ID_TYPE.currencyPopUp);
        }

        base.DestroyPopUP();
    }
}
