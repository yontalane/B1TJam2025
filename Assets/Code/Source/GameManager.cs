using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Game Manager")]
    public sealed class GameManager : MonoBehaviour
    {
        public delegate void GameHandler();
        public static GameHandler OnGameStart = null;


        private const string PERP_FLEE_TARGET_TAG = "PerpFleeTarget";


        private static GameManager s_instance;
        private readonly List<GameObject> m_fleeTargets = new();
        private readonly List<GameObject> m_tempFleeTargets = new();
        private bool m_inRandomSegment;
        private int m_sequenceIndex;
        private int m_perpsToBeat;


        [Header("Script")]

        [SerializeField]
        private GameSequence m_sequence;

        [Header("Prefabs")]

        [SerializeField]
        private Player m_playerPrefab;

        [SerializeField]
        private SubwayStop m_subwayStopPrefab;


        public static bool IsPaused { get; set; }

        public static Player Player { get; private set; }

        public static CityMap CityMap { get; private set; }

        public static int TotalBeats { get; private set; } = 0;

        public static int TotalEscapes { get; private set; } = 0;


        private void Reset()
        {
            m_playerPrefab = null;
            m_subwayStopPrefab = null;
        }


        private void OnEnable()
        {
            Perp.OnPerpEscape += OnPerpEscape;
            Perp.OnPerpKO += OnPerpKO;
            PerpSpawnLocationManager.OnSpawnPerp += OnSpawnPerp;
        }

        private void OnDisable()
        {
            Perp.OnPerpEscape -= OnPerpEscape;
            Perp.OnPerpKO -= OnPerpKO;
            PerpSpawnLocationManager.OnSpawnPerp -= OnSpawnPerp;
        }


        private void Awake()
        {
            s_instance = this;
        }

        private void Start()
        {
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

        private void OnSpawnPerp(GameObject perpObj)
        {
            if (!perpObj.TryGetComponent(out Perp perp))
            {
                return;
            }

            perp.IsRandom = true;
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
                Debug.Log($"<b>YOU LOSE</b>");
                return;
            }

            if (m_inRandomSegment && m_perpsToBeat > 0)
            {
                SpawnRandomPerp();
            }
        }

        private void OnPerpKO(Perp _)
        {
            TotalBeats++;
            m_perpsToBeat--;
            Debug.Log($"Perp defeated.");

            if (m_inRandomSegment && m_perpsToBeat > 0)
            {
                SpawnRandomPerp();
            }
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
                InitializeScriptedSegment(segment);
            }
            else
            {
                m_inRandomSegment = true;
                m_perpsToBeat += segment.randomCount;
                SpawnRandomPerp();
            }
        }

        private void InitializeScriptedSegment(GameSequenceSegment segment)
        {
            GameObject spawnTarget = GameObject.Find(segment.spawnTarget);

            if (spawnTarget == null || segment.perp == null)
            {
                Debug.LogError($"GameSequenceSegment is not set up properly.");
                return;
            }

            GameObject instance = segment.perp.SpawnPerp();
            instance.transform.position = spawnTarget.transform.position;
            instance.transform.eulerAngles = spawnTarget.transform.eulerAngles;
            instance.transform.localScale = Vector3.one;

            if (instance.TryGetComponent(out Perp perp))
            {
                perp.IsRandom = false;
            }

            m_perpsToBeat++;

            Debug.Log($"Scripted perp spawned.");

            if (!segment.beatBeforeContinuing)
            {
                AdvanceSequence();
            }
        }


        private void LateUpdate()
        {
            if (m_perpsToBeat <= 0)
            {
                if (m_sequenceIndex < m_sequence.segments.Length)
                {
                    AdvanceSequence();
                }
                else
                {
                    Debug.Log($"<b>YOU WIN</b>");
                }
            }
        }
    }
}
