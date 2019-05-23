using System.Collections.Generic;
using UnityEngine;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Simple class for holding Player position info in a race.
    /// </summary>
    public class PlayerPositionInfo
    {
        public HashSet<GameObject> Checkpoints { get; } = new HashSet<GameObject>();
        public float FinishingTime { get; private set; } = float.PositiveInfinity;
        public bool IsFinished => !float.IsPositiveInfinity(FinishingTime);

        /// <summary>
        /// Returns a properly formatted string showing their race length duration.
        /// </summary>
        public string TimeString
        {
            get
            {
                if (!IsFinished)
                {
                    return "DNF";
                }
                else
                {
                    float playerRaceLength = FinishingTime - RacerrRaceSessionManager.Singleton.RaceStartTime;
                    return playerRaceLength.ToRaceTimeFormat();
                }
            }
        }

        /// <summary>
        /// When player finishes the race, call this to mark their finishing time.
        /// </summary>
        public void MarkAsFinished()
        {
            FinishingTime = Time.time;
        }
    }
}