using TMPro;
using UnityEngine;

namespace Racerr.UX.UI
{
    public class IntermissionTimerUIComponent : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI intermissionTimerTMP;

        public void UpdateIntermissionTimer(int intermissionSecondsRemaining)
        {
            intermissionTimerTMP.text = intermissionSecondsRemaining.ToString();
        }
    }
}

