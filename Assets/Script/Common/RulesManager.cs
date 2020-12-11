using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// 利用規約や資金関連を管理する
/// </summary>
public class RulesManager : MonoBehaviour
{

    public Button 特定商取引法Btn;
    public Button 資金決済法Btn;

    private const string URL = "https://sun471044.wixsite.com/mysite";

    void Start() {
        特定商取引法Btn.onClick.AddListener(OnClick);
        資金決済法Btn.onClick.AddListener(OnClick);
    }

    public void OnClick() {
        //コルーチンを呼び出す
        //StartCoroutine(OnSend(URL));
        Application.OpenURL(URL);
    }

}
