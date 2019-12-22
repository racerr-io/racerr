using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NWH.VehiclePhysics
{
    /// <summary>
    /// Class for controlling all of the vehicle lights.
    /// </summary>
    [System.Serializable]
    public class Lights
    {
        /// <summary>
        /// Determines the state of all lights.
        /// </summary>
        [HideInInspector]
        public bool enabled = true;
        private bool prevEnabled = false;

        /// <summary>
        /// Single vehicle light.
        /// </summary>
        [System.Serializable]
        public class VehicleLight
        {
            protected bool active;

            /// <summary>
            /// List of light sources of any type.
            /// </summary>
            [Tooltip("List of light sources of any type (point, spot, etc.).")]
            public List<Light> lightSources = new List<Light>();

            /// <summary>
            /// List of mesh renderers with standard shader. Emission will be turned on or off depending on light state.
            /// </summary>
            [Tooltip("List of mesh renderers with standard shader. Emission will be turned on or off depending on light state.")]
            public List<MeshRenderer> lightMeshRenderers = new List<MeshRenderer>();

            /// <summary>
            /// Set this list to point to your sub-materials if your lights use multiple materials. First element should correspond to first element of lightMeshRenderers list, etc.
            /// If not set for the given mesh renderer, first material will be used.
            /// </summary>
            [Tooltip("Set this list to point to your sub-materials if your lights use multiple materials. First element should correspond to first element of lightMeshRenderers list, etc." +
                " If not set for the given mesh renderer, first material will be used.")]
            public List<int> lightMeshMaterialSubindices = new List<int>();

            /// <summary>
            /// State of the light.
            /// </summary>
            public bool On { get { return active; } }

            /// <summary>
            /// Turns on the light source or enables emission on the mesh. Mesh is required to have standard shader.
            /// </summary>
            public void TurnOn()
            {
                active = true;

                foreach (Light light in lightSources)
                {
                    light.enabled = true;
                }

                for (int i = 0; i < lightMeshRenderers.Count; i++)
                {
                    if (i >= lightMeshMaterialSubindices.Count)
                    {
                        lightMeshRenderers[i].material.EnableKeyword("_EMISSION");
                    }
                    else
                    {
                        lightMeshRenderers[i].materials[lightMeshMaterialSubindices[i]].EnableKeyword("_EMISSION");
                    }
                }
            }

            /// <summary>
            /// Turns off the light source or disables emission on the mesh. Mesh is required to have standard shader.
            /// </summary>
            public void TurnOff()
            {
                active = false;

                foreach(Light light in lightSources)
                {
                    light.enabled = false;
                }

                for(int i = 0; i < lightMeshRenderers.Count; i++)
                {
                    if (i >= lightMeshMaterialSubindices.Count)
                    {
                        lightMeshRenderers[i].material.DisableKeyword("_EMISSION");
                    }
                    else
                    {
                        lightMeshRenderers[i].materials[lightMeshMaterialSubindices[i]].DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        /// <summary>
        /// Rear lights that will light up when brake is pressed. Always red.
        /// </summary>
        [Tooltip("Rear lights that will light up when brake is pressed.")]
        public VehicleLight stopLights = new VehicleLight();

        /// <summary>
        /// Rear Lights that will light up when headlights are on. Always red.
        /// </summary>
        [Tooltip("Rear Lights that will light up when headlights are on.")]
        public VehicleLight rearLights = new VehicleLight();

        /// <summary>
        /// Rear Lights that will light up when vehicle is in reverse gear(s). Usually white.
        /// </summary>
        [Tooltip("Rear Lights that will light up when vehicle is traveling in reverse. Usually white.")]
        public VehicleLight reverseLights = new VehicleLight();

        /// <summary>
        /// Low beam lights.
        /// </summary>
        [Tooltip("Low beam lights.")]
        public VehicleLight headLights = new VehicleLight();

        /// <summary>
        /// High (full) beam lights.
        /// </summary>
        [Tooltip("High (full) beam lights.")]
        public VehicleLight fullBeams = new VehicleLight();

        /// <summary>
        /// Blinkers on the left side of the vehicle.
        /// </summary>
        [Tooltip("Blinkers on the left side of the vehicle.")]
        public VehicleLight leftBlinkers = new VehicleLight();

        /// <summary>
        /// Blinkers on the right side of the vehicle.
        /// </summary>
        [Tooltip("Blinkers on the right side of the vehicle.")]
        public VehicleLight rightBlinkers = new VehicleLight();

        private VehicleController vc;

        /// <summary>
        /// State in which blinker is at the moment.
        /// </summary>
        public bool BlinkerState
        {
            get
            {
                return (int)(Time.realtimeSinceStartup * 2) % 2 == 0;
            }
        }

        public void Initialize(VehicleController vc)
        {
            this.vc = vc;
        }

        /// <summary>
        /// Turns off all lights and emission on all meshes.
        /// </summary>
        public void TurnOffAllLights()
        {
            stopLights.TurnOff();
            headLights.TurnOff();
            rearLights.TurnOff();
            reverseLights.TurnOff();
            fullBeams.TurnOff();
            leftBlinkers.TurnOff();
            rightBlinkers.TurnOff();
        }


        public void Update()
        {
            if(enabled && vc != null)
            {
                // Stop lights
                if (stopLights != null)
                {
                    if (vc.brakes.Active)
                    {
                        stopLights.TurnOn();
                    }
                    else
                    {
                        stopLights.TurnOff();
                    }
                }

                // Reverse lights
                if (reverseLights != null)
                {
                    if(vc.transmission.Gear < 0)
                    {
                        reverseLights.TurnOn();
                    }
                    else
                    {
                        reverseLights.TurnOff();
                    }
                }

                // Lights
                if (rearLights != null && headLights != null)
                {
                    if (vc.input.lowBeamLights)
                    {
                        rearLights.TurnOn();
                        headLights.TurnOn();
                    }
                    else
                    {
                        rearLights.TurnOff();
                        headLights.TurnOff();

                        if (fullBeams != null && fullBeams.On)
                        {
                            fullBeams.TurnOff();
                            vc.input.fullBeamLights = false;
                        }
                    }
                }

                // Full beam lights
                if (fullBeams != null)
                {
                    if (vc.input.fullBeamLights)
                    {
                        fullBeams.TurnOn();

                        if (headLights != null && rearLights != null && !vc.input.lowBeamLights)
                        {
                            headLights.TurnOn();
                            rearLights.TurnOn();
                            vc.input.lowBeamLights = true;
                        }
                    }
                    else
                    {
                        fullBeams.TurnOff();
                    }
                }

                // Left blinker lights (cancels the other blinker)
                if (leftBlinkers != null)
                {
                    if (vc.input.leftBlinker)
                    {
                        if (BlinkerState)
                            leftBlinkers.TurnOn();
                        else
                            leftBlinkers.TurnOff();
                    }
                }

                // Left blinker lights
                if (rightBlinkers != null)
                {
                    if (vc.input.rightBlinker)
                    {
                        if (BlinkerState)
                            rightBlinkers.TurnOn();
                        else
                            rightBlinkers.TurnOff();
                    }
                }

                // Hazards
                if (leftBlinkers != null && rightBlinkers != null)
                {
                    if (vc.input.hazardLights)
                    {
                        if (BlinkerState)
                        {
                            leftBlinkers.TurnOn();
                            rightBlinkers.TurnOn();
                        }
                        else
                        {
                            leftBlinkers.TurnOff();
                            rightBlinkers.TurnOff();
                        }
                    }
                    else
                    {
                        if (!vc.input.leftBlinker)
                            leftBlinkers.TurnOff();

                        if (!vc.input.rightBlinker)
                            rightBlinkers.TurnOff();
                    }
                }
            }

            // Disable all lights if enabled is set to false.
            if(prevEnabled == true && enabled == false)
            {
                TurnOffAllLights();
            }

            prevEnabled = enabled;
        }


        /// <summary>
        /// Retruns light states as a byte with each bit representing one light;
        /// </summary>
        /// <returns></returns>
        public byte GetByteState()
        {
            byte state = 0;

            if (stopLights.On) state |= (1 << 0);
            if (rearLights.On) state |= (1 << 1);
            if (reverseLights.On) state |= (1 << 2);
            if (headLights.On) state |= (1 << 3);
            if (fullBeams.On) state |= (1 << 4);
            if (leftBlinkers.On) state |= (1 << 5);
            if (rightBlinkers.On) state |= (1 << 6);

            return state;
        }


        /// <summary>
        /// Sets state of lights from a single byte where each bit represents one light.
        /// To be used with GetByteState().
        /// </summary>
        /// <param name="state"></param>
        public void SetStatesFromByte(byte state)
        {
            if ((state & (1 << 0)) != 0) stopLights.TurnOn();
            else stopLights.TurnOff();

            if ((state & (1 << 1)) != 0) rearLights.TurnOn();
            else rearLights.TurnOff();

            if ((state & (1 << 2)) != 0) reverseLights.TurnOn();
            else reverseLights.TurnOff();

            if ((state & (1 << 3)) != 0) headLights.TurnOn();
            else headLights.TurnOff();

            if ((state & (1 << 4)) != 0) fullBeams.TurnOn();
            else fullBeams.TurnOff();

            if ((state & (1 << 5)) != 0) leftBlinkers.TurnOn();
            else leftBlinkers.TurnOff();

            if ((state & (1 << 6)) != 0) rightBlinkers.TurnOn();
            else rightBlinkers.TurnOff();
        }
    }
}
