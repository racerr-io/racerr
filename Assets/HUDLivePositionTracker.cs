using Mirror;
using Racerr.MultiplayerService;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Racerr.MultiplayerService.RacerrRaceSessionManager;

[RequireComponent(typeof(Text))]
public class HUDLivePositionTracker : NetworkBehaviour
{
    Text livePositionTrackerText;
    [SyncVar(hook = "OnTextChange")] string text;

    /// <summary>
    /// Grabs text component
    /// </summary>
    void Start()
    {
        livePositionTrackerText = GetComponent<Text>();
    }

    void OnTextChange(string text)
    {
        livePositionTrackerText.text = text;
    }

    /// <summary>
    /// Update text component every frame with new position infos.
    /// </summary>
    void Update()
    {
        if (RacerrRaceSessionManager.Singleton.IsCurrentlyRacing && isServer)
        {
            text = "racerr.io\n";
            int count = 1;
            IEnumerable<KeyValuePair<Player, PositionInfo>> racePositions = RacerrRaceSessionManager.Singleton.PlayerOrderedPositions;
            foreach (KeyValuePair<Player, PositionInfo> racePosition in racePositions)
            {
                Player player = racePosition.Key;
                PositionInfo posInfo = racePosition.Value;
                text += $"{count}. {player.PlayerName}\n";
                count++;
            }
            livePositionTrackerText.text = text;
        }
    }
}
