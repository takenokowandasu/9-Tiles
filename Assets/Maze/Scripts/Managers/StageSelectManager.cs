using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 作者が用意した問題を遊んでもらうためのステージセレクト機能を提供する
/// </summary>

public class StageSelectManager : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private ModeManager modeManager;

    [SerializeField]
    private SaveLoadManager saveLoadManager;

    [SerializeField]
    private SoundManager soundManeger;

    [SerializeField]
    private MapInformation mapInformation;

    // 各ステージを選ぶボタンのプレハブ
    [SerializeField]
    private GameObject buttonPrefab = null;


    //生成したインスタンスとそのコンポーネント
    private GameObject buttonObj;
    private Button button;

    //クリア記録の表示用テキストの一覧
    private Text[] clearDataTextObjects = new Text[SaveLoadManager.STAGENUM];

    /// <summary>
    /// 各ステージを選ぶボタンを生成する
    /// </summary>
    void Awake()
    {
        saveLoadManager.Load();

        for (int stageId = 1; stageId <= SaveLoadManager.STAGENUM; stageId++)
        {
            //プレハブからボタンを子オブジェクトとして生成
            //位置関係は、このスクリプトがアタッチされているオブジェクトのGridLayoutGroupで調整することを想定
            buttonObj = Instantiate(buttonPrefab, this.transform.position , Quaternion.identity, gameObject.transform);

            //プレハブに予め取り付けたテキストコンポーネントを取得
            //ステージIDを設定
            Text stageIdTextObject = buttonObj.transform.Find("stageId").GetComponent<Text>();
            stageIdTextObject.text = stageId.ToString();

            //プレハブに予め取り付けたテキストコンポーネントを取得
            //クリア記録表示用テキストを配列へ格納
            clearDataTextObjects[stageId - 1] = buttonObj.transform.Find("DATA_STAGE").GetComponent<Text>();

            //クリックでステージ読み込みとSE再生が行われるようにイベントリスナーを設定
            //この時、ループのインデックスを引数として直接渡すと参照渡しになるので、一時変数を作成して値渡しにする
            int stageIdTemp = stageId;
            button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => this.GetStageData(stageIdTemp));
            button.onClick.AddListener(() => soundManeger.Play("System_Button"));
        }
    }

    
    /// <summary>
    /// Enableになるたび（ステージセレクト画面に入るたび）、各ステージのクリア記録表示を更新する
    /// </summary>
    private void OnEnable()
    {
        for (int i = 0; i < SaveLoadManager.STAGENUM; i++)
        {
            Debug.Log(i);

            int minChip = saveLoadManager.minChips[i];
            int minTurn = saveLoadManager.minTurns[i];
            int maxTurn = saveLoadManager.maxTurns[i];

            //データが初期値状態なら何も表示しないことに注意
            if (minChip == SaveLoadManager.INITIALVALUE)
            {
                clearDataTextObjects[i].text = "";
            }
            else
            {
                clearDataTextObjects[i].text = " " + minChip.ToString() + " / " + minTurn.ToString() + " / " + maxTurn.ToString();
            }

        }
    }


    /// <summary>
    /// mapDataに指定ファイルのデータを読み込ませる
    /// ファイルネームは、「STAGE」＋「引数で与える番号」で指定
    /// ボタンから呼び出せるようにpublicにしておく
    /// </summary>
    /// <param name="_stageId">指定したいステージのId</param>
    public void GetStageData(int _stageId)
    {
        string filename = "STAGE" + _stageId.ToString();
        Debug.Log(filename);

        //ステージセレクト用のプレイモード移行
        modeManager.SetMode("ProgramEdit");

        //マップデータを読み込ませる
        mapInformation.InitByFile(filename);

        //プレイヤーキャラの位置を初期化
        playerController.SetStartPosition();
    }
}
