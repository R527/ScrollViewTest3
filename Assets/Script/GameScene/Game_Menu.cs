using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ゲームシーンにあるメニューボタンを制御する
/// </summary>
public class Game_Menu : MonoBehaviour
{

    public Button gameMenuButton;
    public GameObject gameMenu;

    void Start()
    {
        gameMenuButton.onClick.AddListener(GameMenuButton);
    }

    public void GameMenuButton() {
        Instantiate(gameMenu);
    }
}
