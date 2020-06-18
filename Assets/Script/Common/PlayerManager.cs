using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    public static PlayerManager instance;
    //main
    public string playerName;

    //Ban関連
    [Header("Ban関連")]
    public List<string> banUniqueIDList = new List<string>();
    public List<string> banUserNickNameList = new List<string>();
    public int banIndex;//ban番号の通し番号
    public Dictionary<string, string> banTable = new Dictionary<string, string>();
    public string myUniqueId;//自分の端末番号
    public int banListMaxIndex;

    //戦績関連
    [Header("戦績関連")]
    public int totalNumberOfMatches;//総対戦回数
    public int totalNumberOfWins;//総勝利数
    public int totalNumberOfLoses;//総敗北数
    public int totalNumberOfSuddenDeath;//突然死数

    /// <summary>
    /// Ban関連
    /// </summary>
    public enum BAN_ID_TYPE {
        myUniqueId,
        banUniqueID,
        banUserNickName,
        banIndex,
        banListMaxIndex,
        playerName,
        friendId
    }

    /// <summary>
    /// 戦績関連
    /// </summary>
    public enum BATTLE_RECORD_TYPE {
        総対戦回数,
        勝利回数,
        敗北回数,
        突然死数
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
    public void SetStringForPlayerPrefs(string setString,BAN_ID_TYPE idType) {
        Debug.Log(idType);
        Debug.Log(setString);

        switch (idType) {
            //端末のIDをセットする
            case BAN_ID_TYPE.myUniqueId:
                PlayerPrefs.SetString(BAN_ID_TYPE.myUniqueId.ToString(), setString);
                break;

            //banUniqueIDListへUniqueIDを追加する
            case BAN_ID_TYPE.banUniqueID:
                PlayerPrefs.SetString(BAN_ID_TYPE.banUniqueID.ToString() + banIndex.ToString(), setString);
                break;
            //banUserNickNameListへバンした名前を追加
            case BAN_ID_TYPE.banUserNickName:
                PlayerPrefs.SetString(BAN_ID_TYPE.banUserNickName.ToString() + banIndex.ToString(), setString);
                break;
            case BAN_ID_TYPE.playerName:
                PlayerPrefs.SetString(BAN_ID_TYPE.playerName.ToString(), setString);
                break;
            case BAN_ID_TYPE.friendId:
                break;
        }

        
        //端末の中に保存する
        PlayerPrefs.Save();
    }


    /// <summary>
    /// PlayerPrefsにBanLIstに関する数字をセットする
    /// </summary>
    /// <param name="setInt"></param>
    /// <param name="idType"></param>
    public void SetIntBanListForPlayerPrefs(int setInt, BAN_ID_TYPE idType) {
        switch (idType) {

            //BanList関連
            case BAN_ID_TYPE.banIndex:
                PlayerPrefs.SetInt(BAN_ID_TYPE.banIndex.ToString(), setInt);
                break;
            case BAN_ID_TYPE.banListMaxIndex:
                PlayerPrefs.SetInt(BAN_ID_TYPE.banListMaxIndex.ToString(), setInt);
                break;
        }
        PlayerPrefs.Save();
    }


    public void SetIntBattleRecordForPlayerPrefs(int setInt, BATTLE_RECORD_TYPE type) {
        //戦績関連
        switch (type) {
            case BATTLE_RECORD_TYPE.総対戦回数:
                PlayerPrefs.SetInt(BATTLE_RECORD_TYPE.総対戦回数.ToString(), setInt);
                break;
            case BATTLE_RECORD_TYPE.勝利回数:
                PlayerPrefs.SetInt(BATTLE_RECORD_TYPE.勝利回数.ToString(), setInt);
                break;
            case BATTLE_RECORD_TYPE.敗北回数:
                PlayerPrefs.SetInt(BATTLE_RECORD_TYPE.敗北回数.ToString(), setInt);
                break;
            case BATTLE_RECORD_TYPE.突然死数:
                PlayerPrefs.SetInt(BATTLE_RECORD_TYPE.突然死数.ToString(), setInt);
                break;
            
        }
            
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
            Debug.Log(PlayerPrefs.HasKey(BAN_ID_TYPE.banUniqueID.ToString() + i.ToString()));
            if(!PlayerPrefs.HasKey (BAN_ID_TYPE.banUniqueID.ToString() + i.ToString())){
                Debug.Log(i);
                banIndex = i;
                banListMaxIndex = i + 1;
                SetIntBanListForPlayerPrefs(banListMaxIndex, BAN_ID_TYPE.banListMaxIndex);
                break;
            } 
        }
        
        //Ban登録
        SetStringForPlayerPrefs("banUniqueID" + banIndex, BAN_ID_TYPE.banUniqueID);
        SetStringForPlayerPrefs("playerName" + banIndex, BAN_ID_TYPE.banUserNickName);
        banUniqueIDList.Add("banUniqueID" + banIndex);
        banUserNickNameList.Add("playerName" + banIndex);
        Debug.Log(PlayerPrefs.GetString(BAN_ID_TYPE.playerName.ToString(), ""));

        

    }

}
