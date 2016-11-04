using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FrictionSimulation : MonoBehaviour {

    public Transform m_desiredDestination;
    public Transform m_ramp = null;
    public float m_forceIncrement = 10.0f;
    public bool m_isImpulse = false;
    public float m_totalTime = 1.0f;

    private Rigidbody m_rb = null;
    private PhysicMaterial m_pm = null;
    private Vector3 m_netForce = Vector3.zero;
    private float m_desiredAcceleration = 0.0f;
    private bool m_isMoving = false;

    public void ApplyForce()
    {
        m_rb.AddForce(m_netForce);
        Debug.Log("Push");
    }

    void CalculateForce()
    {
        if(m_isImpulse)
        {
            
        }
        else if (m_isMoving)
        {
            if(m_ramp != null )
            {
                CalculateRampDynamicFrictionPushForce();
            }
            else
            {
                CalculateDynamicPushForce();
            }
        }
        else
        {
            if (m_ramp != null)
            {
                CalculateRampStaticFrictionPushForce();
            }
            else
            {
                CalculateStaticPushForce();
            }
        }
    }

    void CalculateRampStaticFrictionPushForce()
    {
        //convert this value to a radian
        float theta = m_ramp.rotation.eulerAngles.z;
        Vector3 forceNormal = m_rb.mass * Physics.gravity * -1.0f * Mathf.Sin(theta);
        Vector3 forceGravity = forceNormal * -1.0f;
        Vector3 forceStatic = forceNormal * m_pm.staticFriction * Mathf.Cos(theta);
        //pivot point is centre of ramp so this will not be accurate
        Vector3 direction = (m_ramp.transform.position*5.0f) - transform.position;
        Vector3 displacement = direction;
        Vector3 initialVelocity = Vector3.zero;
        Vector3 desiredAcceleration = Vector3.zero;
        desiredAcceleration = (2.0f * (displacement - initialVelocity * m_totalTime)) / (m_totalTime * m_totalTime);
        //force required to move up the ramp, if there is no friciton to overcome
        Vector3 desiredForce = desiredAcceleration * m_rb.mass;
        desiredForce += forceNormal + forceStatic;
        m_netForce = desiredForce;
    }

    void CalculateRampDynamicFrictionPushForce()
    {
        float theta = m_ramp.rotation.eulerAngles.z;
        Vector3 forceNormal = m_rb.mass * Physics.gravity * -1.0f * Mathf.Sin(theta);
        Vector3 forceGravity = forceNormal * -1.0f;
        Vector3 forceStatic = forceNormal * m_pm.dynamicFriction * Mathf.Cos(theta);
        //pivot point is centre of ramp so this will not be accurate
        Vector3 direction = (m_ramp.transform.position * 5.0f) - transform.position;
        Vector3 displacement = direction;
        Vector3 initialVelocity = m_rb.velocity;
        Vector3 desiredAcceleration = Vector3.zero;
        desiredAcceleration = (2.0f * (displacement - initialVelocity * m_totalTime)) / (m_totalTime * m_totalTime);
        //force required to move up the ramp, if there is no friciton to overcome
        Vector3 desiredForce = desiredAcceleration * m_rb.mass;
        desiredForce += forceNormal + forceStatic;
        m_netForce = desiredForce;
    }

    void CalculateStaticPushForce()
    {
        Vector3 forceNormal = m_rb.mass * Physics.gravity * -1.0f;
        Vector3 forceStatic = forceNormal * m_pm.staticFriction;
        Vector3 direction = GetDirection();
        float displacement = Mathf.Abs(direction.x);
        float initialVelocity = 0.0f;
        m_desiredAcceleration = (2.0f * (displacement - initialVelocity * m_totalTime)) / (m_totalTime * m_totalTime);
        float desiredForce = m_desiredAcceleration * m_rb.mass;
        direction.Normalize();
        m_netForce = direction;
        m_netForce *= (desiredForce + forceStatic.magnitude);
    }
    void CalculateDynamicPushForce()
    {
        Vector3 forceNormal = m_rb.mass * Physics.gravity * -1.0f;
        Vector3 forceDynamic = forceNormal * m_pm.dynamicFriction;
        Vector3 direction = GetDirection();
        float displacement = Mathf.Abs(direction.x);
        float initialVelocity = m_rb.velocity.x;
        m_desiredAcceleration = (2.0f * (displacement - initialVelocity * m_totalTime)) / (m_totalTime * m_totalTime);
        float desiredForce = m_desiredAcceleration * m_rb.mass;
        direction.Normalize();
        m_netForce = direction;
        m_netForce *= (desiredForce + forceDynamic.magnitude);
    }

    public Vector3 GetDirection()
    {
        Vector3 direction = new Vector3();
        direction = m_desiredDestination.position - transform.position;
        direction.y = 0.0f;
        direction.z = 0.0f;
        return direction;
    }

        // Use this for initialization
        void Start ()
    {
        m_rb = this.GetComponent<Rigidbody>();
        m_pm = GetComponent<Collider>().material;
       
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            //run cube simulation
            ApplyForce();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //increment force
            m_netForce.x += m_forceIncrement;
            Mathf.Clamp(m_netForce.x, 0.0f, float.MaxValue);
            Debug.Log("Force = " + m_netForce.x);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            //decrement force
            m_netForce.x -= m_forceIncrement;
            Mathf.Clamp(m_netForce.x, 0.0f, float.MaxValue);
            Debug.Log("Force = " + m_netForce.x);
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            CalculateForce();
        }
    }

    public void UsingImpulseUpdate()
    {
        //m_rb.AddForce(m_netForce);
    }

    public void UsingForceUpdate()
    {
        if (m_rb.velocity.magnitude < float.Epsilon)
        {
            m_isMoving = false;
        }
        else if (m_isMoving == false && m_rb.velocity.magnitude > 0.007f)
        {
            m_isMoving = true;
            CalculateForce();
        }

        if (m_netForce.sqrMagnitude > float.Epsilon)
        {
            m_rb.AddForce(m_netForce);
            m_totalTime -= Time.fixedDeltaTime;
            Debug.Log("Time remaining: " + m_totalTime);
            if (m_totalTime < float.Epsilon)
            {
                m_netForce = Vector3.zero;
            }
        }
    }

    void FixedUpdate()
    {
        if(m_isImpulse)
        {
            if(m_netForce.sqrMagnitude > float.Epsilon)
            {
                m_rb.AddForce(m_netForce);
                m_netForce = Vector3.zero;
            }
        }
        else
        {
            UsingForceUpdate();
        }
    }

}
