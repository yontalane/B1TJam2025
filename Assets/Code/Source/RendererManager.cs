using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Renderer Manager")]
    public sealed class RendererManager : MonoBehaviour
    {
        [System.Serializable]
        private struct MonochromeSet
        {
            public string name;

            [ColorUsage(false, false)]
            public Color colorA;

            [ColorUsage(false, false)]
            public Color colorB;
        }


        private static RendererManager s_instance;
        private bool m_initialized = false;
        private FullScreenPassRendererFeature m_rendererFeature;
        private Material m_rendererFeatureOriginalMaterial;
        private Material m_rendererFeatureInstanceMaterial;
        private MonochromeSet m_defaultSet;


        [Header("Color Sets")]

        [SerializeField]
        private MonochromeSet[] m_sets;

        [Header("Renderer")]

        [SerializeField]
        private UniversalRendererData m_renderer;


        private void Reset()
        {
            m_sets = new MonochromeSet[0];

            m_renderer = null;
        }


        private void OnDisable()
        {
            if (m_initialized)
            {
                m_rendererFeature.passMaterial = m_rendererFeatureOriginalMaterial;
            }
        }


        private void Awake()
        {
            s_instance = this;
        }

        private void Start()
        {
            if (m_renderer.TryGetRendererFeature(out FullScreenPassRendererFeature feature))
            {
                m_rendererFeature = feature;
                m_rendererFeatureOriginalMaterial = m_rendererFeature.passMaterial;
                m_rendererFeatureInstanceMaterial = Instantiate(m_rendererFeatureOriginalMaterial);
                m_rendererFeature.passMaterial = m_rendererFeatureInstanceMaterial;

                m_defaultSet = new()
                {
                    colorA = m_rendererFeatureInstanceMaterial.GetColor("_ColorA"),
                    colorB = m_rendererFeatureInstanceMaterial.GetColor("_ColorB"),
                };

                m_initialized = true;
            }
        }


        public static void ResetColors()
        {
            if (!s_instance.m_initialized)
            {
                return;
            }

            s_instance.m_rendererFeatureInstanceMaterial.SetColor("_ColorA", s_instance.m_defaultSet.colorA);
            s_instance.m_rendererFeatureInstanceMaterial.SetColor("_ColorB", s_instance.m_defaultSet.colorB);
        }

        public static void SetColorsByName(string setName)
        {
            if (!s_instance.m_initialized)
            {
                return;
            }

            foreach (MonochromeSet colorSet in s_instance.m_sets)
            {
                if (colorSet.name != setName)
                {
                    continue;
                }

                s_instance.m_rendererFeatureInstanceMaterial.SetColor("_ColorA", colorSet.colorA);
                s_instance.m_rendererFeatureInstanceMaterial.SetColor("_ColorB", colorSet.colorB);

                return;
            }
        }
    }
}
