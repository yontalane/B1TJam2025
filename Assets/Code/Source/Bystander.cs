using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Bystander")]
    public class Bystander : MonoBehaviour
    {
        private const string VARATION_ANIMATOR_PROPERTY = "Variation";


        [SerializeField]
        private int m_variationCount;


        private void Reset()
        {
            m_variationCount = 3;
        }


        private void Start()
        {
            GetComponentInChildren<Animator>().SetInteger(VARATION_ANIMATOR_PROPERTY, Random.Range(1, m_variationCount + 1));
        }
    }
}
