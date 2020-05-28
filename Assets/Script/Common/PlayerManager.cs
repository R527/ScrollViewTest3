using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{



    //main
    public string playerName;
    public List<string> banList = new List<string>();
    public Dictionary<string, string> banTable = new Dictionary<string, string>();
    public string myUniqueId;//自分の端末番号
    public static PlayerManager instance;
    public int banIndex;
    public int banListMaxIndex;

    public enum ID_TYPE {
        myUniqueId,
        banId,
        playerName,
        friendId
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// PlayerPrefsにデータをセットする
    /// </summary>
    /// <param name="setString"></param>
    /// <param name="idType"></param>
    public void SetStringForPlayerPrefs(string setString,ID_TYPE idType) {
        Debug.Log(idType);
        Debug.Log(setString);

        switch (idType) {
            //端末のIDをセットする
            case ID_TYPE.myUniqueId:
                PlayerPrefs.SetString(ID_TYPE.myUniqueId.ToString(), setString);
                break;

                //banListへPlayerIDを追加する
            case ID_TYPE.banId:
                PlayerPrefs.SetString(ID_TYPE.banId.ToString() + banIndex.ToString(), setString);
                if (PlayerPrefs.HasKey(ID_TYPE.banId.ToString() + banIndex.ToString())) {
                    Debug.Log(ID_TYPE.banId.ToString() + banIndex.ToString() + "Keyあり");
                } else {
                    Debug.Log(ID_TYPE.banId.ToString() + banIndex.ToString() + "Keyなし");
                }
                break;
            case ID_TYPE.playerName:
                PlayerPrefs.SetString(ID_TYPE.playerName.ToString(), setString);
                //if (PlayerPrefs.HasKey(ID_TYPE.playerName.ToString())) {
                //    Debug.Log("Keyあり");
                //} else {
                //    Debug.Log("Keyなし");
                //}
                break;
            case ID_TYPE.friendId:
                break;
        }

        
        //端末の中に保存する
        PlayerPrefs.Save();
    }

}
