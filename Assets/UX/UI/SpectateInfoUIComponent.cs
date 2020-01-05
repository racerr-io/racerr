using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Spectating Player, which shows the spectated player's name.
    /// </summary>
    public class SpectateInfoUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI spectatedPlayerNameTMP;
        [SerializeField] TextMeshProUGUI spaceTMP;

        /// <summary>
        /// Updates spectated player's name and displays the hint to press the
        /// SPACEBAR to swap players if there is more than one player in the race.
        /// </summary>
        /// <param name="name">Spectated player's name</param>
        /// <param name="numSpectatablePlayers">Number of players in race.</param>
        public void UpdateSpectateInfo(string name, int numSpectatablePlayers)
        {
            spectatedPlayerNameTMP.text = name;
            spaceTMP.enabled = numSpectatablePlayers > 1;
        }
    }
}
