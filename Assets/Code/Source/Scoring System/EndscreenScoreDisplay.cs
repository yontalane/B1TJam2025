using UnityEngine.UI;
using System.Collections;
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
        private const float FADE_IN_DURATION = 1f;
        private const float INTERVAL = 0.5f;
        private const float TALLY_DURATION = 3f;

        public static int EndGameScore;
        public static float GameTime;
        public static int PerpsBeaten;
        public static int PerpsEscaped;

        private const string HS_PREFS_KEY = "Highscore";
        private const int ESCAPE_REDUCTION = 50;

        [Header("Refrences")]

        [SerializeField]
        private TextMeshProUGUI _scoreText;
        [SerializeField]
        private TextMeshProUGUI _highscoreText;
        [SerializeField]
        private TextMeshProUGUI _totalBeaten;
        [SerializeField]
        private TextMeshProUGUI _totalEscapes;
        [SerializeField]
        private TextMeshProUGUI _totalGameTime;
        [SerializeField]
        private Image _fade;

        private void Start() => SetTextsAndHighScore(EndGameScore);

        private void SetTextsAndHighScore(int score)
        {
            if (_scoreText == null || _highscoreText == null || _totalBeaten == null || _totalEscapes == null || _totalGameTime == null)
            {
                Debug.LogError($"EndscreenScoreDisplay missing required scene references.");
                return;
            }

            _fade.color = Color.black;
            _fade.gameObject.SetActive(true);
            _scoreText.gameObject.SetActive(false);
            _highscoreText.gameObject.SetActive(false);
            _totalBeaten.gameObject.SetActive(false);
            _totalEscapes.gameObject.SetActive(false);
            _totalGameTime.gameObject.SetActive(false);

            _scoreText.text = "Score -  <br>" + score.ToString();

            if (!PlayerPrefs.HasKey(HS_PREFS_KEY))
            {
                PlayerPrefs.SetInt(HS_PREFS_KEY, score - PerpsEscaped * ESCAPE_REDUCTION);
            }
            else
            {
                int oldHS = PlayerPrefs.GetInt(HS_PREFS_KEY);
                if (oldHS < score) PlayerPrefs.SetInt(HS_PREFS_KEY, score); 

            }

            _highscoreText.text = "Highscore -  <br>" + PlayerPrefs.GetInt(HS_PREFS_KEY);

            StartCoroutine(Animation(score));
        }

        private IEnumerator Animation(int score)
        {
            float startTime = Time.time;
            float t = 0f;
            while (t < 1f)
            {
                _fade.color = new()
                {
                    r = 0f,
                    g = 0f,
                    b = 0f,
                    a = 1f - t,
                };
                yield return new WaitForEndOfFrame();
                t = (Time.time - startTime) / FADE_IN_DURATION;
            }
            _fade.gameObject.SetActive(false);

            yield return new WaitForSeconds(INTERVAL);

            _totalBeaten.text = $"Perps beat: {PerpsBeaten}";
            _totalBeaten.gameObject.SetActive(true);

            yield return new WaitForSeconds(INTERVAL);

            _totalEscapes.text = $"Perps fled: {PerpsEscaped}";
            _totalEscapes.gameObject.SetActive(true);

            yield return new WaitForSeconds(INTERVAL);

            _totalGameTime.text = $"Time: {GameTime:n1}";
            _totalGameTime.gameObject.SetActive(true);

            yield return new WaitForSeconds(INTERVAL);

            int currentTally = 0;
            _scoreText.text = $"Score: {currentTally}";
            _scoreText.gameObject.SetActive(true);
            startTime = Time.time;
            t = 0f;

            while (t < 1f)
            {
                currentTally = Mathf.FloorToInt((float)score * t);
                _scoreText.text = $"Score: {currentTally}";
                yield return new WaitForEndOfFrame();
                t = (Time.time - startTime) / TALLY_DURATION;
            }

            _scoreText.text = $"Score: {score}";

            for (int i = 0; i < PerpsEscaped; i++)
            {
                yield return new WaitForSeconds(INTERVAL);
                score -= ESCAPE_REDUCTION;
                _scoreText.text = $"Score: {score}";
            }

            yield return new WaitForSeconds(INTERVAL);

            _highscoreText.gameObject.SetActive(true);
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
