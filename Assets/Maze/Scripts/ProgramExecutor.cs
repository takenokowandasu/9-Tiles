using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Grobal;
using System.Linq;

/// <summary>
/// �v���O�����}�b�v�ɐݒ肵�����e�����s����@�\��񋟂���N���X
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

    //�R���[�`��1���[�v�̎��s�Ԋu
    private const float ORIGINALSPAN = 1.0f;
    private float span = ORIGINALSPAN;
    //���[�v���ꎞ��~������X�C�b�`
    private bool IsPause = false;

    //�v���O�����̊J�n���W
    private int[] programStartPos = new int[2] { 0, 0 };

    //�v���O�����}�b�v���̎��s�����W
    private int[] pos = new int[2] {0, 0};

    //�v���O�����̎��s��
    public int turn;

    //�v���O�����̎��s�񐔂�\������e�L�X�g�I�u�W�F�N�g
    [SerializeField]
    private Text turnText;



    /// <summary>
    /// �R���[�`��1���[�v�̎��s�Ԋu��i�K�I�ɕύX����
    /// �{�^��������s���A���̃{�^�������e�L�X�g�ɖڈ���\������
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

            //8�{���̎��̓|�[�Y�ɓ���i�|�[�Y���甲�����ۂ̂��߂ɁAspan���̂͏������ݒ�j
            case ORIGINALSPAN / 8:
                span = ORIGINALSPAN / 100;
                IsPause = true;
                button.GetComponentInChildren<Text>().text = "x0";
                break;

            //�|�[�Y������
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
    /// �v���O�������s�������J�n����
    /// �s�v�Ȋe��{�^�����\��/����s�\��Ԃɂ��A�����I���{�^����\���B���s�p�R���[�`�����J�n
    /// </summary>
    public void SetExecutionmode()
    {
        //�v���O�����J�n�ʒu�̓ǂݎ��A�ݒ�ibase��ǂݎ��j
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

        //�^�[����������
        turn = 0;
        turnText.text = "TURN:" + turn.ToString();

        playingCoroutine = StartCoroutine(nameof(Execution));
    }



    /// <summary>
    /// �v���O�������s�������I������
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
    /// ��A�N�e�B�u�ɂȂ����ہA����Coroutine�������Ă���Ȃ炻����~����
    /// �^�C�g����ʂɖ߂�{�^�������������Ȃǂɖ𗧂�
    /// </summary>
    public void OnDisable()
    {
        if (playingCoroutine != null)
        {
            EndExecutionmode();
        }
    }

    /// <summary>
    /// �R���[�`�����I��������ہA�Ō�̈�����������s����Base���ύX����Ă��܂��ꍇ������͗l�B
    /// �A�N�e�B�u�ɂȂ����ۂɂ��A���S�̂��߂Ƀv���O�����̔w�i�}�b�v������������B
    /// </summary>
    public void OnEnable()
    {
        //�v���O�����J�n�ʒu�̍X�V�A�ݒ�ibase��ǂݎ��j
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
    /// �w�i�}�b�v�������i���s���n�C���C�g�������j
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
    /// ���̌�������A���Ɏ��s����`�b�v�̈ʒu��ǂݎ��
    /// </summary>
    /// <param name="_typeOfArrow">�ǂݎ�肽�����̋敪�BRedArrow��BlueArrow</param>
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

        //���̍��W���v���O�����͈̔͂𒴉߂��Ă���ꍇ�A�܂��̓`�b�v���ݒ肳��Ă��Ȃ��ꍇ�APos�����_�ɖ߂�
        if (pos[0] < 0 || pos[0] >= GrobalConst.ProgramMapSize
            || pos[1] < 0 || pos[1] >= GrobalConst.ProgramMapSize
            || programEditor.programMap[pos[0], pos[1]] == (int)GrobalConst.TileId.Empty)
        {
            programStartPos.CopyTo(pos, 0);
        }
    }



    /// <summary>
    /// �v���O�������s����
    /// ��莞�Ԃ��ƂɃv���O�����}�b�v�̓��e��1�}�X�����s����
    /// </summary>
    /// <returns></returns>
    IEnumerator Execution()
    {
        while (true)
        {
            //�ҋ@���A���s����`�b�v�̔w�i�������\��
            //�iBaseStart�������I�ɓh��ւ��邽�߁A��U�Y���̃}�X���֐�������ύX���邱�Ƃɒ��Ӂj
            programEditor.programBaseMap[pos[0], pos[1]] = (int)GrobalConst.TileId.BaseActive;
            programEditor.programBaseTilemap.SetTile(new Vector3Int(pos[0], pos[1], 0), tileSelector.tileNameToTileDictionary["BaseActive"].tileData);


            //�|�[�Y��Ԃ̏ꍇ�A�|�[�Y���I���܂őҋ@
            yield return new WaitUntil(() => !IsPause);

            //��莞�ԑҋ@
            yield return new WaitForSeconds(span);

            //�^�[�������Z
            turn++;
            turnText.text = "TURN:" + turn.ToString();

            //�����\��������
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



            Debug.LogFormat("{0}�b�o�� ���s������W: {1},{2}", span, pos[0], pos[1]);

            //�v���O�������璷�ɂȂ邽�߈ꎞ�ϐ���u��
            int selectedChip = programEditor.programMap[pos[0], pos[1]];

            //�s���`�b�v�̏���
            //�s���̌��ʂɊւ�炸�A�v���O�����ʒu�̈ړ����s���Ă���
            //��񂵂ɂ���ƁA�S�[���������̏����ȂǂƏՓ˂��ăG���[���o��ꍇ������
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

            //IF�`�b�v�̏���
            //�v���C���[���猩�Ċe�`�b�v�ɑΉ����������ɕǂ����邩�ǂ����Ńv���O���������򂷂�
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
