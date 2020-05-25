using Mirror;
using NWH.VehiclePhysics;
using Racerr.Gameplay.Ability;
using Racerr.Infrastructure;
using Racerr.Infrastructure.Client;
using Racerr.Utility;
using Racerr.UX.Car;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;

namespace Racerr.Gameplay.Car
{
    /// <summary>
    /// Car Manager for all cars in Racerr.
    /// Adds Racerr specific customisation to the vehicle, such as health and the player bar.
    /// WARNING: Car must be automatically generated using Instantiate() and certain fields set (see contracts in Start()).
    /// </summary>
    [RequireComponent(typeof(CarPhysicsManager))]
    public class CarManager : NetworkBehaviour
    {
        [SerializeField] int maxHealth = 100;
        [SerializeField] GameObject playerBarPrefab;
        [SerializeField] Material transparentMaterial;

        /* Player */
        [SyncVar] GameObject playerGO;
        public GameObject PlayerGO
        {
            get => playerGO;
            set
            {
                Contract.Assert(playerGO == null, "PlayerGO must only be set once on instantiation.");
                playerGO = value;
            }
        }
        public Player OwnPlayer { get; private set; }
        public bool IsZombie => OwnPlayer.Car != this;

        /* Player Bar */
        public PlayerBar PlayerBar { get; private set; }

        /* Health */
        public int MaxHealth => maxHealth;
        const double otherCarDamageAdjustmentFactor = 0.00004f;
        const double environmentDamageAdjustmentFactor = 0.00002f;
        [SyncVar] GameObject lastHitByPlayerGO;
        public Player LastHitByPlayer
        {
            get
            {
                if (lastHitByPlayerGO != null)
                {
                    return lastHitByPlayerGO.GetComponentInParent<Player>();
                }

                return null;
            }
        }
        public bool IsInvulnerable { get; private set; }

        /* Car Type */
        public enum CarTypeEnum
        {
            Unset,
            Racer,
            Police
        }
        [SyncVar] CarTypeEnum carType = CarTypeEnum.Unset;
        public CarTypeEnum CarType
        {
            get => carType;
            set
            {
                Contract.Assert(carType == CarTypeEnum.Unset, "CarType must only be set once on instantiation.");
                Contract.Assert(value != CarTypeEnum.Unset, "CarType cannot be set to Unset.");
                carType = value;
            }
        }

        /* Physics */
        public CarPhysicsManager Physics { get; private set; }

        /* Ability */
        public IAbility Ability { get; private set; }

        /// <summary>
        /// Called as soon as the car is instantiated. Caches various fields for later use.
        /// </summary>
        void Awake()
        {
            Physics = GetComponent<CarPhysicsManager>();
            Ability = GetComponent<IAbility>(); // Assume cars have only one ability.
        }

        /// <summary>
        /// Called some time after the car is instantiated and all components have Awake()'d. 
        /// Caches various fields for later use and instantiates the Player's Bar, which should appear 
        /// above the car in the game. Assumes PlayerGO and CarType has been set immediately after the car
        /// was Instantiate()'d.
        /// </summary>
        void Start()
        {
            Contract.Assert(PlayerGO != null, "PlayerGO must be set after instantiating the object.");
            Contract.Assert(carType != CarTypeEnum.Unset, "CarType must be set after instantiating the object.");

            OwnPlayer = PlayerGO.GetComponent<Player>();
            name = $"Car ({CarType}) - {OwnPlayer.PlayerName}";

            // Instantiate and setup player's bar
            GameObject PlayerBarGO = Instantiate(playerBarPrefab);
            PlayerBar = PlayerBarGO.GetComponent<PlayerBar>();
            PlayerBar.Car = this;

            GetComponents<PlayerBarConfiguration>()
                .Single(playerBarConfiguration => playerBarConfiguration.CameraType == ClientStateMachine.Singleton.PrimaryCamera.CamType)
                .ApplyConfiguration();

            if (!hasAuthority)
            {
                // Destroy all components which are only required by the person
                // driving the car.
                Destroy(Ability as MonoBehaviour);
                Ability = null;
            }

            if (!hasAuthority || !OwnPlayer.IsAI)
            {
                Destroy(GetComponent<AIInputManager>());
            }

            if (!hasAuthority || OwnPlayer.IsAI)
            {
                Destroy(GetComponent<DesktopInputManager>());
            }
        }

        /// <summary>
        /// Called every frame to determine if we have to activate the ability attached to the car.
        /// </summary>
        void Update()
        {
            if (Ability != null && Input.GetKey(KeyCode.Space))
            {
                StartCoroutine(Ability.Activate());
            }
        }

        /// <summary>
        /// Called every physics tick to check whether the car physics should be active or not.
        /// If the car is not active, it means it cannot be driven.
        /// </summary>
        void FixedUpdate()
        {
            if (isServer && Physics.IsActive && IsZombie)
            {
                SetIsActive(false);
            }
        }

        /// <summary>
        /// Damage our self when hit by another player's front of car, the environment or the back of the other car 
        /// hits the back of our car, by decreasing the player's health by an amount proportional to the force of the collision. 
        /// The purpose of this is to minimise the chance of the aggressor car taking damage when ramming into other cars.
        /// </summary>
        /// <param name="collision">Collision information.</param>
        [ClientCallback]
        void OnCollisionEnter(Collision collision)
        {
            if (OwnPlayer.Health == 0 || IsZombie || !hasAuthority)
            {
                return;
            }

            ContactPoint contactPoint = collision.GetContact(0);
            bool isHitByEnvironment = collision.gameObject.CompareTag(GameObjectIdentifiers.Environment);
            bool isHitByOtherCarFront = contactPoint.otherCollider.gameObject.CompareTag(GameObjectIdentifiers.CarFrontCollider);
            bool isHitByOtherCarBackIntoOurBack = contactPoint.thisCollider.gameObject.CompareTag(GameObjectIdentifiers.CarBackCollider) && contactPoint.otherCollider.gameObject.CompareTag(GameObjectIdentifiers.CarBackCollider);

            Vector3 collisionForce = collision.impulse / Time.fixedDeltaTime;
            if (isHitByEnvironment)
            {
                OwnPlayer.Health -= Convert.ToInt32(collisionForce.magnitude * environmentDamageAdjustmentFactor);
            }
            else if (isHitByOtherCarFront || isHitByOtherCarBackIntoOurBack)
            {
                CmdSetLastHitPlayerGO(contactPoint.otherCollider.gameObject.GetComponentInParent<CarManager>().OwnPlayer.gameObject);
                OwnPlayer.Health -= Convert.ToInt32(collisionForce.magnitude * otherCarDamageAdjustmentFactor);
            }
            else
            {
                // Prevent the car flying up and going out of control if it is the aggressor.
                Rigidbody rigidbody = GetComponent<Rigidbody>();
                rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
                this.YieldThenExecuteAsync(new WaitForSeconds(0.5f), () =>
                {
                    rigidbody.constraints = RigidbodyConstraints.None;
                });
            }
        }

        /// <summary>
        /// Command send by the client to update the LastHitByPlayer on the server.
        /// </summary>
        /// <param name="lastHitByPlayerGO">The player GameObject of the player who hit this car.</param>
        [Command]
        void CmdSetLastHitPlayerGO(GameObject lastHitByPlayerGO)
        {
            this.lastHitByPlayerGO = lastHitByPlayerGO;
        }

        /// <summary>
        /// Cars initially spawn in an inactive state, so they cannot be driven before the race starts.
        /// Once the race starts, this function is called by the server to allow all players in the race
        /// to drive their car. The client will then call this again to deactivate themself if they die,
        /// since we leave their corpse on the track and we don't want them to be driving it.
        /// </summary>
        /// <remarks>NOTE: MUST BE CALLED ON SERVER ONLY, CALLING ON CLIENT IS UNDEFINED BEHAVIOUR.</remarks>
        /// <param name="isActive">Whether the car should be active or not.</param>
        public void SetIsActive(bool isActive)
        {
            if (!hasAuthority || OwnPlayer.IsAI)
            {
                RpcSetIsActive(isActive);
            }

            Physics.IsActive = isActive;
        }

        /// <summary>
        /// Helper function for SetIsActive() which is called on the server only, which sends
        /// a signal to the client to execute SetIsActive() and change the active status of the
        /// car physics. The server calls this function in SetIsActive(), 
        /// but the body is executed on the client, so there is no infinite loop.
        /// </summary>
        /// <param name="isActive">Whether the car should be active or not.</param>
        [ClientRpc]
        void RpcSetIsActive(bool isActive)
        {
            Physics.IsActive = isActive;
        } 

        /// <summary>
        /// Temporarily sets the car as invulnerable, meaning it takes no damage and can pass through objects.
        /// This will modify the physics and also the shaders which makes the car change its material temporarily.
        /// </summary>
        /// <remarks>NOTE: MUST BE CALLED ON SERVER ONLY, CALLING ON CLIENT IS UNDEFINED BEHAVIOUR.</remarks>
        /// <param name="durationSeconds">How long to stay invulnerable for.</param>
        public void SetInvulnerableTemporarily(int durationSeconds = 5)
        {
            if (!hasAuthority || OwnPlayer.IsAI)
            {
                RpcSetInvulnerableTemporarily(durationSeconds);
            }

            Code(durationSeconds);
        }

        /// <summary>
        /// Helper function for SetInvulnerableTemporarily() which is called on the server only, which sends
        /// a signal to the client to execute SetInvulnerableTemporarily() and make the car invulnerable. 
        /// The server calls this function in SetInvulnerableTemporarily(), 
        /// but the body is executed on the client, so there is no infinite loop.
        /// </summary>
        /// <param name="durationSeconds">How long to stay invulnerable for.</param>
        [ClientRpc]
        void RpcSetInvulnerableTemporarily(int durationSeconds) => Code(durationSeconds);

        void Code(int durationSeconds)
        {
            if (IsInvulnerable)
            {
                return;
            }

            IsInvulnerable = true;
            Physics.SetInvulnerableTemporarily(durationSeconds);
            SetAllShadersTransparentTemporarily(gameObject, durationSeconds);

            this.YieldThenExecuteAsync(new WaitForSeconds(durationSeconds), () =>
            {
                IsInvulnerable = false;
            });
        }

        /// <summary>
        /// Set shaders to the transparent material for a short time. Useful for showing that
        /// the car is invulnerable. This operation is done recursively for all children.
        /// </summary>
        /// <param name="rootGameObject">Parent game object to set the shader.</param>
        /// <param name="durationSeconds">How long to set the shader for.</param>
        void SetAllShadersTransparentTemporarily(GameObject rootGameObject, int durationSeconds)
        {
            MeshRenderer meshRenderer = rootGameObject.GetComponent<MeshRenderer>();
            Material originalMaterial = null;
            if (meshRenderer != null)
            {
                originalMaterial = meshRenderer.sharedMaterial;
                meshRenderer.sharedMaterial = transparentMaterial;
            }

            foreach (Transform child in rootGameObject.transform)
            {
                SetAllShadersTransparentTemporarily(child.gameObject, durationSeconds);
            }

            this.YieldThenExecuteAsync(new WaitForSeconds(durationSeconds), () =>
            {
                if (meshRenderer != null)
                {
                    meshRenderer.sharedMaterial = originalMaterial;
                }
            });
        }
    }
}
