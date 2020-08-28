using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
[RequireComponent(typeof(Camera))]
public class StableAspect : MonoBehaviour {

    public static StableAspect instance;
    private Camera cam;

    // 固定したい表示サイズ
    private float width = 203;
    private float height = 406f;

    // 画像のPixel Per Unit
    private float pixelPerUnit = 200f;

    public Text debugText;

    //カメラのSize設定が height / 2 / picelParUnit である必要がある
    //picelParUnitが 200 で height が 1920 なら カメラのサイズは 4.8になる。

    void Awake() {

        //if (instance == null) {
        //    instance = this;
        //    DontDestroyOnLoad(gameObject);
        //} else {
        //    Destroy(gameObject);
        //}

        float aspect = (float)Screen.height / (float)Screen.width; //表示画面のアスペクト比
        float bgAcpect = height / width; //理想とするアスペクト比
        Debug.Log("aspect" + aspect);//1.994382
        Debug.Log("bgAcpect" + bgAcpect);//2
                                         // カメラコンポーネントを取得します
        cam = GetComponent<Camera>();

        // カメラのorthographicSizeを設定
        cam.orthographicSize = (height / 2f / pixelPerUnit);
        Debug.Log("cam.orthographicSize" + cam.orthographicSize);

        if (bgAcpect > aspect) {
            //画面が横に広いとき
            // 倍率
            float bgScale = height / Screen.height;
            Debug.Log("height" + height);
            Debug.Log("Screen.height" + Screen.height);
            // viewport rectの幅
            float camWidth = width / (Screen.width * bgScale);
            // viewportRectを設定
            cam.rect = new Rect((1f - camWidth) / 2f, 0f, camWidth, 1f);
            Debug.Log("bgScale" + bgScale);
            Debug.Log("camWidth" + camWidth);
            Debug.Log("cam.rect" + cam.rect);

            //debugText.text = "画面が横に広い　height" + height + "Screen.height" + Screen.height + "bgScale" + bgScale + "camWidth" + camWidth + "cam.rect" + cam.rect;

        } else {
            //画面が縦に長い
            //想定しているアスペクト比とどれだけ差があるかを出す
            float bgScale = aspect / bgAcpect;

            // カメラのorthographicSizeを縦の長さに合わせて設定しなおす
            cam.orthographicSize *= bgScale;

            // viewportRectを設定
            cam.rect = new Rect(0f, 0f, 1f, 1f);

            //debugText.text = "画面が縦に長い　bgScale" + bgScale + "cam.orthographicSize" + cam.orthographicSize + "cam.rect" + cam.rect;
        }
    }


}
