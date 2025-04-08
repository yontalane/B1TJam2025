using UnityEngine;

namespace B1TJam2025.MainMenu
{
    public class TextHoverPalleteChange : MonoBehaviour
    {
        [SerializeField]
        private string _palleteCode;

        private void OnMouseEnter() => RendererManager.SetColorsByName(_palleteCode);

        private void OnMouseExit() => RendererManager.ResetColors();

        private void OnMouseDown() => RendererManager.ResetColors();
    }
}
