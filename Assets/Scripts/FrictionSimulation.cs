using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FrictionSimulation : MonoBehaviour
{

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
    private float m_timeRemaining = 0.0f;

    public void ApplyForce()
    {
        m_rb.AddForce(m_netForce);
        Debug.Log("Push");
    }

    void CalculateForce()
    {
        if (m_isImpulse)
        {

        }
        else if (m_isMoving)
        {
            if (m_ramp != null)
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
        //use this to get our displacement
        Collider coll = m_ramp.GetComponent<Collider>();
        Vector3 rampEdge = Vector3.zero;
        Vector3 ourPositionOnRamp = Vector3.zero;
        if (coll)
        {
            Vector3 xPos = m_ramp.position + (m_ramp.right * m_ramp.localScale.x * 5.0f);
            rampEdge = coll.ClosestPointOnBounds(xPos);
            rampEdge.y += 1.0f;
            ourPositionOnRamp = coll.ClosestPointOnBounds(transform.position);
        }

        Vector3 rampDirection = m_ramp.rotation.eulerAngles;
        float theta = rampDirection.z * Mathf.Deg2Rad;
        Vector3 forceGravity = m_rb.mass * Physics.gravity * Mathf.Sin(theta) * -1.0f;
        Vector3 forceNormal = m_rb.mass * Physics.gravity * Mathf.Cos(theta) * -1.0f;
        Vector3 forceStatic = forceNormal * m_pm.staticFriction; 
        Vector3 direction = rampEdge - ourPositionOnRamp;
        Vector3 displacement = direction;
        direction.Normalize();
        forceStatic = direction * forceStatic.magnitude;
        forceGravity = direction * forceGravity.magnitude;
        Vector3 initialVelocity = Vector3.zero;
        Vector3 desiredAcceleration = Vector3.zero;
        desiredAcceleration = (2.0f * (displacement - (initialVelocity * m_timeRemaining) ) ) / (m_timeRemaining * m_timeRemaining);
        Vector3 desiredForce = desiredAcceleration * m_rb.mass;
        //force required to move up the ramp, if there is no friciton to overcome
        //desiredForce = direction * (desiredForce.magnitude + forceNormal.magnitude + forceStatic.magnitude);
        desiredForce += forceGravity + forceStatic;
        m_netForce = desiredForce;
    }

    void CalculateRampDynamicFrictionPushForce()
    {
        Collider coll = m_ramp.GetComponent<Collider>();
        Vector3 rampEdge = Vector3.zero;
        Vector3 ourPositionOnRamp = Vector3.zero;
        if (coll)
        {
            Vector3 xPos = m_ramp.position + (m_ramp.right * m_ramp.localScale.x * 5.0f);
            rampEdge = coll.ClosestPointOnBounds(xPos);
            rampEdge.y += 1.0f;
            ourPositionOnRamp = coll.ClosestPointOnBounds(transform.position);
        }

        Vector3 rampDirection = m_ramp.rotation.eulerAngles;
        float theta = rampDirection.z * Mathf.Deg2Rad;
        Vector3 forceGravity = m_rb.mass * Physics.gravity * Mathf.Sin(theta) * -1.0f;
        Vector3 forceNormal = m_rb.mass * Physics.gravity * Mathf.Cos(theta) * -1.0f;
        Vector3 forceDynamic = forceNormal * m_pm.dynamicFriction;
        Vector3 direction = rampEdge - ourPositionOnRamp;
        Vector3 displacement = direction;
        direction.Normalize();

        //our push force is being applied along the plane, our gravity and friciton forces only have a y component
        //we only want the magnitude(length aka total force) from these vectors 
        //we then want that force distributed on our directional axis, aka the ramp
        forceDynamic = direction * forceDynamic.magnitude;
        forceGravity = direction * forceGravity.magnitude;

        Vector3 initialVelocity = m_rb.velocity;
        Vector3 desiredAcceleration = Vector3.zero;
        desiredAcceleration = (2.0f * (displacement - initialVelocity * m_timeRemaining)) / (m_timeRemaining * m_timeRemaining);
        //force required to move up the ramp, if there is no friciton to overcome
        Vector3 desiredForce = desiredAcceleration * m_rb.mass;
        desiredForce += forceGravity + forceDynamic;
        //desiredForce = direction * (desiredForce.magnitude + forceNormal.magnitude + forceDynamic.magnitude);
        m_netForce = desiredForce;
    }

    void CalculateStaticPushForce()
    {
        Vector3 forceNormal = m_rb.mass * Physics.gravity * -1.0f;
        Vector3 forceStatic = forceNormal * m_pm.staticFriction;
        Vector3 direction = GetDirection();
        float displacement = Mathf.Abs(direction.x);
        float initialVelocity = 0.0f;
        m_desiredAcceleration = (2.0f * (displacement - initialVelocity * m_timeRemaining)) / (m_timeRemaining * m_timeRemaining);
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
        m_desiredAcceleration = (2.0f * (displacement - initialVelocity * m_timeRemaining)) / (m_timeRemaining * m_timeRemaining);
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
    void Start()
    {
        m_rb = this.GetComponent<Rigidbody>();
        m_pm = GetComponent<Collider>().material;
        m_timeRemaining = m_totalTime;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
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
        else if (Input.GetKeyDown(KeyCode.Space))
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
        else if (m_isMoving == false && m_rb.velocity.magnitude > float.Epsilon )
        {
            m_isMoving = true;
            CalculateForce();
        }

        if (m_netForce.sqrMagnitude > float.Epsilon)
        {
            m_rb.AddForce(m_netForce);
            m_timeRemaining -= Time.fixedDeltaTime;
            Debug.Log("Time remaining: " + m_timeRemaining);
            if (m_timeRemaining < float.Epsilon)
            {
                m_netForce = Vector3.zero;
            }
        }
    }

    void FixedUpdate()
    {
        if (m_netForce.sqrMagnitude > float.Epsilon)
        {
            if (m_isImpulse)
            {
                m_rb.AddForce(m_netForce);
                m_netForce = Vector3.zero;
            }
            else
            {
                UsingForceUpdate();
            }
        }   
    }

}
