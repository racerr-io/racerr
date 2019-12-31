using Racerr.Infrastructure.Server;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for Leaderboard, a list which shows all the alive and dead players in the race, their ranking and
    /// their time if they haven't died.
    /// </summary>
    public class LeaderboardUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI leaderboardTMP;

        /// <summary>
        /// Given a list of leaderboard items, which is send by the server, update the text on the leaderboard to show
        /// details of all players in this form: "RANK. NAME (TIME or 'DNF')", and sorted by rank. Note that the 
        /// leaderboard is just a single text component. 
        /// </summary>
        /// <param name="leaderboardItems">A unmodifiable collection of leaderboard item structs</param>
        public void UpdateLeaderboard(IReadOnlyCollection<RaceSessionState.PlayerLeaderboardItemDTO> leaderboardItems)
        {
            string leaderboardText = string.Empty;

            foreach (RaceSessionState.PlayerLeaderboardItemDTO leaderboardItem in leaderboardItems)
            {
                leaderboardText += $"{leaderboardItem.position}. {leaderboardItem.playerName}";

                if (leaderboardItem.timeString != null)
                {
                    leaderboardText += $" ({leaderboardItem.timeString})";
                }

                leaderboardText += "\n";
            }

            leaderboardTMP.text = leaderboardText;
        }
    }
}

