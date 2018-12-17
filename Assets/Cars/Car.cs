using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField]
    Transform m_Car;
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
        }
    }

    float DirectionalModifier => Speed * Time.deltaTime;

    void Drive()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            m_Car.localPosition += new Vector3(0, 0, DirectionalModifier);
            m_Camera.localPosition += new Vector3(0, 0, DirectionalModifier);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            m_Car.localPosition += new Vector3(0, 0, -DirectionalModifier);
            m_Camera.localPosition += new Vector3(0, 0, -DirectionalModifier);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            m_Car.Rotate(new Vector3(1, 1, 1));
            m_Car.localPosition += new Vector3(DirectionalModifier, 0, 0);
            m_Camera.localPosition += new Vector3(DirectionalModifier, 0, 0);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            m_Car.Rotate(new Vector3(-1, -1, -1));
            m_Car.localPosition += new Vector3(-DirectionalModifier, 0, 0);
            m_Camera.localPosition += new Vector3(-DirectionalModifier, 0, 0);
        }
    }
}
