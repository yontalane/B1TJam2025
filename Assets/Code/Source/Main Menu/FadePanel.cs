using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace B1TJam2025.MainMenu
{
    /// <summary>
    /// Class used on the main menu to make a UI panel fade in and out
    /// </summary>
    public class FadePanel : MonoBehaviour
    {
        private const float ENABLED_FADE_IN_SPEED = 0.4f;

        [SerializeField]
        [Tooltip("The color of the fade cover pannel when fully revealed (likley clear)")]
        private Color _enabledColor = Color.clear;

        [SerializeField]
        [Tooltip("The color of the fade cover pannel when fully covered (likley black)")]
        private Color _disabledColor = Color.black;

        [Header("Refrences")]
        [Space(10)]

        [SerializeField]
        [Tooltip("")]
        private Image _fadePanelImage;

        private void Awake()
        {
            if (_fadePanelImage == null)
            {
                Debug.LogWarning("Forgot to set panel image for FadePanel.cs on " + gameObject.name + ", Please set in inspector. Destroying component to prevent errors");
                Destroy(this);
            }
        }

        /// <summary>
        /// Triggers the Fade Effect
        /// </summary>
        /// <param name="active"> fade in = true, fade out = false </param>
        /// <param name="fadeDuration"> how long should the fade take in seconds </param>
        public IEnumerator Fade(bool active, float fadeDuration)
        {
            Color toColor = active ? _enabledColor : _disabledColor;
            Color fromColor = active ? _disabledColor : _enabledColor;

            for(float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                _fadePanelImage.color = Color.Lerp(fromColor, toColor, Mathf.Clamp01(t / fadeDuration));
                yield return null;
            }
            _fadePanelImage.color = toColor;
           

        }
    }
}
