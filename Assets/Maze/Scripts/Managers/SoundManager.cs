using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BGMやSEを鳴らす機能と、音声データ管理機能を持つクラス
/// </summary>
public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// ゲーム内で使用する音声データと、その名前の情報を持つクラス
    /// インスペクター上で配列に登録する使い方を前提とする
    /// </summary>
    [Serializable]
    private class SoundData
    {
        public string name;
        public AudioClip audioClip;
    }

    [SerializeField]
    private SoundData[] soundDatas;

    //AudioSourceの管理用配列
    private AudioSource[] audioSourceList = new AudioSource[10];

    //SoundData管理用Dictionary。名称をキーとしてSoundDataを取得する
    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();

    //音量調整用のスライダーオブジェクト
    //インスペクター上で登録する
    [SerializeField]
    private Slider soundSlider;
    [SerializeField]
    private Slider BGMSlider;

    //BGMを鳴らす専用のAudioSource
    //ゲーム開始時に再生、ループ再生をするよう設定したオブジェクトを用意しておく
    [SerializeField]
    private AudioSource BGMaudioSource;

    //音量設定をセーブするためのキー基準値
    private const int saveKey = -50;


    /// <summary>
    /// システム音を鳴らすイベントを（アクティブなシーンに存在する）全てのボタンに設定する
    /// </summary>
    private void SetSystemSE()
    {
        //ヒエラルキー上のルートオブジェクトを全て取得し、その子に含まれるボタンコンポーネントをそれぞれ取得する
        //FindWithTagを使わないのは、こちらなら非アクティブでも取得できるため
        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Button button in obj.GetComponentsInChildren<Button>(true))
            {
                button.GetComponent<Button>().onClick.AddListener(() => this.Play("System_Button"));
            }
        }
    }

    /// <summary>
    /// AudioSourceを自分自身に必要な数だけ生成し、それぞれ配列に格納
    /// 音量調整スライダーを同期させる
    /// また、サウンドデータをディクショナリに登録
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
    /// 未使用のAudioSourceを取得
    /// </summary>
    /// <returns>未使用のAudioSource。全て使用中ならnull</returns>
    private AudioSource GetUnusedAudioSource()
    {
        for (int i = 0; i < audioSourceList.Length; ++i)
        {
            if (audioSourceList[i].isPlaying == false) return audioSourceList[i];
        }

        return null;
    }



    /// <summary>
    /// 指定されたAudioClipを未使用のAudioSourceで再生
    /// </summary>
    /// <param name="clip">再生したいAudioClip</param>
    public void Play(AudioClip clip)
    {
        AudioSource audioSource = GetUnusedAudioSource();

        if (audioSource == null)
        {
            return;
        }

        //再生前にスライダーの値を音量に反映する
        audioSource.volume = soundSlider.value;

        audioSource.clip = clip;
        audioSource.Play();
    }


    /// <summary>
    /// 指定された名称に対応したAudioClipを再生する
    /// </summary>
    /// <param name="name">再生したいAudioClipのSoundData上での名称</param>
    public void Play(string name)
    {
        if (soundDictionary.TryGetValue(name, out SoundData soundData))
        {
            Play(soundData.audioClip);
        }
        else
        {
            Debug.LogWarning($"その名称は登録されていません:{name}");
        }
    }


    /// <summary>
    /// 再生中のBGMの音量を変更する
    /// BGM用の音量調整スライダーの値変更時に呼び出すことを想定
    /// </summary>
    public void AdjustBGMVolume()
    {
        BGMaudioSource.volume = BGMSlider.value * 0.5f;
    }

    /// <summary>
    /// 現在の音量設定をセーブする
    /// 音量調整スライダーの値変更時に呼び出すことを想定
    /// 再利用しやすくするため、SaveLoadManagerとは分離しておく
    /// </summary>
    public void SaveVolume()
    {
        Debug.LogFormat("BGM:{0} Sound:{1}", BGMSlider.value, soundSlider.value);
        PlayerPrefs.SetFloat(saveKey.ToString(), BGMSlider.value);
        PlayerPrefs.SetFloat((saveKey + 1).ToString(), soundSlider.value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// セーブされている音量設定をロードする
    /// ゲーム開始時に呼び出す
    /// 再利用しやすくするため、SaveLoadManagerとは分離しておく
    /// </summary>
    public void LoadVolume()
    {
        //.valueを直接変更すると、値変更時のSaveVolume()が先に呼び出されて初期値がセーブを上書きしかねない
        BGMSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(saveKey.ToString(), 0.6f));
        soundSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat((saveKey + 1).ToString(), 0.6f));
        AdjustBGMVolume();

        Debug.LogFormat("BGM:{0} Sound:{1}", BGMSlider.value, soundSlider.value);
    }
}