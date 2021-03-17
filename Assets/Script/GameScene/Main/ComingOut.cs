using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
    public string GetComingOutText(int playerID) {
        string str = string.Empty;
        foreach(Photon.Realtime.Player player in PhotonNetwork.PlayerList) {
            if(player.ActorNumber == playerID) {
                if (player.CustomProperties.TryGetValue("comingOutText", out object comingOutTextObj)) {
                    str = (string)comingOutTextObj;
                    break;
                }
            }
        }
        return str;
    }
}
