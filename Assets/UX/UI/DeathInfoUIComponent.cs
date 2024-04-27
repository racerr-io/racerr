using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component to show some information to the user that they died
    /// and who killed them.
    /// </summary>
    public class DeathInfoUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI destroyedByPlayerNameTMP;
        [SerializeField] TextMeshProUGUI revengeInstructionTMP;

        /// <summary>
        /// Updates the UI component with the person who killed the player (if not null)
        /// and whether it makes sense to show the instruction to the user that they can
        /// get revenge as a police.
        /// </summary>
        /// <param name="destroyedByPlayerName">Player name which killed us (nullable).</param>
        /// <param name="showRevengeInstruction">Whether we should show the revenge instruction.</param>
        public void UpdateDeathInfo(string destroyedByPlayerName, bool showRevengeInstruction)
        {
            if (string.IsNullOrEmpty(destroyedByPlayerName))
            {
                destroyedByPlayerNameTMP.enabled = false;
            }
            else
            {
                destroyedByPlayerNameTMP.text = "Wrecked by " + destroyedByPlayerName;
                destroyedByPlayerNameTMP.enabled = true;
            }

            revengeInstructionTMP.enabled = showRevengeInstruction;
        }
    }
}
