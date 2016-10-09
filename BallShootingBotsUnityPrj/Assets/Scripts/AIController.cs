using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIController : MonoBehaviour
{
    public float maxForce = 5.0f;
    public float maxTorque = 2.0f;
    public Camera mainCamera;

    [SerializeField]
    private GameManager gameManager;

    private Rigidbody agentRigidBody;
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
        agentRigidBody = GetComponent<Rigidbody>();
        groundHeight = agentRigidBody.position.y;
        _radius = GetComponentInChildren<SphereCollider>().radius;
    }

    void FixedUpdate_test_perf()
    {
        agentRigidBody.AddRelativeTorque(0.0f, maxTorque, 0.0f);
        Debug.Log("angular velocity " + agentRigidBody.angularVelocity.magnitude);
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
        agentRigidBody.AddRelativeTorque(0.0f, control, 0.0f);
        //rb.AddRelativeTorque(0.0f, control * maxTorque, 0.0f);


        float distError = Vector3.Distance(transform.position, goal);
        float speedControl = maxForce;
        if (distError < 1.0f)
        {
            float desiredVel = distError;
            float speedError = agentRigidBody.velocity.magnitude - desiredVel;
            speedControl = - kp * speedError;
        }
        agentRigidBody.AddRelativeForce(0.0f, 0.0f, speedControl);

        float sliding = Vector3.Dot(agentRigidBody.velocity, transform.right);
        agentRigidBody.AddRelativeForce(-sliding, 0, 0, ForceMode.VelocityChange);
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
        LinkedList<Ball> balls = gameManager.GetBalls();
        //Please note:!!!
        //we do this like this because we are 100% sure that the list does not 
        //change while we are iterating it
        LinkedListNode<Ball> iterator = balls.First;
        for (int i=0; i < balls.Count && iterator != null; i++)
        {
            Ball ball = iterator.Value;
            iterator = iterator.Next;
            float combinedRadius = ball.GetRadius() + GetRadius();
            Vector3 relativeVelocity = ball.GetVelocity() - agentRigidBody.velocity;
            Vector3 relativePosition = ball.GetPosition() - GetPosition();
            //please remember intersection between ray and sphere
            //and second order equations
            float bCoefficient = 2.0f * Vector3.Dot(relativeVelocity, relativePosition);
            float aCoefficient = relativeVelocity.sqrMagnitude;
            float cCoefficient = relativePosition.sqrMagnitude - combinedRadius * combinedRadius;
            float equationDiscriminant = bCoefficient * bCoefficient - 4.0f * aCoefficient * cCoefficient;
            if (equationDiscriminant >= 0) // potential collision
            {
                float inverseTerm = 1.0f / (2.0f * aCoefficient);
                float sqrRoot = Mathf.Sqrt(equationDiscriminant);
                float t1 = (-bCoefficient - sqrRoot) * inverseTerm;
                float t2 = (-bCoefficient + sqrRoot) * inverseTerm;
                float t = t1;
                if (t1 < 0)
                {
                    t = t2;
                }
                if (t >= 0)
                {
                    if(t <= MAX_PREDICTION_TIME_HORIZON)
                    {
                        //try and dodge
                    }
                }
            }
        }
    }

    private Vector3 GetPosition()
    {
        return agentRigidBody.position;
    }

    private float GetRadius()
    {
        return _radius;
    }

    private void UpdateGoalWithClick()
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

    private float _radius;
    private const float MAX_PREDICTION_TIME_HORIZON = 2.0f;
}
