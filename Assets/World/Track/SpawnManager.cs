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

            Vector3 firstCarStartLineDisplacement = new Vector3(4.5f, 0.2f, -15);
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
        /// Spawns police cars on the finishing track piece
        /// </summary>
        /// <param name="playerToSpawn">The player you wish to spawn the police car for.</param>
        public static void SpawnPoliceCarOnFinishingGrid(Player playerToSpawn)
        {
            SentrySdk.AddBreadcrumb("Spawning police car onto track.");
            // Grab finish line and add slight offset to finish line so car is not spawned inside the ground.
            // Need smarter way of spawn police cars, could spawn multiple cars onto the same spot...
            GameObject[] checkpointsInRace = TrackGenerator.Singleton.CheckpointsInRace;
            GameObject finishingTrackPiece = checkpointsInRace[checkpointsInRace.Length - 1];
            Transform finishLine = checkpointsInRace[checkpointsInRace.Length - 1].transform;
            if (finishLine == null)
            {
                throw new MissingComponentException($"Finishing Track Piece must have a GameObject named { GameObjectIdentifiers.FinishLine } which marks the finishing line.");
            }

            Vector3 firstCarStartLineDisplacement = new Vector3(4.5f, 0.28f, -15);
            Vector3 verticalDistanceBetweenCars = new Vector3(0, 0, 5);
            Vector3 horizontalDistanceBetweenCars = new Vector3(9, 0, 0);
            Vector3 gridFinishPosition = finishLine.position + new Vector3(0, 0.2f, 0);
            // Flip the police car around because we want it to be facing away from the finish line
            playerToSpawn.CmdCreatePoliceCarForPlayer(gridFinishPosition, finishingTrackPiece.transform.rotation * Quaternion.Euler(0, 180f, 0));
        }
    }
}