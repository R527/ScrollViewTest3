using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatePlayerName : MonoBehaviour
{
    public Button createNameButton;
    public InputField inputFieldName;//画面で登録した名前
    public CheckTabooWard checkTabooWard;
    public NGList nGList;
    public bool checkName;

    private void Start() {
        createNameButton.onClick.AddListener(OnClickSubmit);
    }

    public void OnClickSubmit() {
        inputFieldName.text = checkTabooWard.StrMatch(inputFieldName.text, nGList.ngWordList);
        if (inputFieldName.text.Contains("*")) {
            Debug.Log("禁止Word含まれてます");
            inputFieldName.text = string.Empty;
            return;
        }
        //Debug.Log(inputFieldName.text.Trim().Length);
        if (inputFieldName.text.Trim().Length >= 3 && inputFieldName.text.Trim().Length <= 12) {
            PlayerManager.instance.name = inputFieldName.text;
            SceneStateManager.instance.NextScene(SCENE_TYPE.TITLE);
        } else {
            Debug.Log("文字数制限にかかっています。");
            inputFieldName.text = string.Empty;
        }
    }
}
