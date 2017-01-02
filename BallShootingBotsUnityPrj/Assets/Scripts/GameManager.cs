using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void AddPlayer(DodgeBallPlayer dodgeBallPlayer)
    {
        _players.AddLast(dodgeBallPlayer);
    }

    public void RemovePlayer(DodgeBallPlayer dodgeBallPlayer)
    {
        _players.Remove(dodgeBallPlayer);
    }

    public void AddBall(Ball ball)
    {
        _balls.AddLast(ball);
    }

    public void RemoveBall(Ball ball)
    {
        _balls.Remove(ball);
    }

    /// <summary>
    /// Please do not modify this list
    /// </summary>
    /// <returns></returns>
    public LinkedList<Ball> GetBalls()
    {
        return _balls;
    }
    /// <summary>
    /// Please do not modify this list
    /// </summary>
    /// <returns></returns>
    public LinkedList<DodgeBallPlayer> GetPlayers()
    {
        return _players;
    }

    private LinkedList<Ball> _balls = new LinkedList<Ball>();
    private LinkedList<DodgeBallPlayer> _players = new LinkedList<DodgeBallPlayer>();
}
