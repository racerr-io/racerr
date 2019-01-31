using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Racerr.Track
{
    public class CollisionDetector : MonoBehaviour
    {
        public bool IsValidTrackPlacement { get; private set; } = true;
        public GameObject PreviousTrack { get; private set; }
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name == "Road")
            {
                IsValidTrackPlacement = false;
            }
            Debug.Log(collision.gameObject.name);
        }
    }
}