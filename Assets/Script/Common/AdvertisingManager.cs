using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;


/// <summary>
/// 広告に関する情報をまとめる
/// </summary>
public class AdvertisingManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AdsManager();
    }

    /// <summary>
    /// 広告に関する制御
    /// </summary>
    void AdsManager() {
        string gameID = "3851633";
        Advertisement.Initialize(gameID, true);
        Debug.Log("広告準備完了");
    }
}
