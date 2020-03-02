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
    public void NextScene(SCENE_TYPE sceneName) {
        SceneManager.LoadScene(sceneName.ToString());
    }

}
