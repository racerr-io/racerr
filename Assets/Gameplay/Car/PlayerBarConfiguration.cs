using Racerr.UX.Camera;
using Racerr.UX.Car;
using UnityEngine;

namespace Racerr.Gameplay.Car
{
    [RequireComponent(typeof(CarManager))]
    public sealed class PlayerBarConfiguration : MonoBehaviour
    {
        [SerializeField] PrimaryCamera.CameraType cameraType;
        [SerializeField] float playerBarXDisplacement;
        [SerializeField] float playerBarXDisplacementDrivingDown;
        [SerializeField] float playerBarYDisplacement;
        [SerializeField] float playerBarScale;

        CarManager carManager;
        public PrimaryCamera.CameraType CameraType => cameraType;

        void Start()
        {
            carManager = GetComponent<CarManager>();
        }

        public void ApplyConfiguration()
        {
            PlayerBar playerBar = carManager.PlayerBar;
            if (playerBar != null)
            {
                playerBar.XDisplacement = playerBarXDisplacement;
                playerBar.XDisplacementDrivingDown = playerBarXDisplacementDrivingDown;
                playerBar.YDisplacement = playerBarYDisplacement;
                playerBar.Scale = playerBarScale;
            }
        }
    }
}