using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace B1TJam2025.MainMenu
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("B1TJam2025/Main Menu/Credits Shuffler")]
    public class CreditsShuffler : MonoBehaviour
    {
        private readonly List<string> m_strings = new();

        public string[] credits;

        public void Refresh()
        {
            m_strings.Clear();
            m_strings.AddRange(credits);
            TMP_Text text = GetComponent<TMP_Text>();
            text.text = $"Made by:\n\n";

            while (m_strings.Count > 0)
            {
                int index = Mathf.FloorToInt(m_strings.Count * Random.value);
                text.text += $"{m_strings[index]}\n";
                m_strings.RemoveAt(index);
            }

        }
    }
}
