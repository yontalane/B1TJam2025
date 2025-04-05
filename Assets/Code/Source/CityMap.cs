using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    [AddComponentMenu("B1TJam2025/City Map")]
    public class CityMap : MonoBehaviour
    {
        public Bounds Bounds { get; private set; }


        private void Awake()
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            Bounds = boxCollider.bounds;
        }
    }
}
