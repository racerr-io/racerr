using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Racerr.Track
{
    /// <summary>
    /// Determine if this track piece is colliding with another track.
    /// </summary>
    /// <remarks>
    /// Requires Track Game Object to have these to work:
    /// - Non-kinematic rigidbody with all constraints frozen.
    /// - Place this script on the top parent, to detect collisions with all children colliders.
    /// - Mesh colliders to be convex.
    /// </remarks>
    public class TrackPieceCollisionDetector : MonoBehaviour
    {
        public bool IsValidTrackPlacementUponConnection => collidedGameObjects.Where(g => g != null).Count() <= 1;
        HashSet<GameObject> collidedGameObjects = new HashSet<GameObject>();

        /// <summary>
        /// When track collides with another game object, this function is called.
        /// </summary>
        /// <param name="collision">Collision information.</param>
        void OnCollisionEnter(Collision collision)
        {
            GameObject collidedGameObject = collision.gameObject;

            if (collidedGameObject.CompareTag("Road") || collidedGameObject.CompareTag("Highway"))
            {
                collidedGameObjects.Add(collidedGameObject);
            }
        }
    }
}