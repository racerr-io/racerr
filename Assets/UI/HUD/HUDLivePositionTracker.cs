using Mirror;
using Racerr.MultiplayerService;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple text-based live position tracker for displaying race time and player ranks in race.
/// </summary>
public class HUDLivePositionTracker : NetworkBehaviour
{
    Text livePositionTrackerText;
    [SyncVar] string serverText;

    /// <summary>
    /// Grabs text component
    /// </summary>
    void Start()
    {
        livePositionTrackerText = GetComponentsInChildren<Text>().Single(t => t.name == "Live Position Tracker");
    }

    /// <summary>
    /// Update text component every frame with new position infos.
    /// </summary>
    void Update()
    {
        if (RacerrRaceSessionManager.Singleton.IsCurrentlyRacing)
        {
            // Calculate race positions text on server and sync to clients.
            if (isServer)
            {
                string text = string.Empty;
                int count = 1;

                foreach (Player player in RacerrRaceSessionManager.Singleton.PlayersInRaceOrdered)
                {
                    text += $"{count}. {player.PlayerName}";

                    if (player.PositionInfo.IsFinished)
                    {
                        text += $" ({player.PositionInfo.TimeString})";
                    }

                    text += "\n";
                    count++;
                }

                serverText = text; // Sync occurs here
            }

            // Calculate race timer on client to prevent gazillions of SyncVar updates every second.
            if (isClient)
            {
                float currentRaceLength = Time.time - RacerrRaceSessionManager.Singleton.RaceStartTime;
                livePositionTrackerText.text = currentRaceLength.ToRaceTimeFormat() + "\n" + serverText;
            }
        }
    }
}
