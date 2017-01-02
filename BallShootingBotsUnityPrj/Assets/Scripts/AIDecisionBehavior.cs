using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// the decision behaviour decides where to go
/// and passes the position to the aicontroller in order to actually go there
/// 
/// </summary>
public class AIDecisionBehavior : MonoBehaviour
{
    [SerializeField]
    GameManager gameManager;

    private AIController _aiController;
    private DodgeBallPlayer _dodgeBallPlayer;

	// Use this for initialization
	void Start ()
    {
        _dodgeBallPlayer = GetComponent<DodgeBallPlayer>();
        _aiController = GetComponent<AIController>();
	}

    private void FixedUpdate()
    {
        if (_dodgeBallPlayer.HasABall())
        {
            UpdateDecisionHasBallState();
        }
        else
        {
            UpdateDecisionHasNoBallState();
        }
    }

    private void UpdateDecisionHasBallState()
    {
        LinkedList<DodgeBallPlayer> players = gameManager.GetPlayers();
        LinkedListNode<DodgeBallPlayer> iterator = players.First;
        //TODO this decision making is a very first pass,
        //a more detailed decision making process has to be implemented
        //e.g. taking onto account the health of a potential target
        //or the level of threat that it represents
        DodgeBallPlayer closestPlayer = null;
        float minDist = float.MaxValue;
        for (int i = 0; i < players.Count && iterator != null; i++)
        {
            DodgeBallPlayer player = iterator.Value;
            iterator = iterator.Next;
            if (player.GetTeamId() != _dodgeBallPlayer.GetTeamId())
            {
                float dist = Vector3.Distance(player.transform.position, this.transform.position);
                if (dist < minDist)
                {
                    closestPlayer = player;
                    minDist = dist;
                }
            }
        }
        if (closestPlayer != null)
        {

        }
    }

    private void UpdateDecisionHasNoBallState()
    {
        LinkedList<Ball> balls = gameManager.GetBalls();
        LinkedListNode<Ball> iterator = balls.First;
        float minDistance = float.MaxValue;
        Ball closest = null;
        for (int i = 0; i < balls.Count && iterator != null; i++)
        {
            Ball ball = iterator.Value;
            iterator = iterator.Next;
            if (ball.teamId == TeamId.NoTeam || ball.teamId == _dodgeBallPlayer.GetTeamId())
            {
                //check if this ball is on our team or no team
                //pick the closest and tell the ai controller to go there to attempt to collect the ball
                //if none is available 
                //check if there is a "enemy" ball moving slow enough to attempt to collect it
                float dist = Vector3.Distance(this.transform.position, ball.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = ball;
                }
            }
            else
            {

            }
        }
        if (closest != null)
        {
            _aiController.SetGoal(closest.transform.position);
        }
    }

    // Update is called once per frame
    void Update () {
	
	}

    public bool IsBallDangerous(Ball ball)
    {
        return (ball.teamId != TeamId.NoTeam && ball.teamId != _dodgeBallPlayer.GetTeamId());
        //TODO check the relative velocity so that the agent can try and collect it
    }
}
