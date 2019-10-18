using System.Linq;
using Racerr.MultiplayerService;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour  
{
    const string defaultPlayerName = "Player";

    public void OnStartRaceButtonClick()
    {
        string playerName = GetComponentsInChildren<TMP_Text>().Single(t => t.name == "Player Name Text").text;
        Debug.Log("Fail comp4128: '" + playerName + "': " + string.IsNullOrWhiteSpace(playerName));
        Player.LocalPlayer.PlayerName = (string.IsNullOrWhiteSpace(playerName) || playerName == "") ? defaultPlayerName : playerName;
       
        Player.LocalPlayer.IsReady = true;
    }
}
