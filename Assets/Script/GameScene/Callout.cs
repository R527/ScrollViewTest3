using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 吹き出しの設定
/// </summary>
public class Callout : MonoBehaviour
{
    public Text CallOutText;
    public bool callOut;

    public void ChangeCallOutmode() {
        if (CallOutText.text == "通常") {
            CallOutText.text = "青";
            callOut = true;
        } else {
            CallOutText.text = "通常";
            callOut = false;
        }
    }
}
