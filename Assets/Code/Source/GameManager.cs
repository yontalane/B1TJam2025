using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Game Manager")]
    public sealed class GameManager : MonoBehaviour
    {
        public delegate void GameHandler();
        public static GameHandler OnGameStart = null;


        private const string PERP_FLEE_TARGET_TAG = "PerpFleeTarget";
        private const int ENDSCREEN_NUMBER = 2;
        private const float END_DURATION = 1f;
        private const string MAIN_MENU_SCENE = "Main Menu";

        private static GameManager s_instance;
        private readonly List<GameObject> m_fleeTargets = new();
        private readonly List<GameObject> m_tempFleeTargets = new();
        private readonly List<Conversation> m_conversationQueue = new();
        private bool m_inRandomSegment;
        private int m_sequenceIndex;
        private int m_perpsToBeat;
        private static bool s_isPaused = false;
        private static bool s_haveEverEnteredVehicle = false;
        private float m_gameStartTime;
        private bool m_isEnding;
        private float m_endStartTime;


        [Header("Script")]

        [SerializeField]
        private GameSequence m_sequence;

        [Header("Scene References")]

        [SerializeField]
        private GameObject m_loadingScreen;

        [SerializeField]
        private GameObject m_footInstructions;

        [SerializeField]
        private GameObject m_vehicleInstructions;

        [SerializeField]
        private Animator m_instructionsFTUX;

        [SerializeField]
        private RectTransform m_barnDoorL;

        [SerializeField]
        private RectTransform m_barnDoorR;

        [Header("Prefabs")]

        [SerializeField]
        private Player m_playerPrefab;

        [SerializeField]
        private SubwayStop m_subwayStopPrefab;


        public static bool IsPaused {
            get
            {
                return s_isPaused;
            }

            set
            {
                s_isPaused = value;
            }
        }

        public static Player Player { get; private set; }

        public static CityMap CityMap { get; private set; }

        public static int TotalBeats { get; private set; } = 0;

        public static int TotalEscapes { get; private set; } = 0;


        private void Reset()
        {
            m_sequence = null;

            m_loadingScreen = null;
            m_footInstructions = null;
            m_vehicleInstructions = null;
            m_instructionsFTUX = null;

            m_playerPrefab = null;
            m_subwayStopPrefab = null;
        }


        private void OnEnable()
        {
            Perp.OnPerpEscape += OnPerpEscape;
            Perp.OnPerpKO += OnPerpKO;
            PerpSpawnLocationManager.OnSpawnPerp += OnSpawnPerp;
            Vehicle.OnVehicleEntered += OnVehicleEntered;
            Vehicle.OnVehicleExited += OnVehicleExited;
            DialougeManager.OnDialougeComplete += OnDialogComplete;
            DialougeManager.OnConversationComplete += OnConversationComplete;
        }

        private void OnDisable()
        {
            Perp.OnPerpEscape -= OnPerpEscape;
            Perp.OnPerpKO -= OnPerpKO;
            PerpSpawnLocationManager.OnSpawnPerp -= OnSpawnPerp;
            Vehicle.OnVehicleEntered -= OnVehicleEntered;
            Vehicle.OnVehicleExited -= OnVehicleExited;
            DialougeManager.OnDialougeComplete -= OnDialogComplete;
            DialougeManager.OnConversationComplete -= OnConversationComplete;
        }


        private void Awake()
        {
            s_instance = this;
            m_loadingScreen.SetActive(true);
        }

        private void Start()
        {
            m_gameStartTime = Time.time;


            MapObject[] mapObjects = FindObjectsByType<MapObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (MapObject mapObject in mapObjects)
            {
                switch (mapObject.Type)
                {
                    case MapObjectType.PlayerStart:
                        Player = Instantiate(m_playerPrefab);
                        Player.transform.position = mapObject.Point;
                        break;

                    case MapObjectType.SubwayStop:
                        SubwayStop subwayStop = Instantiate(m_subwayStopPrefab);
                        subwayStop.Name = mapObject.Name;
                        subwayStop.transform.position = mapObject.Point;
                        break;
                }
            }

            Player = FindAnyObjectByType<Player>();
            CityMap = FindAnyObjectByType<CityMap>();

            m_fleeTargets.AddRange(GameObject.FindGameObjectsWithTag(PERP_FLEE_TARGET_TAG));

            NavMeshSurface navMeshSurface = FindAnyObjectByType<NavMeshSurface>();
            navMeshSurface.BuildNavMesh();

            OnGameStart?.Invoke();

            m_sequenceIndex = -1;
            AdvanceSequence();

            StartCoroutine(DelayedRemoveLoadingScreen());
        }

        private IEnumerator DelayedRemoveLoadingScreen()
        {
            yield return new WaitForEndOfFrame();
            m_loadingScreen.SetActive(false);
        }


        public static bool TryGetRandomFleeTarget(Transform perp, float minDistance, float maxDistance, out Transform target)
        {
            s_instance.m_tempFleeTargets.Clear();
            s_instance.m_tempFleeTargets.AddRange(s_instance.m_fleeTargets);

            while (s_instance.m_tempFleeTargets.Count > 0)
            {
                int index = Mathf.FloorToInt(Random.value * s_instance.m_tempFleeTargets.Count);

                float distance = Vector3.Distance(perp.position, s_instance.m_tempFleeTargets[index].transform.position);

                if (distance >= minDistance && distance <= maxDistance)
                {
                    target = s_instance.m_tempFleeTargets[index].transform;
                    return true;
                }

                s_instance.m_tempFleeTargets.RemoveAt(index);
            }

            target = null;
            return false;
        }


        private void SpawnRandomPerp()
        {
            PerpSpawnLocationManager.SpawnNextPerp();
            Debug.Log($"Random perp spawned.");
        }

        private void OnSpawnPerp(GameObject perpObj, Vector3 spawnLocation)
        {
            perpObj.name = $"Random Perp";

            if (perpObj.TryGetComponent(out NavMeshAgent navMeshAgent))
            {
                navMeshAgent.Warp(spawnLocation);
            }

            if (perpObj.TryGetComponent(out Perp perp))
            {
                perp.IsRandom = true;

                if (m_sequenceIndex < m_sequence.segments.Length)
                {
                    GameSequenceSegment segment = m_sequence.segments[m_sequenceIndex];
                    if (segment.randomVictorySoliloquys != null && segment.randomVictorySoliloquys.Length > 0)
                    {
                        perp.VictorySpeech = segment.randomVictorySoliloquys[Mathf.FloorToInt(segment.randomVictorySoliloquys.Length * Random.value)];
                    }
                }
            }
        }

        private void OnPerpEscape(Perp perp)
        {
            TotalEscapes++;
            Debug.Log($"Perp escaped.");

            if (!perp.IsRandom)
            {
                m_perpsToBeat--;
            }

            if (TotalEscapes >= m_sequence.escapeCountToLose)
            {
                StartEnding();
                return;
            }

            if (m_inRandomSegment && m_perpsToBeat > 0)
            {
                SpawnRandomPerp();
            }
        }

        private void OnPerpKO(Perp perp)
        {
            TotalBeats++;
            m_perpsToBeat--;
            Debug.Log($"Perp defeated.");

            if (m_inRandomSegment && m_perpsToBeat > 0)
            {
                SpawnRandomPerp();
            }

            if (m_perpsToBeat <= 0 && m_sequenceIndex >= m_sequence.segments.Length - 1)
            {
                return;
            }

            _ = TryPlayVictorySpeech(perp);
        }

        private bool TryPlayVictorySpeech(Perp perp)
        {
            if (perp == null || perp.VictorySpeech == null)
            {
                return false;
            }

            if (perp.GameSequenceSegmentIndex == -1)
            {
                m_conversationQueue.Add(perp.VictorySpeech);
                return true;
            }

            // HACK. These should be stored somewhere.
            Perp[] allPerps = FindObjectsByType<Perp>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach(Perp otherPerp in allPerps)
            {
                if (otherPerp == perp)
                {
                    continue;
                }

                if (otherPerp.GameSequenceSegmentIndex != perp.GameSequenceSegmentIndex)
                {
                    continue;
                }

                if (!otherPerp.IsDead)
                {
                    return false;
                }
            }

            m_conversationQueue.Add(perp.VictorySpeech);
            return true;
        }

        private void OnDialogComplete()
        {
            if (Player.DialogCameraIsActive)
            {
                Player.DialogCameraIsActive = false;
            }
        }

        private void OnConversationComplete(string eventCode)
        {
            if (eventCode != "Initial")
            {
                return;
            }

            m_instructionsFTUX.SetTrigger("Notify");
        }

        private void OnVehicleEntered(Vehicle _)
        {
            m_footInstructions.SetActive(false);
            m_vehicleInstructions.SetActive(true);

            if (!s_haveEverEnteredVehicle)
            {
                s_haveEverEnteredVehicle = true;
                m_instructionsFTUX.SetTrigger("Notify");
            }
        }

        private void OnVehicleExited(Vehicle _)
        {
            m_footInstructions.SetActive(true);
            m_vehicleInstructions.SetActive(false);
        }


        private void AdvanceSequence()
        {
            m_sequenceIndex++;

            Debug.Log($"Advanced to segment number {m_sequenceIndex + 1} of {m_sequence.segments.Length}.");

            if (m_sequenceIndex >= m_sequence.segments.Length)
            {
                return;
            }

            GameSequenceSegment segment = m_sequence.segments[m_sequenceIndex];

            if (segment.type == GameSequenceSegmentType.Scripted)
            {
                m_inRandomSegment = false;
                InitializeScriptedSegment(segment, m_sequenceIndex);
            }
            else
            {
                m_inRandomSegment = true;
                m_perpsToBeat += segment.randomCount;
                SpawnRandomPerp();
            }

            if (segment.beatBuddyAPB != null)
            {
                s_instance.m_conversationQueue.Add(segment.beatBuddyAPB);
            }
        }

        private void InitializeScriptedSegment(GameSequenceSegment segment, int index)
        {
            if (segment.spawnTargets.Length != segment.perps.Length)
            {
                Debug.LogError($"Game sequence has {segment.spawnTargets.Length} spawn targets and {segment.perps.Length} perps. These numbers need to be the same.");
                return;
            }
            else if (segment.spawnTargets.Length == 0)
            {
                Debug.LogError($"Game sequence has no perps assigned.");
                return;
            }

            for (int i = 0; i < segment.perps.Length; i++)
            {
                GameObject spawnTarget = GameObject.Find(segment.spawnTargets[i]);

                if (spawnTarget == null || segment.perps == null)
                {
                    Debug.LogError($"GameSequenceSegment is not set up properly.");
                    return;
                }

                GameObject instance = segment.perps[i].SpawnPerp();
                instance.name = $"Perp at {spawnTarget.name}";

                instance.transform.eulerAngles = spawnTarget.transform.eulerAngles;
                instance.transform.localScale = Vector3.one;

                if (instance.TryGetComponent(out NavMeshAgent navMeshAgent))
                {
                    navMeshAgent.Warp(spawnTarget.transform.position);
                }

                if (instance.TryGetComponent(out Perp perp))
                {
                    perp.VictorySpeech = segment.victorySoliloquy;
                    perp.IsRandom = false;
                    perp.GameSequenceSegmentIndex = index;
                }

                m_perpsToBeat++;

                Debug.Log($"Scripted perp spawned.");
            }

            //if (!segment.beatBeforeContinuing)
            //{
            //    AdvanceSequence();
            //}
        }


        private void LateUpdate()
        {
            if (m_isEnding)
            {
                UpdateEnding();
                return;
            }

            if (m_conversationQueue.Count > 0 && !Player.DialogCameraIsActive)
            {
                Player.DialogCameraIsActive = true;
                DialougeManager.InitiateConversation(m_conversationQueue[0]);
                m_conversationQueue.RemoveAt(0);
            }

            if (m_perpsToBeat <= 0)
            {
                if (m_sequenceIndex < m_sequence.segments.Length)
                {
                    AdvanceSequence();
                }
                else
                {
                    StartEnding();
                }
            }
        }

        private void StartEnding()
        {
            IsPaused = true;
            m_isEnding = true;
            m_endStartTime = Time.unscaledTime;
        }

        private void UpdateEnding()
        {
            m_barnDoorL.gameObject.SetActive(true);
            m_barnDoorR.gameObject.SetActive(true);

            float t = (Time.unscaledTime - m_endStartTime) / END_DURATION;

            if (t <= 1f)
            {
                UpdateBarnDoorsPosition(t);
                return;
            }

            if (TotalEscapes >= m_sequence.escapeCountToLose)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(MAIN_MENU_SCENE);
            }
            else
            {
                Time.timeScale = 1f;

                ScoringSystem system = (ScoringSystem)FindAnyObjectByType(typeof(ScoringSystem));
                if (system != null)
                {
                    EndscreenScoreDisplay.EndGameScore = system.Score;
                    EndscreenScoreDisplay.PerpsBeaten = TotalBeats;
                    EndscreenScoreDisplay.PerpsEscaped = TotalEscapes;
                    EndscreenScoreDisplay.GameTime = Time.time - m_gameStartTime;
                }
                else
                {
                    Debug.LogWarning("Couldn't find scoring system in scene");
                    EndscreenScoreDisplay.EndGameScore = 0;
                }

                SceneManager.LoadScene(ENDSCREEN_NUMBER);
            }
        }

        private void UpdateBarnDoorsPosition(float t)
        {
            Vector2 s = m_barnDoorL.sizeDelta;
            m_barnDoorL.anchorMin = new()
            {
                x = Mathf.Lerp(-0.5f, 0f, t),
                y = 0f,
            };
            m_barnDoorL.anchorMax = new()
            {
                x = Mathf.Lerp(0f, 0.5f, t),
                y = 1f,
            };
            m_barnDoorL.anchoredPosition = Vector2.zero;
            m_barnDoorL.sizeDelta = s;

            s = m_barnDoorR.sizeDelta;
            m_barnDoorR.anchorMin = new()
            {
                x = Mathf.Lerp(1f, 0.5f, t),
                y = 0f,
            };
            m_barnDoorR.anchorMax = new()
            {
                x = Mathf.Lerp(1.5f, 1f, t),
                y = 1f,
            };
            m_barnDoorR.anchoredPosition = Vector2.zero;
            m_barnDoorR.sizeDelta = s;
        }
    }
}
