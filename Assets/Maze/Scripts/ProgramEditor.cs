using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Grobal;


/// <summary>
/// �e��v���O�����}�b�v�̏��ƁA�N���b�N�ɂ��G�f�B�b�g�@�\��񋟂���
/// </summary>
public class ProgramEditor : MonoBehaviour
{

    //���ꏈ���Ɋւ��Id
    public const int ROTATE_MODE_RED = 100;
    public const int ROTATE_MODE_BLUE = 101;

    //�v���O�����}�b�v���_�̃O���b�h��̍��W
    public int[] origin = new int[] { 0, 0 };

    //�v���O�����}�b�v��̃`�b�v�̔z�u�f�[�^
    public int[,] programMap = new int[3,3];
    //�v���O�����}�b�v��̃`�b�v�̑J�ڐ�������Ԗ��̔z�u�f�[�^
    public int[,] programArrowMap = new int[3,3];
    //�v���O�����}�b�v��̃`�b�v�iIf�n�`�b�v��else�̏ꍇ�j�̑J�ڐ���������̔z�u�f�[�^
    public int[,] programIfArrowMap = new int[3, 3];
    //�v���O�����}�b�v�̔w�i�Ǝ��s�J�n�ʒu / ���s���̈ʒu�������z�u�f�[�^
    public int[,] programBaseMap = new int[3, 3];


    //��L�̃v���O�����}�b�v�֘A�ϐ����ꂼ��ɑΉ�����^�C���}�b�v
    public Tilemap programBaseTilemap;
    public Tilemap programTilemap;
    public Tilemap programArrowTilemap;
    public Tilemap programIfArrowTilemap;

    //�^�C���}�b�v��������Grid
    [SerializeField]
    private Grid grid;

    //�^�C���}�b�v���f��J����
    [SerializeField]
    private Camera targetCamera;

    [SerializeField]
    private TileSelector tileSelector;

    [SerializeField]
    private SoundManager soundManager;


    //History���������
    private const int HISTORYSIZE = 10;



    /// <summary>
    /// �}�b�v���̋L�^����������BUndo�@�\�Ɏg�p����
    /// </summary>
    private class History
    {
        public int[,] programMap = new int[3, 3];
        public int[,] programArrowMap = new int[3, 3];
        public int[,] programIfArrowMap = new int[3, 3];
        public int[,] programBaseMap = new int[3, 3];
    }
    //�X�^�b�N�Ń}�b�v���̗������Ǘ�����
    private Stack<History> historyStack = new Stack<History>();



    /// <summary>
    /// �X�^�b�N����A1�O�̏󋵂�History�����o���A�����v���O�����}�b�v�ɔ��f����
    /// �{�^������Ăяo��
    /// </summary>
    public void LoadHistory()
    {
        //HistoryStack�ɂ͏�Ɂu���݂̏󋵁v����ԏ�Ɋi�[����Ă���̂ŁA
        //��O�̏󋵂��Ăяo���ɂ�1��Pop�����Ă���Peek����΂���
        if (historyStack.Count < 2)
        {
            Debug.Log("No stack");
        }
        else
        {
            historyStack.Pop();
            History history = historyStack.Peek();

            //1�}�X���ǂݎ���Đݒ�
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
    /// ���݂̃}�b�v�󋵂����ɁA�X�^�b�N��History��ǉ�����
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

        //���̎��AhistoryStack�̒��g������𒴂��Ă���ꍇ�A
        //��U�ʂ̃X�^�b�N�𐶐����Ă���ꕔ�������߂����ƂŃf�[�^�����炷
        if (historyStack.Count > HISTORYSIZE)
        {
            Debug.Log("Delete oldest history");

            //�R���X�g���N�^�ŃX�^�b�N���R�s�[�B���̎��v�f�̏��Ԃ͋t�ɂȂ�
            Stack<History> tempHistoryStack = new Stack<History>(historyStack);

            //��xPop���Ă���R�s�[�������ƁA��ԌÂ�������������Ԃł��Ƃɖ߂�
            tempHistoryStack.Pop();
            historyStack = new Stack<History>(tempHistoryStack);
        }

    }


    /// <summary>
    /// �X�^�b�N���̍ŐV��History���A���݂̃v���O�����}�b�v�ƈ�v���Ȃ����ǂ����𒲂ׂ�
    /// </summary>
    /// <returns>��v���Ȃ��ꍇ��true�A��v����ꍇ��false</returns>
    private bool NotTheTopHistoryIsSame()
    {
        //�X�^�b�N����History�������Ȃ��ꍇ�A��������True��Ԃ�
        if (historyStack.Count < 1)
        {
            Debug.Log("Check:No stack");
            return true;
        }
        else
        {
            Debug.Log("Check:GetStack");
            //�ŐV��History���f�[�^�������o���Ĕ�r
            History history = historyStack.Peek();

            //false��Ԃ��̂�4�̃}�b�v�S�Ă����S�Ɉ�v����ꍇ����
            return !(programBaseMap.Cast<int>().SequenceEqual(history.programBaseMap.Cast<int>())
                && programMap.Cast<int>().SequenceEqual(history.programMap.Cast<int>())
                && programArrowMap.Cast<int>().SequenceEqual(history.programArrowMap.Cast<int>())
                && programIfArrowMap.Cast<int>().SequenceEqual(history.programIfArrowMap.Cast<int>()));
        }

    }


    /// <summary>
    /// �v���O�����}�b�v������������
    /// </summary>
    void Start()
    {
        //�e��^�C���}�b�v�̌��_�ƂȂ���W��ݒ肷��
        //���̂Ƃ��AGridLayout.CellToWorld���g�����ƂŃO���b�h�ɉ����Ĕz�u����
        //�i���̂܂�Vector3�������悤�Ƃ����ꍇ�A�e�ł���L�����o�X��O���b�h�̃T�C�Y�̉e���ňӐ}���Ȃ��z�u�ɂȂ�\���������j
        programBaseTilemap.transform.position = grid.CellToWorld(new Vector3Int(origin[0], origin[1], 4));
        programTilemap.transform.position = grid.CellToWorld(new Vector3Int (origin[0], origin[1], 3) );
        programArrowTilemap.transform.position = grid.CellToWorld(new Vector3Int(origin[0], origin[1], 2));
        programIfArrowTilemap.transform.position = grid.CellToWorld(new Vector3Int(origin[0], origin[1], 1));


        //programTilemap.ClearAllTiles();
        //programBaseTilemap.ClearAllTiles();
        //programArrowTilemap.ClearAllTiles();
        //programIfArrowTilemap.ClearAllTiles();

        //�v���O�����}�b�v��w�i�����Ȃ���Ԃɐݒ�
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

        //������Ԃ𗚗��Ɏc��
        PushHistory();
    }


    /// <summary>
    /// �v���O�����}�b�v�̔w�i�ȊO�̑S�Ă�����������B
    /// �{�^���ŌĂяo�����Ƃ�z��
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
    /// �v���O�����}�b�v��̃`�b�v�����i���j���E���ɉ�]������
    /// �Ԗ��ł����ł���{�I�ȏ����͋��ʂł��邽�߁A1�̊֐��ŏ�������
    /// </summary>
    /// <param name="x">�v���O�����}�b�v���x���W</param>
    /// <param name="y">�v���O�����}�b�v���y���W</param>
    /// <param name="_arrowMap">�}�b�v�p�z��BprogramArrowMap��programIfArrowMap</param>
    /// <param name="_tilemap">�Ή�����^�C���}�b�v</param>
    void RotateArrow(int x, int y, int[,] _arrowMap, Tilemap _tilemap)
    {
        //����������󂪂Ȃ���΁A�������Ȃ�
        if (_arrowMap[x, y] == (int)GrobalConst.TileId.Empty)
        {
            return;
        }

        Debug.Log(tileSelector.GetNextArrowTileName(_arrowMap[x, y]));

        //�����łȂ��ꍇ�A�E���ɉ�]������
        tileSelector.SetTile(x, y,
            _arrowMap, _tilemap,
            tileSelector.GetNextArrowTileName(_arrowMap[x, y]));

        //�Ԗ��Ɛ�󂪏d�Ȃ��Ă��܂��ꍇ�A��������1���ړ�������
        //�璷�����A�Ԗ��̃}�b�v���n����Ă���ꍇ�͐��̃}�b�v�ƁA
        //���̃}�b�v���n����Ă���ꍇ�͐Ԗ��̃}�b�v�Ɣ�r����悤�ɖ��L����
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
    /// �v���O�����}�b�v�𑀍삷��
    /// </summary>
    void Update()
    {
        //Ctrl+Z��Undo
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && Input.GetKeyDown(KeyCode.Z))
        {
            LoadHistory();
            soundManager.Play("System_Button");
            return;
        }


        //�}�E�X�̍��W���v���O�����}�b�v��̍��W�ɕϊ�����
        Vector3Int gridPos = grid.WorldToCell(targetCamera.ScreenToWorldPoint(Input.mousePosition));
        int x = gridPos.x - origin[0];
        int y = gridPos.y - origin[1];

        //�v���O�����}�b�v�͈̔͂𒴂��Ă���ꍇ�A�������Ȃ�
        if (x < 0 || GrobalConst.ProgramMapSize <= x 
            || y < 0 || GrobalConst.ProgramMapSize <= y)
        {
            return;
        }


        //�}�E�X���E�N���b�N����ƁA���̃`�b�v�̕�������]������
        if (Input.GetMouseButtonDown(1))
        {
            //Shift�������Ă����Ԃ̏ꍇ�AIF�p�̐�����]������
            //�����łȂ��ꍇ�A�Ԗ�����]������
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


            //�e��}�b�v�ɕύX���N�����ꍇ�A�V�����󋵂�History�Ƃ��ĕۑ�����
            if (NotTheTopHistoryIsSame())
            {
                soundManager.Play("TileSet");
                PushHistory();
            }
        }


        //�}�E�X�����N���b�N����ƁATileSelector.tileId�ɉ����ăv���O�����}�b�v�𑀍삷��
        if (Input.GetMouseButtonDown(0))
        {
            //����ȑ�����s���ꍇ
            switch (tileSelector.tileId)
            {
                //�Ԗ���]���[�h
                case ROTATE_MODE_RED:
                    RotateArrow(x, y, programArrowMap, programArrowTilemap);
                    break;

                //����]���[�h
                case ROTATE_MODE_BLUE:
                    RotateArrow(x, y, programIfArrowMap, programIfArrowTilemap);
                    break;

                default:
                    break;
            }


            Debug.Log(tileSelector.idToTileNameDictionary[programMap[x, y]] 
                + " " + tileSelector.idToTileNameDictionary[programArrowMap[x, y]]
                + " " + tileSelector.idToTileNameDictionary[programIfArrowMap[x, y]]);




            //�w�肵���ʒu�̃`�b�v���I�𒆂̂��̂ƈ�v����ꍇ�F����]���[�h�łȂ��Ă����̉�]���s���B
            if (programMap[x, y] == tileSelector.tileId)
            {
                //Shift�������Ă����Ԃ̏ꍇ�AIF�p�̐�����]������B
                //�����łȂ��ꍇ�A�Ԗ�����]������B
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    RotateArrow(x, y, programIfArrowMap, programIfArrowTilemap);
                }
                else
                {
                    RotateArrow(x, y, programArrowMap, programArrowTilemap);
                }
            }




            //�s���`�b�v��z�u����ꍇ
            if (tileSelector.tileId == (int)GrobalConst.TileId.MoveForward ||
                tileSelector.tileId == (int)GrobalConst.TileId.MoveBackward ||
                tileSelector.tileId == (int)GrobalConst.TileId.RotateRight ||
                tileSelector.tileId == (int)GrobalConst.TileId.RotateLeft ||
                tileSelector.tileId == (int)GrobalConst.TileId.Wait)
            {
                //�w�肵���ʒu�ɐԖ�󂪖�����Ή��ݒ肷��B
                if (programArrowMap[x, y] == (int)GrobalConst.TileId.Empty)
                {
                    tileSelector.SetTile(x, y,
                        programArrowMap, programArrowTilemap,
                        GrobalConst.TileId.RedArrowUp.ToString());
                }

                //�w�肵���s���`�b�v��ݒ肷��B���͕K�����������
                tileSelector.SetTile(x, y, 
                    programMap, programTilemap,
                    tileSelector.idToTileNameDictionary[tileSelector.tileId]);

                tileSelector.SetTile(x, y,
                    programIfArrowMap, programIfArrowTilemap,
                    GrobalConst.TileId.Empty.ToString());
            }



            //If�n�`�b�v��z�u����ꍇ
            if (tileSelector.tileId == (int)GrobalConst.TileId.IfFront ||
                tileSelector.tileId == (int)GrobalConst.TileId.IfRight ||
                tileSelector.tileId == (int)GrobalConst.TileId.IfBack ||
                tileSelector.tileId == (int)GrobalConst.TileId.IfLeft)
            {
                //�w�肵���ʒu�ɐԖ�󂪖�����Ή��ݒ肷��B
                if (programArrowMap[x, y] == (int)GrobalConst.TileId.Empty)
                {
                    tileSelector.SetTile(x, y,
                        programArrowMap, programArrowTilemap,
                        GrobalConst.TileId.RedArrowUp.ToString());
                }

                //���l�ɁA�w�肵���ʒu�ɐ�󂪖�����Ή��ݒ肷��B
                //�����͏d����h�����߂ɐԖ��ׂ̗ɂ���B
                if (programIfArrowMap[x, y] == (int)GrobalConst.TileId.Empty)
                {
                    tileSelector.SetTile(x, y,
                        programIfArrowMap, programIfArrowTilemap,
                        "BlueArrow" + tileSelector.GetNextArrowName(programArrowMap[x, y]));
                }

                //�w�肵���s���`�b�v��ݒ肷��B
                tileSelector.SetTile(x, y,
                    programMap, programTilemap,
                    tileSelector.idToTileNameDictionary[tileSelector.tileId]);
            }


            //Empty�FEmpty��I�������ꍇ�A��/�����܂Ƃ߂ď���������
            if (tileSelector.tileId == (int)GrobalConst.TileId.Empty)
            {
                tileSelector.SetTile(x, y, programMap, programTilemap, GrobalConst.TileId.Empty.ToString());
                tileSelector.SetTile(x, y, programArrowMap, programArrowTilemap, GrobalConst.TileId.Empty.ToString());
                tileSelector.SetTile(x, y, programIfArrowMap, programIfArrowTilemap, GrobalConst.TileId.Empty.ToString());
            }


            //BaseStart�F�v���O�����J�n�n�_�̕ύX�i�d���Ȃǂ̔����SetTile���ōs���j
            if (tileSelector.tileId == (int)GrobalConst.TileId.BaseStart)
            {
                tileSelector.SetTile(x, y, programBaseMap, programBaseTilemap, GrobalConst.TileId.BaseStart.ToString());
            }


            //�e��}�b�v�ɕύX���N�����ꍇ�A�V�����󋵂�History�Ƃ��ĕۑ�����
            if (NotTheTopHistoryIsSame())
            {
                soundManager.Play("TileSet");
                PushHistory();
            }

        }


    }
}