using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Debug関連をまとめます。
/// </summary>
public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;


    
    public bool isDebug;//Debugのboolを管理する

    [Header("TimeControllerの時間管理用")]
    //public bool isTimeController;//TimeControllerの時間管理用のDebug
    public bool isTimeController;
    public int testMainTime;
    public int testNightTime;
    public int testVotingTime;
    public int testExecutionTime;
    public int testCheckGameOverTime;
    public int testResultTime;
    public int num;//GameManagerの参加人数設定
    public int enterNum;//GameManagerの参加希望人数設定
    public int numLimit;
    public float setEnterNumTime;
    public VOTING openVoting;
    public FORTUNETYPE fortuneType;
    public TIME timeType;
    
    [Header("PlayerPrefs削除 trueなら削除")]
    public bool isPlayerPrefsDeleteAll;

    [Header("trueならGameOver走らない")]
    public bool isGameOver;

    [Header("trueなら突然死しない")]
    public bool isCheckSuddenDeath;

    [Header("trueなら処刑しない")]
    public bool isVoteCount;

    [Header("役職管理用")]
    public bool isTestPlay;//プレイヤーの役職を任意に決める
    public List<ROLLTYPE> testRollTypeList = new List<ROLLTYPE>();

    //Nav切り替え
    public Button navBtn;
    public Button fullScrBtn;
    public bool isNavCheack;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        navBtn.onClick.AddListener(NavbarBtn);
        fullScrBtn.onClick.AddListener(FullScreanBtn);
    }

    public void NavbarBtn() {
        Screen.fullScreen = false;
        EndSetUp();
    }

    public void FullScreanBtn() {
        Screen.fullScreen = true;
        isNavCheack = true;
        EndSetUp();
    }

    public void EndSetUp() {
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
            SceneStateManager.instance.NextScene(SCENE_TYPE.SetName);
        }
    }
}
