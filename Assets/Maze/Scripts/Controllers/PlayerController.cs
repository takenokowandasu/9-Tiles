using UnityEngine;
using Grobal;

/// <summary>
/// �v���C���[�L�����N�^�[�̈ړ����Ǘ�����N���X
/// </summary>
public class PlayerController : MonoBehaviour
{
	[SerializeField]
	//���[�h�`�F���W���[�i�S�[������ɂ̂ݎg�p�j
	private ModeManager modeManager;

	[SerializeField]
	//�}�b�v�f�[�^
	private MapInformation mapInformation;

	[SerializeField]
	//�T�E���h�}�l�[�W���[
	private SoundManager soundManager;


	// �}�b�v��ł�1�}�X������̍��W�����i�萔�j
	// �e��^�C���}�b�v��O���b�h�̃f�t�H���g�̃T�C�Y�Ɠ��l�ł���
	private const float DISTANCE = 1.0f;

	// �^�C���}�b�v��̃^�C���ƃv���C���[�I�u�W�F�N�g�̕\���ʒu�����킹�邽�߂̃s�{�b�g
	private const float PIVOT = 0.5f;


	// transform�̃L���b�V�����i�[����
	private Transform _transform;

	// ������transform�Ƃ͕ʂɕϐ��ŊǗ�����
	private GrobalConst.DirectionId direction;


	void Awake()
    {
    	// transform�̃L���b�V�����擾
    	_transform = transform;
	}


	/// <summary>
	/// mapData�����}�b�v�̃X�^�[�g�n�_�Ɏ��@���ړ�������
	/// </summary>
	public void SetStartPosition()
	{
		if (mapInformation.mapsize == null)
        {
			return;
        }

		//map��ǂݎ���ăX�^�[�g�ʒu���m�F���A����ɂ��킹�Ĉʒu��ݒ�
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

		//�����z�u�ł͌�������ɌŒ�
		direction = GrobalConst.DirectionId.Up;
		transform.rotation = GrobalMethod.DirToRot(direction);
	}



	/// <summary>
	/// �v���C���[�̌��݈ʒu���S�[���n�_���ǂ����𔻒肵�A���̏ꍇ�̓S�[�����̉��o�����s����
	/// </summary>
	private void CheckGoal()
	{
		//���݂̍��W���擾
		Vector2 pos = _transform.position;

		if (mapInformation.mapData[(int)(pos.x - mapInformation.origin[0] - PIVOT),
									(int)(pos.y - mapInformation.origin[1] - PIVOT)] == (int)GrobalConst.TileId.Goal)
        {
			modeManager.SetMode("GoalEffect");
            soundManager.Play("Goal");
        }
    }



	/// <summary>
	/// �v���C���[�L������1�}�X�ړ�������
	/// �v���C���[�L�������猩�Ăǂ̌����Ɉړ����邩�͈����ŗ^����
	/// <param name="_directionNum">��]�̑傫���B���Ȃ�E���B���Ȃ獶���</param>
	/// </summary>
	public void Move(int _directionNum)
	{
		//���݂̌�������O�i����ꍇ�̈ʒu�֌W���擾
		int[] forward = GrobalMethod.DirToRelativePosition(direction + _directionNum);

		//���݂̍��W���擾���A�ړ���̍��W���v�Z
		Vector2 pos = _transform.position;
		pos.x += forward[0] * DISTANCE;
		pos.y += forward[1] * DISTANCE;

		//�ړ���ɕǂ��Ȃ��ꍇ�A�ړ��͐������A���߂����W���I�u�W�F�N�g�ɔ��f
		//�ړ���ɕǂ�����ꍇ�A�ړ��͎��s���A���W�͕ύX���Ȃ�
		//���ꂼ��A�Ή�����SE��炷
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
	/// �v���C���[�L�����N�^�[����]������
	/// </summary>
	/// <param name="_directionNum">��]�̑傫���B���Ȃ�E���B���Ȃ獶���</param>
	public void Rotate(int _directionNum)
	{
		direction = GrobalMethod.GetRelativeDirection(direction, _directionNum);

		//���̌����ɑΉ����ăX�v���C�g����]������
		transform.rotation = GrobalMethod.DirToRot(direction);

		soundManager.Play("Action");
	}

	/// <summary>
	/// �v���C���[�L�����Ɨׂ荇���}�X�ɕǂ����邩�ǂ�����Ԃ�
	/// </summary>
	/// <param name="_directionNum">�v���C���[�L�������猩�ď㉺���E�ǂ̌��������w��</param>
	/// <returns>�w��ʒu�ɕǂ������true</returns>
	public bool WhetherThereIsAWall(int _directionNum)
    {
		//���݈ʒu
		Vector2 pos = _transform.position;

		//�w������ŗׂɂ���}�X�̍��W���v�Z
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
