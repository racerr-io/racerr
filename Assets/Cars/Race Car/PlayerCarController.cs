using System.Collections;
using System.Collections.Generic;
using Mirror;
using Racerr.UX.Camera;
using Racerr.UX.HUD;
using UnityEngine;

public class PlayerCarController : NetworkBehaviour
{
    private float m_horizontalInput;
    private float m_verticalInput;
    private float m_steeringAngle;

    public WheelCollider wheelFrontLeft, wheelFrontRight;
    public WheelCollider wheelRearLeft, wheelRearRight;
    public Transform transformFrontLeft, transformFrontRight;
    public Transform transformRearLeft, transformRearRight;
    public float maxSteerAngle = 30;
    public float motorForce = 5000;

    void Start()
    {
        //if (isLocalPlayer)
        //{
        //    FindObjectOfType<HUDRPM>().Car = this;
        //    FindObjectOfType<HUDSpeed>().Car = this;
        //    FindObjectOfType<AutoCam>().SetTarget(transform);
        //}
    }

    private void FixedUpdate()
    {
        GetInput();
        Steer();
        Accelerate();
        UpdateWheelPositions();
    }

    private void GetInput()
    {
        m_horizontalInput = Input.GetAxis("Horizontal");
        m_verticalInput = Input.GetAxis("Vertical");
    }

    private void Steer()
    {
        m_steeringAngle = maxSteerAngle * m_horizontalInput;
        wheelFrontLeft.steerAngle = m_steeringAngle;
        wheelFrontRight.steerAngle = m_steeringAngle;
    }

    private void Accelerate()
    {
        wheelRearLeft.motorTorque = m_verticalInput * motorForce;
        wheelRearRight.motorTorque = m_verticalInput * motorForce;
    }

    private void UpdateWheelPositions()
    {
        UpdateWheelPosition(wheelFrontLeft, transformFrontLeft);
        UpdateWheelPosition(wheelFrontRight, transformFrontRight);
        UpdateWheelPosition(wheelRearLeft, transformRearLeft);
        UpdateWheelPosition(wheelRearRight, transformRearRight);
    }

    private void UpdateWheelPosition(WheelCollider collider, Transform transform)
    {
        Vector3 pos = transform.position;
        Quaternion quat = transform.rotation;

        collider.GetWorldPose(out pos, out quat);
        transform.position = pos;
        transform.rotation = quat;
    }
}
