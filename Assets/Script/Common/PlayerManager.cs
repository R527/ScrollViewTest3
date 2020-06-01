using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{


    //main
    public string playerName;
    public List<string> banUniqueIDList = new List<string>();
    public List<string> banUserNickNameList = new List<string>();
    public int banIndex;//ban番号の通し番号
    public Dictionary<string, string> banTable = new Dictionary<string, string>();
    public string myUniqueId;//自分の端末番号
    public static PlayerManager instance;
    public int banListMaxIndex;

    public enum ID_TYPE {
        myUniqueId,
        banUniqueID,
        banUserNickName,
        banIndex,
        banListMaxIndex,
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
    /// PlayerPrefsにstringをセットする
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

            //banUniqueIDListへUniqueIDを追加する
            case ID_TYPE.banUniqueID:
                PlayerPrefs.SetString(ID_TYPE.banUniqueID.ToString() + banIndex.ToString(), setString);
                break;
            //banUserNickNameListへバンした名前を追加
            case ID_TYPE.banUserNickName:
                PlayerPrefs.SetString(ID_TYPE.banUserNickName.ToString() + banIndex.ToString(), setString);
                break;
            case ID_TYPE.playerName:
                PlayerPrefs.SetString(ID_TYPE.playerName.ToString(), setString);
                break;
            case ID_TYPE.friendId:
                break;
        }

        
        //端末の中に保存する
        PlayerPrefs.Save();
    }


    /// <summary>
    /// PlayerPrefsに数字をセットする
    /// </summary>
    /// <param name="setInt"></param>
    /// <param name="idType"></param>
    public void SetIntForPlayerPrefs(int setInt, ID_TYPE idType) {
        switch (idType) {

            //BanList関連
            case ID_TYPE.banIndex:
                PlayerPrefs.SetInt(ID_TYPE.banIndex.ToString(), setInt);
                break;
            case ID_TYPE.banListMaxIndex:
                PlayerPrefs.SetInt(ID_TYPE.banListMaxIndex.ToString(), setInt);
                break;
        }
        PlayerPrefs.Save();
    }


    /// <summary>
    /// banListにプレイヤーを追加します。
    /// </summary>
    public void SetBanList() {
        //Ban枠の限界を見る
        if (banUniqueIDList.Count >= 3) {
            Debug.Log("Ban枠がいっぱいです。");
            return;
        }

        //空いている通し番号を探す
        for(int i = 0; i < 3; i++) {
            Debug.Log("test");
            Debug.Log(PlayerPrefs.HasKey(ID_TYPE.banUniqueID.ToString() + i.ToString()));
            if(!PlayerPrefs.HasKey (ID_TYPE.banUniqueID.ToString() + i.ToString())){
                Debug.Log(i);
                banIndex = i;
                banListMaxIndex = i + 1;
                SetIntForPlayerPrefs(banListMaxIndex, ID_TYPE.banListMaxIndex);
                break;
            } 
        }
        
        //Ban登録
        SetStringForPlayerPrefs("banUniqueID" + banIndex, ID_TYPE.banUniqueID);
        SetStringForPlayerPrefs("playerName" + banIndex, ID_TYPE.banUserNickName);
        banUniqueIDList.Add("banUniqueID" + banIndex);
        banUserNickNameList.Add("playerName" + banIndex);
        Debug.Log(PlayerPrefs.GetString(ID_TYPE.playerName.ToString(), ""));

        

    }

}
