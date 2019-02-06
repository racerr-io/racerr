using Mirror;
using UnityEngine;

public class RacerrCarNetworkTransform : NetworkBehaviour
{
    [SyncVar] Vector3 RealPosition = Vector3.zero;
    [SyncVar] Quaternion RealRotation;
    [SyncVar] Vector3 RealVelocity;

    [SerializeField] [Range(0,1)] float InterpolationFactor = 0.4f;
    Rigidbody Rigidbody { get; set; }

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();

        if (!isLocalPlayer)
        {
            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;

            foreach (WheelCollider wheelCollider in GetComponentsInChildren<WheelCollider>())
            {
                Destroy(wheelCollider);
            }
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            RealPosition = transform.position;
            RealRotation = transform.rotation;
            RealVelocity = Rigidbody.velocity;
            CmdSync(transform.position, transform.rotation, Rigidbody.velocity);
        }
        else
        {
            Vector3 predictedPosition = RealPosition + Time.deltaTime * RealVelocity;
            transform.position = Vector3.Lerp(transform.position, predictedPosition, InterpolationFactor);
            transform.rotation = Quaternion.Lerp(transform.rotation, RealRotation, InterpolationFactor);
            Rigidbody.velocity = RealVelocity;
        }
    }

    [Command]
    void CmdSync(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        RealPosition = position;
        RealRotation = rotation;
        RealVelocity = velocity;
    }
}