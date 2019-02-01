using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.Track
{
    public class TrackPieceCollisionDetector : MonoBehaviour
    {
        public bool IsValidTrackPlacementUponConnection => CollidedGameObjects.Where(g => g != null).Count() <= 1; // Is valid placement given we haven't connected it to the next piece.
        List<GameObject> CollidedGameObjects { get; set; } = new List<GameObject>();

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(TrackPieceComponent.TrackPiece))
            {
                CollidedGameObjects.Add(collision.gameObject);
            }
        }
    }
}