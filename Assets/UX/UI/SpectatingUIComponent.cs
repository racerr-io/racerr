using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    /// <summary>
    /// UI Component for the Spectating Player, which shows the spectated player's name.
    /// </summary>
    public class SpectatingUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI spectatedPlayerNameTMP;

        /// <summary>
        /// Given the spectated player's name, display it.
        /// </summary>
        /// <param name="name">Spectated player's name as a string</param>
        public void UpdateSpectatedPlayerName(string name)
        {
            spectatedPlayerNameTMP.text = "Spectating " + name;
        }
    }
}
