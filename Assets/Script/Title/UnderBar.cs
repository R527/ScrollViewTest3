﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


/// <summary>
/// アンダーバーのボタンを制御
/// </summary>
public class UnderBar : MonoBehaviour
{

    //class
    public PlayerInfoButton playerInfoButton;

    //main
    public RectTransform contentRectTransform;

    /// <summary>
    /// Shopに移動
    /// </summary>
    public void MovingShopContent() {
        contentRectTransform.DOLocalMoveX(-101.5f, 0.4f);
    }

    /// <summary>
    /// Homeに移動
    /// </summary>
    public void MovingHomeContent() {
        //contentRectTransform.DOLocalMoveX(-313.5f, 0.4f);
        contentRectTransform.DOLocalMoveX(-305f, 0.4f);
        playerInfoButton.UpdateCurrencyText();
        PlayerManager.instance.SetSubscribe();
    }

    //public void MovingFriendContent() {
    //    //contentRectTransform.DOLocalMoveX(-522.5f, 0.4f);
    //    contentRectTransform.DOLocalMoveX(-507.5f, 0.4f);

    //}
}
