using Racerr.Infrastructure.Server;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    public class LeaderboardUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI leaderboardTMP;

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

