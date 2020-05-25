using NWH.WheelController3D;
using Racerr.Infrastructure;
using Racerr.Infrastructure.Server;
using Racerr.Utility;
using Racerr.World.Track;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.Gameplay.Car
{
    /// <summary>
    /// Become handsfree and drive the car automatically!
    /// As a racer, follows the path of checkpoints to finish the race.
    /// As a police, follows the path of checkpoints to the racer in 1st place.
    /// </summary>
    [RequireComponent(typeof(CarManager))]
    [RequireComponent(typeof(CarPhysicsManager))]
    public class AIInputManager : MonoBehaviour
    {
        [SerializeField] WheelController wheelFL;
        [SerializeField] WheelController wheelFR;

        const int crashWaitingSeconds = 4;
        const int crashMaxSpeed = 5;
        const int policeTargetFirstPlaceDistance = 50;
        const int closeToCheckpointDistance = 10;
        const float respawnStartHeight = 0.36f;

        ServerRaceState serverRaceState;
        CarManager car;
        bool crashCoroutineActive = false;
        Transform[] currentPath;

        /// <summary>
        /// Cache the car and server race state.
        /// </summary>
        void Awake()
        {
            car = GetComponent<CarManager>();
            serverRaceState = FindObjectOfType<ServerRaceState>();
        }

        /// <summary>
        /// Called every physics tick to perform all automatically driving functions
        /// when the race is active.
        /// </summary>
        void FixedUpdate()
        {
            if (ServerStateMachine.Singleton.StateType == StateEnum.Race)
            {
                Drive();
                Steer();
                RespawnIfCrashed();
            }
        }

        /// <summary>
        /// Drive the car by stepping on the pedal all the way.
        /// Very primitive but works for now.
        /// </summary>
        void Drive()
        {
            car.Physics.InputVertical = 1;
        }

        /// <summary>
        /// Calculate the next target and drive to it on the correct route.
        /// As a racer, the target is always the finish line, we will just follow the route.
        /// As a police, the target is the 1st place player, we will follow the route to the closest checkpoint to the 1st place player,
        /// then attempt to crash into them when we are close.
        /// </summary>
        void Steer()
        {
            Player firstPlacePlayer = serverRaceState.GetFirstPlaceAliveRacer();
            if (firstPlacePlayer == null || firstPlacePlayer.Car == null)
            {
                // This is possible in the small gap between race finish and server state machine transition.
                return;
            }

            Vector3 targetPosition = serverRaceState.GetFirstPlaceAliveRacer().Car.transform.position;
            if (car.CarType != CarManager.CarTypeEnum.Police || Vector3.Distance(transform.position, targetPosition) > policeTargetFirstPlaceDistance)
            {
                if (currentPath == null)
                {
                    // This is possible when the race has started initially and the player hasn't passed through any checkpoint.
                    GeneratePath();
                }

                if (currentPath.Length > 1 && Vector3.Distance(transform.position, currentPath[0].position) < closeToCheckpointDistance)
                {
                    // If we are really close to the first waypoint it is more natural to target the second waypoint.
                    targetPosition = currentPath[1].position;
                }
                else if (currentPath.Length > 0)
                {
                    // Standard case.
                    targetPosition = currentPath[0].position;
                }
                else
                {
                    // If path is empty it means we must be really close or at the destination, so don't bother steering.
                    return;
                }
            }

            // Makes the two front wheels point to the target position. 
            // See https://www.youtube.com/watch?v=DIbIGfRbWn4&t=819s for maths explanation.
            Vector3 relativeVector = transform.InverseTransformPoint(targetPosition);
            float newSteer = relativeVector.x / relativeVector.magnitude * car.Physics.LowSpeedSteerAngle;
            wheelFL.steerAngle = wheelFR.steerAngle = newSteer;
        }

        /// <summary>
        /// If the car is "crashed", e.g. it is not moving for a while, then respawn the car at the last
        /// visited checkpoint. A performance optimisation is performed to ensure there is only one coroutine running at a time.
        /// </summary>
        void RespawnIfCrashed()
        {
            if (car.Physics.SpeedKPH < crashMaxSpeed && !car.IsZombie && !crashCoroutineActive)
            {
                crashCoroutineActive = true;
                this.YieldThenExecuteAsync(new WaitForSeconds(crashWaitingSeconds), () =>
                {
                    if (car.Physics.SpeedKPH < crashMaxSpeed && !car.IsZombie)
                    {
                        car.SetInvulnerableTemporarily();
                        GeneratePath();
                        transform.position = new Vector3(currentPath[0].position.x, respawnStartHeight, currentPath[0].position.z);

                        if (currentPath.Length > 1)
                        {
                            transform.LookAt(currentPath[1]);
                        }
                    }

                    crashCoroutineActive = false;
                });
            }
        }

        /// <summary>
        /// As a racer, generates path to the finish line. As a police, generates path to the 1st place player.
        /// Path is constructed via the checkpoints in the race. 
        /// NOTICE: This function is called by TrackPieceCheckpointDetector when car passes through checkpoints as a performance optimisation,
        /// to update the path periodically.
        /// </summary>
        /// <param name="excludeCheckpoints">Array of checkpoints to exclude from the path, e.g. if we are passing through a checkpoint, 
        /// it would make sense to exclude the checkpoint we passed through.</param>
        public void GeneratePath(params Transform[] excludeCheckpoints)
        {
            GameObject[] checkpointsInRace = TrackGenerator.Singleton.CheckpointsInRace;
            if (checkpointsInRace == null)
            {
                // Just to be safe if the race has finished.
                return;
            }

            Transform endCheckpoint;
            if (car.CarType == CarManager.CarTypeEnum.Racer)
            {
                // Find the finish line checkpoint.
                endCheckpoint = checkpointsInRace[checkpointsInRace.Length - 1].transform;
            }
            else
            {
                // Find the closest checkpoint to the first place player's car.
                Player firstPlacePlayer = serverRaceState.GetFirstPlaceAliveRacer();
                if (firstPlacePlayer == null || firstPlacePlayer.Car == null)
                {
                    // This is possible in the small gap between race finish and server state machine transition.
                    return;
                }

                Transform firstPlaceCarTransform = firstPlacePlayer.Car.transform;
                endCheckpoint = checkpointsInRace.OrderBy(checkpoint => Vector3.Distance(firstPlaceCarTransform.position, checkpoint.transform.position)).First().transform;
            }

            List<Transform> newPath = new List<Transform>();

            // Find the closest checkpoint to our current location.
            Transform startCheckpoint = checkpointsInRace.OrderBy(checkpoint => Vector3.Distance(transform.position, checkpoint.transform.position)).First().transform;

            // Construct path from startCheckpoint to endCheckpoint.
            // Checkpoints in race is already in the correct order, so it is just a matter of grabbing the correct subarray in the correct order.
            for (int i = 0; i < checkpointsInRace.Length; i++)
            {
                Transform currCheckpoint = checkpointsInRace[i].transform;
                if (currCheckpoint == startCheckpoint)
                {
                    for (int j = i; j < checkpointsInRace.Length && checkpointsInRace[j].transform != endCheckpoint; j++)
                    {
                        newPath.Add(checkpointsInRace[j].transform);
                    }

                    newPath.Add(endCheckpoint);
                    break;
                } 
                else if (currCheckpoint == endCheckpoint)
                {
                    for (int j = i; j < checkpointsInRace.Length && checkpointsInRace[j].transform != startCheckpoint; j++)
                    {
                        newPath.Add(checkpointsInRace[j].transform);
                    }

                    newPath.Add(startCheckpoint);
                    newPath.Reverse(); // Reverse the path so that it will be start -> end.
                    break;
                }
            }

            currentPath = newPath.Except(excludeCheckpoints).ToArray();
        }
    }
}