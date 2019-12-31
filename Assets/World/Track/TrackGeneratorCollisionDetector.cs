using System.Linq;
using UnityEngine;

namespace Racerr.World.Track
{
    /// <summary>
    /// Determine if a NEWLY placed track piece is colliding with another track.
    /// You must allow this script to run for at least one physics tick (i.e. one FixedUpdate) 
    /// of the game, so collisions can be calculated.
    /// We use this script solely during track generation, and remove it once track gen
    /// is completed.
    /// </summary>
    /// <remarks>
    /// Requires Track Game Object to have these to work:
    /// - Non-kinematic rigidbody with all constraints frozen.
    /// - Place this script on the top parent, to detect collisions with all children colliders.
    /// - Mesh colliders to be convex.
    /// </remarks>
    public class TrackGeneratorCollisionDetector : MonoBehaviour
    {
        public bool IsValidTrackPlacementUponConnection { get; private set; } = true;

        /// <summary>
        /// When track collides with another game object, this function is called.
        /// If we collided with another track piece, that means the new track piece is intersecting
        /// with an existing track piece, and is thus not a valid track placement.
        /// Note that a track piece will always constantly collide with the previous track piece since
        /// they are adjacent to each other, and thus we assume they are not intersecting and ignore that collision.
        /// </summary>
        /// <param name="collision">Collision information.</param>
        void OnCollisionEnter(Collision collision)
        {
            GameObject collidedGameObject = collision.gameObject;
            GameObject previousTrackPiece = TrackGeneratorCommon.Singleton.GeneratedTrackPieces.LastOrDefault();
            bool collidedWithTrackPiece = collidedGameObject.CompareTag(TrackPieceComponent.Road) || collidedGameObject.CompareTag(TrackPieceComponent.Highway);
            bool collidedWithPreviousTrackPiece = collidedGameObject == previousTrackPiece;

            if (collidedWithTrackPiece && !collidedWithPreviousTrackPiece)
            {
                IsValidTrackPlacementUponConnection = false;
            }
        }
    }
}