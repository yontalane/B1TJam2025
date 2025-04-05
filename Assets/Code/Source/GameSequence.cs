using UnityEngine;

namespace B1TJam2025
{
    [System.Serializable]
    public struct GameSequenceSegment
    {
        // Type

        [Tooltip("Is this segment of the game scripted or random?")]
        public GameSequenceSegmentType type;

        // Scripted

        [Tooltip("Name of a GameObject in the city map that defines where the perp should spawn.")]
        public string spawnTarget;

        [Tooltip("The perp to spawn.")]
        public PerpWrapper perp;

        [Tooltip("Does the player need to beat the perp in this sequence before we activate the next segment?")]
        public bool beatBeforeContinuing;

        // Random

        [Tooltip("How many randomly spawned perps does the player need to beat before proceeding to the next segment?")]
        public int randomCount;
    }

    public enum GameSequenceSegmentType
    {
        Scripted = 0,
        Random = 1,
    }

    [CreateAssetMenu(fileName ="Sequence", menuName ="B1TJam2025/GameSequence")]
    public class GameSequence : ScriptableObject
    {
        [Min(1)]
        public int escapeCountToLose;

        [Space]

        public GameSequenceSegment[] segments;
    }
}
