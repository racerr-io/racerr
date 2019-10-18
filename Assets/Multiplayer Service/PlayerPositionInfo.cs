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
        public double FinishingTime { get; set; } = double.PositiveInfinity;
        public bool IsFinished => !double.IsPositiveInfinity(FinishingTime);

        /// <summary>
        /// Returns a properly formatted string (in M:SS.FFF format) showing their race length duration.
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
                    double playerRaceLength = FinishingTime - RaceSessionManager.Singleton.RaceStartTime;
                    return playerRaceLength.ToRaceTimeFormat();
                }
            }
        }
    }
}