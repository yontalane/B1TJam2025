using UnityEngine;

namespace B1TJam2025.Utility
{
    [AddComponentMenu("B1TJam2025/Utility/Disable Component")]
    public class DisableComponent : MonoBehaviour
    {
        [SerializeField]
        private Component m_component;

        private void Awake()
        {
            if (m_component is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.enabled = false;
            }
            else if (m_component is Renderer renderer)
            {
                renderer.enabled = false;
            }
        }
    }
}
