using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��҂��p�ӂ�������V��ł��炤���߂̃X�e�[�W�Z���N�g�@�\��񋟂���
/// </summary>

public class StageSelectManager : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private ModeManager modeManager;

    [SerializeField]
    private SaveLoadManager saveLoadManager;

    [SerializeField]
    private SoundManager soundManeger;

    [SerializeField]
    private MapInformation mapInformation;

    // �e�X�e�[�W��I�ԃ{�^���̃v���n�u
    [SerializeField]
    private GameObject buttonPrefab = null;


    //���������C���X�^���X�Ƃ��̃R���|�[�l���g
    private GameObject buttonObj;
    private Button button;

    //�N���A�L�^�̕\���p�e�L�X�g�̈ꗗ
    private Text[] clearDataTextObjects = new Text[SaveLoadManager.STAGENUM];

    /// <summary>
    /// �e�X�e�[�W��I�ԃ{�^���𐶐�����
    /// </summary>
    void Awake()
    {
        saveLoadManager.Load();

        for (int stageId = 1; stageId <= SaveLoadManager.STAGENUM; stageId++)
        {
            //�v���n�u����{�^�����q�I�u�W�F�N�g�Ƃ��Đ���
            //�ʒu�֌W�́A���̃X�N���v�g���A�^�b�`����Ă���I�u�W�F�N�g��GridLayoutGroup�Œ������邱�Ƃ�z��
            buttonObj = Instantiate(buttonPrefab, this.transform.position , Quaternion.identity, gameObject.transform);

            //�v���n�u�ɗ\�ߎ��t�����e�L�X�g�R���|�[�l���g���擾
            //�X�e�[�WID��ݒ�
            Text stageIdTextObject = buttonObj.transform.Find("stageId").GetComponent<Text>();
            stageIdTextObject.text = stageId.ToString();

            //�v���n�u�ɗ\�ߎ��t�����e�L�X�g�R���|�[�l���g���擾
            //�N���A�L�^�\���p�e�L�X�g��z��֊i�[
            clearDataTextObjects[stageId - 1] = buttonObj.transform.Find("DATA_STAGE").GetComponent<Text>();

            //�N���b�N�ŃX�e�[�W�ǂݍ��݂�SE�Đ����s����悤�ɃC�x���g���X�i�[��ݒ�
            //���̎��A���[�v�̃C���f�b�N�X�������Ƃ��Ē��ړn���ƎQ�Ɠn���ɂȂ�̂ŁA�ꎞ�ϐ����쐬���Ēl�n���ɂ���
            int stageIdTemp = stageId;
            button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => this.GetStageData(stageIdTemp));
            button.onClick.AddListener(() => soundManeger.Play("System_Button"));
        }
    }

    
    /// <summary>
    /// Enable�ɂȂ邽�сi�X�e�[�W�Z���N�g��ʂɓ��邽�сj�A�e�X�e�[�W�̃N���A�L�^�\�����X�V����
    /// </summary>
    private void OnEnable()
    {
        for (int i = 0; i < SaveLoadManager.STAGENUM; i++)
        {
            Debug.Log(i);

            int minChip = saveLoadManager.minChips[i];
            int minTurn = saveLoadManager.minTurns[i];
            int maxTurn = saveLoadManager.maxTurns[i];

            //�f�[�^�������l��ԂȂ牽���\�����Ȃ����Ƃɒ���
            if (minChip == SaveLoadManager.INITIALVALUE)
            {
                clearDataTextObjects[i].text = "";
            }
            else
            {
                clearDataTextObjects[i].text = " " + minChip.ToString() + " / " + minTurn.ToString() + " / " + maxTurn.ToString();
            }

        }
    }


    /// <summary>
    /// mapData�Ɏw��t�@�C���̃f�[�^��ǂݍ��܂���
    /// �t�@�C���l�[���́A�uSTAGE�v�{�u�����ŗ^����ԍ��v�Ŏw��
    /// �{�^������Ăяo����悤��public�ɂ��Ă���
    /// </summary>
    /// <param name="_stageId">�w�肵�����X�e�[�W��Id</param>
    public void GetStageData(int _stageId)
    {
        string filename = "STAGE" + _stageId.ToString();
        Debug.Log(filename);

        //�X�e�[�W�Z���N�g�p�̃v���C���[�h�ڍs
        modeManager.SetMode("ProgramEdit");

        //�}�b�v�f�[�^��ǂݍ��܂���
        mapInformation.InitByFile(filename);

        //�v���C���[�L�����̈ʒu��������
        playerController.SetStartPosition();
    }
}
