using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Grobal;
using System.Linq;

/// <summary>
/// プログラムマップに設定した内容を実行する機能を提供するクラス
/// </summary>
/// 
public class ProgramExecutor : MonoBehaviour
{
    [SerializeField]
    private ProgramEditor programEditor;

    [SerializeField]
    private TileSelector tileSelector;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private ModeManager changeMode;

    [SerializeField]
    private MapInformation mapInformation;


    private Coroutine playingCoroutine;

    //コルーチン1ループの実行間隔
    private const float ORIGINALSPAN = 1.0f;
    private float span = ORIGINALSPAN;
    //ループを一時停止させるスイッチ
    private bool IsPause = false;

    //プログラムの開始座標
    private int[] programStartPos = new int[2] { 0, 0 };

    //プログラムマップ内の実行中座標
    private int[] pos = new int[2] {0, 0};

    //プログラムの実行回数
    public int turn;

    //プログラムの実行回数を表示するテキストオブジェクト
    [SerializeField]
    private Text turnText;



    /// <summary>
    /// コルーチン1ループの実行間隔を段階的に変更する
    /// ボタンから実行し、そのボタンが持つテキストに目安を表示する
    /// </summary>
    /// <param name="button"></param>
    public void ChangeSpan(Button button)
    {
        switch (span)
        {
            case ORIGINALSPAN:
                span = ORIGINALSPAN / 2;
                IsPause = false;
                button.GetComponentInChildren<Text>().text = "x2";
                break;


            case ORIGINALSPAN / 2:
                span = ORIGINALSPAN / 4;
                IsPause = false;
                button.GetComponentInChildren<Text>().text = "x4";
                break;

            case ORIGINALSPAN / 4:
                span = ORIGINALSPAN / 8;
                IsPause = false;
                button.GetComponentInChildren<Text>().text = "x8";
                break;

            //8倍速の次はポーズに入る（ポーズから抜けた際のために、span自体は小さく設定）
            case ORIGINALSPAN / 8:
                span = ORIGINALSPAN / 100;
                IsPause = true;
                button.GetComponentInChildren<Text>().text = "x0";
                break;

            //ポーズを解除
            case ORIGINALSPAN / 100:
                span = ORIGINALSPAN;
                IsPause = false;
                button.GetComponentInChildren<Text>().text = "x1";
                break;

            default:
                button.GetComponentInChildren<Text>().text = "error";
                break;
        }
    }




    /// <summary>
    /// プログラム実行処理を開始する
    /// 不要な各種ボタンを非表示/操作不可能状態にし、強制終了ボタンを表示。実行用コルーチンを開始
    /// </summary>
    public void SetExecutionmode()
    {
        //プログラム開始位置の読み取り、設定（baseを読み取る）
        for (int x = 0; x < GrobalConst.ProgramMapSize; x++)
        {
            for (int y = 0; y < GrobalConst.ProgramMapSize; y++)
            {
                if (programEditor.programBaseMap[x, y] == (int)GrobalConst.TileId.BaseStart)
                {
                    programStartPos = new int[2] { x, y };
                    programStartPos.CopyTo(pos, 0);
                }
            }
        }

        //ターン数初期化
        turn = 0;
        turnText.text = "TURN:" + turn.ToString();

        playingCoroutine = StartCoroutine(nameof(Execution));
    }



    /// <summary>
    /// プログラム実行処理を終了する
    /// </summary>
    public void EndExecutionmode()
    {
        if (playingCoroutine == null)
        {
            return;
        }

        StopCoroutine(playingCoroutine);

        InitProgramMap();

        playingCoroutine = null;
        turnText.text = "";
    }


    /// <summary>
    /// 非アクティブになった際、もしCoroutineが動いているならそれを停止する
    /// タイトル画面に戻るボタンを押した時などに役立つ
    /// </summary>
    public void OnDisable()
    {
        if (playingCoroutine != null)
        {
            EndExecutionmode();
        }
    }

    /// <summary>
    /// コルーチンを終了させる際、最後の一周分処理が行われてBaseが変更されてしまう場合がある模様。
    /// アクティブになった際にも、安全のためにプログラムの背景マップを初期化する。
    /// </summary>
    public void OnEnable()
    {
        //プログラム開始位置の更新、設定（baseを読み取る）
        for (int x = 0; x < GrobalConst.ProgramMapSize; x++)
        {
            for (int y = 0; y < GrobalConst.ProgramMapSize; y++)
            {
                if (programEditor.programBaseMap[x, y] == (int)GrobalConst.TileId.BaseStart)
                {
                    programStartPos = new int[2] { x, y };
                    programStartPos.CopyTo(pos, 0);
                }
            }
        }

        InitProgramMap();
    }


    /// <summary>
    /// 背景マップ初期化（実行中ハイライトを消す）
    /// </summary>
    private void InitProgramMap()
    {
        for (int x = 0; x < GrobalConst.ProgramMapSize; x++)
        {
            for (int y = 0; y < GrobalConst.ProgramMapSize; y++)
            {
                tileSelector.SetTile(x, y,
                    programEditor.programBaseMap,
                    programEditor.programBaseTilemap,
                    GrobalConst.TileId.Base.ToString());
            }
        }

        tileSelector.SetTile(programStartPos[0], programStartPos[1],
            programEditor.programBaseMap,
            programEditor.programBaseTilemap,
            GrobalConst.TileId.BaseStart.ToString());
    }


    /// <summary>
    /// 矢印の向きから、次に実行するチップの位置を読み取る
    /// </summary>
    /// <param name="_typeOfArrow">読み取りたい矢印の区分。RedArrowかBlueArrow</param>
    private void ReadArrow(string _typeOfArrow)
    {
        int[] nextPos = new int[2];

        switch (_typeOfArrow)
        {
            case "RedArrow":
                nextPos = GrobalMethod.DirToRelativePosition(programEditor.programArrowMap[pos[0], pos[1]]
                                % tileSelector.tileNameToIdDictionary[_typeOfArrow + "Up"]);
                break;

            case "BlueArrow":
                nextPos = GrobalMethod.DirToRelativePosition(programEditor.programIfArrowMap[pos[0], pos[1]]
                                % tileSelector.tileNameToIdDictionary[_typeOfArrow + "Up"]);
                break;

            default:
                Debug.Log("ArrowType Error");
                break;
        }
        pos[0] += nextPos[0];
        pos[1] += nextPos[1];

        //次の座標がプログラムの範囲を超過している場合、またはチップが設定されていない場合、Posを原点に戻す
        if (pos[0] < 0 || pos[0] >= GrobalConst.ProgramMapSize
            || pos[1] < 0 || pos[1] >= GrobalConst.ProgramMapSize
            || programEditor.programMap[pos[0], pos[1]] == (int)GrobalConst.TileId.Empty)
        {
            programStartPos.CopyTo(pos, 0);
        }
    }



    /// <summary>
    /// プログラム実行処理
    /// 一定時間ごとにプログラムマップの内容を1マスずつ実行する
    /// </summary>
    /// <returns></returns>
    IEnumerator Execution()
    {
        while (true)
        {
            //待機中、実行するチップの背景を強調表示
            //（BaseStartも強制的に塗り替えるため、一旦該当のマスを関数を介さず変更することに注意）
            programEditor.programBaseMap[pos[0], pos[1]] = (int)GrobalConst.TileId.BaseActive;
            programEditor.programBaseTilemap.SetTile(new Vector3Int(pos[0], pos[1], 0), tileSelector.tileNameToTileDictionary["BaseActive"].tileData);


            //ポーズ状態の場合、ポーズが終わるまで待機
            yield return new WaitUntil(() => !IsPause);

            //一定時間待機
            yield return new WaitForSeconds(span);

            //ターン数加算
            turn++;
            turnText.text = "TURN:" + turn.ToString();

            //強調表示を解除
            if (pos.SequenceEqual(programStartPos))
            {
                tileSelector.SetTile(pos[0], pos[1],
                    programEditor.programBaseMap, programEditor.programBaseTilemap,
                    GrobalConst.TileId.BaseStart.ToString());
            }
	        else
            {
                tileSelector.SetTile(pos[0], pos[1],
                    programEditor.programBaseMap, programEditor.programBaseTilemap,
                    GrobalConst.TileId.Base.ToString());
            }



            Debug.LogFormat("{0}秒経過 実行する座標: {1},{2}", span, pos[0], pos[1]);

            //プログラムが冗長になるため一時変数を置く
            int selectedChip = programEditor.programMap[pos[0], pos[1]];

            //行動チップの処理
            //行動の結果に関わらず、プログラム位置の移動を行っておく
            //後回しにすると、ゴールした時の処理などと衝突してエラーが出る場合がある
            if (selectedChip == (int)GrobalConst.TileId.MoveForward)
            {
                ReadArrow("RedArrow");
                playerController.Move(0);
            }
            else if (selectedChip == (int)GrobalConst.TileId.MoveBackward)
            {
                ReadArrow("RedArrow");
                playerController.Move(2);
            }
            else if (selectedChip == (int)GrobalConst.TileId.RotateRight)
            {
                ReadArrow("RedArrow");
                playerController.Rotate(1);
            }
            else if (selectedChip == (int)GrobalConst.TileId.RotateLeft)
            {
                ReadArrow("RedArrow");
                playerController.Rotate(-1);
            }
            else if (selectedChip == (int)GrobalConst.TileId.Wait)
            {
                ReadArrow("RedArrow");
            }

            //IFチップの処理
            //プレイヤーから見て各チップに対応した向きに壁があるかどうかでプログラムが分岐する
            else if (selectedChip == (int)GrobalConst.TileId.IfFront ||
                selectedChip == (int)GrobalConst.TileId.IfRight ||
                selectedChip == (int)GrobalConst.TileId.IfBack ||
                selectedChip == (int)GrobalConst.TileId.IfLeft)
            {
                if (playerController.WhetherThereIsAWall(selectedChip % (int)GrobalConst.TileId.IfFront))
                {
                    ReadArrow("RedArrow");
                }
                else
                {
                    ReadArrow("BlueArrow");
                }
            }
        }
    }

}
