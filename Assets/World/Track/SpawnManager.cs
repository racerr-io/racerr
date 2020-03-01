using Racerr.Infrastructure;
using Racerr.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.World.Track
{
    /// <summary>
    /// Manages spawning of all types of cars.
    /// </summary>
    public static class SpawnManager
    {   
        // Keep track of the police cars spawned that have not left the finishing track piece
        // to ensure that we do not spawn two police cars in the same position.
        public static List<Player> policeCarsOnFinishingGrid = new List<Player>();

        /// <summary>
        /// Spawns race cars on a given track piece in starting grid formation
        /// (https://www.motorsport.com/f1/photos/the-starting-grid-drivers-get-ready-for-the-pace-lap-4897739/4897739/).
        /// Note that this does not check whether all the given players will actually fit
        /// on the track. Player limits are set in Server Manager.
        /// </summary>
        /// <param name="startingTrackPiece">The track piece representing the first track.</param>
        /// <param name="playersToSpawn">The players you wish to spawn cars for.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        public static IEnumerator SpawnRaceCarOnStartingGrid(GameObject startingTrackPiece, IEnumerable<Player> playersToSpawn)
        {
            SentrySdk.AddBreadcrumb("Spawning players onto track.");
            Transform startLine = startingTrackPiece.transform.Find(GameObjectIdentifiers.StartLine);
            if (startLine == null)
            {
                throw new MissingComponentException($"Starting Track Piece must have a GameObject named { GameObjectIdentifiers.StartLine } which marks the starting line.");
            }

            Vector3 firstCarStartLineDisplacement = new Vector3(4.5f, 0.1f, -15);
            Vector3 verticalDistanceBetweenCars = new Vector3(0, 0, 5);
            Vector3 horizontalDistanceBetweenCars = new Vector3(9, 0, 0);
            Vector3 gridStartPosition = startLine.position + firstCarStartLineDisplacement;
            int spawnedPlayers = 0;
            foreach (Player player in playersToSpawn.Where(player => player != null))
            {
                player.CreateRaceCarForPlayer(gridStartPosition, startingTrackPiece.transform.rotation);
                gridStartPosition -= verticalDistanceBetweenCars + horizontalDistanceBetweenCars * LanguageExtensions.FastPow(-1, spawnedPlayers);
                spawnedPlayers++;
                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Returns a dummy gameobject that has the transform of the police car to be spawned on the finishing grid
        /// </summary>
        /// <param name="playerToSpawn">The player you wish to spawn the police car for.</param>
        public static GameObject GetPoliceCarOnFinishingGridPosition(Player playerToSpawn)
        {
            SentrySdk.AddBreadcrumb("Spawning police car onto track.");
            GameObject finishPosition = new GameObject();
            // Grab finish line and add slight offset to finish line so car is not spawned inside the ground.
            // Need smarter way of spawn police cars, could spawn multiple cars onto the same spot...
            GameObject[] checkpointsInRace = TrackGenerator.Singleton.CheckpointsInRace;
            GameObject finishingTrackPiece = checkpointsInRace[checkpointsInRace.Length - 1];
            Transform finishLine = finishingTrackPiece.transform;
            if (finishLine == null)
            {
                throw new MissingComponentException($"Finishing Track Piece must have a GameObject named { GameObjectIdentifiers.FinishLine } which marks the finishing line.");
            }
            // Apply the same rotation from the finishing track piece + flipping 180 degrees to the vectors that were 
            // originally used to calculate the position of the car in the starting track piece so we can spawn the 
            // police cars in the same position as if we were spawning the race cars on the starting track piece but
            // facing away from the finish line.
            Vector3 firstCarStartLineDisplacement = finishingTrackPiece.transform.rotation * Quaternion.Euler(0, 180f, 0) * new Vector3(4.5f, 0.1f, 45);
            Vector3 verticalDistanceBetweenCars = finishingTrackPiece.transform.rotation * Quaternion.Euler(0, 180f, 0) * new Vector3(0, 0, 5);
            Vector3 horizontalDistanceBetweenCars = finishingTrackPiece.transform.rotation * Quaternion.Euler(0, 180f, 0) * new Vector3(9, 0, 0);
            Vector3 gridFinishPosition = finishLine.position + firstCarStartLineDisplacement;
            int spawnedPoliceCarsOnFinishingGrid = 0;
            foreach (Player player in policeCarsOnFinishingGrid)
            {
                gridFinishPosition -= verticalDistanceBetweenCars + horizontalDistanceBetweenCars * LanguageExtensions.FastPow(-1, spawnedPoliceCarsOnFinishingGrid);
                spawnedPoliceCarsOnFinishingGrid++;
            }

            finishPosition.transform.position = gridFinishPosition;
            // Flip the police car around because we want it to be facing away from the finish line
            finishPosition.transform.rotation = finishingTrackPiece.transform.rotation * Quaternion.Euler(0, 180f, 0);
            return finishPosition;
        }
    }
}