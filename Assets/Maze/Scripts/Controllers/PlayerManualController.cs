using UnityEngine;


/// <summary>
/// �v���C���[�L�����N�^�[�̎蓮�����񋟂���N���X
/// </summary>
public class PlayerManualController : MonoBehaviour
{
    [SerializeField]
    private PlayerController playerController;

    /// <summary>
    /// �v���C���[�I�u�W�F�N�g���������ĕێ�
    /// </summary>
    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    /// <summary>
    /// �L�[����ɉ������������Ăяo��
    /// </summary>
    void Update()
    {	
    	//���։�]
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
			playerController.Rotate(-1);
        } 
    	//�E�։�]
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            playerController.Rotate(1);
        } 
    	//�O�i
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            playerController.Move(0);
        } 
    	//���
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            playerController.Move(2);
        }
    	
    }
}
