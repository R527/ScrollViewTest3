using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public string name;
    public List<string> banList = new List<string>();
    public string myUniqueId;//自分の端末番号
    public static PlayerManager instance;


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

    public void SetStringForPlayerPrefs(string setString,ID_TYPE idType) {
        switch (idType) {
            case ID_TYPE.myUniqueId:
                PlayerPrefs.SetString(ID_TYPE.myUniqueId.ToString(), setString);
                break;
            case ID_TYPE.banId:
                for(int i = 0; i < banList.Count; i++) {
                    if (string.IsNullOrEmpty(banList[i])) {
                        continue;
                    } else {
                        PlayerPrefs.SetString((ID_TYPE.banId + i).ToString(), setString);
                        break;
                    }
                }
                break;
            case ID_TYPE.playerName:
                break;
            case ID_TYPE.friendId:
                break;
        }

        //端末の中に保存する
        PlayerPrefs.Save();
    }

}
