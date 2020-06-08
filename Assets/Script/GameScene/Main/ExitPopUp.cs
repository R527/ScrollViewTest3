using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 入室時に退室するべきプレイヤーに出るPopUpです。
/// ボタンを押すと退出します。
/// 15秒以内にボタンを押さなければ強制退出されます。
/// </summary>
public class ExitPopUp : MonoBehaviour
{
    public Text exitText;
    public Button exitButton;
    float chekTimer;
    bool isExit;
    // Start is called before the first frame update
    void Start()
    {
        exitButton.onClick.AddListener(ExitButton);
    }

    // Update is called once per frame
    void Update() {

        //ボタンが押されるとUpdateの処理を止める
        if (isExit) {
            return;
        }

        //時間経過でも退出させます（15秒
        chekTimer += Time.deltaTime;
        
        if (chekTimer >= 15.0f) {
            NetworkManager.instance.ForcedExitRoom();
        }
    }

    /// <summary>
    /// ボタンで自分を退出させます
    /// </summary>
    private void ExitButton() {
        //連打防止
        if (isExit) {
            return;
        }
        NetworkManager.instance.ForcedExitRoom();
        isExit = true;
    }
}
