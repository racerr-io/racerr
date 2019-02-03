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
            int numTracks = 0;

            bool[][] validAvailableTracks = new bool[trackLength][];
            for (int i = 0; i < trackLength; i++)
            {
                validAvailableTracks[i] = new bool[availableTrackPiecePrefabs.Count];
                for (int j = 0; j < validAvailableTracks[i].Length; j++)
                {
                    validAvailableTracks[i][j] = true;
                }
            }

            while (numTracks < Math.Max(0, trackLength))
            {
                Transform trackPieceLinkTransform = LoadTrackPieceLinkTransform(currentTrackPiece);

                if (trackPieceLinkTransform == null)
                {
                    break;
                }

                List<int> validTrackOptions = new List<int>();
                for (int i = 0; i < validAvailableTracks[numTracks].Length; i++)
                {
                    if (validAvailableTracks[numTracks][i] == true)
                    {
                        validTrackOptions.Add(i);
                    }
                }
                if (validTrackOptions.Count == 0)
                {
                    NetworkServer.Destroy(currentTrackPiece);
                    Destroy(currentTrackPiece);
                    GeneratedTrackPieces.RemoveAt(GeneratedTrackPieces.Count - 1);
                    currentTrackPiece = GeneratedTrackPieces[GeneratedTrackPieces.Count - 1];
                    for (int i = 0; i < validAvailableTracks[numTracks].Length; i++)
                    {
                        validAvailableTracks[numTracks][i] = true;
                    }
                    numTracks--;
                    continue;
                }

                int randomTrack = validTrackOptions[Random.Range(0, validTrackOptions.Count)];

                GameObject newTrackPiecePrefab = availableTrackPiecePrefabs[randomTrack];
                GameObject newTrackPiece = Instantiate(newTrackPiecePrefab);
                newTrackPiece.name = $"Auto Generated Track Piece { numTracks + 1 } ({ newTrackPiecePrefab.name })";
                newTrackPiece.transform.position = trackPieceLinkTransform.position;
                newTrackPiece.transform.rotation *= trackPieceLinkTransform.rotation;

                yield return new WaitForFixedUpdate(); // Wait for next physics calculation so that Track Piece Collision Detector works properly.

                if (newTrackPiece.GetComponent<TrackPieceCollisionDetector>().IsValidTrackPlacementUponConnection)
                {
                    NetworkServer.Spawn(newTrackPiece);
                    GeneratedTrackPieces.Add(newTrackPiece);
                    currentTrackPiece = newTrackPiece;
                    numTracks++;
                }
                else
                {
                    Destroy(newTrackPiece);
                }
                validAvailableTracks[numTracks][randomTrack] = false;
            }

            currentTrackPiece.transform.Find(TrackPieceComponent.Checkpoint).name = TrackPieceComponent.FinishLineCheckpoint; // Set last generated track piece's checkpoint to be the ending checkpoint for the race.
        }
    }
}