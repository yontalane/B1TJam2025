using B1TJam2025.AudioSystems;
using UnityEngine;
using UnityEngine.UI;

namespace B1TJam2025.MainMenu
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Main Menu/Options Manager")]
    public sealed class OptionsManager : MonoBehaviour
    {
        [Header("Scene References")]

        [SerializeField]
        private LoopingMusicPlayer m_musicPlayer;

        [SerializeField]
        private Slider m_volumeSlider;


        private void Reset()
        {
            m_musicPlayer = FindAnyObjectByType<LoopingMusicPlayer>();
            m_volumeSlider = FindAnyObjectByType<Slider>();
        }


        private void Start()
        {
            m_volumeSlider.value = AudioManager.GlobalVolume;
        }


        public void OnSliderChange(float newValue)
        {
            AudioManager.GlobalVolume = newValue;
            m_musicPlayer.RefreshVolume();
        }
    }
}
