using System.Collections.Generic;
using UnityEngine;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Simple class for holding Player position info in a race.
    /// </summary>
    public readonly struct PlayerPositionInfo
    {
        // Syncronished Fields
        public readonly double startTime;
        public readonly double finishTime;

        // Server Only Properties
        public bool IsFinished => !double.IsPositiveInfinity(finishTime);
        public HashSet<GameObject> Checkpoints { get; }

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
                    double playerRaceLength = finishTime - startTime;
                    return playerRaceLength.ToRaceTimeFormat();
                }
            }
        }

        public PlayerPositionInfo(double startTime, double finishTime = double.PositiveInfinity)
        {
            this.Checkpoints = new HashSet<GameObject>();
            this.startTime = startTime;
            this.finishTime = finishTime;
        }
    }
}