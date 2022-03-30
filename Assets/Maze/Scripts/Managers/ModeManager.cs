using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


/// <summary>
/// ゲーム内のモードを切り替える機能を提供するクラス
/// </summary>
public class ModeManager : MonoBehaviour
{
    //コルーチン1ループの実行間隔
    [SerializeField]
    private static float span = 3.0f;

    [Serializable]
    /// <summary>
    /// ゲーム内のオブジェクトの切り替えを正確に行うため、
    /// 以下にモード名を列挙し、これを用いて現在のモードの判定等を行う
    /// </summary>
    public enum Mode
    {
        /// <summary>
        /// タイトル画面
        /// 基本的にどのモードからでもUIで戻ってこられる
        /// </summary>
        Title,

        /// <summary>
        /// ステージセレクト画面
        /// 作者が用意したステージをプレイできる
        /// </summary>
        StageSelect,

        /// <summary>
        /// プログラムマップ編集画面
        /// この状態からプログラムの実行、手動操作モードに移行できる
        /// </summary>
        ProgramEdit,

        /// <summary>
        /// 手動操作モード
        /// プレイヤーキャラをマップ内で操作できる
        /// ゴール演出も見られるが、クリアデータの記録はされなくなる
        /// </summary>
        ManualPlay,

        /// <summary>
        /// プログラム実行モード
        /// プレイヤーキャラを編集したプログラムマップに従って行動させる
        /// ゴールした場合はクリアデータが記録される
        /// </summary>
        ProgramExecute,

        /// <summary>
        /// ゴール演出モード
        /// 一定時間、操作を禁止して演出を表示する
        /// </summary>
        GoalEffect,

        /// <summary>
        /// マップエディットモード
        /// プレイヤーが自由にマップを編集でき、実際にプレイすることもできる
        /// </summary>
        MapEdit,

        /// <summary>
        /// マップデータをCSV形式で表示するモード
        /// </summary>
        CsvView,

        /// <summary>
        /// マップデータをCSV形式で入力し、読み込ませることができるモード
        /// </summary>
        CsvLoad,
    }

    //現在のモードを記録する変数
    [SerializeField]
    private Mode currentMode;

    //プログラム編集モードからマップエディットモードへ遷移するためのボタンをアクティブにするかどうか
    private bool CanGoToMapEditMode = false;

    //名前とゲームオブジェクトの対応を取る辞書
    private Dictionary<string, GameObject> gameObjectDictionary = new Dictionary<string, GameObject>();


    /// <summary>
    /// ゲーム起動時はタイトル画面の状態にする
    /// その他初期設定を済ませる
    /// </summary>
    public void Start()
    {
        //ターゲットフレームレート設定
        Application.targetFrameRate = 60;

        //ゲーム開始時に存在する全てのオブジェクトをディクショナリに登録する
        //ユニークな機能を持つオブジェクトにはヒエラルキー上でそれぞれ名称を割り当てておく
        //名称が被った場合はスキップされる
        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform childObj in obj.transform.GetComponentsInChildren<Transform>(true))
            {
                Debug.Log(childObj.gameObject.name);
                if(gameObjectDictionary.ContainsKey(childObj.gameObject.name))
                {
                    continue;
                }
                gameObjectDictionary.Add(childObj.gameObject.name, childObj.gameObject);
            }
        }

        SetMode("Title");

    }

    /// <summary>
    /// 辞書に登録したオブジェクトがアクティブかどうかを切り替える
    /// 不正な名称であればエラーを返すため、デバッグに役立つ
    /// </summary>
    /// <param name="_objName">設定したいオブジェクトの名称</param>
    /// <param name="_IsActive">アクティブにするかどうか</param>
    private void SetActive(string _objName, bool _IsActive)
    {
        if (gameObjectDictionary.TryGetValue(_objName, out GameObject obj))
        {
            obj.SetActive(_IsActive);
            //Debug.Log(obj.name + " " + obj.activeSelf);
        }
        else
        {
            Debug.LogWarning($"その名称は登録されていません:{_objName}");
        }
    }


    /// <summary>
    /// ゲームの画面や大まかな状態を切り替える
    /// </summary>
    /// <param name="_newModeName">遷移したいモードの名称。Modeに含まれている必要がある</param>
    public void SetMode(string _newModeName)
    {
        Debug.LogFormat("current mode:{0}", currentMode.ToString());

        //もし不正なモード名であれば何もしない
        if (!Enum.TryParse(_newModeName, out Mode newMode))
       {
            Debug.Log("Ellegal modename");
            return;
       }

       switch (newMode)
       {
            case Mode.Title:
                //状況をリセットするため、まずゲーム内の必須オブジェクト群を除く全オブジェクトを非アクティブにする
                foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    if (obj.name == "EventSystem"
                        || obj.name == "UICamera"
                        || obj.name == "MainCamera"
                        || obj.name == "UICanvas"
                        || obj.name == "GrobalManagers"
                        || obj.name == "MapInformation")
                    {
                        obj.SetActive(true);
                    }
                    else
                    {
                        obj.SetActive(false);
                    }
                }

                //タイトル画面用のキャンバスをアクティブにする
                SetActive("TitleCanvas", true);

                //右上のUIボタンは非アクティブにする
                SetActive("ReturnTitleButton", false);
                SetActive("ReturnStageSelectButton", false);
                SetActive("RuleExplanationStartButton", false);
                SetActive("RuleExplanationViewButton", false);

                //ルール表示中のボタンは非アクティブにする
                SetActive("TitleRuleExplanationViewButton", false);

                //メインカメラの手動操作をオフにする
                gameObjectDictionary["MainCamera"].GetComponent<MainCameraController>().CanManualControll = false;

                currentMode = Mode.Title;

                break;


            case Mode.StageSelect:
                if (currentMode != Mode.Title
                    && currentMode != Mode.ProgramEdit
                    && currentMode != Mode.ProgramExecute
                    && currentMode != Mode.ManualPlay)
                {
                    Debug.LogFormat("Ellegal mode transition:{0}", currentMode.ToString());
                    break;
                }
                //直前に使われていた可能性のあるキャンバスを非アクティブにする
                SetActive("ProgramEditCanvas", false);
                SetActive("ManualModeCanvas", false);
                SetActive("TitleCanvas", false);

                //右上のUIボタンは非アクティブにする
                SetActive("ReturnTitleButton", false);
                SetActive("ReturnStageSelectButton", false);
                SetActive("RuleExplanationStartButton", false);
                SetActive("RuleExplanationViewButton", false);

                //マップ表示用グリッドを非表示にする
                SetActive("MapTilemapGrid", false);

                //ステージセレクト画面用のキャンバスをアクティブにする
                SetActive("StageSelectCanvas", true);

                //プログラム実行中なら強制終了
                gameObjectDictionary["ProgramExecutor"].GetComponent<ProgramExecutor>().EndExecutionmode();

                //メインカメラの手動操作をオフにする
                gameObjectDictionary["MainCamera"].GetComponent<MainCameraController>().CanManualControll = false;

                //プレイヤーを非アクティブにする
                SetActive("Player", false);

                currentMode = Mode.StageSelect;

                break;



            case Mode.ProgramEdit:
                if (currentMode != Mode.StageSelect
                    && currentMode != Mode.MapEdit
                    && currentMode != Mode.ManualPlay
                    && currentMode != Mode.ProgramExecute
                    && currentMode != Mode.GoalEffect)
                {
                    Debug.LogFormat("Ellegal mode transition:{0}", currentMode.ToString());
                    break;
                }

                //ステージセレクトモードから遷移してきた場合はマップエディットモードへの遷移を禁じる
                //マップエディットモードから遷移してきた場合は許可する
                //この2つ以外から遷移してきた場合は、元々どちらから遷移してきたか分からないためそのままにする
                CanGoToMapEditMode = (currentMode == Mode.StageSelect) ? false : CanGoToMapEditMode;
                CanGoToMapEditMode = (currentMode == Mode.MapEdit)     ? true  : CanGoToMapEditMode;

                //プログラムマップ編集画面に入る前に使われていた可能性のあるキャンバスを非アクティブにする
                SetActive("StageSelectCanvas", false);
                SetActive("MapEditCanvas", false);
                SetActive("ManualModeCanvas", false);
                SetActive("GoalCanvas", false);

                //プログラムマップ編集画面用のキャンバスをアクティブにする
                SetActive("ProgramEditCanvas", true);

                //タイトル画面/ステージセレクトに戻るボタンとルール表示ボタンをアクティブにする
                SetActive("ReturnTitleButton", true);
                SetActive("RuleExplanationStartButton", true);
                SetActive("ReturnStageSelectButton", true);

                //マップの描画をアクティブにする
                SetActive("MapTilemapGrid", true);

                //プログラム実行中なら強制終了
                gameObjectDictionary["ProgramExecutor"].GetComponent<ProgramExecutor>().EndExecutionmode();

                //このキャンバス内のゲームオブジェクトを一旦全てアクティブにする
                foreach (Transform obj in gameObjectDictionary["ProgramEditCanvas"].transform.GetComponentsInChildren<Transform>(true))
                {
                    obj.gameObject.SetActive(true);
                }
                //プログラム実行モード終了用のボタンだけは非アクティブ
                SetActive("ProgramEndButton", false);
                //マップエディットモードへ遷移するボタンは場合による
                SetActive("MapEditModeButton", CanGoToMapEditMode);

                //メインカメラの手動操作を許可
                gameObjectDictionary["MainCamera"].GetComponent<MainCameraController>().CanManualControll = true;

                //設定するタイルを選択するためのボタンに背景枠をつける
                gameObjectDictionary["TileSelector"].GetComponent<TileSelector>().SetBackground(gameObjectDictionary["ProgramSelectButtons"]);

                //プレイヤーキャラを表示、位置を初期化
                SetActive("Player", true);
                gameObjectDictionary["Player"].GetComponent<PlayerController>().SetStartPosition();

                currentMode = Mode.ProgramEdit;

                break;




            case Mode.ManualPlay:
                if (currentMode != Mode.ProgramEdit)
                {
                    Debug.LogFormat("Ellegal mode transition:{0}", currentMode.ToString());
                    break;
                }

                //プログラムマップ編集画面用のキャンバスを非アクティブにする
                SetActive("ProgramEditCanvas", false);

                //手動操作モード用のキャンバスをアクティブにする
                SetActive("ManualModeCanvas", true);

                //このキャンバス内のゲームオブジェクトを一旦全てアクティブにする
                foreach (Transform obj in gameObjectDictionary["ManualModeCanvas"].transform.GetComponentsInChildren<Transform>(true))
                {
                    obj.gameObject.SetActive(true);
                }

                currentMode = Mode.ManualPlay;

                break;



            case Mode.ProgramExecute:
                if (currentMode != Mode.ProgramEdit)
                {
                    Debug.LogFormat("Ellegal mode transition:{0}", currentMode.ToString());
                    break;
                }
                //以下、ProgramEditCanvasの一部を操作していく

                //プログラム編集用のボタンは非表示にしたい
                SetActive("ProgramEditUIButtons", false);

                //プログラム編集を禁止
                SetActive("ProgramEditor", false);

                //プログラム実行を終えるボタンはアクティブにしたい
                SetActive("ProgramEndButton", true);

                //プレイヤーキャラの位置を初期化
                gameObjectDictionary["Player"].GetComponent<PlayerController>().SetStartPosition();

                //プログラム実行を開始
                gameObjectDictionary["ProgramExecutor"].GetComponent<ProgramExecutor>().SetExecutionmode();

                currentMode = Mode.ProgramExecute;

                break;



            case Mode.GoalEffect:
                if (currentMode != Mode.ManualPlay
                    && currentMode != Mode.ProgramExecute)
                {
                    Debug.LogFormat("Ellegal mode transition:{0}", currentMode.ToString());
                    break;
                }
                //ゴール演出に入る前に使われていた可能性のあるキャンバスを非アクティブにする
                SetActive("ProgramEditCanvas", false);
                SetActive("ManualModeCanvas", false);

                //プログラム実行でゴールしたときのみ、セーブ
                if (currentMode == Mode.ProgramExecute)
                {
                    gameObjectDictionary["SaveLoadManager"].GetComponent<SaveLoadManager>().Save();
                }

                //プログラムマップ編集画面用のキャンバスを非アクティブにする
                SetActive("ProgramEditCanvas", false);

                //右上のUIボタンを非アクティブにする
                SetActive("ReturnTitleButton", false);
                SetActive("RuleExplanationStartButton", false);
                SetActive("ReturnStageSelectButton", false);

                StartCoroutine("GoalDirecting");

                //カメラ側のゴール演出を呼び出す
                gameObjectDictionary["MainCamera"].GetComponent<MainCameraController>().CanManualControll = false;
                gameObjectDictionary["MainCamera"].GetComponent<MainCameraController>().StartGoalEffect(gameObjectDictionary["Player"]);

                currentMode = Mode.GoalEffect;

                break;



            case Mode.MapEdit:
                if (currentMode != Mode.Title
                    && currentMode != Mode.ProgramEdit
                    && currentMode != Mode.CsvView
                    && currentMode != Mode.CsvLoad)
                {
                    Debug.LogFormat("Ellegal mode transition:{0}", currentMode.ToString());
                    break;
                }
                //使われていた可能性のあるキャンバスを非アクティブにする
                SetActive("TitleCanvas", false);
                SetActive("ProgramEditCanvas", false);

                //マップ編集画面用のキャンバスをアクティブにする
                SetActive("MapEditCanvas", true);

                //マップの描画をアクティブにする
                SetActive("MapTilemapGrid", true);

                //タイトル画面に戻るボタンとルール表示ボタンをアクティブにする
                SetActive("ReturnTitleButton", true);
                SetActive("RuleExplanationStartButton", true);

                //プレイヤーキャラを表示、位置を初期化
                SetActive("Player", true);
                gameObjectDictionary["Player"].GetComponent<PlayerController>().SetStartPosition();

                //このキャンバス内のゲームオブジェクトを一旦全てアクティブにする
                foreach (Transform obj in gameObjectDictionary["MapEditCanvas"].transform.GetComponentsInChildren<Transform>(true))
                {
                    obj.gameObject.SetActive(true);
                }
                //開発用機能に対応するボタンは非アクティブ
                SetActive("ButtonsForGameDevelopment", false);
                //CSV形式での表示/読み込みに対応するオブジェクトは非アクティブ
                SetActive("CsvViewer", false);
                SetActive("CsvLoader", false);

                //設定するタイルを選択するためのボタンに背景枠をつける
                gameObjectDictionary["TileSelector"].GetComponent<TileSelector>().SetBackground(gameObjectDictionary["TileSelectButtons"]);

                //メインカメラの手動操作を許可
                gameObjectDictionary["MainCamera"].GetComponent<MainCameraController>().CanManualControll = true;

                currentMode = Mode.MapEdit;

                break;



            case Mode.CsvView:
                if (currentMode != Mode.MapEdit)
                {
                    Debug.LogFormat("Ellegal mode transition:{0}", currentMode);
                    break;
                }
                //マップの編集に対応するオブジェクトを非アクティブにする
                SetActive("ObjectsForTilemapEdit", false);

                //CSV形式での表示/読み込みに対応するオブジェクトをアクティブにする
                SetActive("CsvViewer", true);

                //CSV形式でマップデータを表示させる
                gameObjectDictionary["MapInformation"].GetComponent<MapInformation>().
                    ViewCsvData(gameObjectDictionary["CsvInputField"].GetComponent<InputField>());

                //メインカメラの手動操作を禁止
                gameObjectDictionary["MainCamera"].GetComponent<MainCameraController>().CanManualControll = false;

                currentMode = Mode.CsvView;

                break;



            case Mode.CsvLoad:
                if (currentMode != Mode.MapEdit)
                {
                    Debug.LogFormat("Ellegal mode transition:{0}", currentMode);
                    break;
                }
                //マップの編集に対応するオブジェクトを非アクティブにする
                SetActive("ObjectsForTilemapEdit", false);

                //CSV形式での表示/読み込みに対応するオブジェクトをアクティブにする
                SetActive("CsvLoader", true);

                //メインカメラの手動操作を禁止
                gameObjectDictionary["MainCamera"].GetComponent<MainCameraController>().CanManualControll = false;

                currentMode = Mode.CsvLoad;

                break;
        }
    }


    /// <summary>
    /// ゴール演出を制御する
    /// 一定時間ゴール演出を表示したあと、プログラムマップ編集モードに戻る
    /// </summary>
    /// <returns></returns>
    IEnumerator GoalDirecting()
    {
        while (true)
        {
            SetActive("GoalCanvas", true);

            yield return new WaitForSeconds(span);
            Debug.LogFormat("{0}秒経過", span);

            SetActive("GoalCanvas", false);

            //ここで自機の位置をスタート地点に戻す
            gameObjectDictionary["Player"].GetComponent<PlayerController>().SetStartPosition();

            SetMode("ProgramEdit");
            break;
        }
    }





}
