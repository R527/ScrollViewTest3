using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// バックボタンをまとめる
/// </summary>
public class BackButton : MonoBehaviour {


    public void Back() {
        this.gameObject.SetActive(false);
    }
}

