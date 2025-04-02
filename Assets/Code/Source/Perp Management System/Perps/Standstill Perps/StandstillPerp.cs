using UnityEngine;

namespace B1TJam2025
{
    /// <summary>
    /// Sample Subclass of PerpBase. Models a "Standstill Perp" meaning a perp that stands still.
    /// </summary>
    [CreateAssetMenu(fileName = "StandstillPerp", menuName = "Scriptable Objects/Perps/StandstillPerp")]
    public class StandstillPerp : PerpBase
    {
        public override GameObject SpawnPerp()
        {
            //can extend SpawnPerp for sending information to other systems such as the dialouge system
            return base.SpawnPerp();
        }
    }
}
