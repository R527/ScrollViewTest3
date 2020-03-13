using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public string name;
    public static PlayerManager instance;

    [Header("TimeControllerの時間管理用")]
    public bool isDebug;//TimeControllerの時間管理用のDebug
    public int testMainTime;
    public int testNightTime;


    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

}
