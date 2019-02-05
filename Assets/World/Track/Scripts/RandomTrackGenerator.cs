using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
            GeneratedTrackPieces.Add(currentTrackPiece);
            int numTracks = 0;
            
            // Stores a validity map for the current track marked by numTrack index, where all of the possible track piece candidates are either valid or invalid.
            bool[,] validAvailableTracks = new bool[trackLength, availableTrackPiecePrefabs.Count];
            for (int numTracksIndex = 0; numTracksIndex < trackLength; numTracksIndex++)
            {
                for (int candidateTrackPiece = 0; candidateTrackPiece < availableTrackPiecePrefabs.Count; candidateTrackPiece++)
                {
                    validAvailableTracks[numTracksIndex, candidateTrackPiece] = true;
                }
            }

            while (numTracks < Math.Max(0, trackLength))
            {
                // Compile a list of valid track piece options.
                List<int> validTrackOptions = new List<int>();
                for (int candidateTrackPiece = 0; candidateTrackPiece < availableTrackPiecePrefabs.Count; candidateTrackPiece++)
                {
                    if (validAvailableTracks[numTracks, candidateTrackPiece] == true)
                    {
                        validTrackOptions.Add(candidateTrackPiece);
                    }
                }
                // Check if there exists any valid track pieces to choose from.
                if (validTrackOptions.Count == 0)
                {
                    // All track options for the current track piece are exhausted with no valid tracks.
                    // Must backtrack from the current track piece by destroying the current track piece.
                    NetworkServer.Destroy(currentTrackPiece);
                    Destroy(currentTrackPiece);
                    GeneratedTrackPieces.RemoveAt(GeneratedTrackPieces.Count - 1);
                    currentTrackPiece = GeneratedTrackPieces[GeneratedTrackPieces.Count - 1];
                    // Reset validAvailableTracks memory of this track's options for the future track pieces to use this space.
                    for (int candidateTrackPiece = 0; candidateTrackPiece < availableTrackPiecePrefabs.Count; candidateTrackPiece++)
                    {
                        validAvailableTracks[numTracks, candidateTrackPiece] = true;
                    }
                    numTracks--;
                    continue;
                }

                Transform trackPieceLinkTransform = LoadTrackPieceLinkTransform(currentTrackPiece);
                if (trackPieceLinkTransform == null)
                {
                    break;
                }

                int randomTrack = validTrackOptions[Random.Range(0, validTrackOptions.Count)];
                GameObject newTrackPiecePrefab = availableTrackPiecePrefabs[randomTrack];
                GameObject newTrackPiece = Instantiate(newTrackPiecePrefab);
                newTrackPiece.name = $"Auto Generated Track Piece { numTracks + 1 } ({ newTrackPiecePrefab.name })";
                newTrackPiece.transform.position = trackPieceLinkTransform.position;
                newTrackPiece.transform.rotation *= trackPieceLinkTransform.rotation;

                yield return new WaitForFixedUpdate(); // Wait for next physics calculation so that Track Piece Collision Detector works properly.

                validAvailableTracks[numTracks, randomTrack] = false;
                if (newTrackPiece.GetComponent<TrackPieceCollisionDetector>().IsValidTrackPlacementUponConnection)
                {
                    GeneratedTrackPieces.Add(newTrackPiece);
                    currentTrackPiece = newTrackPiece;
                    numTracks++;
                }
                else
                {
                    Destroy(newTrackPiece);

                }
            }

            currentTrackPiece.transform.Find(TrackPieceComponent.Checkpoint).name = TrackPieceComponent.FinishLineCheckpoint; // Set last generated track piece's checkpoint to be the ending checkpoint for the race.
        }
    }
}