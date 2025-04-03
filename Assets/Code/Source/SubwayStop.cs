using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("B1TJam2025/Subway Stop")]
    public class SubwayStop : MonoBehaviour
    {
        [SerializeField]
        private string m_name;


        public string Name => m_name;
    }
}
