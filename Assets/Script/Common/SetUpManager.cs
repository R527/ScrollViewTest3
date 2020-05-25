using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// ゲーム開始時に準備シーン、もしくはタイトル画面にどちらに遷移するかを決定する
/// </summary>
public class SetUpManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {


        //PlayerNameが登録されてない場合は準備シーンにて名前を登録する


        

        //自分のIDのロードをする、ない場合は空白を入れる
        PlayerManager.instance.myUniqueId = PlayerPrefs.GetString(PlayerManager.ID_TYPE.myUniqueId.ToString(),"");

        //myUniqueIdが登録されていないなら、取得する
        if (string.IsNullOrEmpty(PlayerManager.instance.myUniqueId)) {
            PlayerManager.instance.myUniqueId = SystemInfo.deviceUniqueIdentifier;
            PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.myUniqueId, PlayerManager.ID_TYPE.myUniqueId);
        } else {
            //myUniqueIdが登録されているかの一致確認
            if (PlayerManager.instance.myUniqueId != SystemInfo.deviceUniqueIdentifier) {
                PlayerManager.instance.myUniqueId = SystemInfo.deviceUniqueIdentifier;
                PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.myUniqueId, PlayerManager.ID_TYPE.myUniqueId);
            }
        }

        //BanListのロード
        for(int i = 0; i < PlayerManager.instance.banList.Count; i++) {
            PlayerManager.instance.banList[i] = PlayerPrefs.GetString((PlayerManager.ID_TYPE.banId + i).ToString(), "");
        }
       

        //PlayerNameが既に登録されている場合はタイトルシーンへ遷移する
        if (PlayerManager.instance.name != string.Empty) {
            SceneStateManager.instance.NextScene(SCENE_TYPE.TITLE);
            return;
        } 


    }


}
