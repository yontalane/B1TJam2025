using UnityEngine;
using System.Collections.Generic;
using System;

//Only needed for custom editor
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace B1TJam2025
{
    public class PerpSpawnLocationManager : MonoBehaviour
    {
        //delegates
        public delegate void SpawnPerpHandler(GameObject perp);
        public static SpawnPerpHandler OnSpawnPerp = null;

        //private statics
        //singleton instance
        private PerpSpawnLocationManager __instance;
        //events
        private static Action __spawnNextPerp;

        //public statics
        /// <summary>
        /// Spawns the next Perp
        /// </summary>
        public static void SpawnNextPerp() => __spawnNextPerp?.Invoke();


        [Header("Settings")]

        [SerializeField]
        private bool _attractMode;

        [SerializeField] 
        private List<PerpSpawnPoint> _perpSpawns;

        [SerializeField] 
        protected List<PerpBase> _perps = new();


        private void Awake()
        {
            //singleton pattern
            if (__instance != null) Destroy(this);
            else __instance = this;

            if (_attractMode) SpawnRandom(_perps); //starts the demo spawn chain
        }

        private void OnEnable() => __spawnNextPerp += () => SpawnRandom(_perps);
        private void OnDisable() => __spawnNextPerp = null;


        public void SpawnRandom(List<PerpBase> perps)
        {
            if(perps.Count > 0 && _perpSpawns.Count > 0)
            {
                GameObject perpObj = perps[UnityEngine.Random.Range(0, perps.Count)].SpawnPerp();
                PerpSpawnPoint spawn = _perpSpawns[UnityEngine.Random.Range(0, _perpSpawns.Count)];
                perpObj.transform.SetPositionAndRotation(spawn.Position, spawn.Rotation);

                OnSpawnPerp?.Invoke(perpObj);
            }
        }



        //Custom Editor (needs acsess to private variables)
#if UNITY_EDITOR
        [CustomEditor(typeof(PerpSpawnLocationManager))]
        public class PerpSpawnLocationManagerEditor : Editor
        {
            public void OnSceneGUI()
            {
                PerpSpawnLocationManager manager = (PerpSpawnLocationManager)target;

                if(manager._perpSpawns != null)
                {
                    //for marking dirty on change
                    Vector3 initPos;
                    Quaternion initRot;

                    //for making labels more visible
                    Handles.color = Color.green;
                    GUIStyle labelStyle = GUI.skin.label;
                    labelStyle.normal.textColor = Color.red;

                    PerpSpawnPoint curPoint;

                    for(int i = 0; i < manager._perpSpawns.Count; i++)
                    {
                        curPoint = manager._perpSpawns[i]; //gets current
                        initPos = curPoint.Position;
                        initRot = curPoint.Rotation;
                        

                        //draws handles for display, label and editing easier
                        Handles.DrawWireDisc(curPoint.Position, curPoint.Rotation * Vector3.forward, 1);
                        Handles.Label(curPoint.Position + Vector3.one * 0.8f, curPoint.SpawnPointName, labelStyle);
                        Handles.TransformHandle(ref curPoint.Position, ref curPoint.Rotation);
                        //saves changes and markes the scene dirty when edited through handles
                        if(initPos != curPoint.Position || initRot != curPoint.Rotation)
                        {
                            SaveChanges();
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                            manager._perpSpawns[i] = curPoint; //writes changes
                        }
                    }
                    labelStyle.normal.textColor = Color.white; //resets label color to prevent miscoloration in other GUI
                }
            }
        }
#endif
    }
}

/// <summary>
/// Data storage for individual perp spawn points
/// </summary>
[Serializable]
public struct PerpSpawnPoint
{
    [SerializeField] public Vector3 Position;
    [SerializeField] public Quaternion Rotation;
    [SerializeField] public string SpawnPointName;
}
