﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/// <summary>
/// Lobbyに関するボタン等を管理
/// </summary>
public class LobbyButtonManager : MonoBehaviour
{

    //main
    public Button backButton;
    public Button menuButton;
    public GameObject menuPopUp;
    public NetworkManager networkManagerPrefab;

    //createRoom
    public Button createRoomButton;
    public GameObject roomSelectCanvasObj;
    public GameObject rollSettingCanvasObj;


    /// <summary>
    /// NetworkManagerのSetUpをする
    /// </summary>
    private void Awake() {

        GameObject network = GameObject.FindGameObjectWithTag("networkManager");
        if(network == null) {
            NetworkManager networkManager = Instantiate(networkManagerPrefab);
            networkManager.SetUp();
        } else {
            network.GetComponent<NetworkManager>().SetUp();
        }
    }


    private void Start() {
        backButton.onClick.AddListener(() => SceneStateManager.instance.NextScene(SCENE_TYPE.TITLE));
        menuButton.onClick.AddListener(MenuPopUp);
        createRoomButton.onClick.AddListener(CreateRoomButton);
    }

    /// <summary>
    /// メニューPopUpをインスタンスする
    /// </summary>
    public void MenuPopUp() {
        Instantiate(menuPopUp);
    }

    public void CreateRoomButton() {
        roomSelectCanvasObj.SetActive(false);
        rollSettingCanvasObj.SetActive(true);
    }
}