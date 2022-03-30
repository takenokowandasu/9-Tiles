using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// �J��������p�v���O����
/// ���C���J�����̃I�u�W�F�N�g�ɃA�^�b�`���Ďg��
/// </summary>
public class MainCameraController : MonoBehaviour
{
    //�J������Transform��Camera�R���|�[�l���g
    Transform tf;
    Camera cam;

    //�J�����̈ړ����x�i�萔�j
    private const float CAMERASPEED = 0.02f;

    //�J�����̃T�C�Y�ύX�E���W�ړ��̏���l�E�����l�i�萔�j
    private const int CAMERALIMIT_SIZEMIN = 2;
    private const int CAMERALIMIT_SIZEMAX = 9;
    private const int CAMERALIMIT_X = 5;
    private const int CAMERALIMIT_Y = 5;

    //�J�������C�ӂő���\�ȏ�Ԃł��邩�ǂ���
    public bool CanManualControll = true;

    //�}�E�X���h���b�O���Ă��邩�ǂ���
    private bool IsMouseWheelDown;

    //�}�E�X�h���b�O���̍��W
    private Vector3 mouseDragPos;



    //�ȉ��A�S�[�����o�Ɋւ��e��ϐ��̓C���X�y�N�^�[��Œ����\�ɂ���

    //�S�[�����o�̕K�v����
    [SerializeField]
    private float goalEffectTime = 3.0f;

    //�S�[�����o�̏���������
    [SerializeField]
    private int loopTimes = 100;

    //�S�[�����o���A�Ώۂɂ���I�u�W�F�N�g
    private GameObject targetGameObject;

    //�S�[�����o���A�ΏۃI�u�W�F�N�g����̍��W�̂�������߂�s�{�b�g
    [SerializeField]
    private Vector3 goalEffectPivot = new Vector3(0, 2, -10);

    //�S�[�����o�̖ڕW�J�����T�C�Y
    [SerializeField]
    private float targetCameraSize = 7.0f;




    /// <summary>
    /// �J�����̃R���|�[�l���g���擾
    /// </summary>
    void Start()
    {
        tf = this.gameObject.GetComponent<Transform>();
        cam = this.gameObject.GetComponent<Camera>();
    }


    /// <summary>
    /// ���t���[���̃L�[������擾���A����ɑΉ������J����������s��
    /// </summary>
    void Update()
    {
        //�蓮����֎~���͉������Ȃ�
        if (!CanManualControll)
        {
            return;
        }

        //�J�����̑O�㍶�E����̃X�s�[�h�́A��{�̑��x�ɃJ�����̃T�C�Y����Z���邱�ƂŒ���
        float cameraMove = CAMERASPEED * cam.orthographicSize;

        //�L�[�{�[�h�ɂ�鑀��F�����Ă���L�[�ɉ����ăJ�������ړ�������
        if (Input.GetKey(KeyCode.Q))
        {
            cam.orthographicSize -= CAMERASPEED; //�Y�[���C��
        }
        else if (Input.GetKey(KeyCode.E))
        {
            cam.orthographicSize += CAMERASPEED; //�Y�[���A�E�g
        }

        if (Input.GetKey(KeyCode.W))
        {
            tf.position += new Vector3(0.0f, cameraMove, 0.0f); //�J��������ֈړ�
        }
        else if (Input.GetKey(KeyCode.S))
        {
            tf.position += new Vector3(0.0f, -cameraMove, 0.0f); //�J���������ֈړ�
        }

        if (Input.GetKey(KeyCode.A))
        {
            tf.position += new Vector3(-cameraMove, 0.0f, 0.0f); //�J���������ֈړ�
        }
        else if (Input.GetKey(KeyCode.D))
        {
            tf.position += new Vector3(cameraMove, 0.0f, 0.0f); //�J�������E�ֈړ�
        }

        
        //�}�E�X�ɂ�鑀��F�}�E�X�z�C�[���������Ȃ���h���b�O���邱�ƂŃJ�������ړ�������
        if (Input.GetMouseButtonDown(2))
        {
            OnMouseWheelTouchDown();
        }
        else if (Input.GetMouseButton(2))
        {
            OnMouseWheelTouchDrug();
        }
        else if (Input.GetMouseButtonUp(2))
        {
            IsMouseWheelDown = false;
        }

        //�}�E�X�ɂ�鑀��F�}�E�X�z�C�[������]�����邱�ƂŃJ�������g�k����
        OnMouseWheelScroll();

        //�����͈͂Ɏ��܂��Ă��邩�ǂ����`�F�b�N
        CheckCameraLimit();
    }

    /// <summary>
    /// �J�����������͈͓��Ɏ��܂��Ă��Ȃ��ꍇ�A�����I�ɔ͈͓��Ɉړ�������
    /// </summary>
    private void CheckCameraLimit()
    {
        if (tf.position.x > CAMERALIMIT_X)
        {
            tf.position = new Vector3(CAMERALIMIT_X, tf.position.y, tf.position.z);
        }
        else if (tf.position.x < -CAMERALIMIT_X)
        {
            tf.position = new Vector3(-CAMERALIMIT_X, tf.position.y, tf.position.z);
        }

        if (tf.position.y > CAMERALIMIT_Y)
        {
            tf.position = new Vector3(tf.position.x, CAMERALIMIT_Y, tf.position.z);
        }
        else if (tf.position.y < -CAMERALIMIT_Y)
        {
            tf.position = new Vector3(tf.position.x, -CAMERALIMIT_Y, tf.position.z);
        }

        if (cam.orthographicSize < CAMERALIMIT_SIZEMIN)
        {
            cam.orthographicSize = CAMERALIMIT_SIZEMIN;
        }
        else if (cam.orthographicSize > CAMERALIMIT_SIZEMAX)
        {
            cam.orthographicSize = CAMERALIMIT_SIZEMAX;
        }
    }


    /// <summary>
    /// �S�[�����̃J�����ړ����o���J�n����
    /// </summary>
    public void StartGoalEffect(GameObject _gameObject)
    {
        //�J�����̎蓮������֎~
        CanManualControll = false;

        //�Ώۂɂ���I�u�W�F�N�g����ێ�
        targetGameObject = _gameObject;

        StartCoroutine("GoalEffect");
    }


    /// <summary>
    /// �S�[�����̃J�������o
    /// ��莞�Ԃ����đΏۃI�u�W�F�N�g�ւȂ߂炩�ɃJ�������߂Â���
    /// 
    /// �Ȃ߂炩�ɃJ�������ړ������邽�߁A�C�[�W���O���v�Z���ď����݂ɏ������s���Ă���
    /// ���_�FWaitForSeconds�̓t���[���P�ʂŔ�����s���Ă��邽�߁A�e���[�v���Ƃ�1�t���[���̒x�ꂪ�������Ă���\��������
    /// �o���邾���z�肵�Ă�������Ԃɋ߂Â��邽�߁A�ݒ��̖ڕW���Ԃ����łȂ��A���[�v�񐔂ɂ���đz�肳���x��̒������������đҋ@���Ԃ�ݒ肷��
    /// </summary>
    /// <returns></returns>
    IEnumerator GoalEffect()
    {
        //���o�J�n���̃J�����̍��W�ƃT�C�Y��ۑ�
        Vector3 cameraOriginalPos = tf.position;
        float cameraOriginalSize = cam.orthographicSize;

        //�ڕW�̍��W�́A�ΏۃI�u�W�F�N�g�̍��W�{�s�{�b�g
        Vector3 targetPosition = targetGameObject.transform.position + goalEffectPivot;


        Debug.Log(DateTime.Now);

        //�w��̉񐔂ɕ����āA�ҋ@�ƃJ�����̈ړ����J��Ԃ�
        for (int i = 0; i < loopTimes; i++)
        {
            yield return new WaitForSeconds( (goalEffectTime - loopTimes / Application.targetFrameRate) /�@loopTimes);

            //�ڕW���W�ƌ��X�̍��W�̍����g���Ċ�{�ړ��ʂ����߁A����ɃC�[�W���O�p�֐��̒l����Z���Ĉړ����̍��W�����肷��
            //�T�C�Y�����l�ł���
            float easing = easeOutCubic(i, loopTimes);
            tf.position = cameraOriginalPos + (targetPosition - cameraOriginalPos) * easing;
            cam.orthographicSize = cameraOriginalSize + (targetCameraSize - cameraOriginalSize) * easing;
        }

        Debug.Log(DateTime.Now);


        //���o�I�����̏���


        //�J�����̎蓮���������
        CanManualControll = true;

        //�I�u�W�F�N�g����j��
        targetGameObject = null;

        CheckCameraLimit();
    }



    /// <summary>
    /// OutCubic�̃C�[�W���O�Ɋ�Â����ω��x������Ԃ�
    /// Math.Pow��Double�^�ɑ΂��鏈����O��ɂ��Ă��邽�߁A������Double�^�œn�����Ƃɒ���
    /// </summary>
    /// <param name="_loop">���݂̃��[�v��</param>
    /// <param name="_loopTimes">���[�v�ڕW��</param>
    /// <returns></returns>
    private float easeOutCubic(double _loop, double _loopTimes)
    {
        return (float)(1 - Math.Pow(1 - _loop / _loopTimes, 3.0));
    }



    /// <summary>
    /// �}�E�X�z�C�[���ɂ��h���b�O�@�\�̔���ɗ��p����
    /// �}�E�X�z�C�[�������������_�Ń}�E�X���J�����̉�ʓ��ɂ���ꍇ�̂݁A�h���b�O�@�\��L���ɂ��A���̎��_�ł̃O���[�o�����W��ۑ�����
    /// </summary>
    private void OnMouseWheelTouchDown()
    {
        Vector3 mousePosInViewport = cam.ScreenToViewportPoint(Input.mousePosition);

        Debug.Log(mousePosInViewport);

        if (mousePosInViewport.x < 0 || mousePosInViewport.x > 1 
            || mousePosInViewport.y < 0 || mousePosInViewport.y > 1)
        {
            return;
        }
        else
        {
            IsMouseWheelDown = true;
            mouseDragPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    /// <summary>
    /// �h���b�O���̏���
    /// �J�����������͈͓��Ɏ��܂��Ă���ꍇ�A�O��Ăяo����������̃}�E�X�̈ړ��ʂ��J�����̍��W�ɉ��Z����
    /// </summary>
    private void OnMouseWheelTouchDrug()
    {
        //���݂̃}�E�X�̃O���[�o�����W���擾
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        //�h���b�O���łȂ����A�}�E�X���W���ω����Ă��Ȃ���Ή������Ȃ�
        if (!IsMouseWheelDown || mouseDragPos == mousePos)
        {
            return;
        }

        //���݂̃}�E�X���W���A�L�^����Ă����_�Ɣ�r���ăJ�����̍��W���ړ�
        tf.position += mouseDragPos - mousePos;

        //�}�E�X�̃O���[�o�����W�̊�_���X�V����
        //�i�J�������猩���}�E�X�̍��W�́A�J�������ړ��������Ƃɂ���ĕϓ����邱�Ƃɒ��Ӂj
        mouseDragPos = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// �}�E�X�z�C�[���̉�]�ʂɉ����ăJ�����̃T�C�Y��ω�������
    /// </summary>
    private void OnMouseWheelScroll()
    {
        //�}�E�X���J������ʓ��ɓ����Ă��Ȃ��ꍇ�͉������Ȃ�
        var mousePosInViewport = cam.ScreenToViewportPoint(Input.mousePosition);
        if (mousePosInViewport.x < 0 || mousePosInViewport.x > 1
            || mousePosInViewport.y < 0 || mousePosInViewport.y > 1)
        {
            return;
        }

        //�z�C�[���̉�]�ʂ��擾
        float scroll = Input.mouseScrollDelta.y;

        //��]�ʂ����ƂɃJ�����̃T�C�Y��ω�������
        //���ӓ_�F�J�����̃T�C�Y���傫���قǃI�u�W�F�N�g��������������B���Ƀz�C�[�����񂷂�Input.mouseScrollDelta�̓v���X�̒l�����B
        cam.orthographicSize -= cam.orthographicSize * scroll * CAMERASPEED;

    }
}