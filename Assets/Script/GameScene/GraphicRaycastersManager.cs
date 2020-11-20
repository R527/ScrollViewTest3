using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// ゲームシーンにあるグラフィックレイキャストのオンオフを制御
/// </summary>
public class GraphicRaycastersManager : MonoBehaviour
{

    public static GraphicRaycastersManager instance;

    public GraphicRaycaster[] graphicRaycasters;
    public Button openBtn;
    public Button closeBtn;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// ゲームシーンにある上部のGraphicRaycasterを切る
    /// </summary>
    public virtual void SwitchGraphicRaycasters(bool isSwitch) {
        graphicRaycasters[0].enabled = isSwitch;
        graphicRaycasters[1].enabled = isSwitch;
    }
}
