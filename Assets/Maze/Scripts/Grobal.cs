using UnityEngine;
using System;

namespace Grobal
{
    /// <summary>
    /// �Q�[���S�̂ł悭�g�p�����֐���񋟂���N���X
    /// </summary>
    public static class GrobalMethod
    {
        /// <summary>
        /// ����}�X����㉺���E��1�}�X�ړ������ꍇ�A���W�͂ǂ��ω����邩��Ԃ�
        /// Unity��̍��W�͍�������E��֐����グ�Ă������Ƃɒ��ӂ���
        /// </summary>
        /// <param name="_direction">�������</param>
        /// <returns></returns>
        public static int[] DirToRelativePosition(int _direction)
        {
            //(x,y)�̏��Ɋi�[
            int[] relativePosition = new int[2];

            //���̒l��^����ꂽ�ꍇ�͐��ɂȂ�悤�␳
            while (_direction < 0)
            {
                _direction += GrobalConst.DirectionLength;
            }


            switch (_direction % GrobalConst.DirectionLength)
            {
                case (int)GrobalConst.DirectionId.Up:
                    relativePosition = new int[] { 0, 1 };
                    break;

                case (int)GrobalConst.DirectionId.Right:
                    relativePosition = new int[] { 1, 0 };
                    break;

                case (int)GrobalConst.DirectionId.Down:
                    relativePosition = new int[] { 0, -1 };
                    break;

                case (int)GrobalConst.DirectionId.Left:
                    relativePosition = new int[] { -1, 0 };
                    break;
            }

            return relativePosition;
        }

        /// <summary>
        /// DirToRelativePosition(int)�̃I�[�o�[���[�h
        /// </summary>
        /// <param name="_direction">�������</param>
        /// <returns></returns>
        public static int[] DirToRelativePosition(GrobalConst.DirectionId _direction)
        {
            return DirToRelativePosition((int)_direction);
        }




        /// <summary>
        /// �^����ꂽ����������]������������Ԃ�
        /// enum�^�Œ�`���ꂽ�l��int�^�̒l�������ƁA��������Id���X�g���E���ɕ���ł��邱�Ƃ𗘗p���ċ��߂�
        /// </summary>
        /// <param name="_direction">�������</param>
        /// <param name="_num">Id�̍��B�E���Ȃ琳�A�����Ȃ畉</param>
        /// <returns></returns>
        public static GrobalConst.DirectionId GetRelativeDirection(GrobalConst.DirectionId _direction, int _num)
        {
            //�w�肷��Id���v�Z
            int relativeDirection = (int)_direction + _num;

            //�w�肵��Id�����̒l�ɂȂ����ꍇ�͐��̒l�ɂȂ�悤�␳����
            while (relativeDirection < 0)
            {
                relativeDirection += GrobalConst.DirectionLength;
            }

            //�w�肵��Id��Id���X�g�̃T�C�Y�𒴂����ꍇ�͏�]�����
            while (relativeDirection >= GrobalConst.DirectionLength)
            {
                relativeDirection %= GrobalConst.DirectionLength;
            }

            return (GrobalConst.DirectionId)relativeDirection;
        }



        /// <summary>
        /// �������ɑΉ�����Z���܂��̃I�C���[�p��Ԃ�
        /// Unity��̊p�x�͍����Ő����邱�Ƃɒ���
        /// </summary>
        /// <param name="_direction">�������</param>
        /// <returns></returns>
        public static Quaternion DirToRot(GrobalConst.DirectionId _direction)
        {
            //��������360�x��4�������Ă��邱�Ƃ𗘗p
            return Quaternion.Euler(0, 0, (int)_direction * (-90));
        }

    }


    /// <summary>
    /// �Q�[���S�̂ł悭�g�p����鋤�ʒ萔��ݒ肷��N���X
    /// </summary>
    public static class GrobalConst
    {
        /// <summary>
        /// �e��^�C����ID��ݒ�
        /// �^�C�����Ƃɏ����𕪂��邽�߁A��ӂɌ��߂Ă���
        /// (���̃v���O�����ŁA���̏��Ԃł��邱�Ƃ𗘗p���Č��������v�Z���Ă���ꍇ������)
        /// </summary>
        public enum TileId{
            //�}�b�v���\������^�C��
            //MapInformation�N���X��mapData�imapTilemap�j�ň���

            /// <summary>
            /// �v���C���[�L������������A�}�b�v��̏�
            /// </summary>
            Floor = 0,

            /// <summary>
            /// �v���C���[�L�������i���ł��Ȃ���
            /// </summary>
            Wall = 1,

            /// <summary>
            /// �}�b�v��̃X�^�[�g�n�_
            /// </summary>
            Start = 2,

            /// <summary>
            /// �}�b�v��̃S�[���n�_
            /// </summary>
            Goal = 3,


            //�v���O�����}�b�v���\������^�C���i�Q�[�����ł̓`�b�v�ƌĂԁj
            //ProgramEditor��programMap�iprogramTilemap�j�ň���

            /// <summary>
            /// �s���`�b�v�F�v���C���[�L������O�i������
            /// </summary>
            MoveForward = 4,

            /// <summary>
            /// �s���`�b�v�F�v���C���[�L��������ނ�����
            /// </summary>
            MoveBackward = 5,

            /// <summary>
            /// �s���`�b�v�F�v���C���[�L�������E�ɉ�]������
            /// </summary>
            RotateRight = 6,

            /// <summary>
            /// �s���`�b�v�F�v���C���[�L���������ɉ�]������
            /// </summary>
            RotateLeft = 7,

            /// <summary>
            /// ����`�b�v�F�v���C���[�L�����̐��ʂɕǂ����邩�ǂ���
            /// </summary>
            IfFront = 8,

            /// <summary>
            /// ����`�b�v�F�v���C���[�L�����̉E���ɕǂ����邩�ǂ���
            /// </summary>
            IfRight = 9,

            /// <summary>
            /// ����`�b�v�F�v���C���[�L�����̔w��ɕǂ����邩�ǂ���
            /// </summary>
            IfBack = 10,

            /// <summary>
            /// ����`�b�v�F�v���C���[�L�����̍����ɕǂ����邩�ǂ���
            /// </summary>
            IfLeft = 11,

            /// <summary>
            /// �s���`�b�v�F�������Ȃ�
            /// </summary>
            Wait = 12,


            //�v���O�����}�b�v�̔w�i���\������^�C��
            //ProgramEditor��programBaseMap�iprogramBaseTilemap�j�ň���

            /// <summary>
            /// �v���O�����}�b�v�̔w�i
            /// </summary>
            Base = 13,

            /// <summary>
            /// �v���O�����}�b�v�̔w�i�i���s�̃X�^�[�g�n�_�������j
            /// </summary>
            BaseStart = 14,

            /// <summary>
            /// �v���O�����}�b�v�̔w�i�i���s���̒n�_�������j
            /// </summary>
            BaseActive = 15,


            //�v���O�����}�b�v�ɔz�u�����`�b�v�́u���̎��s��v�������^�C��
            //����`�b�v�Ɋւ��Ắu�ǂ�����ꍇ�̎��s��v������
            //ProgramEditor��programArrowMap�iprogramArrowTilemap�j�ň���

            /// <summary>
            /// �Ԗ��F��
            /// </summary>
            RedArrowUp = 16,

            /// <summary>
            /// �Ԗ��F�E
            /// </summary>
            RedArrowRight = 17,

            /// <summary>
            /// �Ԗ��F��
            /// </summary>
            RedArrowDown = 18,

            /// <summary>
            /// �Ԗ��F��
            /// </summary>
            RedArrowLeft = 19,


            //�v���O�����}�b�v�ɔz�u��������`�b�v�́u�ǂ��Ȃ��ꍇ�̎��s��v�������^�C��
            //�s���`�b�v�ɂ͖��֌W
            //ProgramEditor��programIfArrowMap�iprogramIfArrowTilemap�j�ň���

            /// <summary>
            /// ���F��
            /// </summary>
            BlueArrowUp = 20,

            /// <summary>
            /// ���F�E
            /// </summary>
            BlueArrowRight = 21,

            /// <summary>
            /// ���F��
            /// </summary>
            BlueArrowDown = 22,

            /// <summary>
            /// ���F��
            /// </summary>
            BlueArrowLeft = 23,


            //�ėp�̃^�C��

            /// <summary>
            /// �^�C���������Ȃ���Ԃ𖾎����鎞�Ɏg��
            /// </summary>
            Empty = 24
        };


        /// <summary>
        /// �㉺���E�̌����ɑΉ�����ID��ݒ�
        /// ���������90�x���A�E���ɏ��Ԃ��ݒ肳��Ă���C���[�W
        /// (���̃v���O�����ŁA���̏��Ԃł��邱�Ƃ𗘗p���Č������v�Z���Ă���ꍇ������)
        /// </summary>
        public enum DirectionId{
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3
        };
        //
        /// <summary>
        /// ����ID���X�g�̗v�f��
        /// </summary>
        public static int DirectionLength = Enum.GetNames(typeof(DirectionId)).Length;

        /// <summary>
        /// �Q�[�����̃v���O�����}�b�v�̑傫����ݒ�
        /// �����`�̈�ӂ̃}�X���Ŏw�肷��
        /// </summary>
        public const int ProgramMapSize = 3;
    }

}