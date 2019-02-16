using Racerr.MultiplayerService;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Racerr.UX.Menu
{
    public class StartMenu : MonoBehaviour
    {
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

        public void ShowErrorMessage()
        {
            Text connectionInfoString = transform.Find("Connection Info String").GetComponent<Text>();
            connectionInfoString.text = "An error has occurred. Unable to connect to racerr.io game servers.";
        }

        public void HideMenu()
        {
            string usersName = GetComponentsInChildren<Text>().Single(t => t.name == "Name Text").text;
            Player.LocalPlayer.PlayerName = usersName;
            gameObject.SetActive(false);
            Player.LocalPlayer.IsReady = true;
        }
    }
}

