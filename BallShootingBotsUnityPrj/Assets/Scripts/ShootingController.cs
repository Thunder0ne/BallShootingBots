using UnityEngine;
using System.Collections.Generic;

public class ShootingController : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    public const float INITIAL_BALL_VELOCITY = 10.0f;

    // Use this for initialization
    void Awake ()
    {
        //ShootingBarrel[] shootingBarrelsAttachedScripts = this.GetComponentsInChildren<ShootingBarrel>(true);
        //if(shootingBarrelsAttachedScripts.Length != 1)
        //{
        //    Debug.LogError("Noooooo");
        //    return;
        //}

        //shotsStartPosition = shootingBarrelsAttachedScripts[0].gameObject;

        //search for a game object in the children 
        //by name?, by tag
        //if (this.GetType() == typeof(ShootingController))
        //if (this is ShootingController)
        //{
        //    System.Attribute[] attributes = System.Attribute.GetCustomAttributes(this.GetType());
        //    for(int i = 0; i < attributes.Length; i++)
        //    {
        //        System.Attribute attr = attributes[i];
        //        Debug.Log("attribute " + attr.GetType().ToString());
        //    }
        //}
        _ballSpawn = GetComponentInChildren<ShootingBarrel>().gameObject.transform;
    }

	// Update is called once per frame
	void Update ()
    {
        
	}

    public void ShootBall(Ball ball)
    {
        ball.transform.position = _ballSpawn.position;
        ball.transform.rotation = _ballSpawn.rotation;
        ball.gameObject.SetActive(true);
        Rigidbody ballRigidBody = ball.GetRigidBody();
        ballRigidBody.velocity = this.transform.forward * INITIAL_BALL_VELOCITY;
        gameManager.AddBall(ball);
    }

    private Transform _ballSpawn;
}
