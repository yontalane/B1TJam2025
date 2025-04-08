using UnityEngine;
using UnityEngine.SceneManagement;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Pause Manager")]
    public sealed class PauseManager : MonoBehaviour
    {
        private const string MENU_SCENE_NAME = "Main Menu";


        public void ExitGame()
        {
            SceneManager.LoadScene(MENU_SCENE_NAME, LoadSceneMode.Single);
        }
    }
}
