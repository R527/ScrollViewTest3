using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 利用規約や資金関連を管理する
/// </summary>
public class RulesManager : MonoBehaviour
{

    public Button 特定商取引法Btn;
    public Button 資金決済法Btn;
    public RulesText rulesTextObj;
    public Transform shopObjTrn;


    //ボタンの押したテキストを読み取ってインスタンスする内容を変更する
    public void pushJudge(string btnText) {
        RulesText obj = Instantiate(rulesTextObj, shopObjTrn, false);
        switch (btnText) {
            case "特定商取引法":
                obj.titleText.text = "特定商取引法に基づく表記";
                obj.rulesText.text = "◆アプリ名　test\r\n\r\n◆会社名　株式会社　トップコート\r\n\r\n";
                break;
            case "資金決済法":
                obj.titleText.text = "資金決済法に基づく表記";
                obj.rulesText.text = "資金決済法test";
                break;
        }

    }
}
