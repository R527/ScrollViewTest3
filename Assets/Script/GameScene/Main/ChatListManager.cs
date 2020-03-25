using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// ChatListの管理。場合に応じてSetActiveで制御する
/// </summary>
public class ChatListManager : MonoBehaviour {
    //class
    public ChatSystem chatSystem;
    //ChatList
    public List<ChatNode> normalList = new List<ChatNode>();
    public List<ChatNode> wlofList = new List<ChatNode>();
    public List<ChatNode> callOutList = new List<ChatNode>();
    public List<ChatNode> deathList = new List<ChatNode>();
    public List<ChatNode> gameMasterList = new List<ChatNode>();

    //フィルター機能
    //各プレイヤーのChatListを種類別に分ける
    public List<List<ChatNode>> allnormalList = new List<List<ChatNode>>();
    public List<List<ChatNode>> allwolfList = new List<List<ChatNode>>();
    public List<List<ChatNode>> alldeathList = new List<List<ChatNode>>();
    public bool isfilter;//trueならフィルター中 
    public int playerNum;//Playerナンバー

    //MyPlayer情報
    public bool myWolfChat;
    public bool myLive;


    /// <summary>
    /// 参加人数分のListを用意する(中身のない箱を作るだけ
    /// </summary>
    /// <param name="numLimit"></param>
    public void PlayerListSetUp(int numLimit, bool wolf, bool live) {

        //MyPlayerの情報を取得
        myWolfChat = wolf;
        myLive = live;
        for (int i = 0; i < numLimit; i++) {
            allnormalList.Add(new List<ChatNode>());
            allwolfList.Add(new List<ChatNode>());
            alldeathList.Add(new List<ChatNode>());
        }
    }


    /// <summary>
    /// フィルター機能をOnにします。
    /// 押したボタンの相手のPlayerの情報wolfとliveが渡されている
    /// </summary>
    /// <param name="id"></param>
    public void OnFilter(int id) {
        //isfilter = true;

        //GMチャット削除
        foreach (ChatNode chatObj in gameMasterList) {
            chatObj.gameObject.SetActive(false);
            Debug.Log("GMチャット削除");
        }

        //通常のチャット
        foreach (List<ChatNode> chatList in allnormalList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(false);
                if (chatObj.playerID == id) {
                    chatObj.gameObject.SetActive(true);
                }
            }
            Debug.Log("通常チャット");
        }
        //狼チャット
        foreach (List<ChatNode> chatList in allwolfList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(false);
                if (chatObj.playerID == id && myWolfChat) {
                    chatObj.gameObject.SetActive(true);
                }
            }
            Debug.Log("狼チャット");
        }
        //死亡チャット
        foreach (List<ChatNode> chatList in alldeathList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(false);
                if (chatObj.playerID == id && !myLive) {
                    chatObj.gameObject.SetActive(true);
                }
            }
            Debug.Log("死亡チャットチャット");
        }
        Debug.Log("フィルター通過");
    }

    /// <summary>
    /// フィルター機能をOffにします。
    /// </summary>
    /// <param name="id"></param>
    public void OffFilter() {
        //for (int i = 0; i < allPlayerList.Count; i++) {
        //    foreach (ChatNode chatObj in allPlayerList[i]) {
        //        chatObj.gameObject.SetActive(true);
        //    }
        //}

        //GMチャット
        foreach (ChatNode chatObj in gameMasterList) {
            chatObj.gameObject.SetActive(true);
        }
        //通常のチャット
        foreach (List<ChatNode> chatList in allnormalList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(true);
            }
        }
        //狼チャット
        foreach (List<ChatNode> chatList in allwolfList) {
            foreach (ChatNode chatObj in chatList) {
                if (myWolfChat) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }
        //死亡チャット
        foreach (List<ChatNode> chatList in alldeathList) {
            foreach (ChatNode chatObj in chatList) {
                if (!myLive) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }
        //isfilter = false;
    }


    /// <summary>
    /// 狼モードの切り替え
    /// </summary>
    public void OnWolfMode() {
        //GMチャット
        foreach (ChatNode chatObj in gameMasterList) {
            chatObj.gameObject.SetActive(false);
        }
        //通常のチャット
        foreach (List<ChatNode> chatList in allnormalList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(false);
            }
        }
        //狼チャット
        foreach (List<ChatNode> chatList in allwolfList) {
            foreach (ChatNode chatObj in chatList) {
                if (myWolfChat) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }
        //死亡チャット
        foreach (List<ChatNode> chatList in alldeathList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 狼モードの切り替え
    /// </summary>
    public void OffWolfMode() {
        //GMチャット
        foreach (ChatNode chatObj in gameMasterList) {
            chatObj.gameObject.SetActive(true);
        }
        //通常のチャット
        foreach (List<ChatNode> chatList in allnormalList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(true);
            }
        }

        //死亡チャット
        foreach (List<ChatNode> chatList in alldeathList) {
            foreach (ChatNode chatObj in chatList) {
                if (!myLive) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }

        //public void PlayerFilterFalse(int num) {
        //    foreach (ChatNode chatObj in allPlayerList[num]) {
        //        chatObj.gameObject.SetActive(false);
        //    }
        //}


        //メソッドまとめ。状態によりそれぞれ処理をする

        ///// <summary>
        ///// ノーマルチャット、GMチャット、吹き出し色変更チャット一括で管理
        ///// </summary>
        //public void NormalListSetActiveTrue() {
        //    foreach (ChatNode normal in normalList) {
        //        normal.gameObject.SetActive(true);
        //    }
        //    foreach (ChatNode callout in callOutList) {
        //        callout.gameObject.SetActive(true);
        //    }
        //    foreach (ChatNode gamemaster in gameMasterList) {
        //        gamemaster.gameObject.SetActive(true);
        //    }
        //}
        ///// <summary>
        ///// ノーマルチャット、GMチャット、吹き出し色変更チャット一括で管理
        ///// </summary>
        //public void NormalListSetActiveFalse() {
        //    foreach (ChatNode normal in normalList) {
        //        normal.gameObject.SetActive(false);
        //    }
        //    foreach (ChatNode callout in callOutList) {
        //        callout.gameObject.SetActive(false);
        //    }
        //    foreach (ChatNode gamemaster in gameMasterList) {
        //        gamemaster.gameObject.SetActive(false);
        //    }
        //}
        ///// <summary>
        ///// 狼チャット管理
        ///// </summary>
        //public void WolfListSetActiveTrue() {
        //    foreach (ChatNode wolf in wlofList) {
        //        wolf.gameObject.SetActive(true);
        //    }
        //}
        ///// <summary>
        ///// 狼チャット管理
        ///// </summary>
        //public void WolfListSetActiveFalse(){
        //    foreach (ChatNode wolf in wlofList) {
        //        wolf.gameObject.SetActive(false);
        //    }
        //}
        ///// <summary>
        ///// 死亡チャット管理
        ///// </summary>
        //public void DeathListSetActiveTrue() {
        //    foreach (ChatNode death in deathList) {
        //        death.gameObject.SetActive(true);
        //    }
        //}
        ///// <summary>
        ///// 死亡チャット管理
        ///// </summary>
        //public void DeathListSetActiveFalse() {
        //    foreach (ChatNode death in deathList) {
        //        death.gameObject.SetActive(false);
        //    }
        //}
    }
}


