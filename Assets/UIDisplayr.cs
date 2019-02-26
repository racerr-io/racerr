using Racerr.MultiplayerService;
using TMPro;
using UnityEngine;

public class UIDisplayr : MonoBehaviour
{
    Transform panel;
    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        panel = transform.Find("Panel");
    }
    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (Player.LocalPlayer?.Car != null)
        {
            panel.forward = Camera.main.transform.forward;
            transform.position = Player.LocalPlayer.Car.transform.position + new Vector3(0, 10, 0);
        }
    }
}
