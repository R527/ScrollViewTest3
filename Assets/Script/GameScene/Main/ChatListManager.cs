using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ChatListの管理。場合に応じてSetActiveで制御する
/// </summary>
public class ChatListManager : MonoBehaviour {
    //class
    public GameManager gameManager;
    //ChatList
    public List<ChatNode> normalList = new List<ChatNode>();
    public List<ChatNode> wlofList = new List<ChatNode>();
    //public List<ChatNode> callOutList = new List<ChatNode>();
    public List<ChatNode> deathList = new List<ChatNode>();
    public List<ChatNode> gameMasterList = new List<ChatNode>();

    //フィルター機能
    //各プレイヤーのChatListを種類別に分ける
    public List<List<ChatNode>> allnormalList = new List<List<ChatNode>>();
    public List<List<ChatNode>> allwolfList = new List<List<ChatNode>>();
    public List<List<ChatNode>> alldeathList = new List<List<ChatNode>>();
    public bool isfilter;//trueならフィルター中 

    //MyPlayer情報
    public bool myWolfChat;


    private void Start() {
        for (int i = 0; i < gameManager.numLimit; i++) {
            allnormalList.Add(new List<ChatNode>());
            allwolfList.Add(new List<ChatNode>());
            alldeathList.Add(new List<ChatNode>());
        }
    }

    /// <summary>
    /// 参加人数分のListを用意する(中身のない箱を作るだけ
    /// </summary>
    /// <param name="numLimit"></param>
    public void PlayerListSetUp( bool wolf) {
        //MyPlayerの情報を取得
        myWolfChat = wolf;
    }

    /// <summary>
    /// フィルター機能をOnにします。
    /// 押したボタンの相手のPlayerの情報wolfとliveが渡されている
    /// </summary>
    /// <param name="id"></param>
    public void OnFilter(int id) {
        //GMチャット削除
        foreach (ChatNode chatObj in gameMasterList) {
            chatObj.gameObject.SetActive(false);
        }

        //通常のチャット
        foreach (List<ChatNode> chatList in allnormalList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(false);
                if (chatObj.playerID == id) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }
        //狼チャット
        foreach (List<ChatNode> chatList in allwolfList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(false);
                if (chatObj.playerID == id && myWolfChat) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }
        //死亡チャット
        foreach (List<ChatNode> chatList in alldeathList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(false);
                if (chatObj.playerID == id && !gameManager.chatSystem.myPlayer.live) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// フィルター機能をOffにします。
    /// </summary>
    /// <param name="id"></param>
    public void OffFilter() {
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
        if (myWolfChat) {
            foreach (List<ChatNode> chatList in allwolfList) {
                foreach (ChatNode chatObj in chatList) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }
        //死亡チャット
        if (!gameManager.chatSystem.myPlayer.live) {
            foreach (List<ChatNode> chatList in alldeathList) {
                foreach (ChatNode chatObj in chatList) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }
    }


    /// <summary>
    /// 狼モードの切り替え
    /// </summary>
    public void OnWolfMode() {
        //GMチャット
        foreach (ChatNode chatObj in gameMasterList) {
            chatObj.gameObject.SetActive(true);
        }
        //通常のチャット
        foreach (List<ChatNode> chatList in allnormalList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(false);
            }
        }
        //狼チャット
        if (myWolfChat) {
            foreach (List<ChatNode> chatList in allwolfList) {
                foreach (ChatNode chatObj in chatList) {
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
        if (!gameManager.chatSystem.myPlayer.live) {
            foreach (List<ChatNode> chatList in alldeathList) {
                foreach (ChatNode chatObj in chatList) {
                    chatObj.gameObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// GameOver時に全てのチャットを開放する
    /// </summary>
    public void ReleaseChatLog() {
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
                chatObj.gameObject.SetActive(true);
            }
        }
        //死亡チャット
        foreach (List<ChatNode> chatList in alldeathList) {
            foreach (ChatNode chatObj in chatList) {
                chatObj.gameObject.SetActive(true);
            }
        }
    }
}


