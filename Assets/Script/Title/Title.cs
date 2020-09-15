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
    public GameObject playerInfoPopUp;
    public GameObject begginerGuidePopUp;
    public BanPlayer banPlayerPrefab;
    public Transform banListtran;
    public Button playerInfoButton;

    public GameObject mainCanvas;
    public GameObject underBarCanvas;

    public Text debugText;


    private void Awake() {

    }

    private void Start() {

        //if (DebugManager.instance.isNavCheack) {
        //    Screen.fullScreen = true;
        //} else {
        //    Screen.fullScreen = false;
        //}

        if (DebugManager.instance.isDebug) {
            Destroy(debugText.gameObject);
        }
        AudioManager.instance.PlayBGM(AudioManager.BGM_TYPE.TITLE);
        begginerButton.onClick.AddListener(BegginerGuideMenu);
        gameStartButton.onClick.AddListener(StartGameButton);
        menuButton.onClick.AddListener(MenuPopUp);
        playerInfoButton.onClick.AddListener(PlayerInfoPopUP);


        CreateBanList();
    }
    public void PlayerInfoPopUP() {
        AudioManager.instance.PlaySE(AudioManager.SE_TYPE.OK);
        playerInfoPopUp.SetActive(true);

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
    /// 起動時にBanListを作成する
    /// </summary>
    public void CreateBanList() {

        ////BanListがないなら実行しない
        if (PlayerManager.instance.banListIndex <= 0) {
            return;
        }

        //BanList作成
        for (int i = 0; i < PlayerManager.instance.banUniqueIDList.Count; i++) {
            BanPlayer banplayer = Instantiate(banPlayerPrefab, banListtran, false);
            banplayer.SetUp(PlayerManager.instance.banUniqueIDList[i], PlayerManager.instance.banUserNickNameList[i]);
        }
    }

    public void StartGameButton() {
        SceneStateManager.instance.NextScene(SCENE_TYPE.LOBBY);
    }



}
