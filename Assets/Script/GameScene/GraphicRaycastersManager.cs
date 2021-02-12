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

    /// <summary>
    /// ゲームシーンにある上部のGraphicRaycasterを切る
    /// </summary>
    public void SwitchGraphicRaycasters(bool isSwitch) {
        graphicRaycasters[0].enabled = isSwitch;
        graphicRaycasters[1].enabled = isSwitch;
    }
}
