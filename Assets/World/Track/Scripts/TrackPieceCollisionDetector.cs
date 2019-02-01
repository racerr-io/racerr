using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.Track
{
    public class TrackPieceCollisionDetector : MonoBehaviour
    {
        public bool IsValidTrackPlacementUponConnection => CollidedGameObjects.Where(g => g != null).Count() <= 1;
        List<GameObject> CollidedGameObjects { get; } = new List<GameObject>();

        void OnCollisionEnter(Collision collision)
        {
            GameObject collidedGameObject = collision.gameObject;

            if (collidedGameObject.CompareTag(TrackPieceComponent.TrackPiece) && !CollidedGameObjects.Contains(collidedGameObject))
            {
                CollidedGameObjects.Add(collidedGameObject);
            }
        }
    }
}