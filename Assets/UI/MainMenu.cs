using System.Linq;
using Racerr.MultiplayerService;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour  
{
    const string defaultPlayerName = "Player";

    public void OnStartRaceButtonClick()
    {
        string playerName = GetComponentsInChildren<TMP_InputField>().Single(t => t.name == "Player Name Text").text;
        Player.LocalPlayer.PlayerName = string.IsNullOrWhiteSpace(playerName) ? defaultPlayerName : playerName;
        Player.LocalPlayer.IsReady = true;
    }
}
