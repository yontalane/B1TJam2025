using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Game Manager")]
    public sealed class GameManager : MonoBehaviour
    {
        private const string PERP_FLEE_TARGET_TAG = "PerpFleeTarget";


        private static GameManager s_instance;
        private readonly List<GameObject> m_fleeTargets = new();
        private readonly List<GameObject> m_tempFleeTargets = new();


        [Header("Prefabs")]

        [SerializeField]
        private Player m_playerPrefab;

        [SerializeField]
        private SubwayStop m_subwayStopPrefab;


        public static bool IsPaused { get; set; }

        public static Player Player { get; private set; }


        private void Reset()
        {
            m_playerPrefab = null;
            m_subwayStopPrefab = null;
        }


        private void OnEnable()
        {
            Perp.OnPerpEscape += OnPerpEscape;
            Perp.OnPerpKO += OnPerpKO;
        }

        private void OnDisable()
        {
            Perp.OnPerpEscape -= OnPerpEscape;
            Perp.OnPerpKO -= OnPerpKO;
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

            m_fleeTargets.AddRange(GameObject.FindGameObjectsWithTag(PERP_FLEE_TARGET_TAG));

            NavMeshSurface navMeshSurface = FindAnyObjectByType<NavMeshSurface>();
            navMeshSurface.BuildNavMesh();

            Perp[] perps = FindObjectsByType<Perp>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Perp perp in perps)
            {
                perp.EnableNavMeshAgent();
            }

            SpawnPerp();
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


        private void SpawnPerp()
        {
            PerpSpawnLocationManager.SpawnNextPerp();
            Debug.Log($"New perp spawned.");
        }

        private void OnPerpEscape()
        {
            Debug.Log($"Perp escaped.");
            SpawnPerp();
        }

        private void OnPerpKO()
        {
            Debug.Log($"Perp defeated.");
            SpawnPerp();
        }
    }
}
