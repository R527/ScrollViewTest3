using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ゲーム終了するPOｐUp
/// </summary>
public class EndRoomPopUp : MonoBehaviour
{
    public Button btn;
    private float chekTimer;
    private bool isCheck;
    public int time;

    // Start is called before the first frame update
    void Start()
    {
        btn.onClick.AddListener(ExitRoom);
    }

    // Update is called once per frame
    void Update()
    {
        //geme終了後時間を計測する
        chekTimer += Time.deltaTime;
        if (chekTimer >= 1) {
            chekTimer = 0;
            time--;
        }

        //一定時間を過ぎると強制退出する処理
        if (time <= 0 && !isCheck) {
            isCheck = true;
            ExitRoom();
        }
    }

    /// <summary>
    /// 部屋退出
    /// </summary>
    private void ExitRoom() {
        NetworkManager.instance.LeaveRoom();
    }
}
