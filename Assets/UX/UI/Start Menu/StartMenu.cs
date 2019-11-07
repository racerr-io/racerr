using Racerr.Infrastructure.Client;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Racerr.UX.StartMenu
{
    /// <summary>
    /// Start menu interface for game. First thing the user sees.
    /// </summary>
    public class StartMenu : MonoBehaviour
    {
        /// <summary>
        /// Show main menu screen to user following successful connection to Racerr game servers.
        /// </summary>
        public void ShowMenu()
        {
            GameObject connectionInfoGO = transform.Find("Connection Info String").gameObject;
            connectionInfoGO.SetActive(false);

            GameObject titleGO = transform.Find("racerr.io Title").gameObject;
            titleGO.SetActive(true);

            GameObject nameButtonGO = transform.Find("Name Input").gameObject;
            nameButtonGO.SetActive(true);

            GameObject raceButtonGO = transform.Find("Race Button").gameObject;
            raceButtonGO.SetActive(true);
        }

        /// <summary>
        /// Show error message screen to user following unsuccessful connection to Racerr game servers.
        /// </summary>
        public void ShowErrorMessage()
        {
            Text connectionInfoString = transform.Find("Connection Info String").GetComponent<Text>();
            connectionInfoString.text = "An error has occurred. Unable to connect to racerr.io game servers.";
        }

        /// <summary>
        /// Hide the menu interface and mark the player as ready to race.
        /// </summary>
        public void HideMenu()
        {
            string usersName = GetComponentsInChildren<Text>().Single(t => t.name == "Name Text").text;

            if (!string.IsNullOrWhiteSpace(usersName))
            {
                ClientStateMachine.Singleton.LocalPlayer.PlayerName = usersName;
            }
            else
            {
                ClientStateMachine.Singleton.LocalPlayer.PlayerName = "Player";
            }
            
            gameObject.SetActive(false);
            ClientStateMachine.Singleton.LocalPlayer.IsReady = true;
        }
    }
}

