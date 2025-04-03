using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Subway Manager")]
    public sealed class SubwayManager : MonoBehaviour
    {
        private static SubwayManager s_instance;


        [Header("Scene References")]

        [SerializeField]
        private GameObject m_subwayMap;

        [SerializeField]
        private RectTransform m_buttonContainer;

        [SerializeField]
        private RectTransform m_fade;

        [Header("Prefabs")]

        [SerializeField]
        private Button m_buttonPrefab;


        private void Reset()
        {
            m_subwayMap = null;
            m_buttonContainer = FindAnyObjectByType<RectTransform>();
            m_fade = FindAnyObjectByType<RectTransform>();

            m_buttonPrefab = null;
        }


        private void Awake()
        {
            s_instance = this;
        }


        public static void InteractWithSubwayStop(SubwayStop subwayStop)
        {
            GameManager.IsPaused = true;

            for (int i = s_instance.m_buttonContainer.childCount - 1; i >= 0; i--)
            {
                if (s_instance.m_buttonContainer.GetChild(i).TryGetComponent(out Button b))
                {
                    b.onClick.RemoveAllListeners();
                }

                Destroy(s_instance.m_buttonContainer.GetChild(i).gameObject);
            }

            SubwayStop[] stops = FindObjectsByType<SubwayStop>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach (SubwayStop stop in stops)
            {
                if (stop == subwayStop)
                {
                    continue;
                }

                Button b = Instantiate(s_instance.m_buttonPrefab);
                b.transform.SetParent(s_instance.m_buttonContainer);

                b.transform.localPosition = Vector3.zero;
                b.transform.localEulerAngles = Vector3.zero;
                b.transform.localScale = Vector3.one;

                string name = stop.Name;

                TMP_Text text = b.GetComponentInChildren<TMP_Text>();
                text.text = name;

                b.onClick.AddListener(() => s_instance.OnClickButton(name));
            }

            s_instance.StartCoroutine(s_instance.FixLayoutGroups());

            s_instance.m_fade.gameObject.SetActive(false);

            s_instance.m_subwayMap.SetActive(true);
        }

        private IEnumerator FixLayoutGroups()
        {
            m_buttonContainer.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            m_buttonContainer.gameObject.SetActive(true);
            RefreshLayoutGroupsImmediateAndRecursive(m_buttonContainer.gameObject);
            yield return new WaitForEndOfFrame();
            RefreshLayoutGroupsImmediateAndRecursive(m_buttonContainer.gameObject);
        }

        private static void RefreshLayoutGroupsImmediateAndRecursive(GameObject root)
        {
            LayoutGroup[] componentsInChildren = root.GetComponentsInChildren<LayoutGroup>(true);

            foreach (LayoutGroup layoutGroup in componentsInChildren)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            }

            if (root.TryGetComponent(out LayoutGroup parent))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
            }
        }

        private void OnClickButton(string stopName)
        {
            SubwayStop[] stops = FindObjectsByType<SubwayStop>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

            foreach(SubwayStop stop in stops)
            {
                if (stop.Name == stopName)
                {
                    StartCoroutine(TransitionToNewStop(stop));
                    return;
                }
            }

            m_subwayMap.SetActive(false);
            GameManager.IsPaused = false;
        }

        private IEnumerator TransitionToNewStop(SubwayStop stop)
        {
            m_fade.anchorMax = new(0f, 1f);
            m_fade.offsetMin = Vector2.zero;
            m_fade.offsetMax = Vector2.zero;
            m_fade.gameObject.SetActive(true);

            float duration = 0.35f;
            float startTime = Time.time;
            float t = 0f;

            while (t <= 1f)
            {
                yield return new WaitForEndOfFrame();

                t = (Time.time - startTime) / duration;

                m_fade.anchorMax = new(t, 1f);
                m_fade.offsetMin = Vector2.zero;
                m_fade.offsetMax = Vector2.zero;
            }

            GameManager.Player.transform.position = stop.transform.position + Vector3.right;

            m_subwayMap.SetActive(false);
            GameManager.IsPaused = false;
        }
    }
}
