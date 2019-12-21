using UnityEngine;
using System.Collections;

namespace NWH.VehiclePhysics
{
    /// <summary>
    /// Multiplayer-related part of VehicleController
    /// </summary>
    public partial class VehicleController : MonoBehaviour
    {
        #region PHOTON
#if PHOTON_MULTIPLAYER
        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // Steering
                stream.SendNext(input.Horizontal);

                // Sound
                foreach(SoundComponent sc in sound.components)
                {
                    stream.SendNext(sc.GetVolume());
                    stream.SendNext(sc.GetPitch());
                }

                // Lights
                stream.SendNext(effects.lights.GetByteState());
            }
            else
            {
                // Steering
                input.settable = true;
                input.Horizontal = (float)stream.ReceiveNext();
                input.settable = false;

                // Sound
                foreach (SoundComponent sc in sound.components)
                {
                    sc.SetVolume((float)stream.ReceiveNext());
                    sc.SetPitch((float)stream.ReceiveNext());
                }

                // Lights
                effects.lights.SetStatesFromByte((byte)stream.ReceiveNext());
            }
        }
#endif
            #endregion
        }
}
