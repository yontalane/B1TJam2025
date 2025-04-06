using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace B1TJam2025.AudioSystems
{
    public class SFXPlayerSystem : MonoBehaviour
    {
        //private, edited through inspector
        [SerializeField]
        private List<SFXRandomSelectSet> _sets = new();
        [SerializeField]
        private List<string> _nameIDs = new(); //parrelel with sets, becomes dictionary at runtime

        //runtime
        /// <summary>
        /// Bindings of SFX set names to their set name
        /// </summary>
        public Dictionary<string, SFXRandomSelectSet> SetsByName { get; private set; }
        /// <summary>
        /// Array of sets
        /// </summary>
        public SFXRandomSelectSet[] Sets { get; private set; }

        private void Awake()
        {
            SetsByName = new();
            for(int i = 0; i < _sets.Count && i < _nameIDs.Count; i++)
            {
                SetsByName.Add(_nameIDs[i], _sets[i]);
            }

            Sets = _sets.ToArray();
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(SFXPlayerSystem))]
        public class SFXPlayerSystemEditor: Editor
        {
            private List<bool> _setDropdowns = new();
            private HashSet<string> _nameIDUniqueValidation = new();

            public override void OnInspectorGUI ()
            {
                //gets instance
                SFXPlayerSystem ps = (SFXPlayerSystem)target;

                //fills out dropdowns for sets
                if (_setDropdowns.Count != ps._sets.Count)
                {
                    int dif = ps._sets.Count - _setDropdowns.Count;
                    for (int i = 0; i < dif; i++) _setDropdowns.Add(false);
                }

                EditorGUI.BeginChangeCheck();
                {
                    GUILayout.BeginVertical();
                    {
                        if (GUILayout.Button("Add New Set"))
                        {
                            ps._sets.Add(new());
                            _setDropdowns.Add(true);
                            ps._nameIDs.Add("new Set");
                        }
                        GUILayout.Space(20);

                        for (int i = 0; i < ps._sets.Count; i++)
                        {
                            _setDropdowns[i] = EditorGUILayout.Foldout(_setDropdowns[i], ps._nameIDs[i]);
                            if (_setDropdowns[i])
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    if (GUILayout.Button("Move Set Up"))
                                    {
                                        (ps._sets[i], ps._sets[Mathf.Max(i - 1, 0)]) = (ps._sets[Mathf.Max(i - 1, 0)], ps._sets[i]);
                                        (_setDropdowns[i], _setDropdowns[Mathf.Max(i - 1, 0)]) = (_setDropdowns[Mathf.Max(i - 1, 0)], _setDropdowns[i]);
                                        (ps._nameIDs[i], ps._nameIDs[Mathf.Max(i - 1, 0)]) = (ps._nameIDs[Mathf.Max(i - 1, 0)], ps._nameIDs[i]);
                                        EditorGUILayout.EndHorizontal(); break;
                                    }
                                    if (GUILayout.Button("Move Set Down"))
                                    {
                                        (ps._sets[i], ps._sets[Mathf.Min(i + 1, ps._sets.Count - 1)]) = (ps._sets[Mathf.Min(i + 1, ps._sets.Count - 1)], ps._sets[i]);
                                        (_setDropdowns[i], _setDropdowns[Mathf.Min(i + 1, _setDropdowns.Count - 1)]) = (_setDropdowns[Mathf.Min(i + 1, _setDropdowns.Count - 1)], _setDropdowns[i]);
                                        (ps._nameIDs[i], ps._nameIDs[Mathf.Min(i + 1, ps._nameIDs.Count - 1)]) = (ps._nameIDs[Mathf.Min(i + 1, ps._nameIDs.Count - 1)], ps._nameIDs[i]);
                                        EditorGUILayout.EndHorizontal(); break;
                                    }
                                    if (GUILayout.Button("Delete Set"))
                                    {
                                        ps._sets.RemoveAt(i);
                                        _setDropdowns.RemoveAt(i);
                                        ps._nameIDs.RemoveAt(i);
                                        EditorGUILayout.EndHorizontal(); break;
                                    }
                                }
                                GUILayout.EndHorizontal();

                                //field for setting ID
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("Set Name ID -", GUILayout.Width(100));
                                    ps._nameIDs[i] = GUILayout.TextField(ps._nameIDs[i]);
                                }
                                GUILayout.EndHorizontal();


                                ps._sets[i] = SFXSetEditor(ps._sets[i]);
                                GUILayout.Space(25);
                            }
                        }
                    }
                    GUILayout.EndVertical();



                }
                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(this);

                _nameIDUniqueValidation.Clear();
                for(int i = 0; i <  ps._nameIDs.Count; i++)
                {
                    if (_nameIDUniqueValidation.Contains(ps._nameIDs[i]))
                    {
                        ps._nameIDs[i] += "1";
                    }
                    _nameIDUniqueValidation.Add(ps._nameIDs[i]);
                }
            }

            public SFXRandomSelectSet SFXSetEditor(SFXRandomSelectSet set)
            {
                EditorGUILayout.BeginVertical();
                {
                    if(GUILayout.Button("Add")) set.Clips.Add(null); //adds new empty entry

                    for (int i = 0; i < set.Clips.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            if(GUILayout.Button("Move Clip Up"))
                            {
                                (set.Clips[i], set.Clips[Mathf.Max(i - 1, 0)]) = (set.Clips[Mathf.Max(i - 1, 0)], set.Clips[i]);
                                EditorGUILayout.EndHorizontal(); break;
                            }
                            if (GUILayout.Button("Move Clip Down"))
                            {
                                (set.Clips[i], set.Clips[Mathf.Min(i + 1, set.Clips.Count - 1)]) = (set.Clips[Mathf.Min(i + 1, set.Clips.Count - 1)], set.Clips[i]);
                                EditorGUILayout.EndHorizontal(); break;
                            }
                            if (GUILayout.Button("Delete Clip"))
                            {
                                set.Clips.RemoveAt(i);
                                EditorGUILayout.EndHorizontal(); break;
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField("Clip -", GUILayout.Width(40));
                            set.Clips[i] = (AudioClip)EditorGUILayout.ObjectField(set.Clips[i], typeof(AudioClip), false);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(10);
                    }
                }
                EditorGUILayout.EndVertical();

                return set;
            }
        }
#endif
    }
}
