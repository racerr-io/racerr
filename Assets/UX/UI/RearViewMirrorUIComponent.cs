using Racerr.UX.Camera;
using UnityEngine;

/// <summary>
/// A mirror which shows what is behind the player's car.
/// </summary>
public class RearViewMirrorUIComponent : MonoBehaviour
{
    [SerializeField] Camera rearViewCamera;

    /// <summary>
    /// Given the current state of the primary camera,
    /// moves the rear view camera close to it to display the
    /// cars behind the player's car in the mirror.
    /// Only display the rear view mirror in third person mode.
    /// </summary>
    /// <param name="primaryCamera">The primary camera.</param>
    public void UpdateRearViewMirror(PrimaryCamera primaryCamera)
    {
        if (primaryCamera.CamType == PrimaryCamera.CameraType.ThirdPerson)
        {
            // Move rear view camera to the location of the primary 3rd person camera...
            rearViewCamera.transform.position = primaryCamera.transform.position + new Vector3(0, 3, 0); // Move up and a little forward
            rearViewCamera.transform.rotation = primaryCamera.transform.rotation * Quaternion.Euler(0, 180, 0); // Rotate to face rear of car
            gameObject.SetActive(true);
        } 
        else
        {
            gameObject.SetActive(false);
        }
    }
}
