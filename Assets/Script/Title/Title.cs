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

    //test
    public Button bantest1;
    public Button bantest2;
    public Button bantest3;

    private void Start() {
        AudioManager.instance.PlayBGM(AudioManager.BGM_TYPE.TITLE);
        begginerButton.onClick.AddListener(BegginerGuideMenu);
        gameStartButton.onClick.AddListener(() => SceneStateManager.instance.NextScene(SCENE_TYPE.LOBBY));
        menuButton.onClick.AddListener(MenuPopUp);
        playerInfoButton.onClick.AddListener(PlayerInfoPopUP);

        //test
        bantest1.onClick.AddListener(() => SetBanList(0));
        bantest2.onClick.AddListener(() => SetBanList(1));
        bantest3.onClick.AddListener(() => SetBanList(2));


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

    /// <summary>
    /// banListにプレイヤーを追加します。
    /// </summary>
    public void SetBanList(int banIndex) {
        if(PlayerManager.instance.banList.Count >= 3) {
            return;
        }
        PlayerManager.instance.banIndex = banIndex;
        PlayerManager.instance.SetStringForPlayerPrefs("player" + banIndex, PlayerManager.ID_TYPE.banId);
        PlayerManager.instance.banList.Add("player" + banIndex);
        Debug.Log(PlayerPrefs.GetString(PlayerManager.ID_TYPE.playerName.ToString(), ""));
    }

    public void DeleteBanList(int banIndex) {
        PlayerManager.instance.banList.RemoveAt(banIndex);

    }

    //ブロックしたプレイヤーを削除する処理
    //ブロックしたいプレイヤーをインスタンスする
    //ブロックしたいプレイヤーのBanIndexをもとに削除する
}
