using UnityEngine;

public class LaunchImpulse : MonoBehaviour {

    [Tooltip("The time it will take to go ovet the wall. Half the time it takes to go to the target.")]
    public float m_time;

    [Tooltip("Reference Object Transform")]
    public Transform m_referenceTransform;
    public float m_acceleration;

    // Reference to rigidbody.
    private Rigidbody m_rb;
    private Vector3 m_force = Vector3.zero;
    private Vector3 m_desiredDisplacement = Vector3.zero;

    // Use this for initialization
    void Start() {
        

    }

    void OnEnable() {
        m_rb = GetComponent<Rigidbody>();
        Vector3 ourPos = GetOffsetPosition(this.transform, true);
        Vector3 refPos = GetOffsetPosition(m_referenceTransform, false);
        m_desiredDisplacement = refPos - ourPos;
        LaunchProjectile();
    }

    Vector3 GetOffsetPosition(Transform refTransform, bool isBottom) {
        return isBottom ? refTransform.position - (new Vector3(0.0f, refTransform.localScale.y, 0.0f) * 0.5f) : refTransform.position + (new Vector3(0.0f, refTransform.localScale.y, 0.0f) * 0.5f);
    }

    // Update is called once per frame
    //void Update() {
    //    if (Input.GetKeyDown(KeyCode.Space)) {
    //        LaunchProjectile();
    //    }

    //    float axis = Input.GetAxis("Horizontal");
    //    //check if there is movement on the horzintal axis
    //    if (Mathf.Abs(axis) > Mathf.Epsilon) {
    //        //clamp the value to be either 1 or -1 we don't want fractions
    //        axis = axis > Mathf.Epsilon ? 1.0f : -1.0f;
    //        //right is the x axis, if you are moving on the z axis use forward
    //        Vector3 force = m_acceleration * m_rb.mass * this.transform.right * axis;
    //        m_rb.AddForce(force);
    //    }
    //}

    void LaunchProjectile() {
        m_force.y = CalculateYImpulse(m_desiredDisplacement.y, m_time);
        m_force.x = (m_desiredDisplacement.x / m_time) * m_rb.mass;
        m_rb.AddForce(m_force / Time.fixedDeltaTime);
        m_force = Vector3.zero;
    }

    float CalculateYImpulse(float displacement, float time) {
        float velocity = (displacement - (0.5f * Physics.gravity.y * (time * time))) / (time);
        //since we are starting at rest, the difference in velocity is the velocity we calculated
        return velocity * m_rb.mass;
    }
}
