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
    public Transform banListTran;
    public Button playerInfoButton;

    public Text debugText;

    private void Start() {
        //Debug用のテキストを用意する
        DebugManager.instance.debugText = debugText;

        if (DebugManager.instance.isDebug) {
            Destroy(DebugManager.instance.debugText.gameObject);
        }

        StartCoroutine(AudioManager.instance.PlayBGM(AudioManager.BGM_TYPE.TITLE));
        begginerButton.onClick.AddListener(BegginerGuideMenu);
        gameStartButton.onClick.AddListener(StartGameButton);
        menuButton.onClick.AddListener(MenuPopUp);
        playerInfoButton.onClick.AddListener(PlayerInfoPopUP);

        CreateBanList();
    }

    /// <summary>
    /// Player情報のPopUpの表示
    /// </summary>
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
        Debug.Log("tesst");
        //BanList作成
        for (int i = 0; i < PlayerManager.instance.banUniqueIDList.Count; i++) {
            BanPlayer banplayer = Instantiate(banPlayerPrefab, banListTran, false);
            banplayer.SetUp(PlayerManager.instance.banUniqueIDList[i], PlayerManager.instance.banUserNickNameList[i]);
        }
    }

    /// <summary>
    /// GameStartボタン
    /// </summary>
    public void StartGameButton() {
        StartCoroutine(SceneStateManager.instance.NextScene(SCENE_TYPE.LOBBY));
    }
}
