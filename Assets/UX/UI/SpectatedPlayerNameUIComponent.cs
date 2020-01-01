using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Spectating Player, which shows the spectated player's name.
    /// </summary>
    public class SpectatedPlayerNameUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI SpectatedPlayerNameTMP;

        /// <summary>
        /// Given the spectated player's name, display it.
        /// </summary>
        /// <param name="name">Spectated player's name as a string</param>
        public void UpdateSpectatedPlayerName(string name)
        {
            SpectatedPlayerNameTMP.text = "Spectating " + name;
        }
    }
}
