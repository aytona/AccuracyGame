using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class FrictionSimulation : MonoBehaviour
{

    public Transform m_desiredDestination = null;
    public Transform m_ramp = null;

    //ignore impulse
    public bool m_isImpulse = false;
    //total time will be calculated
    private float m_totalTime = 1.0f;

    //desired velocity will be calculated
    private Vector3 m_desiredExitVelocity = Vector3.zero;

    private Rigidbody m_rb = null;
    private PhysicMaterial m_pm = null;
    private Vector3 m_netForce = Vector3.zero;
    private Vector3 m_impulseForce = Vector3.zero;
    private float m_desiredAcceleration = 0.0f;
    private bool m_isMoving = false;
    private float m_timeRemaining = 0.0f;

    public void OnCollisionEnter(Collision coll)
    {
        //check to see if collision with ramp
        //call rampdynamicpushforce
        if(coll.gameObject.CompareTag("Ramp"))
        {
            //run logic
        }
    }

    public void ApplyForce()
    {
        m_rb.AddForce(m_netForce);
        Debug.Log("Push");
    }

    void CalculateForce()
    {
        if (m_isImpulse)
        {
            CalculateFinalVelocity();
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

    //need a quadratic equation function
    float CalculateQuadraticEquation(float theta, Vector3 acceleration, Vector3 displacement)
    {
        float time = 0.0f;
        //Viy = (x * tan theta) / t

        //0.5at^2 + 0t + xtantheta - d = 0

        time = (Mathf.Sqrt(-4.0f * 0.5f * acceleration.y * (displacement.x * Mathf.Tan(theta) - displacement.y))) / acceleration.y;
        time = Mathf.Abs(time);
        return time;
    }

    //need a function that calculates the final velocity
    void CalculateFinalVelocity()
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
        Vector3 forceStatic = forceNormal * m_pm.staticFriction;
        Vector3 forceDynamic = forceNormal * m_pm.dynamicFriction;
        Vector3 direction = rampEdge - ourPositionOnRamp;
        Vector3 displacement = direction;
        direction.Normalize();

        //our push force is being applied along the plane, our gravity and friciton forces only have a y component
        //we only want the magnitude(length aka total force) from these vectors 
        //we then want that force distributed on our directional axis, aka the ramp
        forceStatic = direction * forceStatic.magnitude;
        forceDynamic = direction * forceDynamic.magnitude;
        forceGravity = direction * forceGravity.magnitude;

        Vector3 deceleration = direction + (forceDynamic/m_rb.mass + forceGravity/m_rb.mass);
        deceleration *= -1.0f;

        Vector3 timeFinalVel = (displacement - (0.5f * deceleration * (m_timeRemaining * m_timeRemaining))) / m_timeRemaining;

        Vector3 initVel = m_rb.velocity;
        Vector3 finalVelocity = (initVel.normalized * initVel.sqrMagnitude) + ((2.0f * (deceleration.magnitude * displacement)));

        Vector3 finalVel = new Vector3(5.0f,5.0f,0.0f);
        Vector3 testFinalZer = (finalVel.normalized * finalVel.sqrMagnitude) - ((2.0f * (deceleration.magnitude * displacement)));
        Vector3 altAltVel = direction * Mathf.Sqrt(Mathf.Abs(testFinalZer.magnitude));

        Vector3 altVel = direction * Mathf.Sqrt(Mathf.Abs(finalVelocity.magnitude));
        m_impulseForce = altAltVel;
        m_impulseForce += forceStatic;
        m_isMoving = true;
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
        Vector3 forceStatic = forceNormal * m_pm.staticFriction;
        Vector3 forceDynamic = forceNormal * m_pm.dynamicFriction;
        Vector3 direction = rampEdge - ourPositionOnRamp;
        Vector3 displacement = direction;
        direction.Normalize();

        //our push force is being applied along the plane, our gravity and friciton forces only have a y component
        //we only want the magnitude(length aka total force) from these vectors 
        //we then want that force distributed on our directional axis, aka the ramp
        forceStatic = direction * forceStatic.magnitude;
        forceDynamic = direction * forceDynamic.magnitude;
        forceGravity = direction * forceGravity.magnitude;

        Vector3 initialVelocity = m_rb.velocity;
        Vector3 desiredAcceleration = Vector3.zero;
        desiredAcceleration = (2.0f * (displacement - initialVelocity * m_timeRemaining)) / (m_timeRemaining * m_timeRemaining);
        Vector3 dynamicAcceleration = forceDynamic / m_rb.mass;

        
        //Vector3 finalVel = direction * m_desiredExitVelocity.magnitude;
        Vector3 finalVel = m_desiredExitVelocity;
        Vector3 newAcceleration = ((finalVel.normalized * finalVel.sqrMagnitude) - (initialVelocity.normalized * initialVelocity.sqrMagnitude)) / (2.0f * displacement.magnitude);

        //force required to move up the ramp, if there is no friciton to overcome
        Vector3 desiredForce = newAcceleration * m_rb.mass;
        desiredForce += forceGravity + forceDynamic;

        m_timeRemaining = (finalVel.magnitude - initialVelocity.magnitude) / newAcceleration.magnitude;

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

    public Vector3 GetRampToRefObjectDisplacement(Transform ramp, Transform refObject)
    {
        Vector3 rampEdge = Vector3.zero;
        Vector3 refObjectEdgePositive = Vector3.zero;
        Vector3 refObjectEdgeNegative = Vector3.zero;

        rampEdge = ramp.position;
        Vector3 rampDirection = ramp.right * ramp.localScale.x * 5.0f;
        rampEdge += rampDirection;

        refObjectEdgePositive = refObject.position;
        Vector3 refObjectDirection = refObject.right * refObject.localScale.x;
        refObjectEdgePositive += refObjectDirection;

        refObjectEdgeNegative = refObject.position;
        Vector3 refObjectNegativeDirection = refObject.right * refObject.localScale.x * -1.0f;
        refObjectEdgeNegative += refObjectNegativeDirection;

        Vector3 rampEdgeToPositive = refObjectEdgePositive - rampEdge;
        Vector3 rampEdgeToNegative = refObjectEdgeNegative - rampEdge;

        //return (rampEdgeToPositive.sqrMagnitude < rampEdgeToNegative.sqrMagnitude) ? rampEdgeToPositive : rampEdgeToNegative;
        return refObject.position - rampEdge;
    }

    public Vector3 CalculateExitVelocity(float time, float theta, Vector3 displacement)
    {
        float Vx = displacement.x / time;
        float Velocity = Vx / Mathf.Cos(theta);
        float Vy = Velocity * Mathf.Sin(theta);
        return new Vector3(Vx, Vy, 0.0f);
    }

    // Use this for initialization
    void Start()
    {
        m_rb = this.GetComponent<Rigidbody>();
        m_pm = GetComponent<Collider>().material;
        m_timeRemaining = m_totalTime;

        //use the displacement from the edge of the ramp to edge of platform
        //use the angle of the ramp
        Vector3 displacement = GetRampToRefObjectDisplacement(m_ramp, m_desiredDestination);

        float theta;
        Vector3 axisOfRotation;
        m_ramp.localRotation.ToAngleAxis(out theta, out axisOfRotation);
        theta *= Mathf.Deg2Rad;
        float time = CalculateQuadraticEquation(theta, Physics.gravity, displacement);

        m_desiredExitVelocity = CalculateExitVelocity(time,theta,displacement);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //run cube simulation
            ApplyForce();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            CalculateForce();
        }
    }

    public void UsingImpulseUpdate()
    {
        if(m_impulseForce.sqrMagnitude > float.Epsilon)
        {
            m_rb.AddForce(m_impulseForce,ForceMode.Impulse);
            m_impulseForce = Vector3.zero;
        }
        m_timeRemaining -= Time.fixedDeltaTime;
        Debug.Log("Time remaining: " + m_timeRemaining);
        Debug.Log("Velocity: " + m_rb.velocity);
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

        Debug.Log("Velocity: " + m_rb.velocity);
    }

    void FixedUpdate()
    {
        if (m_netForce.sqrMagnitude > float.Epsilon || m_isMoving )
        {
            if (m_isImpulse)
            {
                UsingImpulseUpdate();
            }
            else
            {
                UsingForceUpdate();
            }
        }   
    }

}
