using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BGM��SE��炷�@�\�ƁA�����f�[�^�Ǘ��@�\�����N���X
/// </summary>
public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// �Q�[�����Ŏg�p���鉹���f�[�^�ƁA���̖��O�̏������N���X
    /// �C���X�y�N�^�[��Ŕz��ɓo�^����g������O��Ƃ���
    /// </summary>
    [Serializable]
    private class SoundData
    {
        public string name;
        public AudioClip audioClip;
    }

    [SerializeField]
    private SoundData[] soundDatas;

    //AudioSource�̊Ǘ��p�z��
    private AudioSource[] audioSourceList = new AudioSource[10];

    //SoundData�Ǘ��pDictionary�B���̂��L�[�Ƃ���SoundData���擾����
    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();

    //���ʒ����p�̃X���C�_�[�I�u�W�F�N�g
    //�C���X�y�N�^�[��œo�^����
    [SerializeField]
    private Slider soundSlider;
    [SerializeField]
    private Slider BGMSlider;

    //BGM��炷��p��AudioSource
    //�Q�[���J�n���ɍĐ��A���[�v�Đ�������悤�ݒ肵���I�u�W�F�N�g��p�ӂ��Ă���
    [SerializeField]
    private AudioSource BGMaudioSource;

    //���ʐݒ���Z�[�u���邽�߂̃L�[��l
    private const int saveKey = -50;


    /// <summary>
    /// �V�X�e������炷�C�x���g���i�A�N�e�B�u�ȃV�[���ɑ��݂���j�S�Ẵ{�^���ɐݒ肷��
    /// </summary>
    private void SetSystemSE()
    {
        //�q�G�����L�[��̃��[�g�I�u�W�F�N�g��S�Ď擾���A���̎q�Ɋ܂܂��{�^���R���|�[�l���g�����ꂼ��擾����
        //FindWithTag���g��Ȃ��̂́A������Ȃ��A�N�e�B�u�ł��擾�ł��邽��
        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Button button in obj.GetComponentsInChildren<Button>(true))
            {
                button.GetComponent<Button>().onClick.AddListener(() => this.Play("System_Button"));
            }
        }
    }

    /// <summary>
    /// AudioSource���������g�ɕK�v�Ȑ������������A���ꂼ��z��Ɋi�[
    /// ���ʒ����X���C�_�[�𓯊�������
    /// �܂��A�T�E���h�f�[�^���f�B�N�V���i���ɓo�^
    /// </summary>
    private void Awake()
    {
        for (int i = 0; i < audioSourceList.Length; ++i)
        {
            audioSourceList[i] = gameObject.AddComponent<AudioSource>();
        }

        foreach (SoundData soundData in soundDatas)
        {
            soundDictionary.Add(soundData.name, soundData);
        }
        SetSystemSE();
        LoadVolume();
    }

    /// <summary>
    /// ���g�p��AudioSource���擾
    /// </summary>
    /// <returns>���g�p��AudioSource�B�S�Ďg�p���Ȃ�null</returns>
    private AudioSource GetUnusedAudioSource()
    {
        for (int i = 0; i < audioSourceList.Length; ++i)
        {
            if (audioSourceList[i].isPlaying == false) return audioSourceList[i];
        }

        return null;
    }



    /// <summary>
    /// �w�肳�ꂽAudioClip�𖢎g�p��AudioSource�ōĐ�
    /// </summary>
    /// <param name="clip">�Đ�������AudioClip</param>
    public void Play(AudioClip clip)
    {
        AudioSource audioSource = GetUnusedAudioSource();

        if (audioSource == null)
        {
            return;
        }

        //�Đ��O�ɃX���C�_�[�̒l�����ʂɔ��f����
        audioSource.volume = soundSlider.value;

        audioSource.clip = clip;
        audioSource.Play();
    }


    /// <summary>
    /// �w�肳�ꂽ���̂ɑΉ�����AudioClip���Đ�����
    /// </summary>
    /// <param name="name">�Đ�������AudioClip��SoundData��ł̖���</param>
    public void Play(string name)
    {
        if (soundDictionary.TryGetValue(name, out SoundData soundData))
        {
            Play(soundData.audioClip);
        }
        else
        {
            Debug.LogWarning($"���̖��͓̂o�^����Ă��܂���:{name}");
        }
    }


    /// <summary>
    /// �Đ�����BGM�̉��ʂ�ύX����
    /// BGM�p�̉��ʒ����X���C�_�[�̒l�ύX���ɌĂяo�����Ƃ�z��
    /// </summary>
    public void AdjustBGMVolume()
    {
        BGMaudioSource.volume = BGMSlider.value * 0.5f;
    }

    /// <summary>
    /// ���݂̉��ʐݒ���Z�[�u����
    /// ���ʒ����X���C�_�[�̒l�ύX���ɌĂяo�����Ƃ�z��
    /// �ė��p���₷�����邽�߁ASaveLoadManager�Ƃ͕������Ă���
    /// </summary>
    public void SaveVolume()
    {
        Debug.LogFormat("BGM:{0} Sound:{1}", BGMSlider.value, soundSlider.value);
        PlayerPrefs.SetFloat(saveKey.ToString(), BGMSlider.value);
        PlayerPrefs.SetFloat((saveKey + 1).ToString(), soundSlider.value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// �Z�[�u����Ă��鉹�ʐݒ�����[�h����
    /// �Q�[���J�n���ɌĂяo��
    /// �ė��p���₷�����邽�߁ASaveLoadManager�Ƃ͕������Ă���
    /// </summary>
    public void LoadVolume()
    {
        //.value�𒼐ڕύX����ƁA�l�ύX����SaveVolume()����ɌĂяo����ď����l���Z�[�u���㏑�������˂Ȃ�
        BGMSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(saveKey.ToString(), 0.6f));
        soundSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat((saveKey + 1).ToString(), 0.6f));
        AdjustBGMVolume();

        Debug.LogFormat("BGM:{0} Sound:{1}", BGMSlider.value, soundSlider.value);
    }
}