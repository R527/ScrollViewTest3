﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// ゲーム開始時にデータをロードする
/// </summary>
public class SetUpManager : MonoBehaviour
{

    public bool resetSwich;
    
    void Start()
    {

        //DeBug用　trueならPlayerPrefsのKeyを削除する
        if (DebugManager.instance.isPlayerPrefsDeleteAll) {
            Debug.Log("KeyALL削除");
            PlayerPrefs.DeleteAll();
        }


        //自分のIDのロードをする、ない場合は空白を入れる
        PlayerManager.instance.myUniqueId = PlayerPrefs.GetString(PlayerManager.ID_TYPE.myUniqueId.ToString(),"");

        //myUniqueIdが登録されていないなら、取得する
        if (string.IsNullOrEmpty(PlayerManager.instance.myUniqueId)) {
            PlayerManager.instance.myUniqueId = SystemInfo.deviceUniqueIdentifier;
            PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.myUniqueId, PlayerManager.ID_TYPE.myUniqueId);
            Debug.Log(PlayerManager.instance.myUniqueId);

        } else if (PlayerManager.instance.myUniqueId != SystemInfo.deviceUniqueIdentifier) {
            //myUniqueIdが登録されているかの一致確認してちうがうID の場合新たにIDを登録する
            PlayerManager.instance.myUniqueId = SystemInfo.deviceUniqueIdentifier;
            PlayerManager.instance.SetStringForPlayerPrefs(PlayerManager.instance.myUniqueId, PlayerManager.ID_TYPE.myUniqueId);
            Debug.Log(PlayerManager.instance.myUniqueId);
        }



        //BanListのロード
        //,BanListが0人の場合は回さない
        PlayerManager.instance.banListMaxIndex = PlayerPrefs.GetInt(PlayerManager.ID_TYPE.banListMaxIndex.ToString(),0);
        for (int i = 0; i < PlayerManager.instance.banListMaxIndex; i++) {
            if (PlayerPrefs.HasKey(PlayerManager.ID_TYPE.banUniqueID.ToString() + i.ToString())) {
                PlayerManager.instance.banUniqueIDList.Add(PlayerPrefs.GetString(PlayerManager.ID_TYPE.banUniqueID.ToString() + i.ToString(), ""));
                PlayerManager.instance.banUserNickNameList.Add(PlayerPrefs.GetString(PlayerManager.ID_TYPE.banUserNickName.ToString() + i.ToString(), ""));
            }
        }

        //ban専用の通し番号を取得する
        PlayerManager.instance.banIndex = PlayerPrefs.GetInt(PlayerManager.ID_TYPE.banIndex.ToString(), 0);
        

        //PlayerNameが登録されてない場合は準備シーンにて名前を登録する
        //自分の名前をロードする
        //PlayerPrefs.DeleteKey(PlayerManager.ID_TYPE.playerName.ToString());
        PlayerManager.instance.playerName = PlayerPrefs.GetString(PlayerManager.ID_TYPE.playerName.ToString(), "");


        //戦績を取得する
        PlayerManager.instance.totalNumberOfMatches = PlayerPrefs.GetInt(PlayerManager.BATTLE_RECORD_TYPE.総対戦回数.ToString(), 0);
        PlayerManager.instance.totalNumberOfWins = PlayerPrefs.GetInt(PlayerManager.BATTLE_RECORD_TYPE.勝利回数.ToString(), 0);
        PlayerManager.instance.totalNumberOfLoses = PlayerPrefs.GetInt(PlayerManager.BATTLE_RECORD_TYPE.敗北回数.ToString(), 0);
        PlayerManager.instance.totalNumberOfSuddenDeath = PlayerPrefs.GetInt(PlayerManager.BATTLE_RECORD_TYPE.突然死数.ToString(), 0);
        PlayerManager.instance.checkTotalNumberOfMatches = PlayerPrefs.GetInt(PlayerManager.BATTLE_RECORD_TYPE.突然死減少チェック.ToString(), 0);



        //PlayerNameが既に登録されている場合はタイトルシーンへ遷移する
        if (!string.IsNullOrEmpty(PlayerManager.instance.playerName)) {

            //名前が登録されている状態のみ確認する
            //突然死用のフラグを見て戦績に反映する
            if (PlayerPrefs.GetString("突然死用のフラグ", "") != PlayerManager.SuddenDeath_TYPE.ゲーム正常終了.ToString()) {
                Debug.Log("突然死確認" + PlayerPrefs.GetString("突然死用のフラグ"));
                PlayerManager.instance.totalNumberOfSuddenDeath++;
                PlayerManager.instance.SetBattleRecordForPlayerPrefs(PlayerManager.instance.totalNumberOfSuddenDeath, PlayerManager.BATTLE_RECORD_TYPE.突然死数);
            }
            Debug.Log("正常");
            PlayerManager.instance.SetStringSuddenDeathTypeForPlayerPrefs(PlayerManager.SuddenDeath_TYPE.ゲーム正常終了);

            SceneStateManager.instance.NextScene(SCENE_TYPE.TITLE);
        } else {
            SceneStateManager.instance.NextScene(SCENE_TYPE.名前登録);
        }
    }


}
