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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    //ボタンの押したテキストを読み取ってインスタンスする内容を変更する
    public void pushJudge(string btnText) {
        RulesText obj = Instantiate(rulesTextObj, shopObjTrn, false);
        obj.titleText.text = btnText;
        switch (btnText) {
            case "特定商取引法":
                obj.rulesText.text = "特定商取引法test";
                break;
            case "資金決済法":
                obj.rulesText.text = "資金決済法test";
                break;
        }
    }
}
