using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace B1TJam2025.MainMenu
{
    /// <summary>
    /// Manager for main menu, mostly functions to attatch to buttons
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        private static FadePanel __fade_out_panel;
        private static FadePanel __fade_in_panel;

        [Header("Settings")]
        [SerializeField]
        [Tooltip("The build's scene number for the main game scene")]
        private int _gameSceneNum;

        [Header("References")]
        [SerializeField]
        [Tooltip("Quit button. Hide this on web builds.")]
        private Button _quitButton;

        private void Start()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                _quitButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Sets what pannel to fade, is used in InitiateFadeTransition
        /// </summary>
        /// <param name="fadeOutPanel"> panel to fade out </param>
        public void SetFadeOutPanel(FadePanel fadeOutPanel) => __fade_out_panel = fadeOutPanel;

        /// <summary>
        /// Sets what pannel to fade, is used in InitiateFadeTransition
        /// </summary>
        /// <param name="fadeOutPanel"> panel to fade in </param>
        public void SetFadeInPanel(FadePanel fadeInPanel) => __fade_in_panel = fadeInPanel;

        /// <summary>
        /// Initiates a fade transition, set what to fade with SetFadeInPanel() and SetFadeOutPanel()
        /// </summary>
        /// <param name="fadeDuration"></param>
        public void InitiateFadeTransition(float fadeDuration) => StartCoroutine(FadeTransition(fadeDuration));

        private IEnumerator FadeTransition(float fadeDuration)
        {
            if (__fade_out_panel == null || __fade_in_panel == null)
            {
                Debug.LogWarning("Both the Fade in and Fade Panel must be set before a fade transition can be ran (SetFadeOutPanel, SetFadeInPanel)");
            }
            else
            {
                if (__fade_out_panel.TryGetComponent(out Image img)) { img.raycastTarget = true; } //disables clicking

                yield return __fade_out_panel.Fade(false, fadeDuration);
                __fade_out_panel.transform.parent.gameObject.SetActive(false); //disables panel through parent


                yield return new WaitForSeconds(0.2f); //middle delay
                if (img != null) { img.raycastTarget = false; } //resets clicking being enabled

                __fade_in_panel.transform.parent.gameObject.SetActive(true); //enables panel through parent
                yield return __fade_in_panel.Fade(true, fadeDuration);
            }
        }

        /// <summary>
        /// Starte the game, loading into the main game scene with a time delay
        /// </summary>
        /// <param name="delay"> time delay in seconds before game scene changes </param>
        public void StartGameDelay(float delay) => StartCoroutine(DelayGameStart(delay));

        /// <summary>
        /// Moves the scenery away from the camera to get rid of lines at sharp normals
        /// </summary>
        /// <param name="scenery"> parent ofbject of main menu 3D objects </param>
        public void FadeOutScenery(GameObject scenery) => StartCoroutine(YeetScenery(scenery));

        private IEnumerator YeetScenery(GameObject scenery)
        {
            yield return new WaitForSeconds(1.0f);
            Camera cam = Camera.main;
            Vector3 direc = cam.transform.forward;

            for (float t = 0; t < 0.45f; t += Time.deltaTime)
            {
                scenery.transform.position += (10 * Time.deltaTime * direc);
            }
            scenery.SetActive(false);
        }


        private IEnumerator DelayGameStart(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(_gameSceneNum);
        }

        /// <summary>
        /// Quits the software. If in editor, exits Play Mode.
        /// </summary>
        public void QuitSoftware()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
