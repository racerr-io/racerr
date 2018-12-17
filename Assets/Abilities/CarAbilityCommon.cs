using UnityEngine;

public abstract class CarAbilityCommon : MonoBehaviour
{
    [SerializeField]
    KeyCode m_Key;

    Car Car { get; set; }
    bool IsKeyPressed { get; set; }
    protected bool IsActivated { get; set; }
    
    void Start()
    {
        Car = GetComponentInParent<Car>();

        if (Car == null)
        {
            Destroy(this);
            Debug.LogError("Car Ability Intialisation Failure - Please ensure you attached this ability script to a GameObject which has the Car script. " +
                "The ability has been removed from the attached object for safety.");
        }
    }

    void Update()
    {
        if (Car.IsUsersCar)
        {
            IsKeyPressed = Input.GetKeyDown(m_Key);
        }
    }

    void FixedUpdate()
    {
        if (Car.IsUsersCar)
        {
            FixedUpdateCore(IsKeyPressed);
        }
    }

    /// <summary>
    /// Called every physics tick (everytime FixedUpdate() is called).
    /// Depending on the ability you want to implement you may need to override this.
    /// </summary>
    /// <param name="isKeyPressed"></param>
    protected virtual void FixedUpdateCore(bool isKeyPressed)
    {
        if (isKeyPressed)
        {
            if (!IsActivated)
            {
                IsActivated = true;
                ActivateAbility();
            }
            else if (IsActivated)
            {
                IsActivated = false;
                DeactivateAbility();
            }
        }
    }

    /// <summary>
    /// Activate ability on the attached car. Method is public so multiplayer
    /// infrastructure can activate other players abilities.
    /// </summary>
    public void ActivateAbility()
    {
        ActivateAbilityCore(Car);
    }

    /// <summary>
    /// Deactivate ability on the attached car. Method is public so multiplayer
    /// infrastructure can deactivate other players abilities.
    /// </summary>
    public void DeactivateAbility()
    {
        DeactivateAbilityCore(Car);
    }

    abstract protected void ActivateAbilityCore(Car car);
    abstract protected void DeactivateAbilityCore(Car car);
}
