using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerNameと利用規約の同意
/// </summary>
public class CreatePlayerName : MonoBehaviour
{
    public Button createNameButton;
    public InputField inputFieldName;//画面で登録した名前
    public CheckTabooWard checkTabooWard;
    public NGList nGList;
    public SetNameWrongPopUP wrongPopUp;
    public Transform trn;

    //利用規約関連　
    public GameObject rulesObj;
    public Button rulesBtn;
    public Button agreeBtn;
    private const string URL = "https://sun471044.wixsite.com/mysite";

    private void Start() {
        createNameButton.onClick.AddListener(OnClickSubmit);
        rulesBtn.onClick.AddListener(SubmitRules);
        agreeBtn.onClick.AddListener(CheckRules);
    }

    /// <summary>
    /// 名前を登録します。
    /// </summary>
    public void OnClickSubmit() {
        //禁止用語のチェック
        inputFieldName.text = checkTabooWard.StrMatch(inputFieldName.text, nGList.ngWordList);
        if (inputFieldName.text.Contains("*")) {
            SetNameWrongPopUP obj = Instantiate(wrongPopUp, trn, false);
            obj.wrongText.text = "禁止ワードが含まれています。";
            inputFieldName.text = string.Empty;
            return;
        }

        //文字数制限のチェック
        if (inputFieldName.text.Trim().Length >= 3 && inputFieldName.text.Trim().Length <= 10) {
            //名前を登録する
            PlayerManager.instance.playerName = inputFieldName.text;
            PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.playerName, PlayerManager.ID_TYPE.playerName);
            StartCoroutine(SceneStateManager.instance.NextScene(SCENE_TYPE.TITLE));
        } else {
            SetNameWrongPopUP obj = Instantiate(wrongPopUp, trn, false);
            obj.wrongText.text = "文字数制限があります。\n\r 3～10文字に収めてください。";
            inputFieldName.text = string.Empty;
        }
    }

    /// <summary>
    /// 利用規約同意をさせてからName登録画面に移る
    /// </summary>
    public void CheckRules() {
        Destroy(rulesObj);
    }

    /// <summary>
    /// 利用規約が書かれているサイトに飛ぶ
    /// </summary>
    public void SubmitRules() {
        Application.OpenURL(URL);
    }
}
