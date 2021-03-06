﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct VelocityObstacle
{
    public Vector3 VOLeft;
    public Vector3 VORight;

    public VelocityObstacle(Vector3 VOLeft, Vector3 VORight)
    {
        this.VOLeft = VOLeft;
        this.VORight = VORight;
    }
}

public class AIController : MonoBehaviour
{
    public float maxForce = 5.0f;
    public float maxTorque = 2.0f;
    public Camera mainCamera;

    [SerializeField]
    private GameManager gameManager;

    private AIDecisionBehavior _aiDecisionBehaviour;


    //float at max angul vel of 3.6 Radians / sec
    //good params are kp = 3.75 and kd = 0.65

    void Awake()
    {
        Debug.Log("Awake called");
        objectiveGoal = transform.position;
    }

    void Start()
    {
        Debug.Log("Start called");
        agentRigidBody = GetComponent<Rigidbody>();
        //groundHeight = agentRigidBody.position.y;
        _radius = GetComponentInChildren<SphereCollider>().radius;
        _aiDecisionBehaviour = GetComponent<AIDecisionBehavior>();
#if QUICK_TEST
        objectiveGoal = new Vector3(-13.0f, 0.5f, 0.0f);
#endif
    }

    void FixedUpdate_test_perf()
    {
        agentRigidBody.AddRelativeTorque(0.0f, maxTorque, 0.0f);
        Debug.Log("angular velocity " + agentRigidBody.angularVelocity.magnitude);
    }

    void FixedUpdate()//_actual_control()//
    {
        bool currAvoidanceActive = AvoidBallsFindGoal();

        //TODO this is a debug code fragment: remove after testing
        if (_avoidanceActive != currAvoidanceActive)
        {
            if (currAvoidanceActive)
            {
                _avoideObstacleTimer = 0;
                _avoidanceActive = currAvoidanceActive;
            }
            else
            {
                if (_avoideObstacleTimer < MAX_PREDICTION_TIME_HORIZON * 0.5f)
                {
                    //Debug.Log("Keepping current avoidance behavior + _avoideObstacleTimer " + _avoideObstacleTimer);
                }
                else
                {
                    _avoidanceActive = currAvoidanceActive;
                }
            }
        }

        if (_avoidanceActive != currAvoidanceActive)
        {
            //string behaviorName = _avoidanceActive ? "Avoidance" : "Go To Goal";
            //Debug.LogWarning("Changed behaviour to " + behaviorName);
        }

        if (_avoidanceActive)
        {
            _avoideObstacleTimer += Time.fixedDeltaTime;
            GoToGoal(avoidanceGoal);
        }
        else
        {
            GoToGoal(objectiveGoal);
        }                
    }

    void Update()
    {
        if (!_avoidanceActive)
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

    public List<VelocityObstacle> GetVelocityObstacles()
    {
        return _velocityObstacles;
    }

    public Vector3 GetGoal()
    {
        return objectiveGoal;
    }

    public void SetGoal(Vector3 position)
    {
        objectiveGoal = position;
        objectiveGoal.y = transform.position.y;
    }

    private bool AvoidBallsFindGoal()
    {
        _velocityObstacles.Clear();
        LinkedList<Ball> balls = gameManager.GetBalls();
        LinkedListNode<Ball> iterator = balls.First;
        bool potentialCollisionDetected = false;
        for (int i = 0; i < balls.Count && iterator != null; i++)
        {
            Ball ball = iterator.Value;
            //this is some sort of patch, improve the code design
            if (!_aiDecisionBehaviour.IsBallDangerous(ball))
            {
                continue;
            }
            iterator = iterator.Next;
            float combinedRadius = ball.GetRadius() * 1.1f + GetRadius() * 1.1f;
            Vector3 relativeVelocity = agentRigidBody.velocity - ball.GetVelocity();

            //Debug.DrawLine(GetPosition(), GetPosition() + relativeVelocity, Color.green);


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
            float tangetPointDistance = Mathf.Sqrt(tangetPointDistanceSqr);
            float n = (combinedRadius * relativeDistance) / tangetPointDistance;



            Vector3 fwdCollisionComponent = relativePosition * (h / relativeDistance);
            Vector3 sidewaysCollisionComponent_R = Vector3.Cross(Vector3.up, relativePosition / relativeDistance) * n;
            Vector3 sidewaysCollisionComponent_L = -sidewaysCollisionComponent_R;

            Debug.DrawLine(GetPosition(), GetPosition() + fwdCollisionComponent + sidewaysCollisionComponent_R, Color.yellow);
            Debug.DrawLine(GetPosition(), GetPosition() + fwdCollisionComponent + sidewaysCollisionComponent_L, Color.yellow);

            Debug.DrawLine(GetPosition(), GetPosition() + agentRigidBody.velocity, Color.blue);


            //checking if the relative velocity is insie the VO (Velocity Obstacle)
            //if the cross product of the relative velocity by the two tangent vectors
            //goes downwards or upwards for BOTH (or one of them is zero) the relative velocity is 
            //outside the VO anbd therefore no collision will occur
            Vector3 VOLegRight = fwdCollisionComponent + sidewaysCollisionComponent_R;
            Vector3 VOLegLeft = fwdCollisionComponent + sidewaysCollisionComponent_L;
            Vector3 t1Cross = Vector3.Cross(relativeVelocity, VOLegRight);
            Vector3 t2Cross = Vector3.Cross(relativeVelocity, VOLegLeft);
            if (Vector3.Dot(relativeVelocity, VOLegRight) > 0
                && Vector3.Dot(relativeVelocity, VOLegLeft) > 0
                && Vector3.Dot(t1Cross, t2Cross) < 0)
            {
                //potential collision

                //the direction of the velocity is within the forbidden velocity obstacle
                //now we need to check if the velocity is big enough given the time horizon
                //(or predictiontime) to cause a collision.                
                bool isGoingToCollide = true;
                float rVmax = combinedRadius / MAX_PREDICTION_TIME_HORIZON;
                Vector3 centerOfMaxVel = relativePosition / MAX_PREDICTION_TIME_HORIZON;
                //TODO optimize and use the sqr mag instead
                float velDistToCenterOfMaxVel = (relativeVelocity - centerOfMaxVel).magnitude;
                if(velDistToCenterOfMaxVel > rVmax)
                {
                    //over here
                    //compare the projection of 
                    //the velocity on the velocity obstacle leg
                    //and compare it to tangetPointDistanceSqr scaled by the time horizon
                    VOLegRight.Normalize();
                    VOLegLeft.Normalize();
                    float velProjOnVOLeg = Mathf.Max(Vector3.Dot(relativeVelocity, VOLegRight)
                        , Vector3.Dot(relativeVelocity, VOLegLeft));
                    if (velProjOnVOLeg < (tangetPointDistance / MAX_PREDICTION_TIME_HORIZON))
                    {
                        //Debug.Log("Collision is too far away in time");
                        isGoingToCollide = false;
                    }
                    else
                    {
                        //Debug.Log("vel proj r leg" + Vector3.Dot(relativeVelocity, VOLegRight));
                        //Debug.Log("vel proj l leg" + Vector3.Dot(relativeVelocity, VOLegLeft));
                        //Debug.Log("tang dist scaled" + (tangetPointDistance / MAX_PREDICTION_TIME_HORIZON));
                        //Debug.Log("Collision is within time horizon");
                    }
                }

                if (isGoingToCollide)
                {
                    //Debug.Log("Potential collision detected");
                    //we decided at the current stage that the robot will avoid
                    //the obstacle at top speed, but this needs to be improved

                    //if we choose exactly one of the tangent direction the agent
                    //will touch the obstacle, which is a collision anyway
                    //so we need to pick a direction close to the VO (Velocity Obstacle)
                    //but outside of it
                    //therefore we choose a forward component slilghtly smaller
                    //Vector3 fwd = fwdCollisionComponent * 0.9f;
                    Vector3 sideways = sidewaysCollisionComponent_R;
                    if (Vector3.Dot(relativeVelocity, sidewaysCollisionComponent_R) < 0)
                    {
                        sideways = sidewaysCollisionComponent_L;
                    }
                    //_avoidanceActive = true;
                    potentialCollisionDetected = true;
                    //SQUARE ROOT
                    //Vector3 desiredDirection = (fwdCollisionComponent + sideways).normalized * maxSpeed;

                    //WARNING non optinmized code: it needs optimizations!!!!
                    Vector3 desiredRelVel = (fwdCollisionComponent + sideways).normalized * relativeVelocity.magnitude;
                    Vector3 desiredAbsoluteVel = desiredRelVel + ball.GetVelocity();
                    avoidanceGoal = GetPosition() + desiredAbsoluteVel;
                    //avoidanceGoal = GetPosition() + desiredDirection * 100.0f;
                    //avoidanceGoal = GetPosition() + (fwdCollisionComponent + sideways).normalized * 1000.0f;//just an arbitrary number
                    Debug.DrawLine(GetPosition(), avoidanceGoal, Color.red);

                    Debug.DrawLine(GetPosition(), GetPosition() + relativeVelocity, Color.magenta);

                    _velocityObstacles.Add(new VelocityObstacle(VOLegLeft, VOLegRight));

                    //to set the goal far away in order the robot not to slow down
                    //TODO this choice can be optimized
                }
                //else
                //{
                //    _avoidanceActive = false;
                //}
            }
            //else
            //{
            //    _avoidanceActive = false;
            //}
        }
        return potentialCollisionDetected;
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
                        objectiveGoal = GetPosition() + relativeFleeGoal;
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
                    objectiveGoal = hits[0].point;
                    objectiveGoal.y = transform.position.y;
                    //float err = ComputeAngleError(objectiveGoal) * 180.0f / Mathf.PI;
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

    private Rigidbody agentRigidBody;
    //private float groundHeight;
    private Vector3 objectiveGoal;
    private float prevError;

    private bool _avoidanceActive;
    private Vector3 avoidanceGoal;

    private float _radius;
    private const float MAX_PREDICTION_TIME_HORIZON = 1.0f;
    private float _avoideObstacleTimer;

    private List<VelocityObstacle> _velocityObstacles = new List<VelocityObstacle>();
}
