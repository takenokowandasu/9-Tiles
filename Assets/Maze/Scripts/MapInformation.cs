using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Text;
using UnityEditor;
using Grobal;

/// <summary>
/// プレイヤーキャラが移動するマップの情報を管理する
/// ファイル等からの読み込み機能も持つ
/// </summary>
public class MapInformation : MonoBehaviour
{
	//マップ名
	public string mapName;

	//マップの大きさ(x,y)
	public int[] mapsize;
	//各座標のタイルId
	public int[,] mapData;
	//マップの始点（左下のマス）が、グリッド上でどのマスにあるか
	public int[] origin;

	//マップを描画するタイルマップ
	public Tilemap mapTilemap;

	//マップサイズを入出力するInputField
	[SerializeField]
	private InputField InputOfMapsizeX;
	[SerializeField]
	private InputField InputOfMapsizeY;

	[SerializeField]
	private TileSelector tileSelector;


	//CSV形式で読み取った内容を格納する変数
	private List<string[]> DataList = new List<string[]>();

	private const string DEFAULTMAPNAME = "map";
	private const int MAPSIZEMIN = 3;
	private const int MAPSIZEMAX = 16;

	/// <summary>
	/// デフォルトマップを読み込んでおく
	/// </summary>
	private void Awake()
	{
		InitByFile(DEFAULTMAPNAME);
	}



	/// <summary>
	/// 指定のCSVファイルから情報を読取り、DataListに格納
	/// </summary>
	/// <param name="_Filename">読み込ませたいファイル名</param>
	private void CsvReader(string _Filename)
	{
		// Assets/Resourceフォルダ内の相対パスで指定したファイルを読み込む
		TextAsset csvFile = Resources.Load(_Filename) as TextAsset;

		// データリストを初期化し、各行をカンマ区切りで読み込む
		DataList = new List<string[]>();
		StringReader reader = new StringReader(csvFile.text);
		while (reader.Peek() != -1)
		{
			string line = reader.ReadLine();
			DataList.Add(line.Split(','));
		}
	}


	/// <summary>
	/// DataListを元に、各種変数やタイルマップを操作する
	/// </summary>
	private void SetMapDatas()
	{
		//先頭にマップサイズがある
		//不正な値ならキャンセルしデフォルトマップを呼び出す
		if (int.TryParse(DataList[0][0], out int mapsizeX)
			&& int.TryParse(DataList[0][1], out int mapsizeY)
			&& mapsizeX >= MAPSIZEMIN && mapsizeX <= MAPSIZEMAX && mapsizeY >= MAPSIZEMIN && mapsizeY <= MAPSIZEMAX)
		{
			mapsize = new int[] { mapsizeX, mapsizeY };
		}
		else
		{
			InitByFile(DEFAULTMAPNAME);
			return;
		}

		//UIに反映
		InputOfMapsizeX.text = DataList[0][0];
		InputOfMapsizeY.text = DataList[0][1];


		//タイルマップ上の原点座標はマップサイズから求める
		//座標(0,0,0)がマップの中心になるよう設定
		origin = new int[] { -mapsize[0] / 2, -mapsize[1] / 2 };
		mapTilemap.transform.position = new Vector3(origin[0], origin[1], 0);

		//マップ情報とタイルマップを初期化
		mapData = new int[mapsize[0], mapsize[1]];
        mapTilemap.ClearAllTiles();

        //スタート地点の位置を調べる
        int[] startPosition = null;

		//ニ行目以降がマップの配置なので、それを1マス1要素で格納・タイルマップに反映
		for (int y = 0; y < mapsize[1]; y++)
		{
			for (int x = 0; x < mapsize[0]; x++)
			{
				//もし不正な値が格納されていればキャンセルしデフォルトマップを呼び出す
				if (int.TryParse(DataList[y + 1][x], out int result)
					&& (result == (int)GrobalConst.TileId.Floor
						|| result == (int)GrobalConst.TileId.Wall
						|| result == (int)GrobalConst.TileId.Start
						|| result == (int)GrobalConst.TileId.Goal))
				{
					//マップの外周に関しては強制的に壁を配置する
					if (x == 0 || mapsize[0] - 1 == x
						|| y == 0 || mapsize[1] - 1 == y)
					{
						result = (int)GrobalConst.TileId.Wall;
					}

					//もしスタート地点があれば位置を記録する
					if (result == (int)GrobalConst.TileId.Start)
					{
						startPosition = new int[2] { x, mapsize[1] - y - 1 };
					}

					//DataList上の行数と合うように添字をずらすことに注意
					//また、DataList上では上から行数を数えるが、ゲーム内では下から数えていくという点に注意する
					tileSelector.SetTile(x, mapsize[1] - y - 1,
					mapData, mapTilemap,
					tileSelector.idToTileNameDictionary[result]);
				}
				else
				{
					InitByFile(DEFAULTMAPNAME);
					return;
				}

			}
		}

		//もしスタート地点がどこにも無ければ、強制的に配置する
		if (startPosition == null)
		{
			tileSelector.SetTile(1, 1,
			mapData, mapTilemap,
			GrobalConst.TileId.Start.ToString());
		}
	}


	/// <summary>
	/// 指定されたファイルからマップデータを読み込む
	/// </summary>
	/// <param name="_filename">読み込ませたいファイル名</param>
	public void InitByFile(string _filename)
	{
		CsvReader(_filename);

		mapName = _filename;

		SetMapDatas();
	}

	/// <summary>
	/// InitByFile(string)のオーバーロード
	/// ボタンから呼び出すことを想定
	/// 開発中のエディタ上でのマップ作成に使用するプログラムで、公開時は使用しない
	/// </summary>
	/// <param name="_filename">読み込ませたいファイル名が入力されたInputField</param>
	public void InitByFile(InputField _filename)
    {
		InitByFile(_filename.text);
    }


	/// <summary>
	/// 指定のInputFieldに書き込まれた情報をCSV形式で読み込む
	/// ボタンから呼び出すことを想定
	/// </summary>
	/// <param name="_inputField">読み込ませたいデータが入力されたInpufField</param>
	public void InitByInput(InputField _inputField)
    {
		// データリストを初期化
		DataList = new List<string[]>();

		// inputField内の文字を改行区切りで配列にする
		string[] lines = _inputField.text.Split('\n');

		// 各行の文字列をカンマ区切りでDataListに保存
		foreach(string line in lines)
        {
			DataList.Add(line.Split(','));
        }

		mapName = "InputData";

		SetMapDatas();
	}


	/// <summary>
	/// 保持しているデータがデフォルトで用意しているステージかどうかを判定する。用意したものであればその通し番号も返す
	/// ステージ名のファイルは必ず "STAGE" + (int)Id という形式で書いているため、これを利用して判定する
	/// </summary>
	/// <returns>判定結果とそのマップID</returns>
	public (bool IsMap, int mapId) GetMapId()
    {
        //用意したステージであれば、"STAGE"を消してId部分の数字だけが残り、そのまま数値に変換できるはず
        //変換できない場合、TryParseのoutには「0」が格納される点に注意
        bool IsMap = int.TryParse(mapName.Replace("STAGE", ""), out int mapId);

		return (IsMap, mapId);
	}




	/// <summary>
	/// InputFieldに入力されたマップサイズを読み込んでmapDataに反映する
	/// マップサイズの拡縮に対応するため、元のマップを出来る範囲でコピーしてから外周を壁に設定し直す
	/// ボタンで呼び出すことを想定
	/// </summary>
	public void SetMapsize()
	{
		//新しいマップデータを生成する。nullが返ってきた場合は何もしない
		(int[] new_mapsize, int[,] new_map) = CreateNewMap();
		if (new_mapsize == null)
		{
			return;
		}

		//データを転写してタイルマップに反映させる
		
		DataList = new List<string[]>();

		DataList.Add(new string[] { new_mapsize[0].ToString(), new_mapsize[1].ToString() });

        for (int y = new_mapsize[1] - 1; y >= 0; y--)
        {
			string line = "";

            for (int x = 0; x < new_mapsize[0]; x++)
            {
				line += new_map[x, y].ToString();

				if (x < new_mapsize[0] - 1)
				{
					line += ",";
				}
			}
			DataList.Add(line.Split(','));

		}

		SetMapDatas();
    }


	/// <summary>
	/// 指定のInputFieldに、現状のマップデータをCSV形式で表示させる
	/// ボタンで呼び出すことを想定
	/// </summary>
	/// <param name="_inputField">表示を行うInputField</param>
	public void ViewCsvData(InputField _inputField)
	{
		//最初にマップサイズを書く
		string data = mapsize[0] + "," + mapsize[1] + "\n";

		//Unity上ではyの正が上向きであることに注意して一行ずつ書き込む
		for (int y = mapsize[1] - 1; y >= 0; y--)
		{
			for (int x = 0; x < mapsize[0]; x++)
			{
				data += mapData[x, y].ToString();

				if (x < mapsize[0] - 1)
				{
					data += ",";
				}
			}
			data += "\n";
		}

		_inputField.text = data;
	}


	/// <summary>
	/// 指定のInputFieldの内容を、クリップボードへコピーする
	/// ボタンで呼び出すことを想定
	/// </summary>
	/// <param name="_inputField">コピーを行うInputField</param>
	public void CopyCsvDataToClipBoard(InputField _inputField)
	{
		GUIUtility.systemCopyBuffer = _inputField.text;
	}


	/// <summary>
	/// 指定ファイル名でマップデータを保存する
	/// ボタンで呼び出すことを想定。ファイル名はfilenameObjectから取得する
	/// 開発中のエディタ上でのマップ作成に使用するプログラムで、公開時は使用しない
	/// </summary>
	/// <param name="_inputField">表示を行うInputField</param>
	public void SaveMap(InputField _inputField)
	{
		string filename;

		//もしfilenameObjectが空の場合はデフォルトの名前を設定する
		if (_inputField.text == null | _inputField.text == "")
		{
			filename = "map";
		}
		else
		{
			filename = _inputField.text;
		}

		//マップサイズの入力データを同時に反映する。
		(int[] new_mapsize, int[,] new_map) = CreateNewMap();
		if (new_mapsize == null)
		{
			return;
		}


		//CSVファイルに書き込む
		using (StreamWriter sw = new StreamWriter(@"Assets\Resources\" + filename + @".csv", false, Encoding.GetEncoding("Shift_JIS")))
		{
			sw.WriteLine(string.Join(",", new_mapsize));

			//Unity上ではyの正が上向きであることに注意して一行ずつ書き込む
			for (int y = new_mapsize[1] - 1; y >= 0; y--)
			{
				for (int x = 0; x < new_mapsize[0]; x++)
				{
					sw.Write(new_map[x, y].ToString());

					if (x < new_mapsize[0] - 1)
					{
						sw.Write(",");
					}
				}
				sw.WriteLine();
			}
		}


        //最後に更新したデータをエディタへ登録する
        //エディタ上でしか機能しない。ビルド時はそもそもコメントアウトさせておく必要がある
        AssetDatabase.ImportAsset(@"Assets\Resources\" + filename + @".csv", ImportAssetOptions.Default);

        //保存したデータの状況を反映し、表示位置などを変更させておく
        InitByFile(filename);

	}

	/// <summary>
	/// UIから読み込んだマップサイズを元に新しいマップデータの雛形を作成する
	/// この時、古いマップデータを出来る範囲でコピーする
	/// </summary>
	/// <returns>新しいマップサイズとマップデータ</returns>
	private (int[] new_mapsize, int[,] new_map) CreateNewMap()
	{
		//読み込んだ値を数字に変換できない場合は何もしない
		if (int.TryParse(InputOfMapsizeX.text, out int mapsizeX)
			&& int.TryParse(InputOfMapsizeY.text, out int mapsizeY)
			&& mapsizeX >= MAPSIZEMIN && mapsizeX <= MAPSIZEMAX && mapsizeY >= MAPSIZEMIN && mapsizeY <= MAPSIZEMAX)
		{
			//反映したいマップサイズで新しいマップ配列を作る
			int[] new_mapsize = new int[] { mapsizeX, mapsizeY };
			int[,] new_map = new int[new_mapsize[0], new_mapsize[1]];

			//設定中のデータを出来る範囲でコピー。外周は強制的に壁にする
			for (int x = 0; x < new_mapsize[0]; x++)
			{
				for (int y = 0; y < new_mapsize[1]; y++)
				{
					if (x < mapsize[0] && y < mapsize[1])
					{
						new_map[x, y] = mapData[x, y];
					}

					if (x == 0 | x == new_mapsize[0] - 1 | y == 0 | y == new_mapsize[1] - 1)
					{
						new_map[x, y] = 1;
					}
				}
			}

			return (new_mapsize, new_map);
		}
		else
		{
			Debug.Log("Mapsize data is not appropriate number");
			return (null, null);
		}
	}
}


