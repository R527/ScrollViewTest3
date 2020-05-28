using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// ゲーム開始時にデータをロードする
/// </summary>
public class SetUpManager : MonoBehaviour
{

    //test
    
    void Start()
    {
        //PlayerPrefs.DeleteAll();

        //自分のIDのロードをする、ない場合は空白を入れる
        PlayerManager.instance.myUniqueId = PlayerPrefs.GetString(PlayerManager.ID_TYPE.myUniqueId.ToString(),"");

        //myUniqueIdが登録されていないなら、取得する
        if (string.IsNullOrEmpty(PlayerManager.instance.myUniqueId)) {
            PlayerManager.instance.myUniqueId = SystemInfo.deviceUniqueIdentifier;
            PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.myUniqueId, PlayerManager.ID_TYPE.myUniqueId);
            Debug.Log(PlayerManager.instance.myUniqueId);
        } else if (PlayerManager.instance.myUniqueId != SystemInfo.deviceUniqueIdentifier) {
            //myUniqueIdが登録されているかの一致確認
            PlayerManager.instance.myUniqueId = SystemInfo.deviceUniqueIdentifier;
            PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.myUniqueId, PlayerManager.ID_TYPE.myUniqueId);
            Debug.Log(PlayerManager.instance.myUniqueId);
        }

        //test用にBanList追加
        //for (int i = 0; i < PlayerManager.instance.banList.Count; i++) {
        //    Debug.Log(PlayerManager.instance.banIndex);
        //    PlayerManager.instance.SetStringForPlayerPrefs("player" + i, PlayerManager.ID_TYPE.banId);
        //    Debug.Log("player" + i);
        //    PlayerManager.instance.banIndex++;
        //}

        //BanListのロード
        for (int i = 0; i < PlayerManager.instance.banList.Count; i++) {
            PlayerManager.instance.banList[i] = PlayerPrefs.GetString((PlayerManager.ID_TYPE.banId +i).ToString(), "");
            Debug.Log(PlayerManager.instance.banList[i]);
        }

        //PlayerNameが登録されてない場合は準備シーンにて名前を登録する
        //自分の名前をロードする
        //PlayerPrefs.DeleteKey(PlayerManager.ID_TYPE.playerName.ToString());
        PlayerManager.instance.playerName = PlayerPrefs.GetString(PlayerManager.ID_TYPE.playerName.ToString(), "");
        //if (PlayerPrefs.HasKey(PlayerManager.ID_TYPE.playerName.ToString())) {
        //    Debug.Log("Keyあり");
        //} else {
        //    Debug.Log("Keyなし");
        //}
        Debug.Log(PlayerManager.instance.playerName);
        //PlayerNameが既に登録されている場合はタイトルシーンへ遷移する
        if (!string.IsNullOrEmpty(PlayerManager.instance.playerName)) {
            SceneStateManager.instance.NextScene(SCENE_TYPE.TITLE);
            return;
        } else {
            SceneStateManager.instance.NextScene(SCENE_TYPE.準備);
        }
    }

    


}
