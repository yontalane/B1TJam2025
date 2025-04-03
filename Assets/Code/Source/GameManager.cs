using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Game Manager")]
    public sealed class GameManager : MonoBehaviour
    {
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
        }
    }
}
