using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace B1TJam2025
{
    [CreateAssetMenu(fileName = "Conversation", menuName = "Scriptable Objects/Dialouge/Conversation")]
    public class Conversation : ScriptableObject
    {
        private const int CHAR_CAP = 100;

        [SerializeField]
        [Tooltip("The lines that make up the conversation")]
        private List<Line> _lines = new();

        //properties
        public List<Line> Lines => _lines;

        [Serializable]
        public struct Line
        {
            [SerializeField]
            public string LineText;
            [SerializeField]
            public string CharName;
            [SerializeField]
            public Sprite CharacterSprite;
            [SerializeField]
            public bool FireEventOnClear;
            [SerializeField]
            public string EventCode;

        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Conversation))]
        public class ConversationEditor: Editor
        {
            private List<string> _usedNames = new();

            public override void OnInspectorGUI()
            {
                Conversation _cEditor = (Conversation)target;
                GUIStyle wrapStyle = new(GUI.skin.textArea){wordWrap = true};

                //works out used names
                for (int i = 0; i < _cEditor._lines.Count; i++)
                    if(!_usedNames.Contains(_cEditor._lines[i].CharName))
                        _usedNames.Add(_cEditor._lines[i].CharName);

                if (_usedNames.Contains("")) _usedNames.Remove("");

                GUILayout.BeginVertical();
                {
                    if (GUILayout.Button("Add Line"))
                    {
                        _cEditor._lines.Add(new Line());
                    }
                    GUILayout.Space(15);

                    bool change = false;
                    for (int i = 0; i < _cEditor._lines.Count; i++)
                    {
                        //number, and entry movement
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label((i + 1).ToString(), GUILayout.Width(25));
                            if (GUILayout.Button("Move Up", GUILayout.Width(70)))
                            {
                                Line l = _cEditor._lines[i];
                                int aboveIndex = Mathf.Clamp(i - 1, 0, _cEditor._lines.Count - 1);
                                _cEditor._lines[i] = _cEditor._lines[aboveIndex];
                                _cEditor._lines[aboveIndex] = l;
                                change = true;
                            }
                            if (GUILayout.Button("Move Down", GUILayout.Width(80)))
                            {
                                Line l = _cEditor._lines[i];
                                int belowIndex = Mathf.Clamp(i + 1, 0, _cEditor._lines.Count - 1);
                                _cEditor._lines[i] = _cEditor._lines[belowIndex];
                                _cEditor._lines[belowIndex] = l;
                                change = true;
                            }
                            if (GUILayout.Button("Delete Entry", GUILayout.Width(85))) 
                            { 
                                _cEditor._lines.RemoveAt(i); 
                                change = true;
                            }
                        }
                        GUILayout.EndHorizontal();

                        if (!change)
                        {
                            //line editor
                            Line org = _cEditor._lines[i];
                            _cEditor._lines[i] = DisplayLineEditor(_cEditor._lines[i], ref _usedNames);

                            //change check
                            if (org.LineText != _cEditor._lines[i].LineText ||
                                org.CharName != _cEditor._lines[i].CharName ||
                                org.CharacterSprite != _cEditor._lines[i].CharacterSprite)
                                change = true;
                        }
                    }

                    if(change)
                    {
                        EditorUtility.SetDirty(_cEditor);
                    }
                }
                GUILayout.EndVertical();


                Line DisplayLineEditor(Line l, ref List<string> exsistingNames)
                {
                    GUILayout.BeginVertical();
                    {
                        //character name
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Name -", GUILayout.Width(50));
                            l.CharName = EditorGUILayout.TextField(l.CharName ?? "", GUILayout.Width(150));
                            if(exsistingNames != null)
                            {
                                if (!exsistingNames.Contains(l.CharName)) exsistingNames.Add(l.CharName);
                                int i = exsistingNames.IndexOf(l.CharName);

                                i = EditorGUILayout.Popup(i, exsistingNames.ToArray());
                                l.CharName = exsistingNames[i];
                            }
                        }
                        GUILayout.EndHorizontal();

                        //dialouge line
                        GUILayout.BeginHorizontal();
                        {
                            l.LineText ??= "";

                            EditorGUILayout.LabelField("Line (" + l.LineText.Length.ToString() + "/" + CHAR_CAP.ToString() + ")", GUILayout.Width(75));
                            l.LineText = EditorGUILayout.TextArea(l.LineText, wrapStyle, GUILayout.Height(50));
                            if(l.LineText.Length > CHAR_CAP) l.LineText = l.LineText.Substring(0, CHAR_CAP); //limits length
                        }
                        GUILayout.EndHorizontal();

                        //character sprite
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Sprite - ", GUILayout.Width(50));
                            l.CharacterSprite = (Sprite)EditorGUILayout.ObjectField(l.CharacterSprite, typeof(Sprite), false);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginVertical();
                        {
                            l.FireEventOnClear = GUILayout.Toggle(l.FireEventOnClear, "Fire event after cleared");
                            if (l.FireEventOnClear == true)
                            {
                                l.EventCode ??= "";

                                GUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.LabelField("Event string code -", GUILayout.Width(120));
                                    l.EventCode = EditorGUILayout.TextField(l.EventCode);
                                }
                                GUILayout.EndHorizontal();
                            }
                            else l.EventCode = null;
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.Space(20);
                    GUILayout.EndVertical();

                    return l;
                }

                _usedNames.Clear();
            }


        }
#endif
    }

}
