using System.Collections.Generic;
using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Mini-Map Manager")]
    public sealed class MiniMapManager : MonoBehaviour
    {
        private struct MarkerAndTarget
        {
            public RectTransform marker;
            public Transform target;
        }


        private bool m_initialized;
        private MarkerAndTarget m_playerData;
        private readonly List<MarkerAndTarget> m_perpData = new();
        private Vector2Int m_mapSize;


        [SerializeField]
        private RectTransform m_map;

        [SerializeField]
        private RectTransform m_header;

        [Space]

        [SerializeField]
        private RectTransform m_playerMarker;

        [SerializeField]
        private RectTransform m_perpMarker;


        private void Reset()
        {
            m_map = null;
            m_header = null;

            m_playerMarker = null;
            m_perpMarker = null;
        }


        private void OnEnable()
        {
            GameManager.OnGameStart += OnGameStart;
            Perp.OnPerpSpawn += OnPerpSpawn;
        }

        private void OnDisable()
        {
            GameManager.OnGameStart -= OnGameStart;
            Perp.OnPerpSpawn -= OnPerpSpawn;
        }


        private void OnGameStart()
        {
            m_mapSize = new()
            {
                x = Mathf.FloorToInt(GameManager.CityMap.Bounds.size.x),
                y = Mathf.FloorToInt(GameManager.CityMap.Bounds.size.z),
            };

            m_map.sizeDelta = new()
            {
                x = m_mapSize.x,
                y = m_mapSize.y,
            };

            m_header.sizeDelta = new()
            {
                x = m_mapSize.x,
                y = m_header.sizeDelta.y,
            };

            m_playerData = GenerateMarker(GameManager.Player.transform, m_playerMarker);

            m_initialized = true;
        }

        private void OnPerpSpawn(Perp perp)
        {
            m_perpData.Add(GenerateMarker(perp.transform, m_perpMarker));
        }


        private MarkerAndTarget GenerateMarker(Transform target, RectTransform markerPrefab)
        {
            MarkerAndTarget markerAndTarget = new()
            {
                target = target,
                marker = Instantiate(markerPrefab),
            };

            markerAndTarget.marker.SetParent(m_map);
            markerAndTarget.marker.localPosition = Vector3.zero;
            markerAndTarget.marker.localEulerAngles = Vector3.zero;
            markerAndTarget.marker.localScale = Vector3.one;

            PlaceMarker(markerAndTarget);

            return markerAndTarget;
        }


        private void LateUpdate()
        {
            if (!m_initialized)
            {
                return;
            }

            PlaceMarker(m_playerData);

            for (int i = m_perpData.Count - 1; i >= 0; i--)
            {
                if (m_perpData[i].target == null)
                {
                    Destroy(m_perpData[i].marker.gameObject);
                    m_perpData.RemoveAt(i);
                    continue;
                }

                PlaceMarker(m_perpData[i]);
            }
        }

        private void PlaceMarker(MarkerAndTarget markerAndTarget)
        {
            Bounds b = GameManager.CityMap.Bounds;
            Transform t = markerAndTarget.target;
            RectTransform m = markerAndTarget.marker;

            float x = (t.position.x - b.min.x) / b.size.x;
            float y = (t.position.z - b.min.z) / b.size.z;

            Vector2 size = m.sizeDelta;
            m.anchorMin = Vector2.zero;
            m.anchorMax = Vector2.zero;
            m.offsetMin = new()
            {
                x = Mathf.FloorToInt(m_mapSize.x * x),
                y = Mathf.FloorToInt(m_mapSize.y * y),
            };
            m.sizeDelta = size;
        }
    }
}
