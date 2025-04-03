using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace B1TJam2025
{
    public enum MapObjectType
    {
        None = 0,
        PlayerStart = 10,
        SubwayStop = 20,
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Map Object")]
    public class MapObject : MonoBehaviour
    {
        [SerializeField]
        private string m_name;

        [SerializeField]
        private MapObjectType m_type;


        public string Name => m_name;

        public MapObjectType Type => m_type;

        public Vector3 Point => new()
        {
            x = transform.position.x + 1f,
            y = 0f,
            z = transform.position.z + 1f,
        };


        private void Reset()
        {
            m_name = name;
            m_type = MapObjectType.None;
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            MapObject mapObject = this;

            GUIStyle style1 = new(EditorStyles.centeredGreyMiniLabel);
            style1.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Code/Editor/Gizmos/MapMarker.png");
            style1.fixedWidth = 50f;
            style1.fixedHeight = 50f;
            GUI.color = Color.black;
            Vector3 position1 = mapObject.Point;
            Handles.Label(position1, string.Empty, style1);

            GUIStyle style = new(EditorStyles.centeredGreyMiniLabel);
            style.normal.textColor = Color.black;
            style.richText = true;
            style.fontSize = 20;
            GUI.color = Color.black;
            Vector3 position = mapObject.Point + 0.5f * Vector3.back;

            switch (mapObject.Type)
            {
                case MapObjectType.PlayerStart:
                    Handles.Label(position, $"<b>Player Start</b>", style);
                    break;

                case MapObjectType.SubwayStop:
                    Handles.Label(position, $"<b>Subway Stop: \"{mapObject.Name}\"</b>", style);
                    break;
            }
        }
#endif

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(Point, 0.25f);
        }
#endif
    }
}
