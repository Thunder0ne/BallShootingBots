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

    private bool avoidanceActive;
    private Vector3 avoidanceGoal;

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
        //FleeBallBehavior();
        AvoidBallsFindGoal();
        if (avoidanceActive)
        {
            GoToGoal(avoidanceGoal);
        }
        else
        {
            GoToGoal(goal);
        }
    }

    void Update()
    {
        if (!avoidanceActive)
        {
            UpdateGoalWithClick();
        }
    }

    private void GoToGoal(Vector3 targetPosition)
    {
        float dt = Time.fixedDeltaTime;
        float control = 0;
        //compute the error
        float angleError = ComputeAngleError(targetPosition);
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


        float distError = Vector3.Distance(transform.position, targetPosition);
        float speedControl = maxForce;
        if (distError < 1.0f)
        {
            float desiredVel = distError;
            float speedError = agentRigidBody.velocity.magnitude - desiredVel;
            speedControl = -kp * speedError;
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

    private void AvoidBallsFindGoal()
    {
        LinkedList<Ball> balls = gameManager.GetBalls();
        LinkedListNode<Ball> iterator = balls.First;
        for (int i = 0; i < balls.Count && iterator != null; i++)
        {
            Ball ball = iterator.Value;
            iterator = iterator.Next;
            float combinedRadius = ball.GetRadius() + GetRadius();
            Vector3 relativeVelocity = agentRigidBody.velocity - ball.GetVelocity();
            Vector3 relativePosition = ball.GetPosition() - GetPosition();
            float relativeDistance = relativePosition.magnitude;//SQUARE ROOT
            if (relativeDistance <= combinedRadius)
            {
                //do something else
                continue;
            }

            float tangetPointDistanceSqr = relativeDistance * relativeDistance
                                            - combinedRadius * combinedRadius;
            float h = tangetPointDistanceSqr / relativeDistance;

            //SQUARE ROOT
            float n = (combinedRadius * relativeDistance) / Mathf.Sqrt(tangetPointDistanceSqr);



            Vector3 fwdCollisionComponent = relativePosition * (h / relativeDistance);
            Vector3 sidewaysCollisionComponent_R = Vector3.Cross(Vector3.up, relativePosition / relativeDistance) * n;
            Vector3 sidewaysCollisionComponent_L = -sidewaysCollisionComponent_R;

            Debug.DrawLine(GetPosition(), GetPosition() + fwdCollisionComponent + sidewaysCollisionComponent_R, Color.yellow);
            Debug.DrawLine(GetPosition(), GetPosition() + fwdCollisionComponent + sidewaysCollisionComponent_L, Color.yellow);

            //checking if the relative velocity is insie the VO (Velocity Obstacle)
            //if the cross product of the relative velocity by the two tangent vectors
            //goes downwards or upwards for BOTH (or one of them is zero) the relative velocity is 
            //outside the VO anbd therefore no collision will occur

            Vector3 t1Cross = Vector3.Cross(relativeVelocity, fwdCollisionComponent + sidewaysCollisionComponent_R);
            Vector3 t2Cross = Vector3.Cross(relativeVelocity, fwdCollisionComponent + sidewaysCollisionComponent_L);
            if (Vector3.Dot(t1Cross, t2Cross) < 0)
            {
                //potential collision
                bool isGoingToCollide = true;
                float vMax = (relativeDistance - combinedRadius) / MAX_PREDICTION_TIME_HORIZON;
                float rVmax = combinedRadius / MAX_PREDICTION_TIME_HORIZON;
                if (Vector3.Dot(relativeVelocity, relativePosition / relativeDistance)
                    < vMax + rVmax)
                {
                    if ((relativeVelocity - relativePosition * ((vMax + rVmax) / relativeDistance)).sqrMagnitude
                        > rVmax * rVmax)
                    {
                        //isGoingToCollide = false;
                    }
                }

                if (isGoingToCollide)
                {
                    Debug.Log("Potential collision detected");
                    //we decided at the current stage that the robot will avoid
                    //the obstacle at top speed, but this needs to be improved

                    //if we choose exactly one of the tangent direction the agent
                    //will touch the obstacle, which is a collision anyway
                    //so we need to pick a direction close to the VO (Velocity Obstacle)
                    //but outside of it
                    //therefore we choose a forward component slilghtly smaller
                    Vector3 fwd = fwdCollisionComponent * 0.9f;
                    Vector3 sideways = sidewaysCollisionComponent_R;
                    if (Vector3.Dot(relativeVelocity, sidewaysCollisionComponent_R) < 0)
                    {
                        sideways = sidewaysCollisionComponent_L;
                    }
                    avoidanceActive = true;
                    //SQUARE ROOT
                    avoidanceGoal = (fwd + sideways).normalized * 1000.0f;//just an arbitrary number
                    //to set the goal far away in order the robot not to slow down
                    //TODO this choice can be optimized
                }
                else
                {
                    avoidanceActive = false;
                }
            }
        }
    }

    private void FleeBallBehavior()
    {
        LinkedList<Ball> balls = gameManager.GetBalls();
        //Please note:!!!
        //we do this like this because we are 100% sure that the list does not 
        //change while we are iterating it
        LinkedListNode<Ball> iterator = balls.First;
        for (int i = 0; i < balls.Count && iterator != null; i++)
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
                    if (t <= MAX_PREDICTION_TIME_HORIZON)
                    {
                        //try and dodge
                        //Debug.LogWarning("collision predicted");
                        Vector3 predictedRelativePosition = relativePosition + relativeVelocity * t;
                        //Please note the predicted relative position multipleir needs to be
                        //more than the slow down distance!
                        Vector3 relativeFleeGoal = (predictedRelativePosition * -5.0f);
                        goal = GetPosition() + relativeFleeGoal;
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
                    float err = ComputeAngleError(goal) * 180.0f / Mathf.PI;
                    //Debug.Log("Angle error " + err);
                }
            }
        }
    }

    private float ComputeAngleError(Vector3 targetPosition)
    {
        float angleError = 0;
        Vector3 desiredForward = targetPosition - transform.position;
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
