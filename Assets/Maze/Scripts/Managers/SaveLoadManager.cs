using UnityEngine;
using Grobal;


/// <summary>
/// �f�[�^�̃Z�[�u�����[�h����������N���X
/// WebGL�ł̌��J��z�肵�APlayerPrefs�𗘗p����B
/// �܂��ARPG�A�c�}�[���ւ̌��J���ɂ̓v���O�C������ăT�[�o�[�̎��Z�[�u�f�[�^�𑀍삷��B
/// ���X�^���h�A�����ł��ƁAPlayerPrefs�����ڃ��W�X�g���ɃA�N�Z�X���Ă��܂��s�s���������B
///   ������������ꍇ��JSON�Ń��[�J���ɕۑ�����ق����悢
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


    //��҂��p�ӂ����X�e�[�W�̐�
    public const int STAGENUM = 16;

    //�e�X�e�[�W�̍ŏ��`�b�v���L�^
    public int[] minChips;
    //�e�X�e�[�W�̍ŏ�/�ő�^�[�����L�^
    public int[] minTurns;
    public int[] maxTurns;

    //���N���A�X�e�[�W�̋L�^�����l
    public const int INITIALVALUE = 999;


    /// <summary>
    /// �Z�[�u�f�[�^��ǂݍ���
    /// SyncSaveDataAsync() �֐���ҋ@���邽�߂ɂ́A�֐��� async ��`���K�v
    /// </summary>
    private async void Awake()
    {
        Load();
    }






    /// <summary>
    /// �Z�[�u�f�[�^��ǂݍ���
    /// </summary>
    public void Load()
    {
        //�f�[�^������
        minChips = new int[STAGENUM];
        minTurns = new int[STAGENUM];
        maxTurns = new int[STAGENUM];

        //�f�[�^�ǂݍ���
        //Key��ID*100�ɕϐ����Ƃ̘A�ԂŎw�肷��
        int loadKey;
        for (int i = 1; i <= STAGENUM; i++)
        {
            loadKey = i * 100;

            // �w��̃L�[�̃Z�[�u�f�[�^������Γǂݍ��݁A������Ώ����l��ݒ�
            minChips[i - 1] = PlayerPrefs.GetInt((loadKey + 1).ToString(), INITIALVALUE);
            minTurns[i - 1] = PlayerPrefs.GetInt((loadKey + 2).ToString(), INITIALVALUE);
            maxTurns[i - 1] = PlayerPrefs.GetInt((loadKey + 3).ToString(), -INITIALVALUE);

            Debug.Log(string.Format("stage:{0} minChips:{1} minTurns:{2} maxTurns:{3}", i, minChips[i - 1], minTurns[i - 1], maxTurns[i - 1]));
        }
    }




    /// <summary>
    /// �X�e�[�W�N���A���ɌĂяo���A���̍ۂ̃f�[�^��ۑ�����
    /// </summary>
    public void Save()
    {
        //��҂��p�ӂ����X�e�[�W���ǂ����́AmapData��̏��Ŕ���
        bool IsMap;
        int mapId;
        (IsMap, mapId) = mapInformation.GetMapId();
        
        //��҂��p�ӂ������̂łȂ��ꍇ�A�Z�[�u�͍s��Ȃ�
        //�S�[�������ۂɃ}�j���A�����[�h�������ꍇ�����l
        if (!IsMap || charaManualController.gameObject.activeInHierarchy)
        {
            return;
        }

        //�v���O�����}�b�v��̃`�b�v�����m�F
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

        //�v���O�����̎��s�ɂ��������^�[�������m�F
        int turn = programExecutor.turn;


        //�X�e�[�WID�ɍ��킹��Key�̊�l��ݒ肷��B�e�ϐ���A�ԂŋL�^����
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

        //�ێ����Ă���ϐ����L�^�Ɠ���
        minChips[mapId - 1] = PlayerPrefs.GetInt((saveKey + 1).ToString(), INITIALVALUE);
        minTurns[mapId - 1] = PlayerPrefs.GetInt((saveKey + 2).ToString(), INITIALVALUE);
        maxTurns[mapId - 1] = PlayerPrefs.GetInt((saveKey + 3).ToString(), -INITIALVALUE);


        Debug.Log(string.Format("stage:{0} chips:{1} turns:{2}", mapId, chips, turn));
        Debug.Log(string.Format("stage:{0} minChips:{1} minTurns:{2} maxTurns:{3}", 
            mapId, minChips[mapId - 1], minTurns[mapId - 1], maxTurns[mapId - 1]));
    }

}
