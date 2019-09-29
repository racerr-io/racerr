using Mirror;
using Racerr.MultiplayerService;
using System.Linq;
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
        // Calculate race positions text on server and sync to clients.
        if (isServer)
        {
            string text = string.Empty;
            int count = 1;

            foreach (Player player in RacerrRaceSessionManager.Singleton.PlayersInRaceOrdered)
            {
                text += $"{count}. {player.PlayerName}";

                if (player.IsDead || player.PositionInfo.IsFinished)
                {
                    text += $" ({player.PositionInfo.TimeString})";
                }

                text += "\n";
                count++;
            }
            
            if (serverText != text)
            {
                serverText = text; // Sync occurs here. Sync only occurs if the text itself has changed.
            }
        }

        // Only show the timer when player is ready (i.e. clicked "Race!")
        if (isClient && Player.LocalPlayer != null && Player.LocalPlayer.IsReady)
        {
            if (RacerrRaceSessionManager.Singleton.IsCurrentlyRacing)
            {
                // Calculate the remaining time before the race must end.
                double CountDownTimer = 180f - RacerrRaceSessionManager.Singleton.RaceLength;
                // Check if the player has finished and the timer is still over the threshold.
                if (CountDownTimer > RacerrRaceSessionManager.Singleton.CountdownTimerThreshold/*&& Player is finished*/)
                {
                    CountDownTimer = RacerrRaceSessionManager.Singleton.CountdownTimerThreshold;
                }
                else if (CountDownTimer <= 0) RacerrRaceSessionManager.Singleton.EndRace(); 

                // Calculate race timer on client to prevent gazillions of SyncVar updates every second.
                livePositionTrackerText.text = RacerrRaceSessionManager.Singleton.RaceLength.ToRaceTimeFormat() + "\n" + CountDownTimer.ToRaceTimeFormat() + "\n" + serverText;
            }
            else
            {
                livePositionTrackerText.text = "Intermission\n" + serverText;
            } 
        }
    }
}
