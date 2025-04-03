using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Game Manager")]
    public sealed class GameManager : MonoBehaviour
    {
        public static bool IsPaused { get; set; }

        public static Player Player { get; private set; }

        private void Start()
        {
            Player = FindAnyObjectByType<Player>();
        }
    }
}
