using UnityEngine;
using TMPro;
using System.Collections;

namespace B1TJam2025.MainMenu
{
    /// <summary>
    /// Monobehavior used to make a text element flicker colors when moused over
    /// </summary>
    public class TMP_Text_Flicker : MonoBehaviour
    {
        [Header("Settings")]

        [SerializeField] 
        [Tooltip("The first color to lerp between whe flashing")] 
        private Color _colorOne = Color.white;

        [SerializeField] 
        [Tooltip("The second color to lerp between whe flashing")] 
        private Color _colorTwo = Color.black;

        [SerializeField]
        [Tooltip("how long should it take for the text to lerp both ways between the colors? (seconds)")] 
        [Range(0.01f, 1f)] 
        float _flashDuration = 0.25f;


        [Header("Refrences")]
        [Space(10)]

        [SerializeField]
        [Tooltip("Refrence to the TMP text to flicker when hovered over")]
        private TMP_Text _text_element;



        //private variables
        private Color _initialColor;
        private Coroutine _flickerRoutine = null;

        private void Start()
        {
            if (_text_element == null)
            {
                Debug.LogWarning("TMP_Text_Flicker on " + transform.name + " does not have a text element assigned, please assign one for flicker behaviors");
                Destroy(this);
            }
            else
            {
                _initialColor = _text_element.color;
            }
        }

        private void OnMouseEnter()
        {
            _text_element.color = _colorOne;
            if (_flickerRoutine == null) _flickerRoutine = StartCoroutine(Flicker());
        }

        private void OnMouseExit()
        {
            if (_flickerRoutine != null)
            {
                StopCoroutine(_flickerRoutine);
                _flickerRoutine = null;
            }

            _text_element.color = _initialColor;
        }

        private IEnumerator Flicker()
        {
            float t = 0;
            float duration = _flashDuration * 0.5f;
            (Color, Color) colors = (_colorOne, _colorTwo);

            while (true)
            {
                for (t = 0; t < duration; t += Time.deltaTime)
                {
                    _text_element.color = Color.Lerp(colors.Item1, colors.Item2, Mathf.Clamp01(t / duration));
                    yield return null;
                }
                colors = (colors.Item2, colors.Item1); //swaps 
            }
        }
    }
}


