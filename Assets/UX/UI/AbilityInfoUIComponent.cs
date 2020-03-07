using Racerr.Gameplay.Ability;
using System;
using TMPro;
using UnityEngine;

public class AbilityInfoUIComponent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI displayNameTMP;
    [SerializeField] TextMeshProUGUI infoTMP;

    public void UpdateAbilityInfo(IAbility ability)
    {
        if (ability == null)
        {
            displayNameTMP.text = infoTMP.text = null;
            return;
        }

        displayNameTMP.text = ability.DisplayName;

        if (!ability.IsActive)
        {
            if (ability.CooldownRemaining > 0)
            {
                infoTMP.text = $"Cooldown: { ability.CooldownRemaining.ToString("N1") } seconds";
            }
            else if (ability.UsageRemaining == ability.MaximumUsage)
            {
                infoTMP.text = "<  SPACE  >";
            }
            else
            {
                infoTMP.text = $"{ Math.Round(ability.UsageRemaining) }/{ ability.MaximumUsage }";
            }
        }
        else
        {
            if (ability.UsageRemaining == ability.MaximumUsage)
            {
                infoTMP.text = "Activated";
            }
            else
            {
                infoTMP.text = $"{ Math.Round(ability.UsageRemaining) }/{ ability.MaximumUsage }";
            }
        }
    }
}
