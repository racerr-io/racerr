using UnityEngine;

public class Car : MonoBehaviour {

    [SerializeField]
    Transform m_CarObject;
    [SerializeField]
    Transform m_Camera;
    [SerializeField]
    float m_Speed;

    public float Speed
    {
        get { return m_Speed; }
        set { m_Speed = value; }
    }

    public bool IsUsersCar { get; set; } = true;

    void FixedUpdate()
    {
        if (IsUsersCar)
        {
            Drive();
            MonitorAbilities();
        }
    }

    float DirectionalModifier => Speed * Time.deltaTime;

    void Drive()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            m_CarObject.localPosition += new Vector3(0, 0, DirectionalModifier);
            m_Camera.localPosition += new Vector3(0, 0, DirectionalModifier);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            m_CarObject.localPosition += new Vector3(0, 0, -DirectionalModifier);
            m_Camera.localPosition += new Vector3(0, 0, -DirectionalModifier);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            m_CarObject.Rotate(new Vector3(1, 1, 1));
            m_CarObject.localPosition += new Vector3(DirectionalModifier, 0, 0);
            m_Camera.localPosition += new Vector3(DirectionalModifier, 0, 0);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            m_CarObject.Rotate(new Vector3(-1, -1, -1));
            m_CarObject.localPosition += new Vector3(-DirectionalModifier, 0, 0);
            m_Camera.localPosition += new Vector3(-DirectionalModifier, 0, 0);
        }
    }

    // Abilities Management
    public Turbo Turbo;
    public Clone Clone;

    void MonitorAbilities()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Turbo.enabled = Turbo.enabled ? false : true;
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            Clone.CloneCar();
        }
    }
}
