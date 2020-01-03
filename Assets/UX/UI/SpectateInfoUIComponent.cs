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
        /// Updates spectated player's name and number of spectatable players.
        /// </summary>
        /// <param name="name">Spectated player's name</param>
        /// <param name="numSpectatablePlayers"></param>
        public void UpdateSpectateInfo(string name, int numSpectatablePlayers)
        {
            spectatedPlayerNameTMP.text = name;
            spaceTMP.enabled = numSpectatablePlayers > 1;
        }
    }
}
