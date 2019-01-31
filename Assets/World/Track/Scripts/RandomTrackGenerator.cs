using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;
using System.Collections;

namespace Racerr.Track
{
    /// <summary>
    /// A track generator that randomly generates your tracks.
    /// </summary>
    public class RandomTrackGenerator : TrackGeneratorCommon
    {
        [SerializeField] GameObject m_firstTrackPiece; // Temporary, we will programatically generate the first track piece in the future.

        /// <summary>
        /// Generate tracks by getting the first track piece, then grabbing a random track piece from resources and joining
        /// it together. Track pieces are moved and rotated to the position of the 'Link' on the previous Track Piece.
        /// </summary>
        /// <param name="trackLength">Number of Track Pieces this track should be composed of.</param>
        /// <param name="availableTrackPiecePrefabs">Collection of Track Pieces we can Instantiate.</param>
        /// <returns>IEnumerator for Unity coroutine, so that we can WaitForFixedUpdate() to check if a track is colliding with another one every time we instantiate a new track.</returns>
        protected override IEnumerator GenerateTrack(int trackLength, IReadOnlyList<GameObject> availableTrackPiecePrefabs)
        {
            GameObject currentTrackPiece = m_firstTrackPiece;
            int numTracks = 0;

            while (numTracks < Math.Max(0, trackLength))
            {
                Transform trackPieceLinkTransform = LoadTrackPieceLinkTransform(currentTrackPiece);

                if (trackPieceLinkTransform == null)
                {
                    break;
                }

                GameObject newTrackPiecePrefab = availableTrackPiecePrefabs[Random.Range(0, availableTrackPiecePrefabs.Count)];
                GameObject newTrackPiece = Instantiate(newTrackPiecePrefab);
                newTrackPiece.name = $"Auto Generated Track Piece { numTracks + 1 } ({ newTrackPiecePrefab.name })";
                newTrackPiece.transform.position = trackPieceLinkTransform.position;
                newTrackPiece.transform.rotation *= trackPieceLinkTransform.rotation;

                yield return new WaitForFixedUpdate(); // Wait for next physics calculation so that Track Piece Collision Detector works properly.;

                if (newTrackPiece.GetComponentInChildren<CollisionDetector>().IsValidTrackPlacementUponConnection)
                {
                    NetworkServer.Spawn(newTrackPiece);
                    currentTrackPiece = newTrackPiece;
                    GeneratedTrackPieces.Add(currentTrackPiece);
                    numTracks++;
                }
                else
                {
                    Destroy(newTrackPiece);
                }
            }

            currentTrackPiece.transform.Find("Checkpoint").name = "Finish Line Checkpoint"; // Set last generated track piece's checkpoint to be the ending checkpoint for the race.
        }
    }
}