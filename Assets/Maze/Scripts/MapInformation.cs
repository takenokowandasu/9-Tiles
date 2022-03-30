using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System.Text;
using UnityEditor;
using Grobal;

/// <summary>
/// �v���C���[�L�������ړ�����}�b�v�̏����Ǘ�����
/// �t�@�C��������̓ǂݍ��݋@�\������
/// </summary>
public class MapInformation : MonoBehaviour
{
	//�}�b�v��
	public string mapName;

	//�}�b�v�̑傫��(x,y)
	public int[] mapsize;
	//�e���W�̃^�C��Id
	public int[,] mapData;
	//�}�b�v�̎n�_�i�����̃}�X�j���A�O���b�h��łǂ̃}�X�ɂ��邩
	public int[] origin;

	//�}�b�v��`�悷��^�C���}�b�v
	public Tilemap mapTilemap;

	//�}�b�v�T�C�Y����o�͂���InputField
	[SerializeField]
	private InputField InputOfMapsizeX;
	[SerializeField]
	private InputField InputOfMapsizeY;

	[SerializeField]
	private TileSelector tileSelector;


	//CSV�`���œǂݎ�������e���i�[����ϐ�
	private List<string[]> DataList = new List<string[]>();

	private const string DEFAULTMAPNAME = "map";
	private const int MAPSIZEMIN = 3;
	private const int MAPSIZEMAX = 16;

	/// <summary>
	/// �f�t�H���g�}�b�v��ǂݍ���ł���
	/// </summary>
	private void Awake()
	{
		InitByFile(DEFAULTMAPNAME);
	}



	/// <summary>
	/// �w���CSV�t�@�C���������ǎ��ADataList�Ɋi�[
	/// </summary>
	/// <param name="_Filename">�ǂݍ��܂������t�@�C����</param>
	private void CsvReader(string _Filename)
	{
		// Assets/Resource�t�H���_���̑��΃p�X�Ŏw�肵���t�@�C����ǂݍ���
		TextAsset csvFile = Resources.Load(_Filename) as TextAsset;

		// �f�[�^���X�g�����������A�e�s���J���}��؂�œǂݍ���
		DataList = new List<string[]>();
		StringReader reader = new StringReader(csvFile.text);
		while (reader.Peek() != -1)
		{
			string line = reader.ReadLine();
			DataList.Add(line.Split(','));
		}
	}


	/// <summary>
	/// DataList�����ɁA�e��ϐ���^�C���}�b�v�𑀍삷��
	/// </summary>
	private void SetMapDatas()
	{
		//�擪�Ƀ}�b�v�T�C�Y������
		//�s���Ȓl�Ȃ�L�����Z�����f�t�H���g�}�b�v���Ăяo��
		if (int.TryParse(DataList[0][0], out int mapsizeX)
			&& int.TryParse(DataList[0][1], out int mapsizeY)
			&& mapsizeX >= MAPSIZEMIN && mapsizeX <= MAPSIZEMAX && mapsizeY >= MAPSIZEMIN && mapsizeY <= MAPSIZEMAX)
		{
			mapsize = new int[] { mapsizeX, mapsizeY };
		}
		else
		{
			InitByFile(DEFAULTMAPNAME);
			return;
		}

		//UI�ɔ��f
		InputOfMapsizeX.text = DataList[0][0];
		InputOfMapsizeY.text = DataList[0][1];


		//�^�C���}�b�v��̌��_���W�̓}�b�v�T�C�Y���狁�߂�
		//���W(0,0,0)���}�b�v�̒��S�ɂȂ�悤�ݒ�
		origin = new int[] { -mapsize[0] / 2, -mapsize[1] / 2 };
		mapTilemap.transform.position = new Vector3(origin[0], origin[1], 0);

		//�}�b�v���ƃ^�C���}�b�v��������
		mapData = new int[mapsize[0], mapsize[1]];
        mapTilemap.ClearAllTiles();

        //�X�^�[�g�n�_�̈ʒu�𒲂ׂ�
        int[] startPosition = null;

		//�j�s�ڈȍ~���}�b�v�̔z�u�Ȃ̂ŁA�����1�}�X1�v�f�Ŋi�[�E�^�C���}�b�v�ɔ��f
		for (int y = 0; y < mapsize[1]; y++)
		{
			for (int x = 0; x < mapsize[0]; x++)
			{
				//�����s���Ȓl���i�[����Ă���΃L�����Z�����f�t�H���g�}�b�v���Ăяo��
				if (int.TryParse(DataList[y + 1][x], out int result)
					&& (result == (int)GrobalConst.TileId.Floor
						|| result == (int)GrobalConst.TileId.Wall
						|| result == (int)GrobalConst.TileId.Start
						|| result == (int)GrobalConst.TileId.Goal))
				{
					//�}�b�v�̊O���Ɋւ��Ă͋����I�ɕǂ�z�u����
					if (x == 0 || mapsize[0] - 1 == x
						|| y == 0 || mapsize[1] - 1 == y)
					{
						result = (int)GrobalConst.TileId.Wall;
					}

					//�����X�^�[�g�n�_������Έʒu���L�^����
					if (result == (int)GrobalConst.TileId.Start)
					{
						startPosition = new int[2] { x, mapsize[1] - y - 1 };
					}

					//DataList��̍s���ƍ����悤�ɓY�������炷���Ƃɒ���
					//�܂��ADataList��ł͏ォ��s���𐔂��邪�A�Q�[�����ł͉����琔���Ă����Ƃ����_�ɒ��ӂ���
					tileSelector.SetTile(x, mapsize[1] - y - 1,
					mapData, mapTilemap,
					tileSelector.idToTileNameDictionary[result]);
				}
				else
				{
					InitByFile(DEFAULTMAPNAME);
					return;
				}

			}
		}

		//�����X�^�[�g�n�_���ǂ��ɂ�������΁A�����I�ɔz�u����
		if (startPosition == null)
		{
			tileSelector.SetTile(1, 1,
			mapData, mapTilemap,
			GrobalConst.TileId.Start.ToString());
		}
	}


	/// <summary>
	/// �w�肳�ꂽ�t�@�C������}�b�v�f�[�^��ǂݍ���
	/// </summary>
	/// <param name="_filename">�ǂݍ��܂������t�@�C����</param>
	public void InitByFile(string _filename)
	{
		CsvReader(_filename);

		mapName = _filename;

		SetMapDatas();
	}

	/// <summary>
	/// InitByFile(string)�̃I�[�o�[���[�h
	/// �{�^������Ăяo�����Ƃ�z��
	/// �J�����̃G�f�B�^��ł̃}�b�v�쐬�Ɏg�p����v���O�����ŁA���J���͎g�p���Ȃ�
	/// </summary>
	/// <param name="_filename">�ǂݍ��܂������t�@�C���������͂��ꂽInputField</param>
	public void InitByFile(InputField _filename)
    {
		InitByFile(_filename.text);
    }


	/// <summary>
	/// �w���InputField�ɏ������܂ꂽ����CSV�`���œǂݍ���
	/// �{�^������Ăяo�����Ƃ�z��
	/// </summary>
	/// <param name="_inputField">�ǂݍ��܂������f�[�^�����͂��ꂽInpufField</param>
	public void InitByInput(InputField _inputField)
    {
		// �f�[�^���X�g��������
		DataList = new List<string[]>();

		// inputField���̕��������s��؂�Ŕz��ɂ���
		string[] lines = _inputField.text.Split('\n');

		// �e�s�̕�������J���}��؂��DataList�ɕۑ�
		foreach(string line in lines)
        {
			DataList.Add(line.Split(','));
        }

		mapName = "InputData";

		SetMapDatas();
	}


	/// <summary>
	/// �ێ����Ă���f�[�^���f�t�H���g�ŗp�ӂ��Ă���X�e�[�W���ǂ����𔻒肷��B�p�ӂ������̂ł���΂��̒ʂ��ԍ����Ԃ�
	/// �X�e�[�W���̃t�@�C���͕K�� "STAGE" + (int)Id �Ƃ����`���ŏ����Ă��邽�߁A����𗘗p���Ĕ��肷��
	/// </summary>
	/// <returns>���茋�ʂƂ��̃}�b�vID</returns>
	public (bool IsMap, int mapId) GetMapId()
    {
        //�p�ӂ����X�e�[�W�ł���΁A"STAGE"��������Id�����̐����������c��A���̂܂ܐ��l�ɕϊ��ł���͂�
        //�ϊ��ł��Ȃ��ꍇ�ATryParse��out�ɂ́u0�v���i�[�����_�ɒ���
        bool IsMap = int.TryParse(mapName.Replace("STAGE", ""), out int mapId);

		return (IsMap, mapId);
	}




	/// <summary>
	/// InputField�ɓ��͂��ꂽ�}�b�v�T�C�Y��ǂݍ����mapData�ɔ��f����
	/// �}�b�v�T�C�Y�̊g�k�ɑΉ����邽�߁A���̃}�b�v���o����͈͂ŃR�s�[���Ă���O����ǂɐݒ肵����
	/// �{�^���ŌĂяo�����Ƃ�z��
	/// </summary>
	public void SetMapsize()
	{
		//�V�����}�b�v�f�[�^�𐶐�����Bnull���Ԃ��Ă����ꍇ�͉������Ȃ�
		(int[] new_mapsize, int[,] new_map) = CreateNewMap();
		if (new_mapsize == null)
		{
			return;
		}

		//�f�[�^��]�ʂ��ă^�C���}�b�v�ɔ��f������
		
		DataList = new List<string[]>();

		DataList.Add(new string[] { new_mapsize[0].ToString(), new_mapsize[1].ToString() });

        for (int y = new_mapsize[1] - 1; y >= 0; y--)
        {
			string line = "";

            for (int x = 0; x < new_mapsize[0]; x++)
            {
				line += new_map[x, y].ToString();

				if (x < new_mapsize[0] - 1)
				{
					line += ",";
				}
			}
			DataList.Add(line.Split(','));

		}

		SetMapDatas();
    }


	/// <summary>
	/// �w���InputField�ɁA����̃}�b�v�f�[�^��CSV�`���ŕ\��������
	/// �{�^���ŌĂяo�����Ƃ�z��
	/// </summary>
	/// <param name="_inputField">�\�����s��InputField</param>
	public void ViewCsvData(InputField _inputField)
	{
		//�ŏ��Ƀ}�b�v�T�C�Y������
		string data = mapsize[0] + "," + mapsize[1] + "\n";

		//Unity��ł�y�̐���������ł��邱�Ƃɒ��ӂ��Ĉ�s����������
		for (int y = mapsize[1] - 1; y >= 0; y--)
		{
			for (int x = 0; x < mapsize[0]; x++)
			{
				data += mapData[x, y].ToString();

				if (x < mapsize[0] - 1)
				{
					data += ",";
				}
			}
			data += "\n";
		}

		_inputField.text = data;
	}


	/// <summary>
	/// �w���InputField�̓��e���A�N���b�v�{�[�h�փR�s�[����
	/// �{�^���ŌĂяo�����Ƃ�z��
	/// </summary>
	/// <param name="_inputField">�R�s�[���s��InputField</param>
	public void CopyCsvDataToClipBoard(InputField _inputField)
	{
		GUIUtility.systemCopyBuffer = _inputField.text;
	}


	/// <summary>
	/// �w��t�@�C�����Ń}�b�v�f�[�^��ۑ�����
	/// �{�^���ŌĂяo�����Ƃ�z��B�t�@�C������filenameObject����擾����
	/// �J�����̃G�f�B�^��ł̃}�b�v�쐬�Ɏg�p����v���O�����ŁA���J���͎g�p���Ȃ�
	/// </summary>
	/// <param name="_inputField">�\�����s��InputField</param>
	public void SaveMap(InputField _inputField)
	{
		string filename;

		//����filenameObject����̏ꍇ�̓f�t�H���g�̖��O��ݒ肷��
		if (_inputField.text == null | _inputField.text == "")
		{
			filename = "map";
		}
		else
		{
			filename = _inputField.text;
		}

		//�}�b�v�T�C�Y�̓��̓f�[�^�𓯎��ɔ��f����B
		(int[] new_mapsize, int[,] new_map) = CreateNewMap();
		if (new_mapsize == null)
		{
			return;
		}


		//CSV�t�@�C���ɏ�������
		using (StreamWriter sw = new StreamWriter(@"Assets\Resources\" + filename + @".csv", false, Encoding.GetEncoding("Shift_JIS")))
		{
			sw.WriteLine(string.Join(",", new_mapsize));

			//Unity��ł�y�̐���������ł��邱�Ƃɒ��ӂ��Ĉ�s����������
			for (int y = new_mapsize[1] - 1; y >= 0; y--)
			{
				for (int x = 0; x < new_mapsize[0]; x++)
				{
					sw.Write(new_map[x, y].ToString());

					if (x < new_mapsize[0] - 1)
					{
						sw.Write(",");
					}
				}
				sw.WriteLine();
			}
		}


        //�Ō�ɍX�V�����f�[�^���G�f�B�^�֓o�^����
        //�G�f�B�^��ł����@�\���Ȃ��B�r���h���͂��������R�����g�A�E�g�����Ă����K�v������
        AssetDatabase.ImportAsset(@"Assets\Resources\" + filename + @".csv", ImportAssetOptions.Default);

        //�ۑ������f�[�^�̏󋵂𔽉f���A�\���ʒu�Ȃǂ�ύX�����Ă���
        InitByFile(filename);

	}

	/// <summary>
	/// UI����ǂݍ��񂾃}�b�v�T�C�Y�����ɐV�����}�b�v�f�[�^�̐��`���쐬����
	/// ���̎��A�Â��}�b�v�f�[�^���o����͈͂ŃR�s�[����
	/// </summary>
	/// <returns>�V�����}�b�v�T�C�Y�ƃ}�b�v�f�[�^</returns>
	private (int[] new_mapsize, int[,] new_map) CreateNewMap()
	{
		//�ǂݍ��񂾒l�𐔎��ɕϊ��ł��Ȃ��ꍇ�͉������Ȃ�
		if (int.TryParse(InputOfMapsizeX.text, out int mapsizeX)
			&& int.TryParse(InputOfMapsizeY.text, out int mapsizeY)
			&& mapsizeX >= MAPSIZEMIN && mapsizeX <= MAPSIZEMAX && mapsizeY >= MAPSIZEMIN && mapsizeY <= MAPSIZEMAX)
		{
			//���f�������}�b�v�T�C�Y�ŐV�����}�b�v�z������
			int[] new_mapsize = new int[] { mapsizeX, mapsizeY };
			int[,] new_map = new int[new_mapsize[0], new_mapsize[1]];

			//�ݒ蒆�̃f�[�^���o����͈͂ŃR�s�[�B�O���͋����I�ɕǂɂ���
			for (int x = 0; x < new_mapsize[0]; x++)
			{
				for (int y = 0; y < new_mapsize[1]; y++)
				{
					if (x < mapsize[0] && y < mapsize[1])
					{
						new_map[x, y] = mapData[x, y];
					}

					if (x == 0 | x == new_mapsize[0] - 1 | y == 0 | y == new_mapsize[1] - 1)
					{
						new_map[x, y] = 1;
					}
				}
			}

			return (new_mapsize, new_map);
		}
		else
		{
			Debug.Log("Mapsize data is not appropriate number");
			return (null, null);
		}
	}
}


