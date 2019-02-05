using System;
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
    public float downforce = 1000;

    private int lastStiffness = 0;

    void Start()
    {
        if (isLocalPlayer) 
        {
            FindObjectOfType<HUDSpeed>().Car = this;
            FindObjectOfType<AutoCam>().SetTarget(transform);
        }
    }

    void UpdateStiffnessWithSpeed()
    {
        Vector3 speed = wheelFrontLeft.attachedRigidbody.velocity;
        int stiffness = Convert.ToInt32(Mathf.Lerp(1, 5, speed.magnitude / 50));
        if (stiffness == lastStiffness)
        {
            return;
        }

        lastStiffness = stiffness;
        WheelFrictionCurve wheelFrictionCurve = new WheelFrictionCurve
        {
            extremumSlip = 0.2f,
            extremumValue = 2f,
            asymptoteSlip = 0.5f,
            asymptoteValue = 0.75f,
            stiffness = stiffness
        };

        wheelFrontLeft.sidewaysFriction = wheelFrictionCurve;
        wheelFrontRight.sidewaysFriction = wheelFrictionCurve;
        wheelRearLeft.sidewaysFriction = wheelFrictionCurve;
        wheelRearRight.sidewaysFriction = wheelFrictionCurve;
    }

    private void FixedUpdate()
    {
        GetInput();
        Steer();
        Accelerate();
        UpdateWheelPositions();
        AddDownForce();
        UpdateStiffnessWithSpeed();
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

    private void AddDownForce()
    {
        Rigidbody carRigidBody = wheelFrontLeft.attachedRigidbody;
        carRigidBody.AddForce(-transform.up * downforce * carRigidBody.velocity.magnitude);
    }

}
