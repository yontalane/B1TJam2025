using B1TJam2025.AudioSystems;
using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioManager))]
    [RequireComponent(typeof(SFXPlayerSystem))]
    [AddComponentMenu("B1tJam2025/SFX Manager")]
    public sealed class SFXManager : MonoBehaviour
    {
        private static SFXManager s_instance;
        private AudioManager m_audioManager;
        private SFXPlayerSystem m_sfxPlayerSystem;


        private void Awake()
        {
            s_instance = this;
        }

        private void Start()
        {
            m_audioManager = GetComponent<AudioManager>();
            m_sfxPlayerSystem = GetComponent<SFXPlayerSystem>();
        }


        public static void Play(string sound, Vector3 position, float volume = 1f)
        {
            AudioClip clip = s_instance.m_sfxPlayerSystem.SetsByName[sound].GetRandomSelection();
            s_instance.m_audioManager.PlaySound(clip, position, volume > 0.667 ? AudioManager.Volume.Loud : volume > 0.333f ? AudioManager.Volume.MediumSoft : AudioManager.Volume.Quiet);
        }

        public static void Play(string sound, float volume = 1f)
        {
            AudioClip clip = s_instance.m_sfxPlayerSystem.SetsByName[sound].GetRandomSelection();
            s_instance.m_audioManager.PlaySound(clip, Camera.main.transform.position, volume > 0.667 ? AudioManager.Volume.Loud : volume > 0.333f ? AudioManager.Volume.MediumSoft : AudioManager.Volume.Quiet);
        }
    }
}
