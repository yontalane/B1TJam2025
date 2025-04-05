using System;
using System.Collections.Generic;
using UnityEngine;

namespace B1TJam2025
{
    [Serializable]
    public class SFXRandomSelectSet
    {
        //serializable set
        [SerializeField] public List<AudioClip> Clips = new();

        //private collections that manage clip selection
        private List<AudioClip> _activeClips = null;
        private readonly Queue<AudioClip> _disabledClips = new();

        /// <summary>
        /// Returns a random sound effect from the set.
        /// will not select the same sound effect consecutivly. (unless only one is available)
        /// </summary>
        /// <returns> selected audio clip </returns>
        public AudioClip GetRandomSelection()
        {
            if(_activeClips == null) InitializeFromSerialized();

            AudioClip selectedClip = _activeClips[UnityEngine.Random.Range(0, _activeClips.Count)];
            //prevents selecting same clip twice in a row if more than one clip is available
            if (_activeClips.Count + _disabledClips.Count > 1)
            {
                if(_disabledClips.Count > 0) _activeClips.Add(_disabledClips.Dequeue()); //adds inactive clip back to selectable set
                _disabledClips.Enqueue(selectedClip); //queue's selected clip
            }

            return selectedClip;
        }

        /// <summary>
        /// Add more sound effects to the set
        /// </summary>
        public void AddAudioClips(AudioClip clip, params AudioClip[] clips)
        {
            if (_activeClips == null) InitializeFromSerialized();

            _activeClips.Add(clip);
            if(clips != null) _activeClips.AddRange(clips);
        }

        /// <summary>
        /// Remove all sound effects to the set
        /// </summary>
        public void ClearAudioClips()
        {
            if (_activeClips == null) _activeClips = new();

            _activeClips.Clear();
            _disabledClips.Clear();
        }

        /// <summary>
        /// Loads the serialized clips into the active selection sets
        /// </summary>
        private void InitializeFromSerialized()
        {
            _activeClips = new();

            _activeClips.Clear();
            _disabledClips.Clear();
            _activeClips.AddRange(Clips);
        }
    }
}
