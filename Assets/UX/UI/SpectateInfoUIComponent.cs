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

        int numSpectatablePlayers;

        /// <summary>
        /// Given the spectated player's name, display it.
        /// </summary>
        /// <param name="name">Spectated player's name as a string</param>
        public void UpdateSpectateInfo(string name, int numSpectatablePlayers)
        {
            spectatedPlayerNameTMP.text = name;
            this.numSpectatablePlayers = numSpectatablePlayers;
        }

        /// <summary>
        /// If there are more than 1 spectatable player, display the spacebar UI as we can
        /// change our current spectated player.
        /// </summary>
        public void UpdateSpaceUI()
        {
            if (numSpectatablePlayers <= 1)
            {
                spaceTMP.enabled = false;
            }
            else
            {
                spaceTMP.enabled = true;
            }
        }
    }
}
