using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// タイトル画面のボタンの制御（アンダーバーは除く
/// </summary>
public class Title : MonoBehaviour
{

    public Button gameStartButton;
    public Button menuButton;
    public Button begginerButton;
    public GameObject menuPopUp;
    public GameObject playerInfoPopUpObj;
    public GameObject begginerGuidePopUp;
    public Button playerInfoButton;

    private void Start() {
        AudioManager.instance.PlayBGM(AudioManager.BGM_TYPE.TITLE);
        begginerButton.onClick.AddListener(BegginerGuideMenu);
        gameStartButton.onClick.AddListener(() => SceneStateManager.instance.NextScene(SCENE_TYPE.LOBBY));
        menuButton.onClick.AddListener(MenuPopUp);
        playerInfoButton.onClick.AddListener(PlayerInfoPopUP);

    }
    public void PlayerInfoPopUP() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        Instantiate(playerInfoPopUpObj);

    }
    /// <summary>
    /// メニューPopUpをインスタンスする
    /// </summary>
    public void MenuPopUp() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        Instantiate(menuPopUp);
    }
    /// <summary>
    /// 初心者ガイドメニューの表示ボタンの制御
    /// </summary>
    public void BegginerGuideMenu() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        Instantiate(begginerGuidePopUp);
    }
}
