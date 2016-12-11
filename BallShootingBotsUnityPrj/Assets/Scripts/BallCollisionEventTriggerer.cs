using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IBallCollisionListener
{
    void HandleBallCollision(Ball ball);
}

public class BallCollisionEventTriggerer : MonoBehaviour
{
    private LinkedList<IBallCollisionListener> _listeners = new LinkedList<IBallCollisionListener>();
    //std::list

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {        
        Ball ball = collision.collider.gameObject.GetComponent<Ball>();
        if(ball != null)
        {
            LinkedListNode<IBallCollisionListener> listenerIter = _listeners.First;
            int elementsCount = _listeners.Count;

            //redundant for loop just for the sake of security, it could be a while loop
            for (int elementCounter = 0
                ; elementCounter < elementsCount && listenerIter != null
                ; elementCounter++)
            {
                listenerIter.Value.HandleBallCollision(ball);
                listenerIter = listenerIter.Next;
            }
        }
    }

    public void AddBallCollisionListener(IBallCollisionListener listener)
    {
        _listeners.AddLast(listener);
    }

    public void RemoveCollisionListener(IBallCollisionListener listener)
    {
        _listeners.Remove(listener);
    }

}
