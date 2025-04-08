using UnityEngine;


namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Scoring System/Contextual Score Manager")]
    public sealed class ContextualScoreManager : MonoBehaviour
    {
        private int m_previousScore;


        [Header("Scene References")]

        [SerializeField]
        private ScoringSystem m_scoringSystem;

        [SerializeField]
        private Canvas m_canvas;

        [Header("Prefabs")]

        [SerializeField]
        private ContextualScoreDisplay m_contextualScorePrefab;


        private void Reset()
        {
            m_scoringSystem = FindAnyObjectByType<ScoringSystem>();
            m_canvas = FindAnyObjectByType<Canvas>();

            m_contextualScorePrefab = null;
        }


        private void OnEnable()
        {
            m_scoringSystem.AddListenerToScoreChange(OnScoreChange);
        }

        private void OnDisable()
        {
            m_scoringSystem.RemoveListenerFromScoreChange(OnScoreChange);
        }


        private void Start()
        {
            m_previousScore = m_scoringSystem.Score;
        }


        private void OnScoreChange(int _, ScoringSystem.QualityOfScoreChange quality)
        {
            int delta = m_scoringSystem.Score - m_previousScore;
            m_previousScore = m_scoringSystem.Score;

            ContextualScoreDisplay display = Instantiate(m_contextualScorePrefab);
            display.Initialize(m_canvas, delta, quality);
        }
    }
}
