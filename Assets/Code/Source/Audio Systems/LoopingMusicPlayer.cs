using System.Collections;
using UnityEngine;

namespace B1TJam2025.AudioSystems
{
    /// <summary>
    /// Plays a song on loop, with the ability to play a one-off Intro
    /// </summary>
    public class LoopingMusicPlayer : MonoBehaviour
    {
        private float _originalVolume;


        [Header("Settings")]
        [Space(10)]

        [SerializeField]
        [Tooltip("Intro to song that dosen't loop. If left null will immediatly start loop")]
        private AudioClip _songIntro;

        [SerializeField]
        [Tooltip("main Looping segment of song")]
        private AudioClip _songMain;


        [Header("Refrences")]
        [SerializeField]
        [Tooltip("Source for Music in the scene")]
        private AudioSource _musicSource;

        private void Awake()
        {
            if (_musicSource != null)
            {
                _originalVolume = _musicSource.volume;
            }

            StartSong();
        }

        private void StartSong()
        {
            if(_musicSource != null)
            {
                if (_songIntro != null)
                {
                    SetToIntro();
                    _songMain.LoadAudioData();
                    StartCoroutine(SwapToLoop());
                }
                else
                {
                    SetToLoop();
                }
            }
        }

        private IEnumerator SwapToLoop()
        {
            while (_musicSource.isPlaying) yield return null;
            SetToLoop();
        }

        private void SetToIntro()
        {
            _musicSource.resource = _songIntro;
            _musicSource.time = 0;
            _musicSource.loop = false;
            _musicSource.volume = _originalVolume * AudioManager.GlobalVolume;
            _musicSource.Play();
        }

        private void SetToLoop()
        {
            _musicSource.resource = _songMain;
            _musicSource.time = 0;
            _musicSource.loop = true;
            _musicSource.volume = _originalVolume * AudioManager.GlobalVolume;
            _musicSource.Play();
        }

        public void RefreshVolume() => _musicSource.volume = _originalVolume * AudioManager.GlobalVolume;
    }
}
