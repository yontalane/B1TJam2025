using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace B1TJam2025.AudioSystems
{
    public class AudioManager : MonoBehaviour
    {
        /// <summary>
        /// Single active instance of the audio manager (Singleton)
        /// </summary>
        public static AudioManager Instance {get; private set;}

        /// <summary>
        /// The volume of every sound is multiplied by this.
        /// </summary>
        public static float GlobalVolume
        {
            get => s_globalVolume;
            set => s_globalVolume = Mathf.Clamp01(value);
        }

        [Header("Refrences")]
        [SerializeField]
        [Tooltip("Mixer to direct sfx to")]
        AudioMixerGroup _SFXMixerGroup;

        private static float s_globalVolume = 1f;

        //object pool
        private readonly List<GameObject> _audioSourcePool = new();

        private void Awake()
        {
            //singleton pattern
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        #region "Sound Playing Functions"

        //plays at static position
        /// <summary>
        /// Play a sound effect at a location
        /// </summary>
        /// <param name="clip"> audio clip for the sound effect </param>
        /// <param name="position"> position for the sound effect </param>
        /// <param name="volume"> volume for the sound (simplified Enum) </param>
        public void PlaySound(AudioClip clip, Vector3 position, Volume volume = Volume.Loud)
        {
            //gets objects
            GameObject obj = GetPooled();
            AudioSource src = obj.GetComponent<AudioSource>();

            //enables objects
            obj.SetActive(true);
            src.enabled = true;

            //assigns data
            obj.transform.position = position;
            src.volume = _volumeVals[volume] * GlobalVolume;

            //play
            src.PlayOneShot(clip);
            StartCoroutine(PoolOnDelay(clip.length + 0.25f, obj));
        }

        /// <summary>
        /// Play a sound effect at a location
        /// </summary>
        /// <param name="clip"> audio clip for the sound effect </param>
        /// <param name="position"> position for the sound effect </param>
        /// <param name="volume"> volume for the sound as float (0f-1f) </param>
        public void PlaySound(AudioClip clip, Vector3 position, float volume)
        {
            //gets objects
            GameObject obj = GetPooled();
            AudioSource src = obj.GetComponent<AudioSource>();

            //enables objects
            obj.SetActive(true);
            src.enabled = true;

            //assigns data
            obj.transform.position = position;
            src.volume = volume * GlobalVolume;

            //play
            src.PlayOneShot(clip);
            StartCoroutine(PoolOnDelay(clip.length + 0.25f, obj));
        }

        //tracks to a object as it plays
        /// <summary>
        /// Play a sound effect at a location
        /// </summary>
        /// <param name="clip"> audio clip for the sound effect </param>
        /// <param name="objToTrack"> Transform for the sound effect to follow </param>
        /// <param name="volume"> volume for the sound (simplified Enum) </param>
        public void PlaySoundTracked(AudioClip clip, Transform objToTrack, Volume volume = Volume.Loud)
        {
            //gets objects
            GameObject obj = GetPooled();
            AudioSource src = obj.GetComponent<AudioSource>();

            //enables objects
            obj.SetActive(true);
            src.enabled = true;

            //assigns data
            obj.transform.position = objToTrack.position;
            src.volume = _volumeVals[volume] * GlobalVolume;

            //play
            src.PlayOneShot(clip);
            StartCoroutine(TrackAudioSource(obj, objToTrack));
            StartCoroutine(PoolOnDelay(clip.length + 0.25f, obj));
        }

        /// <summary>
        /// Play a sound effect at a location
        /// </summary>
        /// <param name="clip"> audio clip for the sound effect </param>
        /// <param name="objToTrack"> Transform for the sound effect to follow </param>
        /// <param name="volume"> volume for the sound as float (0f-1f) </param>
        public void PlaySoundTracked(AudioClip clip, Transform objToTrack, float volume)
        {
            //gets objects
            GameObject obj = GetPooled();
            AudioSource src = obj.GetComponent<AudioSource>();

            //enables objects
            obj.SetActive(true);
            src.enabled = true;

            //assigns data
            obj.transform.position = objToTrack.position;
            src.volume = volume * GlobalVolume;

            //play
            src.PlayOneShot(clip);
            StartCoroutine(TrackAudioSource(obj, objToTrack));
            StartCoroutine(PoolOnDelay(clip.length + 0.25f, obj));
        }

        #endregion

        #region Delayed Play Functions

        /// <summary> Run PlaySoundTracked with a delay </summary>
        /// <param name="delay"> delay in seconds </param>
        public void PlaySoundOnDelay(float delay, AudioClip clip, Vector3 position, Volume volume = Volume.Loud) => 
            StartCoroutine(RunOnDelay(delay, () => { PlaySound(clip, position, volume); }));

        /// <summary> Run PlaySoundTracked with a delay </summary>
        /// <param name="delay"> delay in seconds </param>
        public void PlaySoundOnDelay(float delay, AudioClip clip, Vector3 position, float volume) =>
            StartCoroutine(RunOnDelay(delay, () => { PlaySound(clip, position, volume); }));

        /// <summary> Run PlaySoundTracked with a delay </summary>
        /// <param name="delay"> delay in seconds </param>
        public void PlaySoundTrackedOnDelay(float delay, AudioClip clip, Transform objToTrack, Volume volume = Volume.Loud) =>
            StartCoroutine(RunOnDelay(delay, () => { PlaySoundTracked(clip, objToTrack, volume); }));

        /// <summary> Run PlaySoundTracked with a delay </summary>
        /// <param name="delay"> delay in seconds </param>
        public void PlaySoundTrackedOnDelay(float delay, AudioClip clip, Transform objToTrack, float volume) =>
            StartCoroutine(RunOnDelay(delay, () => { PlaySoundTracked(clip, objToTrack, volume); }));

        //runs a function on delay
        private IEnumerator RunOnDelay(float delay, Action func)
        {
            yield return new WaitForSeconds(delay);
            func?.Invoke();
        }

        #endregion


    /// <summary>
    /// Moves a audiosource to be aligned with a transform each frame until it's set inactive
    /// </summary>
    private IEnumerator TrackAudioSource(GameObject source, Transform trackedObj)
        {
            while (trackedObj != null && source.activeInHierarchy)
            {
                source.transform.position = trackedObj.transform.position;
                yield return null;
            }
        }


        #region Pooling Functions

        /// <summary>
        /// Used to disable the source after it is done playing, sending it back to the object pool
        /// </summary>
        private IEnumerator PoolOnDelay(float delay, GameObject obj)
        {
            yield return new WaitForSeconds(delay);
            SendBackToPool(obj);
        }

        /// <summary>
        /// Gets the first available object from the pool, creating a new one if needed
        /// </summary>
        private GameObject GetPooled()
        {
            for(int i = 0; i < _audioSourcePool.Count; i++)
            {
                if (!_audioSourcePool[i].activeInHierarchy) return _audioSourcePool[i];
            }
            return CreateSource();
        }

        /// <summary>
        /// Sends a source back to the pool
        /// </summary>
        private void SendBackToPool(GameObject gameObject)
        {
            gameObject.transform.position = transform.position;
            gameObject.GetComponent<AudioSource>().enabled = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Creates a new source object
        /// </summary>
        private GameObject CreateSource()
        {
            //creates game object
            GameObject obj = new();
            obj.transform.parent = transform;
            obj.name = "Audio Source " + (_audioSourcePool.Count + 1).ToString();

            //adds source
            AudioSource source = obj.AddComponent<AudioSource>();
            source.loop = false;
            source.outputAudioMixerGroup = _SFXMixerGroup;
            source.enabled = false;

            //adds to pool list
            obj.SetActive(false);
            _audioSourcePool.Add(obj);

            return obj;
        }

        #endregion

        #region Volume Input Simplification

        //volume enum -> value dict
        private readonly Dictionary<Volume, float> _volumeVals = new()
        {
            { Volume.Mute, 0.0f },
            { Volume.Quiet, 0.2f },
            { Volume.MediumSoft, 0.4f },
            { Volume.MediumLoud, 0.6f },
            { Volume.Loud, 0.8f },
            { Volume.Max, 1.0f }
        };

        public enum Volume
        {
            /// <summary> 0.0f </summary>
            Mute = 0,
            /// <summary> 0.2f </summary>
            Quiet = 1,
            /// <summary> 0.4f </summary>
            MediumSoft = 2,
            /// <summary> 0.6f </summary>
            MediumLoud = 3,
            /// <summary> 0.8f </summary>
            Loud = 4,
            /// <summary> 1f </summary>
            Max = 5,
        }
        #endregion
    }
}
