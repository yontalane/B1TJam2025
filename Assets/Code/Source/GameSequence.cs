using UnityEngine;

namespace B1TJam2025
{
    [System.Serializable]
    public struct GameSequenceSegment
    {
        // Common

        [Header("Common")]

        [Tooltip("Is this segment of the game scripted or random?")]
        public GameSequenceSegmentType type;

        [Tooltip("Dialog with Beat Buddy announcing this crime.")]
        public Conversation beatBuddyAPB;

        // Scripted

        [Header("Settings for Scripted Only")]

        [Tooltip("Name of GameObjects in the city map that defines where the perps should spawn.")]
        public string[] spawnTargets;

        [Tooltip("The perps to spawn.")]
        public PerpWrapper[] perps;

        //[Tooltip("Does the player need to beat the perp in this sequence before we activate the next segment?")]
        //public bool beatBeforeContinuing;

        [Tooltip("Beat Cop's soliloquy after making the arrest.")]
        public Conversation victorySoliloquy;

        // Random

        [Header("Settings for Random Only")]

        [Tooltip("How many randomly spawned perps does the player need to beat before proceeding to the next segment?")]
        public int randomCount;

        [Tooltip("Beat Cop's soliloquy after making the arrest.")]
        public Conversation[] randomVictorySoliloquys;
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
