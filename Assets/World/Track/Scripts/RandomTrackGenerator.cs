using UnityEditor;
using UnityEngine;

/// <summary>
/// A track generator that randomly generates your tracks.
/// </summary>
public class RandomTrackGenerator : TrackGeneratorCommon
{
    [SerializeField]
    GameObject m_firstTrackPiece; // Temporary, we will programatically generate the first track piece in the future.

    /// <summary>
    /// Generate tracks by getting the first track piece, then grabbing a random track piece from resources and joining
    /// it together. Track pieces are moved and rotated to the position of the 'Track Piece Link' on the previous Track Piece.
    /// </summary>
    /// <param name="trackLength"></param>
    /// <param name="trackAltitude"></param>
    /// <param name="availableTrackPieces"></param>
    protected override void GenerateTrack(int trackLength, float trackAltitude, string[] availableTrackPieces)
    {
        GameObject currentTrackPiece = m_firstTrackPiece;

        for (int i = 0; i < trackLength; i++)
        {
            string newTrackPieceName = availableTrackPieces[Random.Range(0, availableTrackPieces.Length)];

            GameObject trackPiecePrefab;
            Transform trackPieceLinkTransform;

            if ((trackPieceLinkTransform = LoadTrackPieceLinkTransform(currentTrackPiece)) == null)
            {
                break;
            }
            else if ((trackPiecePrefab = LoadTrackPieceFromResources(newTrackPieceName)) == null)
            {
                continue;
            }

            GameObject newTrackPiece = PrefabUtility.InstantiatePrefab(trackPiecePrefab) as GameObject;
            newTrackPiece.name = $"Auto Generated Track Piece {i + 1} ({newTrackPieceName})";
            Vector3 newTrackPieceRotation = trackPieceLinkTransform.rotation.eulerAngles;
            Vector3 currentTrackPieceRotation = currentTrackPiece.transform.rotation.eulerAngles;
            newTrackPieceRotation.x = currentTrackPieceRotation.x;
            newTrackPieceRotation.z = currentTrackPieceRotation.z;

            newTrackPiece.transform.rotation = Quaternion.Euler(newTrackPieceRotation);
            newTrackPiece.transform.position = new Vector3(trackPieceLinkTransform.position.x, trackAltitude, trackPieceLinkTransform.position.z);

            currentTrackPiece = newTrackPiece;
        }
    }
}
