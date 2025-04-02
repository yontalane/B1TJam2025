using UnityEngine;

namespace B1TJam2025
{
    /// <summary>
    /// Sample Perp, Destroys when an object labeled "Player" collides with it, and triggers the cleared event on the _perpCleared manager
    /// </summary>
    public class SamplePerpWorld : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.name == "Player")
            {
                Destroy(gameObject);
                PerpSpawnLocationManager.SpawnNextPerp();
            }
        }
    }
}
