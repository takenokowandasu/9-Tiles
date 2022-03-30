using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Grobal;
using System;


/// <summary>
/// 各種タイルマップ編集用の機能を提供するクラス
/// UI上のボタンで選択するタイルを変えられるほか、選択中のボタンをハイライトする機能もある
/// </summary>

public class TileSelector : MonoBehaviour
{
    //選択中のタイルのID
    public int tileId;

    //ハイライト用スプライト
    [SerializeField]
    private GameObject highlightPrefab;

    //非ハイライト時の外枠用スプライト
    [SerializeField]
    private GameObject backgroundPrefab;

    //オブジェクト判別用の変数
    private GameObject highlightObj;



    /// <summary>
    /// タイル情報をまとめて管理するクラス
    /// 簡単のため、インスペクター上で編集可能にする
    /// </summary>
    [Serializable]
    public class TileInformation
    {
        /// <summary>
        /// ゲーム内でのタイルの名称
        /// GrobalConst.TileIdのリストと名称を統一すること
        /// </summary>
        public string tileName;

        /// <summary>
        /// 対応するTile
        /// </summary>
        public Tile tileData;

        /// <summary>
        /// このタイルが使われるタイルマップ内で1つしか存在しないものであるかどうかを決める
        /// </summary>
        public bool IsSingle;

        /// <summary>
        /// IsSingle==trueでのみ使う情報
        /// 古いタイルを消去する場合に、代わりに置くタイルの名前を決める
        /// </summary>
        public string defaultTileName;

        /// <summary>
        /// 表示する時の回転角（オイラー角）
        /// 1つのタイルで複数の表現を兼ねる時に使用する
        /// </summary>
        public Vector3Int rot;
    }

    [SerializeField]
    private TileInformation[] tileInformations;

    //TileInformationの管理用Dictionary
    public Dictionary<string, TileInformation> tileNameToTileDictionary = new Dictionary<string, TileInformation>();
    public Dictionary<string, int> tileNameToIdDictionary = new Dictionary<string, int>();
    public Dictionary<int, string> idToTileNameDictionary = new Dictionary<int, string>();
    public Dictionary<int, TileInformation> idToTileDictionary = new Dictionary<int, TileInformation>();




    /// <summary>
    /// ゲーム開始時に、配列に登録したタイル情報を呼び出しやすいように辞書にセットする
    /// Idの番号は名称に対応する値を取得して設定する
    /// </summary>
    private void Awake()
    {
        foreach (TileInformation tileInformation in tileInformations)
        {
            //タイル名が不正な場合、スキップする
            if(Enum.TryParse(tileInformation.tileName, out GrobalConst.TileId tileName))
            {
                tileNameToTileDictionary.Add(tileInformation.tileName, tileInformation);
                tileNameToIdDictionary.Add(tileInformation.tileName, (int)tileName);

                idToTileNameDictionary.Add((int)tileName, tileInformation.tileName);
                idToTileDictionary.Add((int)tileName, tileInformation);
            }
        }
    }



    /// <summary>
    /// タイルマップの指定座標へ指定されたタイルを設定する
    /// この時、対応する配列にもIdを設定する
    /// </summary>
    /// <param name="_x">タイルマップ上のx座標</param>
    /// <param name="_y">タイルマップ上のy座標</param>
    /// <param name="_map">タイルマップに対応する配列</param>
    /// <param name="_tilemap">タイルマップ</param>
    /// <param name="_tileName">配置したいタイルの名称</param>
    public void SetTile(int _x, int _y, int[,] _map, Tilemap _tilemap, string _tileName)
    {
        //指定した座標のタイルがIsSingleかどうかをチェック
        if (idToTileDictionary[_map[_x, _y]].IsSingle)
        {
            Debug.Log(string.Format("{0}はsingleです:処理をキャンセル", idToTileNameDictionary[_map[_x, _y]]));
            return;
        }


        TileInformation tileInformation = tileNameToTileDictionary[_tileName];

        //セットするタイルがsingleである場合、マップ内に同一のタイルがあれば置換する
        if (tileInformation.IsSingle)
        {
            for (int loop_x = 0; loop_x < _map.GetLength(0); loop_x++)
            {
                for (int loop_y = 0; loop_y < _map.GetLength(1); loop_y++)
                {
                    if (_map[loop_x, loop_y] == tileNameToIdDictionary[_tileName])
                    {
                        //冗長になるが、無限ループを防ぐため再帰はせず強制的に置換する
                        TileInformation defaultTileInformation = tileNameToTileDictionary[tileInformation.defaultTileName];

                        _map[loop_x, loop_y] = tileNameToIdDictionary[defaultTileInformation.tileName];

                        Vector3Int defaultPos = new Vector3Int(loop_x, loop_y, 0);
                        Quaternion defaultRot = Quaternion.Euler(defaultTileInformation.rot);
                        _tilemap.SetTile(defaultPos, defaultTileInformation.tileData);
                        _tilemap.SetTransformMatrix(defaultPos, Matrix4x4.TRS(Vector3.zero, defaultRot, Vector3.one));

                        Debug.Log(string.Format("{0}はsingleです:余計なタイルを置換", _tileName));
                    }
                }
            }
        }

        //指定座標に指定タイルのIdを設定し、タイルマップに対応するタイルを設定
        _map[_x, _y] = tileNameToIdDictionary[_tileName];

        Vector3Int pos = new Vector3Int(_x, _y, 0);
        Tile tile = tileInformation.tileData;
        Quaternion rot = Quaternion.Euler(tileInformation.rot);

        _tilemap.SetTile(pos, tile);
        _tilemap.SetTransformMatrix(pos, Matrix4x4.TRS(Vector3.zero, rot, Vector3.one));
    }


    /// <summary>
    /// プログラムマップ上で遷移先を示す矢印の向きを返す
    /// 矢印に対応するタイルの名称には、必ず向きを示す文字列が含まれているので、
    /// 余計な部分を削除すると想定している向きが分かる
    /// </summary>
    /// <param name="_tileId">向き名称を取得したいタイルのId</param>
    /// <returns>向き名称の文字列</returns>
    public string GetArrowName(int _tileId)
    {
        string tileName = idToTileNameDictionary[_tileId];

        return tileName.Replace("Red", "").Replace("Blue", "").Replace("Arrow", "");
    }



    /// <summary>
    /// 指定した矢印タイルの向きに対して、右に回転させたタイルの名称を取得
    /// </summary>
    /// <param name="_arrowId">矢印タイルのId</param>
    /// <returns>引数で渡した矢印タイルを1つ右に回転させたタイルの名称</returns>
    public string GetNextArrowTileName(int _arrowId)
    {
        //タイル名称から向きを示す文字列だけを除き、赤矢印か青矢印かの区分を示す文字列にする
        string tileName = idToTileNameDictionary[_arrowId];
        string arrowName = tileName.Replace(GetArrowName(_arrowId), "");

        //区分はそのまま、向きを変えた矢印タイルの名称を返す
        return arrowName + GetNextArrowName(_arrowId);
    }


    /// <summary>
    /// 指定した矢印タイルの向きに対して、右回りで次の向きを取得
    /// </summary>
    /// <param name="_arrowId">矢印タイルのId</param>
    /// <returns>引数で渡した矢印タイルの向きを1つ右に回転させた向き</returns>
    public string GetNextArrowName(int _arrowId)
    {
        return GrobalMethod.GetRelativeDirection( (GrobalConst.DirectionId)Enum.Parse(typeof(GrobalConst.DirectionId), GetArrowName(_arrowId)), 1)
            .ToString();
    }




    
    /// <summary>
    /// 各種UIボタンから呼び出し、タイルの名称に対応するIdを保持する
    /// </summary>
    /// <param name="_tileName">タイルの名称</param>
    public void Select(string _tileName)
    {
        tileId = tileNameToIdDictionary[_tileName];
    }

    /// <summary>
    /// 各種UIボタンから呼び出し、特殊な操作に対応したIDを設定
    /// </summary>
    /// <param name="_tileId">操作Id</param>
    public void SelectUnique(int _tileId)
    {
        tileId = _tileId;
    }


    /// <summary>
    /// 受け取ったオブジェクトの子にあたるボタン全てに背景画像をつける
    /// </summary>
    /// <param name="_parentOfButtons">背景を設定したいボタンの親オブジェクト</param>
    public void SetBackground(GameObject _parentOfButtons)
    {
        foreach(Button button in _parentOfButtons.GetComponentsInChildren<Button>())
        {
            //既に背景画像がついている場合は何もしない
            if (button.transform.Find(backgroundPrefab.name))
            {
                continue;
            }

            SetSprite(button.gameObject, backgroundPrefab);
        }
    }


    /// <summary>
    /// 選択中ボタンハイライト
    /// ボタンから呼び出し、クリックしたボタンそのものを引数として受け取ることを想定する。
    /// その子オブジェクトとしてハイライト表示をつける。
    /// また、他のボタンのハイライト表示を消す
    /// </summary>
    /// <param name="_button">ハイライトをつけるボタン（押したボタンそのものにする）</param>
    public void Highlight(Button _button)
    {
        //もし既にハイライトが存在している場合、それを消去
        if (highlightObj != null)
        {
            Destroy(highlightObj);
        }

        highlightObj = SetSprite(_button.gameObject, highlightPrefab);
    }



    /// <summary>
    /// 指定したオブジェクトに画像を付ける
    /// 羅列されたボタンに枠やハイライトをつける用法を想定する
    /// </summary>
    /// <param name="_obj">画像を付けたいオブジェクト</param>
    /// <param name="_spritePrefab">付けたい画像</param>
    /// <returns></returns>
    private GameObject SetSprite(GameObject _obj, GameObject _spritePrefab)
    {
        GameObject spriteObject = Instantiate(_spritePrefab, _obj.transform.position, Quaternion.identity, transform);
        spriteObject.transform.SetParent(_obj.transform);
        spriteObject.transform.position += _spritePrefab.transform.position;

        spriteObject.name = _spritePrefab.name;

        //引数の親オブジェクトについているGridLayoutGroupを取得
        //セルサイズに合わせて、ボタンの枠として見えるようにハイライトのサイズを変更する（基準はセルサイズ100）
        if (_obj.transform.GetComponentInParent<GridLayoutGroup>() != null)
        {
            Vector2 size = _obj.GetComponentInParent<GridLayoutGroup>().cellSize;
            Vector3 scale = spriteObject.transform.localScale;

            scale.x = (size.x / 100) * scale.x;
            scale.y = (size.y / 100) * scale.y;

            spriteObject.transform.localScale = scale;
        }

        return spriteObject;
    }




    /// <summary>
    /// タイルセレクターの初期化（プレイモードとマップエディットモードの切替時に使用）
    /// タイルIDを無意味な値にしておき、保持しているハイライトを消去
    /// </summary>
    public void Reset()
    {
        //もしハイライトが存在している場合、それを消去
        if (highlightObj != null)
        {
            Destroy(highlightObj);
        }

        tileId = -99;
    }

}
