using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// 部屋の設定ごとにCoボタンを用意する
/// </summary>
public class ComingOut : MonoBehaviourPunCallbacks {

    //main
    public GameObject coContent;
    public ComingOutButton coButton;


    /// <summary>
    /// COボタンの生成
    /// </summary>
    /// <param name="comingOutButtonList"></param>
    public void ComingOutSetUp(List<ROLLTYPE> comingOutButtonList) {
        foreach(ROLLTYPE rollObj in comingOutButtonList) {
            if(rollObj != ROLLTYPE.市民) {
                ComingOutButton buttonObj = Instantiate(coButton, coContent.transform, false);
                buttonObj.comingOutText.text = rollObj.ToString();
            }
        }
    }


    /// <summary>
    /// Coのテキストをもらいます。
    /// </summary>
    /// <returns></returns>
    public string GetComingOutText() {
        string str = string.Empty;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("comingOutText", out object comingOutTextObj)) {
            str = (string)comingOutTextObj;
        }
        Debug.Log("IsTimeUp" + str);
        return str;
    }
}
