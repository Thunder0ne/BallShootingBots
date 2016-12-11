using UnityEngine;
using System.Collections;

public enum TeamId
{
    TeamA,
    TeamB,
    NoTeam
}

public class DodgeBallPlayer : MonoBehaviour, IBallCollisionListener
{
    [SerializeField]
    Color noBallColor = Color.white;

    [SerializeField]
    Color colorWithTheBall = Color.red;

    [SerializeField]
    GameManager gameManager;

    [SerializeField]
    TeamId teamId;

    // Use this for initialization
    void Start ()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _shootingController = GetComponent<ShootingController>();
        _renderers = GetComponentsInChildren<Renderer>();
        BallCollisionEventTriggerer ballCollisionEventTriggerer = GetComponent<BallCollisionEventTriggerer>();
        ballCollisionEventTriggerer.AddBallCollisionListener(this);
    }

    // Update is called once per frame
    void Update ()
    {
	
	}

    public bool HasABall()
    {
        return _ball != null;
    }

    public void ShootBall()
    {
        if (HasABall())
        {
            _ball.teamId = teamId;
            _shootingController.ShootBall(_ball);
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material.color = noBallColor;
            }
            _ball = null;
        }
    }

    public void HandleBallCollision(Ball ball)
    {
        bool collision = (ball.Harmfull) && (ball.teamId != teamId);
        if(!HasABall())
        {
            if (TryAndCollectBall(ball))
            {
                collision = false;
                gameManager.RemoveBall(ball);
                ball.gameObject.SetActive(false);
                for (int i = 0; i < _renderers.Length; i++)
                {
                    _renderers[i].material.color = colorWithTheBall;
                }
            }
            else
            {
                collision = ball.teamId != teamId;
            }
        }
        if (collision)
        {
            AgentGameState agentGameState = GetComponent<AgentGameState>();
            agentGameState.ApplyDamage(ball.damage);
        }
    }

    private bool TryAndCollectBall(Ball ball)
    {
        //TODO check if successful
        Rigidbody ballRigidBody = ball.GetRigidBody();
        Vector3 relativeVelocity = _rigidBody.velocity - ball.GetVelocityBeforePhysicsUpdate();
        //Debug.LogWarning("my velocity " + _rigidBody.velocity);
        //Debug.LogWarning("ball velocity " + ballRigidBody.velocity);
        //Debug.LogWarning("relative velocity sqr mag "+ relativeVelocity.sqrMagnitude);
        bool result = false;
        
        //TODO this is just test code remove it after testing
        if (relativeVelocity.sqrMagnitude < 81.0f)
        {
            _ball = ball;
            _ball.gameObject.SetActive(false);
            result = true;
        }
        return result;
    }

    private Ball _ball;
    private Rigidbody _rigidBody;
    private ShootingController _shootingController;
    private Renderer[] _renderers;
    public const float INITIAL_BALL_VELOCITY = 10.0f;
}
