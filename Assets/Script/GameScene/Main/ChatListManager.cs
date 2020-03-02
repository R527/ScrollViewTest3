using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ChatListの管理。場合に応じてSetActiveで制御する
/// </summary>
public class ChatListManager : MonoBehaviour
{
    //class

    //ChatList
    public List<ChatNode> normalList = new List<ChatNode>();
    public List<ChatNode> wlofList = new List<ChatNode>();
    public List<ChatNode> callOutList = new List<ChatNode>();
    public List<ChatNode> deathList = new List<ChatNode>();
    public List<ChatNode> gameMasterList = new List<ChatNode>();

    //フィルター機能
    public List<List<ChatNode>> allPlayerList = new List<List<ChatNode>>();
    public bool isfilter;//フィルター中か否か
    public int playerNum;//Playerナンバー

    
    //その他
    private int listCount;

    //フィルター機能
    public void PlayerListSetUp(int numLimit) {
        listCount = numLimit;
            for (int i = 0; i < numLimit; i++) {
            allPlayerList.Add(new List<ChatNode>());
        }
    }


    public void OnFilter(int id) {
        playerNum = id;
        PlayerFilterTrue(playerNum);
        for (int i = 0; i < listCount; i++){
            if(i == playerNum) {
                continue;
            }
            foreach (ChatNode chatObj in allPlayerList[i]) {
                chatObj.gameObject.SetActive(false);
            }
        }
        isfilter = true;
    }

    public void OffFilter() {
        for (int i = 0; i < listCount; i++) {
            foreach (ChatNode chatObj in allPlayerList[i]) {
                chatObj.gameObject.SetActive(true);
            }
        }
        isfilter = false;
    }

    public void PlayerFilterFalse(int num) {
        foreach (ChatNode chatObj in allPlayerList[num]) {
            chatObj.gameObject.SetActive(false);
        }
    }
    public void PlayerFilterTrue(int num) {
        foreach (ChatNode chatObj in allPlayerList[num]) {
            chatObj.gameObject.SetActive(true);
        }
    }
    //メソッドまとめ。Playerの状態によりそれぞれ処理をする
    /// <summary>
    /// 市民死亡時
    /// </summary>
    public void DeadCitizen() {
        NormalListSetActiveTrue();
        WolfListSetActiveFalse();
        DeathListSetActiveTrue();
    }
    /// <summary>
    /// 市民生存時
    /// </summary>
    public void LivingCitizen() {
        NormalListSetActiveTrue();
        WolfListSetActiveFalse();
        DeathListSetActiveFalse();
    }
    /// <summary>
    /// 狼死亡時
    /// </summary>
    public void DeadWolf() {
        NormalListSetActiveTrue();
        WolfListSetActiveFalse();
        DeathListSetActiveTrue();
    }
    /// <summary>
    /// 狼生存時
    /// </summary>
    public void LivingWolf() {
        NormalListSetActiveTrue();
        WolfListSetActiveTrue();
        DeathListSetActiveFalse();
    }
    /// <summary>
    /// 狼専用モード
    /// </summary>
    public void WolfMode() {
        NormalListSetActiveFalse();
        WolfListSetActiveTrue();
        DeathListSetActiveFalse();
    }
    //メソッドまとめ。状態によりそれぞれ処理をする

    /// <summary>
    /// ノーマルチャット、GMチャット、吹き出し色変更チャット一括で管理
    /// </summary>
    public void NormalListSetActiveTrue() {
        foreach (ChatNode normal in normalList) {
            normal.gameObject.SetActive(true);
        }
        foreach (ChatNode callout in callOutList) {
            callout.gameObject.SetActive(true);
        }
        foreach (ChatNode gamemaster in gameMasterList) {
            gamemaster.gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// ノーマルチャット、GMチャット、吹き出し色変更チャット一括で管理
    /// </summary>
    public void NormalListSetActiveFalse() {
        foreach (ChatNode normal in normalList) {
            normal.gameObject.SetActive(false);
        }
        foreach (ChatNode callout in callOutList) {
            callout.gameObject.SetActive(false);
        }
        foreach (ChatNode gamemaster in gameMasterList) {
            gamemaster.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 狼チャット管理
    /// </summary>
    public void WolfListSetActiveTrue() {
        foreach (ChatNode wolf in wlofList) {
            wolf.gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// 狼チャット管理
    /// </summary>
    public void WolfListSetActiveFalse(){
        foreach (ChatNode wolf in wlofList) {
            wolf.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 死亡チャット管理
    /// </summary>
    public void DeathListSetActiveTrue() {
        foreach (ChatNode death in deathList) {
            death.gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// 死亡チャット管理
    /// </summary>
    public void DeathListSetActiveFalse() {
        foreach (ChatNode death in deathList) {
            death.gameObject.SetActive(false);
        }
    }
}


