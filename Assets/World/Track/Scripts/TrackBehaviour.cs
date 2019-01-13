using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racerr.Track
{
    public class TrackBehaviour : MonoBehaviour
    {
        public bool isOverlapping { get; set; }

        void Start()
        {
            isOverlapping = false;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Track Piece")
            {
                isOverlapping = true;
            }
        }
    }

}
