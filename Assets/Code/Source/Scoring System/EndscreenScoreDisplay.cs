using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace B1TJam2025
{
    public class EndscreenScoreDisplay : MonoBehaviour
    {
        public static int EndGameScore;
        private const string HS_PREFS_KEY = "Highscore";

        [Header("Refrences")]

        [SerializeField]
        private TextMeshProUGUI _scoreText;
        [SerializeField]
        private TextMeshProUGUI _highscoreText;

        private void Start() => SetTextsAndHighScore(EndGameScore);

        private void SetTextsAndHighScore(int score)
        {
            if(_scoreText != null) _scoreText.text = "Score -  <br>" + score.ToString();

            if (!PlayerPrefs.HasKey(HS_PREFS_KEY)) PlayerPrefs.SetInt(HS_PREFS_KEY, score);
            else
            {
                int oldHS = PlayerPrefs.GetInt(HS_PREFS_KEY);
                if (oldHS < score) PlayerPrefs.SetInt(HS_PREFS_KEY, score); 

            }

            if(_highscoreText != null) _highscoreText.text = "Highscore -  <br>" + PlayerPrefs.GetInt(HS_PREFS_KEY);
        }

        private void DeleteHighScore()
        {
            if (!PlayerPrefs.HasKey(HS_PREFS_KEY)) PlayerPrefs.DeleteKey(HS_PREFS_KEY);
        }

        public void LoadScene(int sceneID) => SceneManager.LoadScene(sceneID);

#if UNITY_EDITOR
        [CustomEditor(typeof(EndscreenScoreDisplay))]
        public class EndscreenScoreDisplayEditor: Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                GUILayout.BeginVertical();
                {
                    GUILayout.Space(20);
                    if(GUILayout.Button("Delete Highscore Data"))
                    {
                        EndscreenScoreDisplay scoreScreen = (EndscreenScoreDisplay)target;
                        if(target != null) { scoreScreen.DeleteHighScore(); }
                    }
                }
                GUILayout.EndVertical();
            }
        }
#endif
    }
}
