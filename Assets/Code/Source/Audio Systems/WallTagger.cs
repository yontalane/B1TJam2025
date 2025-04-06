using System.Collections.Generic;
using UnityEngine;

namespace B1TJam2025.AudioSystems
{
    //tags objects as walls (layer) on start for sfx
    public class WallTagger : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> _objsToTagAsWall = new();

        void Start()
        {
            int layer = LayerMask.NameToLayer("Wall");

            for(int i = 0; i < _objsToTagAsWall.Count; i++)
            {
                TagHeirarchyWithLayer(_objsToTagAsWall[i], layer);
            }

            void TagHeirarchyWithLayer(GameObject obj, int layer)
            {
                if(obj != null) obj.layer = layer;
                for(int i = 0; i < obj.transform.childCount; i++)
                {
                    TagHeirarchyWithLayer(obj.transform.GetChild(i).gameObject, layer);
                }
            }
        }

    }
}
