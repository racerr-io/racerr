using Racerr.Gameplay.Car;
using Racerr.Utility;
using Racerr.World.Track;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.Infrastructure.Server
{
    /// <summary>
    /// Manages spawning of all types of cars.
    /// WARNING: This class is only accessible on the server.
    /// </summary>
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Singleton;

        [SerializeField] GameObject raceCarPrefab;
        [SerializeField] GameObject policeCarPrefab;
        [SerializeField] Vector3 firstCarBaseLineDisplacement = new Vector3(4.5f, 0.1f, -15);
        [SerializeField] Vector3 verticalDistanceBetweenCars = new Vector3(0, 0, 5);
        [SerializeField] Vector3 horizontalDistanceBetweenCars = new Vector3(9, 0, 0);

        // Keep track of the police cars spawned that have not left the finishing track piece
        // to ensure that we do not spawn two police cars in the same position.
        readonly HashSet<Player> policeCarsOnFinishingGrid = new HashSet<Player>();

        /// <summary>
        /// Run when this script is instantiated.
        /// Set up the Singleton variable and ensure only one spawn manager is created
        /// in the scene.
        /// </summary>
        void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                throw new InvalidOperationException("You can only have one spawn manager in the scene.");
            }
        }

        /// <summary>
        /// Spawns race cars on a given track piece in starting grid formation
        /// (https://www.motorsport.com/f1/photos/the-starting-grid-drivers-get-ready-for-the-pace-lap-4897739/4897739/).
        /// Note that this does not check whether all the given players will actually fit
        /// on the track. Player limits are set in Server Manager.
        /// </summary>
        /// <param name="startingTrackPiece">The track piece representing the first track.</param>
        /// <param name="playersToSpawn">The players you wish to spawn cars for.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        public IEnumerator SpawnAllRaceCarsOnStartingGrid(GameObject startingTrackPiece, IEnumerable<Player> playersToSpawn)
        {
            SentrySdk.AddBreadcrumb("Spawning players onto track.");

            Transform startLine = startingTrackPiece.transform.Find(GameObjectIdentifiers.StartLine);
            if (startLine == null)
            {
                throw new MissingComponentException($"Starting Track Piece must have a GameObject named { GameObjectIdentifiers.StartLine } which marks the starting line.");
            }

            int spawnedPlayers = 0;
            foreach (Player player in playersToSpawn.Where(player => player != null))
            {
                player.CreateCarForPlayer(
                    CalculateGridPosition(startLine.position, spawnedPlayers),
                    startingTrackPiece.transform.rotation, 
                    raceCarPrefab,
                    CarManager.CarTypeEnum.Racer);
                spawnedPlayers++;
                yield return new WaitForFixedUpdate();
            }

            policeCarsOnFinishingGrid.Clear();
        }

        /// <summary>
        /// Spawn the police car for a given player.
        /// We will spawn the police car at the finish line, so they can get revenge on the racers.
        /// </summary>
        /// <remarks>
        /// We assume that by default the police car prefab's vehicle controller is set to active, so
        /// the car is immediately driveable after it is spawned (unlike the other cars, which are by default
        /// set to inactive and are activated when transitioning into Server Race State).
        /// </remarks>
        /// <param name="player">The player you wish to spawn the police car for.</param>
        public void SpawnPoliceCarOnFinishingGrid(Player player)
        {
            SentrySdk.AddBreadcrumb("Spawning police car onto track.");

            // Grab finish line and add slight offset to finish line so car is not spawned inside the ground.
            // Need smarter way of spawn police cars, could spawn multiple cars onto the same spot...
            TrackGenerator.SyncListGameObject generatedTrackPiecesInRace = TrackGenerator.Singleton.GeneratedTrackPieces;
            GameObject finishingTrackPiece = generatedTrackPiecesInRace[generatedTrackPiecesInRace.Count - 1];
            Transform finishLine = finishingTrackPiece.transform.Find(GameObjectIdentifiers.FinishLine);
            if (finishLine == null)
            {
                throw new MissingComponentException($"Finishing Track Piece must have a GameObject named { GameObjectIdentifiers.FinishLine } which marks the finishing line.");
            }

            // Apply the same rotation from the finishing track piece + flipping 180 degrees to the vectors that were 
            // originally used to calculate the position of the car in the starting track piece so we can spawn the 
            // police cars in the same position as if we were spawning the race cars on the starting track piece but
            // facing away from the finish line.
            player.CreateCarForPlayer(
                CalculateGridPosition(finishLine.position, policeCarsOnFinishingGrid.Count), 
                finishingTrackPiece.transform.rotation * Quaternion.Euler(0, 180, 0), 
                policeCarPrefab,
                CarManager.CarTypeEnum.Police);
            policeCarsOnFinishingGrid.Add(player);
        }

        /// <summary>
        /// Helper function allowing you to calculate the position of the car
        /// on a starting/finishing grid given the starting/finishing line
        /// and the car position.
        /// </summary>
        /// <param name="baseLine">Starting/finishing line position.</param>
        /// <param name="carPosNo">Car position number.</param>
        /// <returns>Grid position</returns>
        Vector3 CalculateGridPosition(Vector3 baseLine, int carPosNo)
        {
            Vector3 result = baseLine + firstCarBaseLineDisplacement;
            for (int curCarPosNo = 0; curCarPosNo < carPosNo; curCarPosNo++)
            {
                result -= verticalDistanceBetweenCars + horizontalDistanceBetweenCars * LanguageExtensions.FastPow(-1, curCarPosNo);
            }
            return result;
        }

        /// <summary>
        /// Used to notify spawn manager when the police car has left the
        /// finishing grid area so that it can free up a spot.
        /// Note: can be called repeatedly and at any time, 
        /// even when police car has left the finishing grid.
        /// </summary>
        /// <param name="player">The player that left the finishing grid area.</param>
        public void NotifyPlayerPoliceCarNotOnFinishingGrid(Player player)
        {
            policeCarsOnFinishingGrid.Remove(player);
        }
    }
}