using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Collections;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace B1TJam2025
{
    public class DialougeWriter : MonoBehaviour
    {
        [Header("Settings")]

        [SerializeField]
        [Tooltip("The amount of characters per second that get written to the dialouge box")]
        [Range(1, 1000)]
        private float _charsPerSec = 15f;

        [SerializeField]
        [Tooltip("The color of the text, set before triggering dialouge")]
        private Color32 _textColor;

        [Header("Refrences")]

        [SerializeField]
        [Tooltip("The text mesh pro element that is written to for the line text")]
        private TextMeshProUGUI _lineTextElement;

        [SerializeField]
        [Tooltip("The text mesh pro element that is written to for the character name")]
        private TextMeshProUGUI _charNameTextElement;

        [SerializeField]
        [Tooltip("Image for character sprite")] 
        private UnityEngine.UI.Image _characterImage;


        //private vars
        private Coroutine _writeRoutine = null;

        //public properties
        public State WritingState { get; private set; }


        public void WriteText(Conversation.Line line)
        {
            if (_writeRoutine != null) StopCoroutine(_writeRoutine);

            ChangeTextColor(Color.clear);

            _lineTextElement.vertexBufferAutoSizeReduction = true;
            _lineTextElement.SetText(line.LineText);
            _lineTextElement.ForceMeshUpdate();

            //initializes line text and sets to black
            Mesh m = _lineTextElement.textInfo.meshInfo[0].mesh;
            Color32[] vertColors = m.colors32;
            Color clear = Color.clear;
            for (int i = 0; i < vertColors.Length; i++) vertColors[i] = Color.clear;
            m.SetColors(vertColors);
            _lineTextElement.canvasRenderer.SetMesh(m);

            //sets character name text
            _charNameTextElement.SetText(line.CharName);
            _charNameTextElement.color = _textColor;

            //sets char sprite
            _characterImage.sprite = line.CharacterSprite;

            //writing routing
            WritingState = State.Writing;
            _writeRoutine = StartCoroutine(ProgressiveColorChange(_textColor));

            IEnumerator ProgressiveColorChange(Color32 color)
            {
                string text = line.LineText;

                WaitForSeconds delay = new(1f / _charsPerSec);
                int charsPerFrame = (int)Mathf.Ceil(Time.deltaTime * _charsPerSec);

                //slow speeds will have use delay
                if(charsPerFrame == 1)
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        int vIndex = _lineTextElement.textInfo.characterInfo[i].vertexIndex;
                        for (int j = 0; j < 4; j++)
                        {
                            vertColors[vIndex + j] = color;
                        }

                        m.SetColors(vertColors);
                        _lineTextElement.canvasRenderer.SetMesh(m);
                        yield return delay;
                    }
                }
                //fast speeds may require multiple characters per frame
                else if (charsPerFrame < _lineTextElement.textInfo.meshInfo[0].vertexCount / 4)
                {
                    for (int i = 0; i + charsPerFrame < text.Length; i += charsPerFrame)
                    {
                        int vIndex = _lineTextElement.textInfo.characterInfo[i].vertexIndex;
                        for (int j = 0; j < 4 * charsPerFrame; j++) vertColors[vIndex + j] = color;

                        m.SetColors(vertColors);
                        _lineTextElement.canvasRenderer.SetMesh(m);
                        yield return null;
                    }
                    for (int i = 0; i < vertColors.Length; i++) vertColors[i] = color;
                    m.SetColors(vertColors);
                    _lineTextElement.canvasRenderer.SetMesh(m);
                }
                //really fast speeds are effectivly instant
                else
                {
                    for(int i = 0; i < vertColors.Length; i++) vertColors[i] = color;
                    m.SetColors(vertColors);
                    _lineTextElement.canvasRenderer.SetMesh(m);
                }

                WritingState = State.Complete;
            }
        
        }

        public void InstantFinishText()
        {
            if (_writeRoutine != null)
            {
                StopCoroutine(_writeRoutine);
                _lineTextElement.color = _textColor;
                WritingState = State.Complete;

                ChangeTextColor(_textColor);
            }
        }

        public void ClearText()
        {
            _lineTextElement.SetText("");
            WritingState = State.Complete;
        }

        private void ChangeTextColor(Color32 color)
        {
            //initializes text and sets to black
            Mesh m = _lineTextElement.textInfo.meshInfo[0].mesh;
            Color32[] vertColors = m.colors32;
            for (int i = 0; i < vertColors.Length; i++) vertColors[i] = color;
            m.SetColors(vertColors);
            _lineTextElement.canvasRenderer.SetMesh(m);
        }

        public enum State
        {
            Writing,
            Complete
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(DialougeWriter))]
        public class DialougeWriterEditor: Editor
        {
            private DialougeWriter _writer;
            private string _textToTestWrite = "";

            public override VisualElement CreateInspectorGUI()
            {
                _writer = target as DialougeWriter;
                return base.CreateInspectorGUI();
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.BeginVertical();
                GUILayout.Space(10f);

                if (_writer != null)
                {
                    if (!Application.isPlaying)
                    {
                        EditorGUILayout.LabelField("Enter playmode to test text writing");
                    }
                    else
                    {
                        _textToTestWrite = EditorGUILayout.TextField("Text to test write -", _textToTestWrite);

                        if (GUILayout.Button("Write Text"))
                        {
                            Conversation.Line line = new();
                            line.LineText = _textToTestWrite;
                            line.CharacterSprite = null;
                            line.CharName = "Debug Tester";

                            _writer.WriteText(line);
                        }
                    }

                }
                else EditorGUILayout.LabelField("Error getting component instance");
                EditorGUILayout.EndVertical();
            }
        }
#endif
    }
}
