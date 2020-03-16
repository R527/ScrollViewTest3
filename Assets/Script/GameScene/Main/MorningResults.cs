using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 昨夜行われた夜の行動を保存し、結果を朝に表示します
/// ただし、占い結果など個人のみ表示するものは除外します。
/// </summary>
public class MorningResults : MonoBehaviour
{

    //class
    public RollAction rollAction;
    public TimeController timeController;

    //main
    public int biteID;//噛んだプレイヤーID
    public int protectID;//守ったプレイヤーID
    public List<Player> playerList = new List<Player>();



    public void MorningResultsStartUp() {
        GameObject[] Obj = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObj in Obj) {
            Player playerList = playerObj.GetComponent<Player>();
        }
    }



}
