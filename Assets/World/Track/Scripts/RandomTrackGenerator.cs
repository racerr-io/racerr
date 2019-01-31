using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
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
        /// it together. Track pieces are moved and rotated to the position of the 'Track Piece Link' on the previous Track Piece.
        /// </summary>
        /// <param name="trackLength">Number of Track Pieces this track should be composed of.</param>
        /// <param name="availableTrackPiecePrefabs">Collection of Track Pieces we can Instantiate.</param>
        protected override void GenerateTrack(int trackLength, IReadOnlyList<GameObject> availableTrackPiecePrefabs)
        {
            GameObject currentTrackPiece = m_firstTrackPiece;

            for (int i = 0; i < Math.Max(0, trackLength); i++)
            {
                Transform trackPieceLinkTransform = LoadTrackPieceLinkTransform(currentTrackPiece);

                if (trackPieceLinkTransform == null)
                {
                    break;
                }

                GameObject newTrackPiecePrefab = availableTrackPiecePrefabs[Random.Range(0, availableTrackPiecePrefabs.Count)];
                GameObject newTrackPiece = Instantiate(newTrackPiecePrefab);
                newTrackPiece.name = $"Auto Generated Track Piece {i + 1} ({newTrackPiecePrefab.name})";
                Vector3 newTrackPieceRotation = trackPieceLinkTransform.rotation.eulerAngles;
                Vector3 currentTrackPieceRotation = currentTrackPiece.transform.rotation.eulerAngles;
                newTrackPieceRotation.x = currentTrackPieceRotation.x;
                newTrackPieceRotation.z = currentTrackPieceRotation.z;

                newTrackPiece.transform.rotation *= Quaternion.Euler(newTrackPieceRotation);
                newTrackPiece.transform.position = new Vector3(trackPieceLinkTransform.position.x, 0, trackPieceLinkTransform.position.z);
                NetworkServer.Spawn(newTrackPiece);

                currentTrackPiece = newTrackPiece;
                GeneratedTrackPieces.Add(currentTrackPiece);
            }

          //  currentTrackPiece.transform.Find("Track Piece Checkpoint").tag = "Track Piece Checkpoint End"; // Set last generated track piece's checkpoint to be the ending checkpoint for the race.
        }
    }
}