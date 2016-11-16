using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InitialImpulse : MonoBehaviour {

    [Tooltip("Time it will take to get to desired spot.")]
    public float m_time;
    [Tooltip("Gap distance between object and ref object")]
    public Vector3 m_bufferDistance;
    [Tooltip("Reference Object Transform")]
    public Transform m_refTransform;

    public Slider m_slider;

    // Reference rigibbody
    private Rigidbody m_rb;
    // Force used to push object
    private Vector3 m_force = Vector3.zero;
    // Desired displacement
    private Vector3 m_desiredDisplacement = Vector3.zero;

    // Use this for initialization
    void Start() {
        m_rb = GetComponent<Rigidbody>();
        m_desiredDisplacement = m_refTransform.position - (transform.position - m_bufferDistance);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Launch();
        }
    }

    private void Launch() {
        m_force.x = (m_desiredDisplacement.x / m_time) * m_rb.mass * (m_slider.value + 1f);
        m_rb.AddForce(m_force / Time.fixedDeltaTime);
        m_force = Vector3.zero;
    }
}
