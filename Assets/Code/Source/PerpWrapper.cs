using UnityEngine;

namespace B1TJam2025
{
    [CreateAssetMenu(fileName = "PerpWrapper", menuName = "B1TJam2025/PerpWrapper")]
    public class PerpWrapper : PerpBase
    {
        [SerializeField]
        private PerpSettings m_perpSettings;


        public override GameObject SpawnPerp()
        {
            GameObject instance = Instantiate(_perpPrefab);

            if (instance.TryGetComponent(out Perp perp))
            {
                perp.Initialize(m_perpSettings);
            }

            return instance;
        }
    }
}
