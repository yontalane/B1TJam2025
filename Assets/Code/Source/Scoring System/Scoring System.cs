using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

namespace B1TJam2025
{
    public class ScoringSystem : MonoBehaviour
    {
        #region Settings
        [Header("Settings")]

        //Perp Killing
        [Header("Perp KO")]
        [SerializeField]
        [Tooltip("Base Point Amount for Perp KO")]
        [Min(1)]
        private int _perpKillPoints = 100;

        [SerializeField]
        [Tooltip("Base Point Amount for Perp KO")]
        private float _perpKillMultiplierBoost = 1.0f;

        [SerializeField]
        [Tooltip("How long to disable the multiplier drain after killing a perp")]
        [Range(0f, 60f)]
        private float _perpKillDrainDisableDuration = 1.0f;

        //Perp Escape
        [Header("Perp Escape")]
        [SerializeField]
        [Tooltip("Reduction to the multiplier when a perp escapes")]
        [Range(0f, 100f)]
        private float _perpEscapeMultiplierReduction = 1.0f;

        //Multiplier Drain
        [Header("Multiplier Drain Over Time")]
        [SerializeField]
        [Tooltip("Increment by which the multiplier reduces by")]
        [Range(0, 1f)]
        private float _multDrainIncrement = 0.05f;

        [SerializeField]
        [Tooltip("Time delay for each drain increment in seconds")]
        [Range(0.05f, 10f)]
        private float _multDrainInverval = 0.1f;

        #endregion

        #region Other Variables

        //Main variables
        public int Score => _score;
        private int _score = 0;

        public float Multiplier => _multiplier;
        private float _multiplier = 1.0f;

        //instance vars
        private readonly Dictionary<Delegate, PerpEventType> _perpEventSubscriptions = new();
        private Coroutine _multDrainCoroutine = null;

        #endregion

        #region Change Events

        //Score
        private Action<int, QualityOfScoreChange> _onScoreChanged;
        public void AddListenerToScoreChange(Action<int, QualityOfScoreChange> func) => _onScoreChanged += func;
        public void RemoveListenerFromScoreChange(Action<int, QualityOfScoreChange> func) => _onScoreChanged -= func;

        //Multiplier
        private Action<float, QualityOfScoreChange> _onMultiplierChange;
        public void AddListenerToMultChange(Action<float, QualityOfScoreChange> func) => _onMultiplierChange += func;
        public void RemoveListenerFromMultChange(Action<float, QualityOfScoreChange> func) => _onMultiplierChange -= func;

        #endregion

        #region Add to Values
        /// <summary>
        /// Add points to the score, can opt out of point multiplier
        /// </summary>
        /// <param name="points"> amount of points to add (pre multiplier)</param>
        /// <param name="ignoreMultiplier"> ignore the multiplier? false by default </param>
        private void AddToScore(int points, bool ignoreMultiplier = false)
        {
            QualityOfScoreChange quality = QualityOfScoreChange.Neutral;
            switch (points)
            {
                case > 0: quality = QualityOfScoreChange.Positive; break;
                case < 0: quality = QualityOfScoreChange.Negative; break;
                case   0: quality = QualityOfScoreChange.Neutral;  break;
            }

            _score += (int)(ignoreMultiplier ? points : points * _multiplier);
            _onScoreChanged?.Invoke(_score, quality);        
        }

        /// <summary>
        /// Add to the multiplier?
        /// </summary>
        /// <param name="multiplierAddition"> how much to add to the multiplier (+1.0 = +100%) </param>
        private void AddToMultiplier(float multiplierAddition)
        {
            QualityOfScoreChange quality = QualityOfScoreChange.Neutral;
            switch (multiplierAddition)
            {
                case > 0: quality = QualityOfScoreChange.Positive; break;
                case < 0: quality = QualityOfScoreChange.Negative; break;
                case   0: quality = QualityOfScoreChange.Neutral;  break;
            }

            _multiplier = Mathf.Max(1, _multiplier + multiplierAddition);
            _onMultiplierChange?.Invoke(_multiplier, quality);
        }
        #endregion

        #region Unity Messages

        private void OnEnable()
        {
            //KO
            AddPerpEvent((perp) => AddToScore(_perpKillPoints), PerpEventType.KO);                                            // adds points to score
            AddPerpEvent((perp) => AddToMultiplier(_perpKillMultiplierBoost), PerpEventType.KO);                              // adds to multiplier
            AddPerpEvent((perp) => { DrainEnabled(false); }, PerpEventType.KO);                                               // disabled drain of multiplier
            AddPerpEvent((perp) => DelayRun(() => { DrainEnabled(true); }, _perpKillDrainDisableDuration), PerpEventType.KO); // enables drain of multiplier on delay
            //Escape
            AddPerpEvent((perp) => AddToMultiplier(-_perpEscapeMultiplierReduction), PerpEventType.Escape);                   // reduced multiplier when a perp escapes




            ///<summary> Add event to perp static events </summary>
            void AddPerpEvent(Action<Perp> func, PerpEventType @event)
            {
                Delegate del = new Perp.PerpEventHandler(func);

                switch (@event)
                {
                    case PerpEventType.KO:     Perp.OnPerpKO     += del as Perp.PerpEventHandler; break;
                    case PerpEventType.Spawn:  Perp.OnPerpSpawn  += del as Perp.PerpEventHandler; break;
                    case PerpEventType.Escape: Perp.OnPerpEscape += del as Perp.PerpEventHandler; break;
                }
                _perpEventSubscriptions.Add(del, @event);
            }
        }

        private void OnDisable()
        {
            foreach (KeyValuePair<Delegate, PerpEventType> kvp in _perpEventSubscriptions)
            {
                switch (kvp.Value)
                {
                    case PerpEventType.KO:     Perp.OnPerpKO     -= kvp.Key as Perp.PerpEventHandler; break;
                    case PerpEventType.Spawn:  Perp.OnPerpSpawn  -= kvp.Key as Perp.PerpEventHandler; break;
                    case PerpEventType.Escape: Perp.OnPerpEscape -= kvp.Key as Perp.PerpEventHandler; break;
                }
            }

            _perpEventSubscriptions.Clear();
        }

        #endregion

        #region Other

        /// <summary> Enable/Disable multiplier drain </summary>
        /// <param name="enabled"> true means the multiplier will drain over time based on settings, false means it won't </param>
        private void DrainEnabled(bool enabled)
        {
            if (enabled && _multDrainCoroutine == null) _multDrainCoroutine = StartCoroutine(DrainMult());
            else if (!enabled && _multDrainCoroutine != null)
            {
                StopCoroutine(_multDrainCoroutine);
                _multDrainCoroutine = null;
            }
        }

        /// <summary> Runs an action on a delay in seconds </summary>
        private void DelayRun(Action func, float delay)
        {
            StartCoroutine(Delay());
            IEnumerator Delay() { yield return new WaitForSeconds(delay); func?.Invoke(); }
        }

        /// <summary> Drains the Multiplier over time based on settings </summary>
        private IEnumerator DrainMult()
        {
            WaitForSeconds delay = new(_multDrainInverval);
            float currentDelay = _multDrainInverval;
            while (true)
            {
                if (currentDelay != _multDrainInverval) //updates time delay if interval changes
                {
                    delay = new(_multDrainInverval);
                    currentDelay = _multDrainInverval;
                }

                AddToMultiplier(-_multDrainIncrement);
                yield return delay;
            }

        }


        //used for making subscriptions to perp static events easier in OnEnable & OnDisable
        private enum PerpEventType { KO, Spawn, Escape }

        public enum QualityOfScoreChange { Neutral, Positive, Negative}

        #endregion
    }
}
