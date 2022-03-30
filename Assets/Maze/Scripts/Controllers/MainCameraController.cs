using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// カメラ操作用プログラム
/// メインカメラのオブジェクトにアタッチして使う
/// </summary>
public class MainCameraController : MonoBehaviour
{
    //カメラのTransformとCameraコンポーネント
    Transform tf;
    Camera cam;

    //カメラの移動速度（定数）
    private const float CAMERASPEED = 0.02f;

    //カメラのサイズ変更・座標移動の上限値・下限値（定数）
    private const int CAMERALIMIT_SIZEMIN = 2;
    private const int CAMERALIMIT_SIZEMAX = 9;
    private const int CAMERALIMIT_X = 5;
    private const int CAMERALIMIT_Y = 5;

    //カメラが任意で操作可能な状態であるかどうか
    public bool CanManualControll = true;

    //マウスをドラッグしているかどうか
    private bool IsMouseWheelDown;

    //マウスドラッグ中の座標
    private Vector3 mouseDragPos;



    //以下、ゴール演出に関わる各種変数はインスペクター上で調整可能にする

    //ゴール演出の必要時間
    [SerializeField]
    private float goalEffectTime = 3.0f;

    //ゴール演出の処理分割回数
    [SerializeField]
    private int loopTimes = 100;

    //ゴール演出中、対象にするオブジェクト
    private GameObject targetGameObject;

    //ゴール演出時、対象オブジェクトからの座標のずれを決めるピボット
    [SerializeField]
    private Vector3 goalEffectPivot = new Vector3(0, 2, -10);

    //ゴール演出の目標カメラサイズ
    [SerializeField]
    private float targetCameraSize = 7.0f;




    /// <summary>
    /// カメラのコンポーネントを取得
    /// </summary>
    void Start()
    {
        tf = this.gameObject.GetComponent<Transform>();
        cam = this.gameObject.GetComponent<Camera>();
    }


    /// <summary>
    /// 毎フレームのキー操作を取得し、それに対応したカメラ操作を行う
    /// </summary>
    void Update()
    {
        //手動操作禁止中は何もしない
        if (!CanManualControll)
        {
            return;
        }

        //カメラの前後左右操作のスピードは、基本の速度にカメラのサイズを乗算することで調整
        float cameraMove = CAMERASPEED * cam.orthographicSize;

        //キーボードによる操作：押しているキーに応じてカメラを移動させる
        if (Input.GetKey(KeyCode.Q))
        {
            cam.orthographicSize -= CAMERASPEED; //ズームイン
        }
        else if (Input.GetKey(KeyCode.E))
        {
            cam.orthographicSize += CAMERASPEED; //ズームアウト
        }

        if (Input.GetKey(KeyCode.W))
        {
            tf.position += new Vector3(0.0f, cameraMove, 0.0f); //カメラを上へ移動
        }
        else if (Input.GetKey(KeyCode.S))
        {
            tf.position += new Vector3(0.0f, -cameraMove, 0.0f); //カメラを下へ移動
        }

        if (Input.GetKey(KeyCode.A))
        {
            tf.position += new Vector3(-cameraMove, 0.0f, 0.0f); //カメラを左へ移動
        }
        else if (Input.GetKey(KeyCode.D))
        {
            tf.position += new Vector3(cameraMove, 0.0f, 0.0f); //カメラを右へ移動
        }

        
        //マウスによる操作：マウスホイールを押しながらドラッグすることでカメラを移動させる
        if (Input.GetMouseButtonDown(2))
        {
            OnMouseWheelTouchDown();
        }
        else if (Input.GetMouseButton(2))
        {
            OnMouseWheelTouchDrug();
        }
        else if (Input.GetMouseButtonUp(2))
        {
            IsMouseWheelDown = false;
        }

        //マウスによる操作：マウスホイールを回転させることでカメラを拡縮する
        OnMouseWheelScroll();

        //制限範囲に収まっているかどうかチェック
        CheckCameraLimit();
    }

    /// <summary>
    /// カメラが制限範囲内に収まっていない場合、強制的に範囲内に移動させる
    /// </summary>
    private void CheckCameraLimit()
    {
        if (tf.position.x > CAMERALIMIT_X)
        {
            tf.position = new Vector3(CAMERALIMIT_X, tf.position.y, tf.position.z);
        }
        else if (tf.position.x < -CAMERALIMIT_X)
        {
            tf.position = new Vector3(-CAMERALIMIT_X, tf.position.y, tf.position.z);
        }

        if (tf.position.y > CAMERALIMIT_Y)
        {
            tf.position = new Vector3(tf.position.x, CAMERALIMIT_Y, tf.position.z);
        }
        else if (tf.position.y < -CAMERALIMIT_Y)
        {
            tf.position = new Vector3(tf.position.x, -CAMERALIMIT_Y, tf.position.z);
        }

        if (cam.orthographicSize < CAMERALIMIT_SIZEMIN)
        {
            cam.orthographicSize = CAMERALIMIT_SIZEMIN;
        }
        else if (cam.orthographicSize > CAMERALIMIT_SIZEMAX)
        {
            cam.orthographicSize = CAMERALIMIT_SIZEMAX;
        }
    }


    /// <summary>
    /// ゴール時のカメラ移動演出を開始する
    /// </summary>
    public void StartGoalEffect(GameObject _gameObject)
    {
        //カメラの手動操作を禁止
        CanManualControll = false;

        //対象にするオブジェクト情報を保持
        targetGameObject = _gameObject;

        StartCoroutine("GoalEffect");
    }


    /// <summary>
    /// ゴール時のカメラ演出
    /// 一定時間かけて対象オブジェクトへなめらかにカメラを近づける
    /// 
    /// なめらかにカメラを移動させるため、イージングを計算して小刻みに処理を行っている
    /// 問題点：WaitForSecondsはフレーム単位で判定を行っているため、各ループごとに1フレームの遅れが発生している可能性がある
    /// 出来るだけ想定している実時間に近づけるため、設定上の目標時間だけでなく、ループ回数によって想定される遅れの長さを加味して待機時間を設定する
    /// </summary>
    /// <returns></returns>
    IEnumerator GoalEffect()
    {
        //演出開始時のカメラの座標とサイズを保存
        Vector3 cameraOriginalPos = tf.position;
        float cameraOriginalSize = cam.orthographicSize;

        //目標の座標は、対象オブジェクトの座標＋ピボット
        Vector3 targetPosition = targetGameObject.transform.position + goalEffectPivot;


        Debug.Log(DateTime.Now);

        //指定の回数に分けて、待機とカメラの移動を繰り返す
        for (int i = 0; i < loopTimes; i++)
        {
            yield return new WaitForSeconds( (goalEffectTime - loopTimes / Application.targetFrameRate) /　loopTimes);

            //目標座標と元々の座標の差を使って基本移動量を求め、それにイージング用関数の値を乗算して移動中の座標を決定する
            //サイズも同様である
            float easing = easeOutCubic(i, loopTimes);
            tf.position = cameraOriginalPos + (targetPosition - cameraOriginalPos) * easing;
            cam.orthographicSize = cameraOriginalSize + (targetCameraSize - cameraOriginalSize) * easing;
        }

        Debug.Log(DateTime.Now);


        //演出終了時の処理


        //カメラの手動操作を許可
        CanManualControll = true;

        //オブジェクト情報を破棄
        targetGameObject = null;

        CheckCameraLimit();
    }



    /// <summary>
    /// OutCubicのイージングに基づいた変化度合いを返す
    /// Math.PowはDouble型に対する処理を前提にしているため、引数をDouble型で渡すことに注意
    /// </summary>
    /// <param name="_loop">現在のループ回数</param>
    /// <param name="_loopTimes">ループ目標回数</param>
    /// <returns></returns>
    private float easeOutCubic(double _loop, double _loopTimes)
    {
        return (float)(1 - Math.Pow(1 - _loop / _loopTimes, 3.0));
    }



    /// <summary>
    /// マウスホイールによるドラッグ機能の判定に利用する
    /// マウスホイールを押した時点でマウスがカメラの画面内にある場合のみ、ドラッグ機能を有効にし、その時点でのグローバル座標を保存する
    /// </summary>
    private void OnMouseWheelTouchDown()
    {
        Vector3 mousePosInViewport = cam.ScreenToViewportPoint(Input.mousePosition);

        Debug.Log(mousePosInViewport);

        if (mousePosInViewport.x < 0 || mousePosInViewport.x > 1 
            || mousePosInViewport.y < 0 || mousePosInViewport.y > 1)
        {
            return;
        }
        else
        {
            IsMouseWheelDown = true;
            mouseDragPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    /// <summary>
    /// ドラッグ中の処理
    /// カメラが制限範囲内に収まっている場合、前回呼び出した時からのマウスの移動量をカメラの座標に加算する
    /// </summary>
    private void OnMouseWheelTouchDrug()
    {
        //現在のマウスのグローバル座標を取得
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        //ドラッグ中でないか、マウス座標が変化していなければ何もしない
        if (!IsMouseWheelDown || mouseDragPos == mousePos)
        {
            return;
        }

        //現在のマウス座標を、記録されている基準点と比較してカメラの座標を移動
        tf.position += mouseDragPos - mousePos;

        //マウスのグローバル座標の基準点を更新する
        //（カメラから見たマウスの座標は、カメラが移動したことによって変動することに注意）
        mouseDragPos = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// マウスホイールの回転量に応じてカメラのサイズを変化させる
    /// </summary>
    private void OnMouseWheelScroll()
    {
        //マウスがカメラ画面内に入っていない場合は何もしない
        var mousePosInViewport = cam.ScreenToViewportPoint(Input.mousePosition);
        if (mousePosInViewport.x < 0 || mousePosInViewport.x > 1
            || mousePosInViewport.y < 0 || mousePosInViewport.y > 1)
        {
            return;
        }

        //ホイールの回転量を取得
        float scroll = Input.mouseScrollDelta.y;

        //回転量をもとにカメラのサイズを変化させる
        //注意点：カメラのサイズが大きいほどオブジェクトが小さく見える。奥にホイールを回すとInput.mouseScrollDeltaはプラスの値を取る。
        cam.orthographicSize -= cam.orthographicSize * scroll * CAMERASPEED;

    }
}