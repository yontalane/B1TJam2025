using System.Collections.Generic;
using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Bystander Manager")]
    public sealed class BystanderManager : MonoBehaviour
    {
        public delegate void BystanderManagerEventHandler(int newBystandersRemaining, int originalTotalBystanders);
        public static BystanderManagerEventHandler OnBystanderKilled = null;


        private readonly List<GameObject> m_list = new();
        private int m_bystandersCount;


        [SerializeField]
        private string m_spawnTag;

        [SerializeField]
        private string m_spawnName;

        [SerializeField]
        private Vector2 m_maxOffset;

        [SerializeField]
        private int m_spawnCount;

        [Space]

        [SerializeField]
        private Bystander m_prefab;


        private void OnEnable()
        {
            GameManager.OnGameStart += OnGameStart;
            Bystander.OnBystanderKilled += OnIndividualBystanderKilled;
        }

        private void OnDisable()
        {
            GameManager.OnGameStart -= OnGameStart;
            Bystander.OnBystanderKilled -= OnIndividualBystanderKilled;
        }


        private void OnGameStart()
        {
            m_spawnName = m_spawnName.ToLower();
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(m_spawnTag);

            foreach (GameObject gameObject in gameObjects)
            {
                if (!gameObject.name.ToLower().Contains(m_spawnName))
                {
                    continue;
                }

                m_list.Add(gameObject);
            }

            m_bystandersCount = m_spawnCount;

            for (int i = 0; i < m_spawnCount; i++)
            {
                GameObject location = m_list[Mathf.FloorToInt(m_list.Count * Random.value)];

                Vector3 position = location.transform.position + new Vector3()
                {
                    x = Random.Range(-m_maxOffset.x, m_maxOffset.x),
                    z = Random.Range(-m_maxOffset.y, m_maxOffset.y),
                };

                Bystander bystander = Instantiate(m_prefab);

                bystander.Position = position;
                bystander.transform.localEulerAngles = new()
                {
                    x = 0f,
                    y = Random.value * 360f,
                    z = 0f,
                };
                bystander.transform.localScale = Vector3.one;

                bystander.Initialize(m_list);
            }
        }


        private void OnIndividualBystanderKilled(Bystander bystander)
        {
            m_bystandersCount--;
            OnBystanderKilled?.Invoke(m_bystandersCount, m_spawnCount);
        }
    }
}
