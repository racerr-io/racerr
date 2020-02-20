using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Spectating Player, which shows the spectated player's name.
    /// </summary>
    public class KillInfoUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI destroyedByPlayerNameTMP;
        [SerializeField] TextMeshProUGUI revengeInstructionTMP;

        /// <summary>
        /// Updates spectated player's name and displays the hint to press the
        /// SPACEBAR to swap players if there is more than one player in the race.
        /// </summary>
        /// <param name="destroyedByPlayerName">Spectated player's name</param>
        public void UpdateKillInfo(string destroyedByPlayerName, bool showRevengeInstruction)
        {
            if (string.IsNullOrEmpty(destroyedByPlayerName))
            {
                destroyedByPlayerNameTMP.enabled = false;
            }
            else
            {
                destroyedByPlayerNameTMP.text = "By " + destroyedByPlayerName;
                destroyedByPlayerNameTMP.enabled = true;
            }

            revengeInstructionTMP.enabled = showRevengeInstruction;
        }
    }
}
