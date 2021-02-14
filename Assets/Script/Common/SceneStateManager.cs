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
    public IEnumerator NextScene(SCENE_TYPE sceneName) {
        if (sceneName == SCENE_TYPE.GAME) {
            //Lobbyシーンを見えなくする処理

            RollSetting roomSetting = GameObject.FindGameObjectWithTag("roomSetting").GetComponent<RollSetting>();
            roomSetting.roomSelectCanvas.SetActive(false);
            roomSetting.roomSettingCanvas.SetActive(false);
            NetworkManager.instance.roomSetting.GetComponent<RoomSetting>().createRoomButton.interactable = false;

            SceneManager.LoadScene(sceneName.ToString(), LoadSceneMode.Additive);
            
            //次の呼び出すシーンがLobbyかつ現在のシーンがGameならGameシーンを破棄する
        } else if(SceneManager.GetActiveScene().name == "Game"){

            SceneManager.UnloadSceneAsync(SCENE_TYPE.GAME.ToString());

            yield return new WaitUntil(() => SceneManager.GetActiveScene().name != "Game");
            yield return null;

            //Lobbyシーンを見えるようにする処理
            GameObject.FindGameObjectWithTag("roomSetting").GetComponent<RollSetting>().roomSelectCanvas.SetActive(true);
            GameObject.FindGameObjectWithTag("roomSetting").GetComponent<RollSetting>().roomSettingCanvas.SetActive(false);

        }else {
            SceneManager.LoadScene(sceneName.ToString());
        }
    }
}
