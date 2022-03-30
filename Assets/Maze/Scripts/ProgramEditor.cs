using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Grobal;


/// <summary>
/// 各種プログラムマップの情報と、クリックによるエディット機能を提供する
/// </summary>
public class ProgramEditor : MonoBehaviour
{

    //特殊処理に関わるId
    public const int ROTATE_MODE_RED = 100;
    public const int ROTATE_MODE_BLUE = 101;

    //プログラムマップ原点のグリッド上の座標
    public int[] origin = new int[] { 0, 0 };

    //プログラムマップ上のチップの配置データ
    public int[,] programMap = new int[3,3];
    //プログラムマップ上のチップの遷移先を示す赤矢印の配置データ
    public int[,] programArrowMap = new int[3,3];
    //プログラムマップ上のチップ（If系チップのelseの場合）の遷移先を示す青矢印の配置データ
    public int[,] programIfArrowMap = new int[3, 3];
    //プログラムマップの背景と実行開始位置 / 実行中の位置を示す配置データ
    public int[,] programBaseMap = new int[3, 3];


    //上記のプログラムマップ関連変数それぞれに対応するタイルマップ
    public Tilemap programBaseTilemap;
    public Tilemap programTilemap;
    public Tilemap programArrowTilemap;
    public Tilemap programIfArrowTilemap;

    //タイルマップが属するGrid
    [SerializeField]
    private Grid grid;

    //タイルマップが映るカメラ
    [SerializeField]
    private Camera targetCamera;

    [SerializeField]
    private TileSelector tileSelector;

    [SerializeField]
    private SoundManager soundManager;


    //Historyを持つ上限個数
    private const int HISTORYSIZE = 10;



    /// <summary>
    /// マップ情報の記録を持たせる。Undo機能に使用する
    /// </summary>
    private class History
    {
        public int[,] programMap = new int[3, 3];
        public int[,] programArrowMap = new int[3, 3];
        public int[,] programIfArrowMap = new int[3, 3];
        public int[,] programBaseMap = new int[3, 3];
    }
    //スタックでマップ情報の履歴を管理する
    private Stack<History> historyStack = new Stack<History>();



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

            //1マスずつ読み取って設定
            for (int x = 0; x < GrobalConst.ProgramMapSize; x++)
            {
                for (int y = 0; y < GrobalConst.ProgramMapSize; y++)
                {
                    tileSelector.SetTile(x, y, programBaseMap, programBaseTilemap,
                        tileSelector.idToTileNameDictionary[history.programBaseMap[x, y]]);

                    tileSelector.SetTile(x, y, programMap, programTilemap,
                        tileSelector.idToTileNameDictionary[history.programMap[x, y]]);

                    tileSelector.SetTile(x, y, programArrowMap, programArrowTilemap,
                        tileSelector.idToTileNameDictionary[history.programArrowMap[x, y]]);

                    tileSelector.SetTile(x, y, programIfArrowMap, programIfArrowTilemap,
                        tileSelector.idToTileNameDictionary[history.programIfArrowMap[x, y]]);
                }
            }
        }
    }

    /// <summary>
    /// 現在のマップ状況を元に、スタックへHistoryを追加する
    /// </summary>
    private void PushHistory()
    {
        History history = new History();

        Debug.Log("Push data");

        for (int x = 0; x < GrobalConst.ProgramMapSize; x++)
        {
            for (int y = 0; y < GrobalConst.ProgramMapSize; y++)
            {
                history.programBaseMap[x, y] = programBaseMap[x, y];
                history.programMap[x, y] = programMap[x, y];
                history.programArrowMap[x, y] = programArrowMap[x, y];
                history.programIfArrowMap[x, y] = programIfArrowMap[x, y];
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

            //falseを返すのは4つのマップ全てが完全に一致する場合だけ
            return !(programBaseMap.Cast<int>().SequenceEqual(history.programBaseMap.Cast<int>())
                && programMap.Cast<int>().SequenceEqual(history.programMap.Cast<int>())
                && programArrowMap.Cast<int>().SequenceEqual(history.programArrowMap.Cast<int>())
                && programIfArrowMap.Cast<int>().SequenceEqual(history.programIfArrowMap.Cast<int>()));
        }

    }


    /// <summary>
    /// プログラムマップを初期化する
    /// </summary>
    void Start()
    {
        //各種タイルマップの原点となる座標を設定する
        //このとき、GridLayout.CellToWorldを使うことでグリッドに沿って配置する
        //（そのままVector3を代入しようとした場合、親であるキャンバスやグリッドのサイズの影響で意図しない配置になる可能性が高い）
        programBaseTilemap.transform.position = grid.CellToWorld(new Vector3Int(origin[0], origin[1], 4));
        programTilemap.transform.position = grid.CellToWorld(new Vector3Int (origin[0], origin[1], 3) );
        programArrowTilemap.transform.position = grid.CellToWorld(new Vector3Int(origin[0], origin[1], 2));
        programIfArrowTilemap.transform.position = grid.CellToWorld(new Vector3Int(origin[0], origin[1], 1));


        //programTilemap.ClearAllTiles();
        //programBaseTilemap.ClearAllTiles();
        //programArrowTilemap.ClearAllTiles();
        //programIfArrowTilemap.ClearAllTiles();

        //プログラムマップを背景しかない状態に設定
        for (int x = 0; x < GrobalConst.ProgramMapSize; x++)
        {
            for (int y = 0; y < GrobalConst.ProgramMapSize; y++)
            {
                tileSelector.SetTile(x, y, programBaseMap, programBaseTilemap, GrobalConst.TileId.Base.ToString());
                tileSelector.SetTile(x, y, programMap, programTilemap, GrobalConst.TileId.Empty.ToString());
                tileSelector.SetTile(x, y, programArrowMap, programArrowTilemap, GrobalConst.TileId.Empty.ToString());
                tileSelector.SetTile(x, y, programIfArrowMap, programIfArrowTilemap, GrobalConst.TileId.Empty.ToString());
            }
        }
        tileSelector.SetTile(0, 0, programBaseMap, programBaseTilemap, GrobalConst.TileId.BaseStart.ToString());

        //初期状態を履歴に残す
        PushHistory();
    }


    /// <summary>
    /// プログラムマップの背景以外の全てを初期化する。
    /// ボタンで呼び出すことを想定
    /// </summary>
    public void AllClear()
    {
        //programTilemap.ClearAllTiles();
        //programArrowTilemap.ClearAllTiles();
        //programIfArrowTilemap.ClearAllTiles();
        for (int x = 0; x < GrobalConst.ProgramMapSize; x++)
        {
            for (int y = 0; y < GrobalConst.ProgramMapSize; y++)
            {
                tileSelector.SetTile(x, y, programMap, programTilemap, GrobalConst.TileId.Empty.ToString());
                tileSelector.SetTile(x, y, programArrowMap, programArrowTilemap, GrobalConst.TileId.Empty.ToString());
                tileSelector.SetTile(x, y, programIfArrowMap, programIfArrowTilemap, GrobalConst.TileId.Empty.ToString());

            }
        }
    }





    /// <summary>
    /// プログラムマップ上のチップ向き（矢印）を右回りに回転させる
    /// 赤矢印でも青矢印でも基本的な処理は共通であるため、1つの関数で処理する
    /// </summary>
    /// <param name="x">プログラムマップ上のx座標</param>
    /// <param name="y">プログラムマップ上のy座標</param>
    /// <param name="_arrowMap">マップ用配列。programArrowMapかprogramIfArrowMap</param>
    /// <param name="_tilemap">対応するタイルマップ</param>
    void RotateArrow(int x, int y, int[,] _arrowMap, Tilemap _tilemap)
    {
        //そもそも矢印がなければ、何もしない
        if (_arrowMap[x, y] == (int)GrobalConst.TileId.Empty)
        {
            return;
        }

        Debug.Log(tileSelector.GetNextArrowTileName(_arrowMap[x, y]));

        //そうでない場合、右回りに回転させる
        tileSelector.SetTile(x, y,
            _arrowMap, _tilemap,
            tileSelector.GetNextArrowTileName(_arrowMap[x, y]));

        //赤矢印と青矢印が重なってしまう場合、矢印をもう1回り移動させる
        //冗長だが、赤矢印のマップが渡されている場合は青矢印のマップと、
        //青矢印のマップが渡されている場合は赤矢印のマップと比較するように明記する
        if ( (_arrowMap[x, y] != programArrowMap[x, y]
             && tileSelector.GetArrowName(_arrowMap[x, y]) == tileSelector.GetArrowName(programArrowMap[x, y]))
            || (_arrowMap[x, y] != programIfArrowMap[x, y]
             && tileSelector.GetArrowName(_arrowMap[x, y]) == tileSelector.GetArrowName(programIfArrowMap[x, y])) )
        {
            tileSelector.SetTile(x, y,
            _arrowMap, _tilemap,
                tileSelector.GetNextArrowTileName(_arrowMap[x, y]));
        }


    }




    /// <summary>
    /// プログラムマップを操作する
    /// </summary>
    void Update()
    {
        //Ctrl+ZでUndo
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && Input.GetKeyDown(KeyCode.Z))
        {
            LoadHistory();
            soundManager.Play("System_Button");
            return;
        }


        //マウスの座標をプログラムマップ上の座標に変換する
        Vector3Int gridPos = grid.WorldToCell(targetCamera.ScreenToWorldPoint(Input.mousePosition));
        int x = gridPos.x - origin[0];
        int y = gridPos.y - origin[1];

        //プログラムマップの範囲を超えている場合、何もしない
        if (x < 0 || GrobalConst.ProgramMapSize <= x 
            || y < 0 || GrobalConst.ProgramMapSize <= y)
        {
            return;
        }


        //マウスを右クリックすると、そのチップの分岐先を回転させる
        if (Input.GetMouseButtonDown(1))
        {
            //Shiftを押している状態の場合、IF用の青矢印を回転させる
            //そうでない場合、赤矢印を回転させる
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {

                RotateArrow(x, y, programIfArrowMap, programIfArrowTilemap);
            }
            else
            {
                RotateArrow(x, y, programArrowMap, programArrowTilemap);
            }

            Debug.Log(tileSelector.idToTileNameDictionary[programMap[x, y]]
                + " " + tileSelector.idToTileNameDictionary[programArrowMap[x, y]]
                + " " + tileSelector.idToTileNameDictionary[programIfArrowMap[x, y]]);


            //各種マップに変更が起きた場合、新しい状況をHistoryとして保存する
            if (NotTheTopHistoryIsSame())
            {
                soundManager.Play("TileSet");
                PushHistory();
            }
        }


        //マウスを左クリックすると、TileSelector.tileIdに応じてプログラムマップを操作する
        if (Input.GetMouseButtonDown(0))
        {
            //特殊な操作を行う場合
            switch (tileSelector.tileId)
            {
                //赤矢印回転モード
                case ROTATE_MODE_RED:
                    RotateArrow(x, y, programArrowMap, programArrowTilemap);
                    break;

                //青矢印回転モード
                case ROTATE_MODE_BLUE:
                    RotateArrow(x, y, programIfArrowMap, programIfArrowTilemap);
                    break;

                default:
                    break;
            }


            Debug.Log(tileSelector.idToTileNameDictionary[programMap[x, y]] 
                + " " + tileSelector.idToTileNameDictionary[programArrowMap[x, y]]
                + " " + tileSelector.idToTileNameDictionary[programIfArrowMap[x, y]]);




            //指定した位置のチップが選択中のものと一致する場合：矢印回転モードでなくても矢印の回転を行う。
            if (programMap[x, y] == tileSelector.tileId)
            {
                //Shiftを押している状態の場合、IF用の青矢印を回転させる。
                //そうでない場合、赤矢印を回転させる。
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    RotateArrow(x, y, programIfArrowMap, programIfArrowTilemap);
                }
                else
                {
                    RotateArrow(x, y, programArrowMap, programArrowTilemap);
                }
            }




            //行動チップを配置する場合
            if (tileSelector.tileId == (int)GrobalConst.TileId.MoveForward ||
                tileSelector.tileId == (int)GrobalConst.TileId.MoveBackward ||
                tileSelector.tileId == (int)GrobalConst.TileId.RotateRight ||
                tileSelector.tileId == (int)GrobalConst.TileId.RotateLeft ||
                tileSelector.tileId == (int)GrobalConst.TileId.Wait)
            {
                //指定した位置に赤矢印が無ければ仮設定する。
                if (programArrowMap[x, y] == (int)GrobalConst.TileId.Empty)
                {
                    tileSelector.SetTile(x, y,
                        programArrowMap, programArrowTilemap,
                        GrobalConst.TileId.RedArrowUp.ToString());
                }

                //指定した行動チップを設定する。青矢印は必ず消去される
                tileSelector.SetTile(x, y, 
                    programMap, programTilemap,
                    tileSelector.idToTileNameDictionary[tileSelector.tileId]);

                tileSelector.SetTile(x, y,
                    programIfArrowMap, programIfArrowTilemap,
                    GrobalConst.TileId.Empty.ToString());
            }



            //If系チップを配置する場合
            if (tileSelector.tileId == (int)GrobalConst.TileId.IfFront ||
                tileSelector.tileId == (int)GrobalConst.TileId.IfRight ||
                tileSelector.tileId == (int)GrobalConst.TileId.IfBack ||
                tileSelector.tileId == (int)GrobalConst.TileId.IfLeft)
            {
                //指定した位置に赤矢印が無ければ仮設定する。
                if (programArrowMap[x, y] == (int)GrobalConst.TileId.Empty)
                {
                    tileSelector.SetTile(x, y,
                        programArrowMap, programArrowTilemap,
                        GrobalConst.TileId.RedArrowUp.ToString());
                }

                //同様に、指定した位置に青矢印が無ければ仮設定する。
                //向きは重複を防ぐために赤矢印の隣にする。
                if (programIfArrowMap[x, y] == (int)GrobalConst.TileId.Empty)
                {
                    tileSelector.SetTile(x, y,
                        programIfArrowMap, programIfArrowTilemap,
                        "BlueArrow" + tileSelector.GetNextArrowName(programArrowMap[x, y]));
                }

                //指定した行動チップを設定する。
                tileSelector.SetTile(x, y,
                    programMap, programTilemap,
                    tileSelector.idToTileNameDictionary[tileSelector.tileId]);
            }


            //Empty：Emptyを選択した場合、赤/青矢印もまとめて初期化する
            if (tileSelector.tileId == (int)GrobalConst.TileId.Empty)
            {
                tileSelector.SetTile(x, y, programMap, programTilemap, GrobalConst.TileId.Empty.ToString());
                tileSelector.SetTile(x, y, programArrowMap, programArrowTilemap, GrobalConst.TileId.Empty.ToString());
                tileSelector.SetTile(x, y, programIfArrowMap, programIfArrowTilemap, GrobalConst.TileId.Empty.ToString());
            }


            //BaseStart：プログラム開始地点の変更（重複などの判定はSetTile側で行う）
            if (tileSelector.tileId == (int)GrobalConst.TileId.BaseStart)
            {
                tileSelector.SetTile(x, y, programBaseMap, programBaseTilemap, GrobalConst.TileId.BaseStart.ToString());
            }


            //各種マップに変更が起きた場合、新しい状況をHistoryとして保存する
            if (NotTheTopHistoryIsSame())
            {
                soundManager.Play("TileSet");
                PushHistory();
            }

        }


    }
}