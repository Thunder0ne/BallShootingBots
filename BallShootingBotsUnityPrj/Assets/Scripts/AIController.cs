using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour
{
    public float maxForce = 5.0f;
    public float maxTorque = 2.0f;
    public Camera mainCamera;

    private Rigidbody rb;
    private float groundHeight;
    private Vector3 goal;
    private float prevError;

    //float at max angul vel of 3.6 Radians / sec
    //good params are kp = 3.75 and kd = 0.65

    void Awake()
    {
        Debug.Log("Awake called");
        goal = transform.position;
    }

    void Start()
    {
        Debug.Log("Start called");
        rb = GetComponent<Rigidbody>();
        groundHeight = rb.position.y;
    }

    void FixedUpdate_test_perf()
    {
        rb.AddRelativeTorque(0.0f, maxTorque, 0.0f);
        Debug.Log("angular velocity " + rb.angularVelocity.magnitude);
    }

    void FixedUpdate()//_actual_control()//
    {
        float dt = Time.fixedDeltaTime;
        float control = 0;
        //compute the error
        float angleError = ComputeAngleError();
        float kp = 3.75f;
        //float kd = 1.26f;// 65f;
        float kd = 0.65f;
        //compute the control like
        //control = -kp * error - kd * ((err - prevErr) / dt);
        control = -kp * angleError - kd * ((angleError - prevError) / dt);
        //control = Mathf.Clamp(control / maxTorque, -1, 1);
        //Debug.Log("control " + control);
        prevError = angleError;
        rb.AddRelativeTorque(0.0f, control, 0.0f);
        //rb.AddRelativeTorque(0.0f, control * maxTorque, 0.0f);


        float distError = Vector3.Distance(transform.position, goal);
        float speedControl = maxForce;
        if (distError < 1.0f)
        {
            float desiredVel = distError;
            float speedError = rb.velocity.magnitude - desiredVel;
            speedControl = - kp * speedError;
        }
        rb.AddRelativeForce(0.0f, 0.0f, speedControl);

        float sliding = Vector3.Dot(rb.velocity, transform.right);
        rb.AddRelativeForce(-sliding, 0, 0, ForceMode.VelocityChange);
    }

    
    //void FixedUpdate__()
    //{
    //    float moveHorizontal = Input.GetAxis("Horizontal");
    //    float moveVertival = Input.GetAxis("Vertical");

    //    //compensating position and orientation noise if any
    //    Vector3 pos = rb.position;
    //    pos.y = groundHeight;
    //    Quaternion straightUpRotation = Quaternion.Euler(0, rb.transform.rotation.eulerAngles.y, 0);
    //    rb.MovePosition(pos);
    //    rb.MoveRotation(straightUpRotation);

    //    rb.AddRelativeForce(0.0f, 0.0f, moveVertival * maxForce);
    //    rb.AddRelativeTorque(0.0f, moveHorizontal * maxTorque, 0.0f);
    //}

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray r = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(r, float.PositiveInfinity, 1 << LayerMask.NameToLayer("ground"));
            if (hits.Length > 0)
            {
                if (hits.Length > 1)
                {
                    Debug.LogError("NOOOOOOOOOOOO");
                }
                else
                {
                    //Debug.Log("object hit " + hits[0].collider.gameObject.name + " position " + hits[0].point);
                    goal = hits[0].point;
                    goal.y = transform.position.y;
                    float err = ComputeAngleError() * 180.0f / Mathf.PI;
                    //Debug.Log("Angle error " + err);
                }
            }
        }
    }

    private float ComputeAngleError()
    {
        float angleError = 0;
        Vector3 desiredForward = goal - transform.position;
        if (desiredForward.sqrMagnitude > 0.00001f)
        {
            desiredForward.Normalize();
            angleError = Vector3.Angle(desiredForward, transform.forward) * Mathf.PI / 180.0f;
            if (Vector3.Cross(desiredForward, transform.forward).y < 0)
            {
                angleError *= -1.0f;
            }
        }
        return angleError;
    }
}
