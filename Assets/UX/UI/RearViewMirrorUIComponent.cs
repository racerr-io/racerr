using Racerr.UX.Camera;
using UnityEngine;

public class RearViewMirrorUIComponent : MonoBehaviour
{
    [SerializeField] Camera rearViewCamera;
    PrimaryCamera primaryCamera;

    void Start()
    {
        primaryCamera = FindObjectOfType<PrimaryCamera>();
    }

    public void UpdateRearViewMirror(PrimaryCamera.CameraType cameraType)
    {
        // Move rear view camera to the location of the primary 3rd person camera...
        rearViewCamera.transform.position = primaryCamera.transform.position + new Vector3(0, 3, 0); // Move up and a little forward
        rearViewCamera.transform.rotation = primaryCamera.transform.rotation * Quaternion.Euler(0, 180, 0); // Rotate to face rear of car

        if (cameraType == PrimaryCamera.CameraType.Overhead)
        {
            gameObject.SetActive(false);
        }
        else if (cameraType == PrimaryCamera.CameraType.ThirdPerson)
        {
            gameObject.SetActive(true);
        }
    }
}
