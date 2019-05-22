using Mirror;
using Racerr.MultiplayerService;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using static Racerr.MultiplayerService.RacerrRaceSessionManager;

public class HUDLivePositionTracker : NetworkBehaviour
{
    Text livePositionTrackerText;
    [SyncVar(hook = "OnTextChange")] string text;

    /// <summary>
    /// Grabs text component
    /// </summary>
    void Start()
    {
        livePositionTrackerText = GetComponentsInChildren<Text>().Single(t => t.name == "Live Position Tracker");
    }

    void OnTextChange(string text)
    {
        if (RacerrRaceSessionManager.Singleton.IsCurrentlyRacing)
        {
            livePositionTrackerText.text = text;
        }
        else
        {
            livePositionTrackerText.text = string.Empty;
        }
    }

    /// <summary>
    /// Update text component every frame with new position infos.
    /// </summary>
    void Update()
    {
        if (isServer && RacerrRaceSessionManager.Singleton.IsCurrentlyRacing)
        {
            text = "racerr.io\n";
            int count = 1;
            IEnumerable<KeyValuePair<Player, PositionInfo>> racePositions = RacerrRaceSessionManager.Singleton.PlayerOrderedPositions;

            foreach (KeyValuePair<Player, PositionInfo> racePosition in racePositions)
            {
                Player player = racePosition.Key;
                PositionInfo posInfo = racePosition.Value;
                text += $"{count}. {player.PlayerName}";

                if (posInfo.IsFinished)
                {
                    text += " (F)";
                }

                text += "\n";
                count++;
            }

            livePositionTrackerText.text = text;
        }
    }
}
