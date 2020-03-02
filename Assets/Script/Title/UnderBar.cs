using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


/// <summary>
/// アンダーバーのボタンを制御
/// </summary>
public class UnderBar : MonoBehaviour
{

    //class

    //main
    public RectTransform contentRectTransform;

    public void MovingShopContent() {
        contentRectTransform.DOLocalMoveX(-104.5f, 0.4f);
    }

    public void MovingHomeContent() {
        contentRectTransform.DOLocalMoveX(-313.5f, 0.4f);
    }

    public void MovingFriendContent() {
        contentRectTransform.DOLocalMoveX(-522.5f, 0.4f);
    }
}
