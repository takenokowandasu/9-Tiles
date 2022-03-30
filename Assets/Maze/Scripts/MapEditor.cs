using UnityEngine;
using Grobal;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// マウス操作をマップに反映する機能を提供するクラス
/// </summary>
public class MapEditor : MonoBehaviour
{
    [SerializeField]
    private MapInformation mapInformation;

    [SerializeField]
    private TileSelector tileSelector;

    //マップが利用するグリッド
    [SerializeField]
    private Grid grid;

    //マップが表示されるカメラ
    [SerializeField]
    private Camera targetCamera;

    [SerializeField]
    private SoundManager soundManager;

    [SerializeField]
    private PlayerController playerController;



    //Historyを持つ上限個数
    private const int HISTORYSIZE = 10;

    /// <summary>
    /// マップ情報の記録を持たせる。Undo機能に使用する
    /// ProgramEditorのHistoryと同様の機能なので統合
    /// </summary>
    private class History
    {
        public int[] mapsize = new int[2];
        public int[,] mapData;
    }
    //スタックでマップ情報の履歴を管理する
    private Stack<History> historyStack = new Stack<History>();


    private void Start()
    {
        //初期状態を履歴に残す
        PushHistory();
    }

    /// <summary>
    /// スタックから、1つ前の状況のHistoryを取り出し、情報をプログラムマップに反映する
    /// ボタンから呼び出す
    /// </summary>
    public void LoadHistory()
    {
        //HistoryStackには常に「現在の状況」が一番上に格納されているので、
        //一つ前の状況を呼び出すには1つPopさせてからPeekすればいい
        if (historyStack.Count < 2)
        {
            Debug.Log("No stack");
        }
        else
        {
            historyStack.Pop();
            History history = historyStack.Peek();

            //マップサイズが変更されている場合は何もしない
            if (!mapInformation.mapsize.SequenceEqual(history.mapsize))
            {
                Debug.Log("Mapsize is not equal");
                return;
            }

            //1マスずつ読み取って設定
            for (int x = 0; x < mapInformation.mapsize[0]; x++)
            {
                for (int y = 0; y < mapInformation.mapsize[1]; y++)
                {
                    tileSelector.SetTile(x, y, mapInformation.mapData, mapInformation.mapTilemap,
                        tileSelector.idToTileNameDictionary[history.mapData[x, y]]);
                }
            }

            playerController.SetStartPosition();
        }
    }

    /// <summary>
    /// 現在のマップ状況を元に、スタックへHistoryを追加する
    /// </summary>
    private void PushHistory()
    {
        History history = new History();
        history.mapsize = new int[2] { mapInformation.mapsize[0], mapInformation.mapsize[1] };
        history.mapData = new int[mapInformation.mapsize[0], mapInformation.mapsize[1]];

        Debug.Log("Push data");

        for (int x = 0; x < mapInformation.mapsize[0]; x++)
        {
            for (int y = 0; y < mapInformation.mapsize[1]; y++)
            {
                history.mapData[x, y] = mapInformation.mapData[x, y];
            }
        }

        historyStack.Push(history);

        //この時、historyStackの中身が上限を超えている場合、
        //一旦別のスタックを生成してから一部を書き戻すことでデータを減らす
        if (historyStack.Count > HISTORYSIZE)
        {
            Debug.Log("Delete oldest history");

            //コンストラクタでスタックをコピー。この時要素の順番は逆になる
            Stack<History> tempHistoryStack = new Stack<History>(historyStack);

            //一度Popしてからコピーし直すと、一番古い物が消えた状態でもとに戻る
            tempHistoryStack.Pop();
            historyStack = new Stack<History>(tempHistoryStack);
        }

    }


    /// <summary>
    /// スタック内の最新のHistoryが、現在のプログラムマップと一致しないかどうかを調べる
    /// </summary>
    /// <returns>一致しない場合はtrue、一致する場合はfalse</returns>
    private bool NotTheTopHistoryIsSame()
    {
        //スタック内にHistoryが何もない場合、何もせずTrueを返す
        if (historyStack.Count < 1)
        {
            Debug.Log("Check:No stack");
            return true;
        }
        else
        {
            Debug.Log("Check:GetStack");
            //最新のHistoryをデータだけ取り出して比較
            History history = historyStack.Peek();

            //falseを返すのはマップ全てが完全に一致する場合だけ
            //マップサイズは変更される可能性があることに注意
            if (mapInformation.mapsize.SequenceEqual(history.mapsize))
            {
                return !(mapInformation.mapData.Cast<int>().SequenceEqual(history.mapData.Cast<int>()));
            }
            else
            {
                return true;
            }
        }
    }




    void Update()
    {
        //マウスを左クリックすると、クリックした位置にTileSelectorが保持しているタイルを設定する
        if (Input.GetMouseButtonDown(0))
        {
            //マウスの座標をカメラ上の座標に変換、更にGrid上の座標に変換する
            Vector3Int gridPos = grid.WorldToCell(targetCamera.ScreenToWorldPoint(Input.mousePosition));

            //クリックしたGrid上の座標をマップデータの配列と一致させる
            int x = gridPos.x - mapInformation.origin[0];
            int y = gridPos.y - mapInformation.origin[1];

            //xかyが配列の要素数を超えているか、外周の壁を操作しようとしていたらキャンセル
            //値が無意味な場合もキャンセル
            if (x <= 0 || mapInformation.mapsize[0] - 1 <= x
                || y <= 0 || mapInformation.mapsize[1] - 1 <= y
                || tileSelector.tileId < (int)GrobalConst.TileId.Floor
                || tileSelector.tileId > (int)GrobalConst.TileId.Goal)
            {
                return;
            }

            //tileSelectorへ座標とマップ配列、タイルマップ、指定するtileIdの情報を渡して設定させる
            //スタート地点やゴール地点の重複判定などはそちらで処理
            tileSelector.SetTile(x, y,
                mapInformation.mapData,
                mapInformation.mapTilemap,
                tileSelector.idToTileNameDictionary[tileSelector.tileId]);

            //各種マップに変更が起きた場合、新しい状況をHistoryとして保存する
            if (NotTheTopHistoryIsSame())
            {
                soundManager.Play("TileSet");
                PushHistory();
            }

            playerController.SetStartPosition();
        }

        //Ctrl+ZでUndo
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && Input.GetKeyDown(KeyCode.Z))
        {
            LoadHistory();
            soundManager.Play("System_Button");
        }
    }
}
