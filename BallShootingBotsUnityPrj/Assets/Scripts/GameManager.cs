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

    private LinkedList<Ball> _balls = new LinkedList<Ball>();
}
