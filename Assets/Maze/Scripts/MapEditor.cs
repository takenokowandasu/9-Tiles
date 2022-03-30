using UnityEngine;
using Grobal;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// �}�E�X������}�b�v�ɔ��f����@�\��񋟂���N���X
/// </summary>
public class MapEditor : MonoBehaviour
{
    [SerializeField]
    private MapInformation mapInformation;

    [SerializeField]
    private TileSelector tileSelector;

    //�}�b�v�����p����O���b�h
    [SerializeField]
    private Grid grid;

    //�}�b�v���\�������J����
    [SerializeField]
    private Camera targetCamera;

    [SerializeField]
    private SoundManager soundManager;

    [SerializeField]
    private PlayerController playerController;



    //History���������
    private const int HISTORYSIZE = 10;

    /// <summary>
    /// �}�b�v���̋L�^����������BUndo�@�\�Ɏg�p����
    /// ProgramEditor��History�Ɠ��l�̋@�\�Ȃ̂œ���
    /// </summary>
    private class History
    {
        public int[] mapsize = new int[2];
        public int[,] mapData;
    }
    //�X�^�b�N�Ń}�b�v���̗������Ǘ�����
    private Stack<History> historyStack = new Stack<History>();


    private void Start()
    {
        //������Ԃ𗚗��Ɏc��
        PushHistory();
    }

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

            //�}�b�v�T�C�Y���ύX����Ă���ꍇ�͉������Ȃ�
            if (!mapInformation.mapsize.SequenceEqual(history.mapsize))
            {
                Debug.Log("Mapsize is not equal");
                return;
            }

            //1�}�X���ǂݎ���Đݒ�
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
    /// ���݂̃}�b�v�󋵂����ɁA�X�^�b�N��History��ǉ�����
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

            //false��Ԃ��̂̓}�b�v�S�Ă����S�Ɉ�v����ꍇ����
            //�}�b�v�T�C�Y�͕ύX�����\�������邱�Ƃɒ���
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
        //�}�E�X�����N���b�N����ƁA�N���b�N�����ʒu��TileSelector���ێ����Ă���^�C����ݒ肷��
        if (Input.GetMouseButtonDown(0))
        {
            //�}�E�X�̍��W���J������̍��W�ɕϊ��A�X��Grid��̍��W�ɕϊ�����
            Vector3Int gridPos = grid.WorldToCell(targetCamera.ScreenToWorldPoint(Input.mousePosition));

            //�N���b�N����Grid��̍��W���}�b�v�f�[�^�̔z��ƈ�v������
            int x = gridPos.x - mapInformation.origin[0];
            int y = gridPos.y - mapInformation.origin[1];

            //x��y���z��̗v�f���𒴂��Ă��邩�A�O���̕ǂ𑀍삵�悤�Ƃ��Ă�����L�����Z��
            //�l�����Ӗ��ȏꍇ���L�����Z��
            if (x <= 0 || mapInformation.mapsize[0] - 1 <= x
                || y <= 0 || mapInformation.mapsize[1] - 1 <= y
                || tileSelector.tileId < (int)GrobalConst.TileId.Floor
                || tileSelector.tileId > (int)GrobalConst.TileId.Goal)
            {
                return;
            }

            //tileSelector�֍��W�ƃ}�b�v�z��A�^�C���}�b�v�A�w�肷��tileId�̏���n���Đݒ肳����
            //�X�^�[�g�n�_��S�[���n�_�̏d������Ȃǂ͂�����ŏ���
            tileSelector.SetTile(x, y,
                mapInformation.mapData,
                mapInformation.mapTilemap,
                tileSelector.idToTileNameDictionary[tileSelector.tileId]);

            //�e��}�b�v�ɕύX���N�����ꍇ�A�V�����󋵂�History�Ƃ��ĕۑ�����
            if (NotTheTopHistoryIsSame())
            {
                soundManager.Play("TileSet");
                PushHistory();
            }

            playerController.SetStartPosition();
        }

        //Ctrl+Z��Undo
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && Input.GetKeyDown(KeyCode.Z))
        {
            LoadHistory();
            soundManager.Play("System_Button");
        }
    }
}
