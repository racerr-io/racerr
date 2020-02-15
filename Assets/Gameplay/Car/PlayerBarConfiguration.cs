using Racerr.UX.Camera;
using Racerr.UX.Car;
using UnityEngine;

namespace Racerr.Gameplay.Car
{
    /// <summary>
    /// Each car has different configurations for the PlayerBar
    /// because the size of cars might be different, and also
    /// due to different camera types.
    /// Attach this script to each car with a configuration
    /// for each possible CameraType. Upon the CamType changing
    /// in the PrimaryCamera, the appropriate configuration will be
    /// applied.
    /// </summary>
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

        /// <summary>
        /// Copies the values of the configuration to the
        /// actual PlayerBar on the car.
        /// </summary>
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