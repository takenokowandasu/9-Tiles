using UnityEngine;
using Grobal;


/// <summary>
/// データのセーブ＆ロードを処理するクラス
/// WebGLでの公開を想定し、PlayerPrefsを利用する。
/// また、RPGアツマールへの公開時にはプラグインを介してサーバーの持つセーブデータを操作する。
/// ※スタンドアロン版だと、PlayerPrefsが直接レジストリにアクセスしてしまい不都合が多い。
///   もしそれを作る場合はJSONでローカルに保存するほうがよい
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    [SerializeField]
    private ProgramEditor programEditor;

    [SerializeField]
    private PlayerManualController charaManualController;

    [SerializeField]
    private MapInformation mapInformation;

    [SerializeField]
    private ProgramExecutor programExecutor;


    //作者が用意したステージの数
    public const int STAGENUM = 16;

    //各ステージの最小チップ数記録
    public int[] minChips;
    //各ステージの最小/最大ターン数記録
    public int[] minTurns;
    public int[] maxTurns;

    //未クリアステージの記録初期値
    public const int INITIALVALUE = 999;


    /// <summary>
    /// セーブデータを読み込む
    /// SyncSaveDataAsync() 関数を待機するためには、関数に async 定義が必要
    /// </summary>
    private async void Awake()
    {
        Load();
    }






    /// <summary>
    /// セーブデータを読み込む
    /// </summary>
    public void Load()
    {
        //データ初期化
        minChips = new int[STAGENUM];
        minTurns = new int[STAGENUM];
        maxTurns = new int[STAGENUM];

        //データ読み込み
        //KeyはID*100に変数ごとの連番で指定する
        int loadKey;
        for (int i = 1; i <= STAGENUM; i++)
        {
            loadKey = i * 100;

            // 指定のキーのセーブデータがあれば読み込み、無ければ初期値を設定
            minChips[i - 1] = PlayerPrefs.GetInt((loadKey + 1).ToString(), INITIALVALUE);
            minTurns[i - 1] = PlayerPrefs.GetInt((loadKey + 2).ToString(), INITIALVALUE);
            maxTurns[i - 1] = PlayerPrefs.GetInt((loadKey + 3).ToString(), -INITIALVALUE);

            Debug.Log(string.Format("stage:{0} minChips:{1} minTurns:{2} maxTurns:{3}", i, minChips[i - 1], minTurns[i - 1], maxTurns[i - 1]));
        }
    }




    /// <summary>
    /// ステージクリア時に呼び出し、その際のデータを保存する
    /// </summary>
    public void Save()
    {
        //作者が用意したステージかどうかは、mapData上の情報で判定
        bool IsMap;
        int mapId;
        (IsMap, mapId) = mapInformation.GetMapId();
        
        //作者が用意したものでない場合、セーブは行わない
        //ゴールした際にマニュアルモードだった場合も同様
        if (!IsMap || charaManualController.gameObject.activeInHierarchy)
        {
            return;
        }

        //プログラムマップ上のチップ数を確認
        int chips = 0;
        for (int x = 0; x < GrobalConst.ProgramMapSize; x++)
        {
            for (int y = 0; y < GrobalConst.ProgramMapSize; y++)
            {
                if (programEditor.programMap[x, y] != (int)GrobalConst.TileId.Empty)
                {
                    chips++;
                }
            }
        }

        //プログラムの実行にかかったターン数を確認
        int turn = programExecutor.turn;


        //ステージIDに合わせてKeyの基準値を設定する。各変数を連番で記録する
        int saveKey = mapId * 100;

        if (minChips[mapId - 1] > chips)
        {
            PlayerPrefs.SetInt((saveKey + 1).ToString(), chips);
        }

        if (minTurns[mapId - 1] > turn)
        {
            PlayerPrefs.SetInt((saveKey + 2).ToString(), turn);
        }
        if (maxTurns[mapId - 1] < turn)
        {
            PlayerPrefs.SetInt((saveKey + 3).ToString(), turn);
        }

        PlayerPrefs.Save();

        //保持している変数を記録と同期
        minChips[mapId - 1] = PlayerPrefs.GetInt((saveKey + 1).ToString(), INITIALVALUE);
        minTurns[mapId - 1] = PlayerPrefs.GetInt((saveKey + 2).ToString(), INITIALVALUE);
        maxTurns[mapId - 1] = PlayerPrefs.GetInt((saveKey + 3).ToString(), -INITIALVALUE);


        Debug.Log(string.Format("stage:{0} chips:{1} turns:{2}", mapId, chips, turn));
        Debug.Log(string.Format("stage:{0} minChips:{1} minTurns:{2} maxTurns:{3}", 
            mapId, minChips[mapId - 1], minTurns[mapId - 1], maxTurns[mapId - 1]));
    }

}
