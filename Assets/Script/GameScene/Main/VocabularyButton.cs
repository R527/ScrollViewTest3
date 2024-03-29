﻿using System.Collections;
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
    public Button rollBtn;//左上の役職を示すボタン
    public GameObject vocabularyPopUp;

    // Start is called before the first frame update
    void Start()
    {
        vocabularyButton.onClick.AddListener(OpenVocabulary);
        rollBtn.onClick.AddListener(OpenVocabulary);
    }


    public void OpenVocabulary() {
        GraphicRaycastersManager rayCastManagerObj = GameObject.FindGameObjectWithTag("RaycastersManager").GetComponent<GraphicRaycastersManager>();
        rayCastManagerObj.SwitchGraphicRaycasters(false);
        vocabularyPopUp.SetActive(true);
    }
}
