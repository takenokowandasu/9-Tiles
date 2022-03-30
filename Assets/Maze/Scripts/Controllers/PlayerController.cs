using UnityEngine;
using Grobal;

/// <summary>
/// プレイヤーキャラクターの移動を管理するクラス
/// </summary>
public class PlayerController : MonoBehaviour
{
	[SerializeField]
	//モードチェンジャー（ゴール判定にのみ使用）
	private ModeManager modeManager;

	[SerializeField]
	//マップデータ
	private MapInformation mapInformation;

	[SerializeField]
	//サウンドマネージャー
	private SoundManager soundManager;


	// マップ上での1マスあたりの座標距離（定数）
	// 各種タイルマップやグリッドのデフォルトのサイズと同様である
	private const float DISTANCE = 1.0f;

	// タイルマップ上のタイルとプレイヤーオブジェクトの表示位置を合わせるためのピボット
	private const float PIVOT = 0.5f;


	// transformのキャッシュを格納する
	private Transform _transform;

	// 向きはtransformとは別に変数で管理する
	private GrobalConst.DirectionId direction;


	void Awake()
    {
    	// transformのキャッシュを取得
    	_transform = transform;
	}


	/// <summary>
	/// mapDataが持つマップのスタート地点に自機を移動させる
	/// </summary>
	public void SetStartPosition()
	{
		if (mapInformation.mapsize == null)
        {
			return;
        }

		//mapを読み取ってスタート位置を確認し、それにあわせて位置を設定
		for (int x = 0; x < mapInformation.mapsize[0]; x++)
		{
			for (int y = 0; y < mapInformation.mapsize[1]; y++)
			{
				if (mapInformation.mapData[x, y] == (int)GrobalConst.TileId.Start)
                {
                    transform.position = new Vector3(x + mapInformation.origin[0] + PIVOT, 
													y + mapInformation.origin[1] + PIVOT, 
													0);
                }
            }
		}

		//初期配置では向きを上に固定
		direction = GrobalConst.DirectionId.Up;
		transform.rotation = GrobalMethod.DirToRot(direction);
	}



	/// <summary>
	/// プレイヤーの現在位置がゴール地点かどうかを判定し、その場合はゴール時の演出を実行する
	/// </summary>
	private void CheckGoal()
	{
		//現在の座標を取得
		Vector2 pos = _transform.position;

		if (mapInformation.mapData[(int)(pos.x - mapInformation.origin[0] - PIVOT),
									(int)(pos.y - mapInformation.origin[1] - PIVOT)] == (int)GrobalConst.TileId.Goal)
        {
			modeManager.SetMode("GoalEffect");
            soundManager.Play("Goal");
        }
    }



	/// <summary>
	/// プレイヤーキャラを1マス移動させる
	/// プレイヤーキャラから見てどの向きに移動するかは引数で与える
	/// <param name="_directionNum">回転の大きさ。正なら右回り。負なら左回り</param>
	/// </summary>
	public void Move(int _directionNum)
	{
		//現在の向きから前進する場合の位置関係を取得
		int[] forward = GrobalMethod.DirToRelativePosition(direction + _directionNum);

		//現在の座標を取得し、移動先の座標を計算
		Vector2 pos = _transform.position;
		pos.x += forward[0] * DISTANCE;
		pos.y += forward[1] * DISTANCE;

		//移動先に壁がない場合、移動は成功し、求めた座標をオブジェクトに反映
		//移動先に壁がある場合、移動は失敗し、座標は変更しない
		//それぞれ、対応するSEを鳴らす
		if (mapInformation.mapData[(int)(pos.x - mapInformation.origin[0] - PIVOT), 
									(int)(pos.y - mapInformation.origin[1] - PIVOT)] == (int)GrobalConst.TileId.Wall)
		{
			soundManager.Play("Action_Failed");
		}
		else
		{
			transform.position = pos;
			soundManager.Play("Action");
			CheckGoal();
		}
	}



	/// <summary>
	/// プレイヤーキャラクターを回転させる
	/// </summary>
	/// <param name="_directionNum">回転の大きさ。正なら右回り。負なら左回り</param>
	public void Rotate(int _directionNum)
	{
		direction = GrobalMethod.GetRelativeDirection(direction, _directionNum);

		//この向きに対応してスプライトも回転させる
		transform.rotation = GrobalMethod.DirToRot(direction);

		soundManager.Play("Action");
	}

	/// <summary>
	/// プレイヤーキャラと隣り合うマスに壁があるかどうかを返す
	/// </summary>
	/// <param name="_directionNum">プレイヤーキャラから見て上下左右どの向きかを指定</param>
	/// <returns>指定位置に壁があればtrue</returns>
	public bool WhetherThereIsAWall(int _directionNum)
    {
		//現在位置
		Vector2 pos = _transform.position;

		//指定方向で隣にあるマスの座標を計算
		int[] move = GrobalMethod.DirToRelativePosition(direction + _directionNum);
		pos.x += move[0] * DISTANCE;
		pos.y += move[1] * DISTANCE;

		bool result = mapInformation.mapData[(int)(pos.x - mapInformation.origin[0] - PIVOT),
									  (int)(pos.y - mapInformation.origin[1] - PIVOT)] == (int)GrobalConst.TileId.Wall;

		if (result)
        {
			soundManager.Play("IfTrue");
        }
        else
        {
			soundManager.Play("IfFalse");
        }

		return result;

	}


}
