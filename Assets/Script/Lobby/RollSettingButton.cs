using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// RollSettingにあるボタン情報を取得する
/// </summary>
public class RollSettingButton : MonoBehaviour
{
    public ROLLTYPE rollType;
    public Text numText;
    [Header ("trueならプラス")]
    public bool plusOrMinus;
}
