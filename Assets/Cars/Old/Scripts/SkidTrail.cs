using System.Collections;
using UnityEngine;

namespace Racerr.Car.Core
{
    /// <summary>
    /// Skid Trail Destroyer
    /// </summary>
    public class SkidTrail : MonoBehaviour
    {
        [SerializeField] float m_PersistTime;

        /// <summary>
        /// Destroy skid trails.
        /// </summary>
        /// <returns>Null</returns>
        IEnumerator Start()
        {
			while (true)
            {
                yield return null;

                if (transform.parent.parent == null)
                {
					Destroy(gameObject, m_PersistTime);
                }
            }
        }
    }
}
