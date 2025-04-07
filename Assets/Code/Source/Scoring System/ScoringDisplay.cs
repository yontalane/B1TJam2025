using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using System.Text;

namespace B1TJam2025
{
    public class ScoringDisplay : MonoBehaviour
    {
        [Header("Refrences")]
        [SerializeField]
        [Tooltip("Scoring System")]
        private ScoringSystem _scoringSystem;

        [SerializeField]
        [Tooltip("Text element that displays score")]
        private TextMeshProUGUI _scoreText;

        [SerializeField]
        [Tooltip("Text element that displays multiplier")]
        private TextMeshProUGUI _multiplierText;

        private void Awake()
        {
            if(_scoringSystem == null) _scoringSystem = FindAnyObjectByType<ScoringSystem>(); //gets scoring system if not assigned
        }

        private void Start()
        {
            if(_scoringSystem != null)
            {
                if (_scoreText != null) _scoreText.text = "Score: " + _scoringSystem.Score.ToString();
                if (_multiplierText != null) _multiplierText.text = "X " + FloatToString(_scoringSystem.Multiplier, 2);
            }
        }

        private void OnEnable()
        {
            if (_scoringSystem)
            {
                _scoringSystem.AddListenerToScoreChange(UpdateScoreDisplay);
                _scoringSystem.AddListenerToMultChange(UpdateMultDisplay);
            }
        }
        private void OnDisable()
        {
            if (_scoringSystem)
            {
                _scoringSystem.RemoveListenerFromScoreChange(UpdateScoreDisplay);
                _scoringSystem.RemoveListenerFromMultChange(UpdateMultDisplay);
            }
        }


        private void UpdateScoreDisplay(int score, ScoringSystem.QualityOfScoreChange quality)
        {
            if(_scoreText != null) _scoreText.text = "Score: " + score.ToString();
        }

        private void UpdateMultDisplay(float mult, ScoringSystem.QualityOfScoreChange quality)
        {
            if(_multiplierText != null) _multiplierText.text = "X " + FloatToString(mult, 2);
        }

        private string FloatToString(float @float, int places)
        {
            places = Mathf.Max(places,1);
            int wholePlaces = (Mathf.FloorToInt(@float).ToString()).Length;
            string adjusted = Mathf.Round((@float * Mathf.Pow(10, places))).ToString();

            int targetLength = wholePlaces + 1 + places;

            StringBuilder output = new("");
            for(int i = 0; i < adjusted.Length; i++)
            {
                if (i == wholePlaces) { output.Append("."); }
                output.Append(adjusted[i]);

                if (i >= targetLength - 1) break;
            }
            int diff = targetLength - output.Length;
            for (int i = 0; i < diff; i++) output.Append("0");

            return output.ToString();
        }
    }
}
