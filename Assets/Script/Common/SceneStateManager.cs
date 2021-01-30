using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// scene遷移を制御する
/// </summary>
public class SceneStateManager : MonoBehaviour
{

    public static SceneStateManager instance;
    public bool isCeack;
    //public AsyncOperation lobbyScene;

    private void Awake() {
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// シーン名をここでTostringにて変換　大文字小文字を無視して判定できる
    /// </summary>
    /// <param name="sceneName"></param>
    public void NextScene(SCENE_TYPE sceneName) {
        Debug.Log("SceneManager.GetActiveScene().name" + SceneManager.GetActiveScene().name);
        //Debug.Log("SCENE_TYPE.GAME.ToString()" + SCENE_TYPE.GAME.ToString());
        if (sceneName == SCENE_TYPE.GAME) {
            //lobbyScene = SceneManager.UnloadSceneAsync(3);
            Debug.Log("gameシーンを追加");
            SceneManager.LoadScene(sceneName.ToString(), LoadSceneMode.Additive);
            
            //次の呼び出すシーンがLobbyかつ現在のシーンがGameならGameシーンを破棄する
        } else if(SceneManager.GetActiveScene().name == "Game"){
            Debug.Log("次の呼び出すシーンがLobbyかつ現在のシーンがGameならGameシーンを破棄する");
            SceneManager.UnloadSceneAsync(SCENE_TYPE.GAME.ToString());
        }else {
            Debug.Log("その他のシーン移動");
            SceneManager.LoadScene(sceneName.ToString());
        }
    }

}
