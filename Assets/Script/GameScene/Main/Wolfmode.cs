using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 狼と市民チャットを切り替えるボタン
/// </summary>
public class Wolfmode : MonoBehaviour
{
    public Text wolfText;
    public bool wolf;

    public void ChangeWolfmode() {
        if(wolfText.text == "狼") {
            wolfText.text = "市民";
            wolf = false;
        } else {
            wolfText.text = "狼";
            wolf = true;
        }
    }
}
