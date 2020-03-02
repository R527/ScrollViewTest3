using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームシーン上部にある用語ボタン
/// </summary>
public class VocabularyButton : MonoBehaviour
{
    //main
    public Button vocabularyButton;
    public GameObject vocabularyPopUp;

    // Start is called before the first frame update
    void Start()
    {
        vocabularyButton.onClick.AddListener(OpenVocabulary);
    }


    public void OpenVocabulary() {
        vocabularyPopUp.SetActive(true);
    }
}
