using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// バックボタンをまとめる
/// Baseが親、virtualがあると子供が細かい設定をすることができる
/// </summary>
public class BasePopUP : MonoBehaviour {

    public Button closeButton;
    [Header("trueならSetActive(False)")]
    public bool closeButtonSwich;

    /// <summary>
    /// 親子の間だけで使えるメソッド
    /// virtualがあると子供が上書きできる
    /// </summary>
    protected virtual void Start() {
        if (closeButtonSwich) {
            closeButton.onClick.AddListener(ClosePopUp);
        } else {
            closeButton.onClick.AddListener(DestroyPopUP);
        }
    }

    /// <summary>
    /// BackButtonのセットアップ
    /// </summary>
    public virtual void SetUp() {

    }

    /// <summary>
    /// 既にHierarchy上にあるObjを非表示にする
    /// </summary>
    public virtual void ClosePopUp() {
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// インスタンスされたObjを削除する
    /// </summary>
    public virtual void DestroyPopUP() {
        Destroy(this.gameObject);
    }
}

