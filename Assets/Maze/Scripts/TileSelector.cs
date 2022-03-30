using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Grobal;
using System;


/// <summary>
/// �e��^�C���}�b�v�ҏW�p�̋@�\��񋟂���N���X
/// UI��̃{�^���őI������^�C����ς�����ق��A�I�𒆂̃{�^�����n�C���C�g����@�\������
/// </summary>

public class TileSelector : MonoBehaviour
{
    //�I�𒆂̃^�C����ID
    public int tileId;

    //�n�C���C�g�p�X�v���C�g
    [SerializeField]
    private GameObject highlightPrefab;

    //��n�C���C�g���̊O�g�p�X�v���C�g
    [SerializeField]
    private GameObject backgroundPrefab;

    //�I�u�W�F�N�g���ʗp�̕ϐ�
    private GameObject highlightObj;



    /// <summary>
    /// �^�C�������܂Ƃ߂ĊǗ�����N���X
    /// �ȒP�̂��߁A�C���X�y�N�^�[��ŕҏW�\�ɂ���
    /// </summary>
    [Serializable]
    public class TileInformation
    {
        /// <summary>
        /// �Q�[�����ł̃^�C���̖���
        /// GrobalConst.TileId�̃��X�g�Ɩ��̂𓝈ꂷ�邱��
        /// </summary>
        public string tileName;

        /// <summary>
        /// �Ή�����Tile
        /// </summary>
        public Tile tileData;

        /// <summary>
        /// ���̃^�C�����g����^�C���}�b�v����1�������݂��Ȃ����̂ł��邩�ǂ��������߂�
        /// </summary>
        public bool IsSingle;

        /// <summary>
        /// IsSingle==true�ł̂ݎg�����
        /// �Â��^�C������������ꍇ�ɁA����ɒu���^�C���̖��O�����߂�
        /// </summary>
        public string defaultTileName;

        /// <summary>
        /// �\�����鎞�̉�]�p�i�I�C���[�p�j
        /// 1�̃^�C���ŕ����̕\�������˂鎞�Ɏg�p����
        /// </summary>
        public Vector3Int rot;
    }

    [SerializeField]
    private TileInformation[] tileInformations;

    //TileInformation�̊Ǘ��pDictionary
    public Dictionary<string, TileInformation> tileNameToTileDictionary = new Dictionary<string, TileInformation>();
    public Dictionary<string, int> tileNameToIdDictionary = new Dictionary<string, int>();
    public Dictionary<int, string> idToTileNameDictionary = new Dictionary<int, string>();
    public Dictionary<int, TileInformation> idToTileDictionary = new Dictionary<int, TileInformation>();




    /// <summary>
    /// �Q�[���J�n���ɁA�z��ɓo�^�����^�C�������Ăяo���₷���悤�Ɏ����ɃZ�b�g����
    /// Id�̔ԍ��͖��̂ɑΉ�����l���擾���Đݒ肷��
    /// </summary>
    private void Awake()
    {
        foreach (TileInformation tileInformation in tileInformations)
        {
            //�^�C�������s���ȏꍇ�A�X�L�b�v����
            if(Enum.TryParse(tileInformation.tileName, out GrobalConst.TileId tileName))
            {
                tileNameToTileDictionary.Add(tileInformation.tileName, tileInformation);
                tileNameToIdDictionary.Add(tileInformation.tileName, (int)tileName);

                idToTileNameDictionary.Add((int)tileName, tileInformation.tileName);
                idToTileDictionary.Add((int)tileName, tileInformation);
            }
        }
    }



    /// <summary>
    /// �^�C���}�b�v�̎w����W�֎w�肳�ꂽ�^�C����ݒ肷��
    /// ���̎��A�Ή�����z��ɂ�Id��ݒ肷��
    /// </summary>
    /// <param name="_x">�^�C���}�b�v���x���W</param>
    /// <param name="_y">�^�C���}�b�v���y���W</param>
    /// <param name="_map">�^�C���}�b�v�ɑΉ�����z��</param>
    /// <param name="_tilemap">�^�C���}�b�v</param>
    /// <param name="_tileName">�z�u�������^�C���̖���</param>
    public void SetTile(int _x, int _y, int[,] _map, Tilemap _tilemap, string _tileName)
    {
        //�w�肵�����W�̃^�C����IsSingle���ǂ������`�F�b�N
        if (idToTileDictionary[_map[_x, _y]].IsSingle)
        {
            Debug.Log(string.Format("{0}��single�ł�:�������L�����Z��", idToTileNameDictionary[_map[_x, _y]]));
            return;
        }


        TileInformation tileInformation = tileNameToTileDictionary[_tileName];

        //�Z�b�g����^�C����single�ł���ꍇ�A�}�b�v���ɓ���̃^�C��������Βu������
        if (tileInformation.IsSingle)
        {
            for (int loop_x = 0; loop_x < _map.GetLength(0); loop_x++)
            {
                for (int loop_y = 0; loop_y < _map.GetLength(1); loop_y++)
                {
                    if (_map[loop_x, loop_y] == tileNameToIdDictionary[_tileName])
                    {
                        //�璷�ɂȂ邪�A�������[�v��h�����ߍċA�͂��������I�ɒu������
                        TileInformation defaultTileInformation = tileNameToTileDictionary[tileInformation.defaultTileName];

                        _map[loop_x, loop_y] = tileNameToIdDictionary[defaultTileInformation.tileName];

                        Vector3Int defaultPos = new Vector3Int(loop_x, loop_y, 0);
                        Quaternion defaultRot = Quaternion.Euler(defaultTileInformation.rot);
                        _tilemap.SetTile(defaultPos, defaultTileInformation.tileData);
                        _tilemap.SetTransformMatrix(defaultPos, Matrix4x4.TRS(Vector3.zero, defaultRot, Vector3.one));

                        Debug.Log(string.Format("{0}��single�ł�:�]�v�ȃ^�C����u��", _tileName));
                    }
                }
            }
        }

        //�w����W�Ɏw��^�C����Id��ݒ肵�A�^�C���}�b�v�ɑΉ�����^�C����ݒ�
        _map[_x, _y] = tileNameToIdDictionary[_tileName];

        Vector3Int pos = new Vector3Int(_x, _y, 0);
        Tile tile = tileInformation.tileData;
        Quaternion rot = Quaternion.Euler(tileInformation.rot);

        _tilemap.SetTile(pos, tile);
        _tilemap.SetTransformMatrix(pos, Matrix4x4.TRS(Vector3.zero, rot, Vector3.one));
    }


    /// <summary>
    /// �v���O�����}�b�v��őJ�ڐ���������̌�����Ԃ�
    /// ���ɑΉ�����^�C���̖��̂ɂ́A�K�����������������񂪊܂܂�Ă���̂ŁA
    /// �]�v�ȕ������폜����Ƒz�肵�Ă��������������
    /// </summary>
    /// <param name="_tileId">�������̂��擾�������^�C����Id</param>
    /// <returns>�������̂̕�����</returns>
    public string GetArrowName(int _tileId)
    {
        string tileName = idToTileNameDictionary[_tileId];

        return tileName.Replace("Red", "").Replace("Blue", "").Replace("Arrow", "");
    }



    /// <summary>
    /// �w�肵�����^�C���̌����ɑ΂��āA�E�ɉ�]�������^�C���̖��̂��擾
    /// </summary>
    /// <param name="_arrowId">���^�C����Id</param>
    /// <returns>�����œn�������^�C����1�E�ɉ�]�������^�C���̖���</returns>
    public string GetNextArrowTileName(int _arrowId)
    {
        //�^�C�����̂�����������������񂾂��������A�Ԗ�󂩐�󂩂̋敪������������ɂ���
        string tileName = idToTileNameDictionary[_arrowId];
        string arrowName = tileName.Replace(GetArrowName(_arrowId), "");

        //�敪�͂��̂܂܁A������ς������^�C���̖��̂�Ԃ�
        return arrowName + GetNextArrowName(_arrowId);
    }


    /// <summary>
    /// �w�肵�����^�C���̌����ɑ΂��āA�E���Ŏ��̌������擾
    /// </summary>
    /// <param name="_arrowId">���^�C����Id</param>
    /// <returns>�����œn�������^�C���̌�����1�E�ɉ�]����������</returns>
    public string GetNextArrowName(int _arrowId)
    {
        return GrobalMethod.GetRelativeDirection( (GrobalConst.DirectionId)Enum.Parse(typeof(GrobalConst.DirectionId), GetArrowName(_arrowId)), 1)
            .ToString();
    }




    
    /// <summary>
    /// �e��UI�{�^������Ăяo���A�^�C���̖��̂ɑΉ�����Id��ێ�����
    /// </summary>
    /// <param name="_tileName">�^�C���̖���</param>
    public void Select(string _tileName)
    {
        tileId = tileNameToIdDictionary[_tileName];
    }

    /// <summary>
    /// �e��UI�{�^������Ăяo���A����ȑ���ɑΉ�����ID��ݒ�
    /// </summary>
    /// <param name="_tileId">����Id</param>
    public void SelectUnique(int _tileId)
    {
        tileId = _tileId;
    }


    /// <summary>
    /// �󂯎�����I�u�W�F�N�g�̎q�ɂ�����{�^���S�Ăɔw�i�摜������
    /// </summary>
    /// <param name="_parentOfButtons">�w�i��ݒ肵�����{�^���̐e�I�u�W�F�N�g</param>
    public void SetBackground(GameObject _parentOfButtons)
    {
        foreach(Button button in _parentOfButtons.GetComponentsInChildren<Button>())
        {
            //���ɔw�i�摜�����Ă���ꍇ�͉������Ȃ�
            if (button.transform.Find(backgroundPrefab.name))
            {
                continue;
            }

            SetSprite(button.gameObject, backgroundPrefab);
        }
    }


    /// <summary>
    /// �I�𒆃{�^���n�C���C�g
    /// �{�^������Ăяo���A�N���b�N�����{�^�����̂��̂������Ƃ��Ď󂯎�邱�Ƃ�z�肷��B
    /// ���̎q�I�u�W�F�N�g�Ƃ��ăn�C���C�g�\��������B
    /// �܂��A���̃{�^���̃n�C���C�g�\��������
    /// </summary>
    /// <param name="_button">�n�C���C�g������{�^���i�������{�^�����̂��̂ɂ���j</param>
    public void Highlight(Button _button)
    {
        //�������Ƀn�C���C�g�����݂��Ă���ꍇ�A���������
        if (highlightObj != null)
        {
            Destroy(highlightObj);
        }

        highlightObj = SetSprite(_button.gameObject, highlightPrefab);
    }



    /// <summary>
    /// �w�肵���I�u�W�F�N�g�ɉ摜��t����
    /// ���񂳂ꂽ�{�^���ɘg��n�C���C�g������p�@��z�肷��
    /// </summary>
    /// <param name="_obj">�摜��t�������I�u�W�F�N�g</param>
    /// <param name="_spritePrefab">�t�������摜</param>
    /// <returns></returns>
    private GameObject SetSprite(GameObject _obj, GameObject _spritePrefab)
    {
        GameObject spriteObject = Instantiate(_spritePrefab, _obj.transform.position, Quaternion.identity, transform);
        spriteObject.transform.SetParent(_obj.transform);
        spriteObject.transform.position += _spritePrefab.transform.position;

        spriteObject.name = _spritePrefab.name;

        //�����̐e�I�u�W�F�N�g�ɂ��Ă���GridLayoutGroup���擾
        //�Z���T�C�Y�ɍ��킹�āA�{�^���̘g�Ƃ��Č�����悤�Ƀn�C���C�g�̃T�C�Y��ύX����i��̓Z���T�C�Y100�j
        if (_obj.transform.GetComponentInParent<GridLayoutGroup>() != null)
        {
            Vector2 size = _obj.GetComponentInParent<GridLayoutGroup>().cellSize;
            Vector3 scale = spriteObject.transform.localScale;

            scale.x = (size.x / 100) * scale.x;
            scale.y = (size.y / 100) * scale.y;

            spriteObject.transform.localScale = scale;
        }

        return spriteObject;
    }




    /// <summary>
    /// �^�C���Z���N�^�[�̏������i�v���C���[�h�ƃ}�b�v�G�f�B�b�g���[�h�̐ؑ֎��Ɏg�p�j
    /// �^�C��ID�𖳈Ӗ��Ȓl�ɂ��Ă����A�ێ����Ă���n�C���C�g������
    /// </summary>
    public void Reset()
    {
        //�����n�C���C�g�����݂��Ă���ꍇ�A���������
        if (highlightObj != null)
        {
            Destroy(highlightObj);
        }

        tileId = -99;
    }

}
