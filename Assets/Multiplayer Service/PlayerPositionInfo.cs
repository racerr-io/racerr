using System.Collections.Generic;
using UnityEngine;

namespace Racerr.MultiplayerService
{
    /// <summary>
    /// Holds Player Position Info so we can determine a player's status in the race.
    /// Some properties are synced with the client so it can determine how to transition the UI.
    /// E.g. if the client discovers they've finished, they should show the spectate UI.
    /// Some properties such as the Checkpoints are not synced as they are used solely by the server.
    /// </summary>
    public readonly struct PlayerPositionInfo
    {
        /* Server and Client Properties */
        public readonly double startTime;
        public readonly double finishTime;

        public bool IsFinished => !double.IsPositiveInfinity(finishTime);

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

        /* Server Only Properties */
        public HashSet<GameObject> Checkpoints { get; }

        public PlayerPositionInfo(double startTime, double finishTime = double.PositiveInfinity)
        {
            this.Checkpoints = new HashSet<GameObject>();
            this.startTime = startTime;
            this.finishTime = finishTime;
        }
    }
}