using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public class AgentGoal
//{
//    public Vector3 position { get; private set; }
//    public float score { get; private set; }

//    public AgentGoal(Vector3 position, float score)
//    {
//        this.position = position;
//        this.score = score;
//    }
//}

public class ContextSteering 
{
    private struct DirectionBounds
    {
        public Vector3 minDir;
        public Vector3 maxDir;

        public DirectionBounds(Vector3 minDir, Vector3 maxDir)
        {
            this.minDir = minDir;
            this.maxDir = maxDir;
        }
    }

    public const int DIRECTION_COUNT = 8;

    public ContextSteering()
    {
        for (int i = 0; i < DIRECTION_COUNT; i++)
        {
            _forbiddenDirections.Add(0);
            //_interestingDirections.Add(0);
        }

        float angle = 0;
        Vector3 dir = Vector3.right;
        float angleStep = (Mathf.PI * 2.0f) / DIRECTION_COUNT;
        for (int i = 0; i < DIRECTION_COUNT; i++)
        {
            float minAngle = angle - angleStep * 0.5f;
            float maxAngle = angle + angleStep * 0.5f;

            float minCompX = Mathf.Cos(minAngle);
            float minCompZ = Mathf.Sin(minAngle);

            Vector3 minDir = new Vector3(minCompX, 0, minCompZ);

            float maxCompX = Mathf.Cos(maxAngle);
            float maxCompZ = Mathf.Sin(maxAngle);
            Vector3 maxDir = new Vector3(maxCompX, 0, maxCompZ);

            _directionBounds.Add(new DirectionBounds(minDir, maxDir));
        }
    }

    //TODO this function will take as input the agent and its
    //_desired_ velocity (that can be computed by the current goal)
    //and will return the best _desired_ velocity compatible with the VO obstacles
    //computed previously
    public void Update(AIController agent)
    {
        for (int i = 0; i < _forbiddenDirections.Count; i++)
        {
            _forbiddenDirections[i] = 0;
        }

        //for (int i = 0; i < _interestingDirections.Count; i++)
        //{
        //    _interestingDirections[i] = 0;
        //}
        List<VelocityObstacle> velocityObstacles = agent.GetVelocityObstacles();
        Transform agentTransform = agent.transform;
        for (int i = 0; i < velocityObstacles.Count; i++)
        {
            VelocityObstacle VO = velocityObstacles[i];
            Vector3 voLeftLocal = agentTransform.InverseTransformVector(VO.VOLeft);
            Vector3 voRightLocal = agentTransform.InverseTransformVector(VO.VORight);
            for (int dirIndex = 0; dirIndex < _directionBounds.Count; dirIndex++)
            {
                DirectionBounds directionBounds = _directionBounds[i];
                int rightIndex = -1;
                int leftIndex = -1;
                if (Vector3.Dot(voRightLocal, directionBounds.minDir) > 0
                    && Vector3.Dot(voRightLocal, directionBounds.maxDir) > 0)
                {
                    if (Vector3.Cross(voRightLocal, directionBounds.minDir).y * Vector3.Cross(voLeftLocal, directionBounds.maxDir).y < 0)
                    {
                        rightIndex = dirIndex;
                    }
                }

                if (Vector3.Dot(voLeftLocal, directionBounds.minDir) > 0 
                    && Vector3.Dot(voLeftLocal, directionBounds.maxDir) > 0)
                {
                    if (Vector3.Cross(voLeftLocal, directionBounds.minDir).y * Vector3.Cross(voLeftLocal, directionBounds.maxDir).y < 0)
                    {
                        leftIndex = dirIndex;
                    }                    
                }

                bool leftIndexReached = false;
                for(int safetyCounter = 0; safetyCounter < DIRECTION_COUNT && !leftIndexReached; safetyCounter++)
                {
                    int index = (rightIndex + safetyCounter) % DIRECTION_COUNT;
                    _forbiddenDirections[index] = -1.0f;
                    leftIndexReached = (index == leftIndex);
                }
            }
        }
        //TODO now in _forbiddenDirections we have stored what directions are forbidden
        //at this point the agent has to compute what is the closest allowed direction
        //to the direction defined by the current goal
        Vector3 goalLocal = agentTransform.InverseTransformPoint(agent.GetGoal());
        

    }

    private List<float> _forbiddenDirections = new List<float>();
    private List<DirectionBounds> _directionBounds = new List<DirectionBounds>();
    //private List<float> _interestingDirections = new List<float>();
}
