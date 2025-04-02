using UnityEngine;

namespace B1TJam2025
{
    /// <summary>
    /// Base class for scritable objects that act as perps. Can be inherited from to create diffrent categories of perp data containers. 
    /// For use with the PerpSpawnLocationManager
    /// </summary>
    public class PerpBase : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Prefab of the perp to spawn in the world. Perp prefab needs to notify PerpSpawnLocationManager when they dissapear")]
        protected GameObject _perpPrefab;

        public virtual GameObject SpawnPerp()
        {
            return Instantiate(_perpPrefab);
        }

    }
}
