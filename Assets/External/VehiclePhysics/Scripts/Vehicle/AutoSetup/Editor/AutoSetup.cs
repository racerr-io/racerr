using NWH.WheelController3D;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NWH.VehiclePhysics
{
    public class AutoSetup : EditorWindow
    {

        // General
        private GameObject vehicleObject;
        private GameObject colliderObject;
        private bool addEnterExitPoints = true;
        private bool addCamera = true;

        // Axles
        private int axleCount = 2;

        // Wheels
        private List<GameObject> wheelObjects = new List<GameObject>();
        private List<WheelController> wheelControllers = new List<WheelController>();


        [MenuItem("NWH Vehicle Physics/AutoSetup")]
        static void Init()
        {
            AutoSetup window = (AutoSetup)EditorWindow.GetWindowWithRect(typeof(AutoSetup), new Rect(0, 0, 500, 420), true, "NWH Vehicle Physics Auto-Setup");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 160f;
            EditorGUIUtility.fieldWidth = 160f;

            // Vehicle
            EditorGUILayout.LabelField("Vehicle Auto-Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Auto-Setup assumes correct model rotation (Z-forward, X-right, Y-up). If this is not the case the vehicle will not work properly. " +
                "How to fix model rotation inside Unity: https://docs.unity3d.com/Manual/HOWTO-FixZAxisIsUp.html", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Vehicle", EditorStyles.centeredGreyMiniLabel);
            vehicleObject = (GameObject)EditorGUILayout.ObjectField("Vehicle GameObject", vehicleObject, typeof(GameObject), true);
            colliderObject = (GameObject)EditorGUILayout.ObjectField("Collider GameObject", colliderObject, typeof(GameObject), true);
            addEnterExitPoints = EditorGUILayout.Toggle("Add Enter/Exit Points", addEnterExitPoints);

            // Axles
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Axles", EditorStyles.centeredGreyMiniLabel);
            axleCount = EditorGUILayout.IntField("Axle Count", axleCount);
            if(axleCount <= 0)
            {
                Debug.LogError("Axle count must be larger than 0.");
            }

            int wheelCount = axleCount * 2;
            if (wheelCount != wheelObjects.Count)
            {
                wheelObjects = new List<GameObject>();
                for (int i = 0; i < wheelCount; i++) wheelObjects.Add(null);
            }

            // Wheels
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wheels", EditorStyles.centeredGreyMiniLabel);
            EditorGUIUtility.labelWidth = 20f;
            EditorGUIUtility.fieldWidth = 20f;
            for (int i = 0; i < wheelCount; i+= 2)
            {
                if(i + 1 > wheelCount)
                {
                    break;
                }

                int axle = i == 0 ? 0 : (int)(i / 2);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Axle {axle} Wheel GOs:");
                wheelObjects[i] = (GameObject)EditorGUILayout.ObjectField($"L{axle}", wheelObjects[i], typeof(GameObject), true);
                wheelObjects[i + 1] = (GameObject)EditorGUILayout.ObjectField($"R{axle}", wheelObjects[i + 1], typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = 160f;
            EditorGUIUtility.fieldWidth = 160f;

            EditorGUILayout.Space();


            EditorGUI.BeginDisabledGroup(vehicleObject == null);

            if (GUILayout.Button("Setup"))
            {
                Debug.Log($"Setting up {vehicleObject.name}");

                // **********************
                // RUN CHECKS
                // **********************

                // Scale check
                if (vehicleObject.transform.localScale != Vector3.one)
                {
                    Debug.LogError($"Scale of object {vehicleObject.name} is other than [1,1,1]. Make sure that the root object of the vehicle ({vehicleObject.name}) has scale of [1,1,1]." +
                        $"The easiest way to do this is to click on the model inside the Unity Editor and change the model scale in the import settings. Remove the model from the scene " +
                        $" and re-add it back as the scaling is known to break if the model hierarchy has been changed.");
                    
                    return;
                }

                // Rigidbody check
                Rigidbody vehicleRb = vehicleObject.GetComponent<Rigidbody>();
                if(vehicleRb == null)
                {
                    Debug.Log("Rigidbody not found. Adding new rigidbody");
                    vehicleRb = vehicleObject.AddComponent<Rigidbody>();
                    if(vehicleRb == null)
                    {
                        Debug.LogError($"Could not add a rigidbody to vehicle object {vehicleObject.name}");
                        return;
                    }
                    vehicleRb.mass = 1300f;
                    vehicleRb.drag = 0.02f;
                    vehicleRb.interpolation = RigidbodyInterpolation.None;
                }

                // Collider
                if(colliderObject == null && vehicleObject.transform.GetComponentsInChildren<Collider>().Length == 0)
                {
                    Debug.LogError($"Found 0 colliders attached to {vehicleObject.name} and its children. At least one collider is required for Rigidbody to work properly.");
                    return;
                }
                if (colliderObject != null && colliderObject.GetComponent<Collider>() == null)
                {
                    if (colliderObject.GetComponent<MeshFilter>() == null)
                    {
                        Debug.LogError($"ColliderObject {colliderObject.name} does not contain a MeshFilter component. MeshCollider cannot be added to an object without a mesh. Assign different ColliderObject.");
                        return;
                    }
                    Debug.Log($"Adding collider to object {colliderObject.name}");
                    MeshCollider meshCollider = colliderObject.AddComponent<MeshCollider>();
                    if(meshCollider == null)
                    {
                        Debug.LogError($"Failed adding MeshCollider to object {colliderObject.name}");
                        return;
                    }
                    meshCollider.convex = true;
                    colliderObject.layer = 2;
                }

                // **********************
                // ADD WHEELCONTROLLERS
                // **********************
                wheelControllers.Clear();
                for(int i = 0; i < wheelObjects.Count; i++)
                {
                    GameObject wheelObject = wheelObjects[i];
                    string wcGoName = wheelObject.name + "_WheelController";
                    GameObject existingWcGO = wheelObject.transform.parent?.Find(wcGoName)?.gameObject;
                    if (wheelObject.transform.parent != null && existingWcGO != null)
                    {
                        Debug.Log($"GameObject {wcGoName} already exists. Delete the existing WheelController GameObject if you want new one to be generated, otherwise ignore the message.");
                        WheelController existingWC = existingWcGO.GetComponent<WheelController>();
                        if(existingWcGO == null)
                        {
                            Debug.Log($"Adding WheelController to {wcGoName}");
                        }
                        else
                        {
                            wheelControllers.Add(existingWcGO.GetComponent<WheelController>());
                            continue;
                        }
                    }

                    Debug.Log($"Setting up wheel {wheelObject.name}");
                    GameObject wcGO = new GameObject();
                    wcGO.transform.parent = wheelObject.transform.parent;
                    wcGO.transform.position = wheelObject.transform.position;
                    wcGO.name = wcGoName;
                    WheelController wheelController = wcGO.AddComponent<WheelController>();
                    wheelController.Initialize();
                    wheelController.wheel.visual = wheelObject;
                    wheelController.VehicleSide = i % 2 == 0 ? WheelController.Side.Left : WheelController.Side.Right;
                    wheelControllers.Add(wheelController);

                    // Move the pivot of the spring up, above the wheel
                    wcGO.transform.position += vehicleObject.transform.up * wheelController.springLength * 0.5f;
                }
                Debug.Assert(wheelControllers.Count == axleCount * 2, "Number of WheelControllers incorrect.");
                Debug.Assert(wheelControllers.Count == wheelObjects.Count, "WheelController and WheelObject list size mismatch.");

                // **********************
                // ADD VEHICLECONTROLLER
                // **********************
                VehicleController vehicleController = vehicleObject.GetComponent<VehicleController>();
                if(vehicleController == null)
                {
                    Debug.Log($"Adding VehicleController to {vehicleObject.name}.");
                    vehicleController = vehicleObject.AddComponent<VehicleController>();
                    if(vehicleController == null)
                    {
                        Debug.LogError("Failed adding VehicleController.");
                        return;
                    }
                }
                else
                {
                    vehicleController.axles = new List<Axle>();
                    for (int i = 0; i < axleCount; i++)
                    {
                        vehicleController.axles.Add(new Axle());
                        vehicleController.axles[i].leftWheel.wheelController = wheelControllers[i].GetComponent<WheelController>();
                        vehicleController.axles[i].rightWheel.wheelController = wheelControllers[i + 1].GetComponent<WheelController>();
                    }
                }
                vehicleObject.tag = "Vehicle";
                CenterOfMass com = vehicleObject.GetComponent<CenterOfMass>();
                com.centerOfMassOffset = new Vector3(0, -0.4f, 0);

                // Add enter/exit point
                if(addEnterExitPoints)
                {
                    if (!vehicleObject.transform.Find("LeftEnterExitPoint"))
                    {
                        GameObject enterExitPoint = new GameObject();
                        enterExitPoint.name = "LeftEnterExitPoint";
                        enterExitPoint.tag = "EnterExitPoint";
                        enterExitPoint.transform.SetParent(vehicleObject.transform);
                        enterExitPoint.transform.localPosition = new Vector3(-1.2f, 0f, 0f);
                    }
                    if (!vehicleObject.transform.Find("RightEnterExitPoint"))
                    {
                        GameObject enterExitPoint = new GameObject();
                        enterExitPoint.name = "RightEnterExitPoint";
                        enterExitPoint.tag = "EnterExitPoint";
                        enterExitPoint.transform.SetParent(vehicleObject.transform);
                        enterExitPoint.transform.localPosition = new Vector3(1.2f, 0f, 0f);
                    }
                }

                // Add camera
                if(addCamera)
                {
                    GameObject cameraGO = vehicleObject.transform.Find("Vehicle Camera Follow")?.gameObject;
                    if (cameraGO == null)
                    {
                        cameraGO = new GameObject();
                        cameraGO.name = "Vehicle Camera Follow";
                        cameraGO.tag = "VehicleCamera";
                        cameraGO.transform.SetParent(vehicleObject.transform);
                        cameraGO.transform.localPosition = new Vector3(0f, 2f, -5f);
                        Camera camera = cameraGO.AddComponent<Camera>();
                        camera.fieldOfView = 80f;
                        CameraFollow cameraFollow = cameraGO.AddComponent<CameraFollow>();
                        cameraFollow.target = vehicleObject.transform;
                        cameraFollow.distance = 4f;
                    }
                    else
                    {
                        cameraGO.tag = "VehicleCamera";
                    }
                }

                Debug.Log($"Setup for vehicle {vehicleObject.name} successful.");
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}

