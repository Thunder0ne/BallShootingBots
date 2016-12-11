using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    public float maxForce = 5.0f;
    public float maxTorque = 2.0f;

    private Rigidbody rb;
    //private float groundHeight;

    //Please note: we do not need to remove this listener on destroy
    //because both the event triggerer and the listener belong to the same gameobject
    //adn we are assuming thet their life span is the same

    //private void OnDestroy()
    //{
    //    BallCollisionEventTriggerer ballCollisionEventTriggerer = GetComponent<BallCollisionEventTriggerer>();
    //    ballCollisionEventTriggerer.RemoveCollisionListener(this);
    //}

    void Start()
    {
        Debug.Log("Start called");
        rb = GetComponent<Rigidbody>();
        _dodgeBallPlayer = GetComponent<DodgeBallPlayer>();
        //groundHeight = rb.position.y;        
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertival = Input.GetAxis("Vertical");

        ////compensating position and orientation noise if any
        //Vector3 pos = rb.position;
        //pos.y = groundHeight;
        //Quaternion straightUpRotation = Quaternion.Euler(0, rb.transform.rotation.eulerAngles.y, 0);
        //rb.MovePosition(pos);
        //rb.MoveRotation(straightUpRotation);

        rb.AddRelativeForce(0.0f, 0.0f, moveVertival * maxForce);
        rb.AddRelativeTorque(0.0f, moveHorizontal * maxTorque, 0.0f);

        float sliding = Vector3.Dot(rb.velocity, transform.right);
        rb.AddRelativeForce(-sliding, 0, 0, ForceMode.VelocityChange);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (_dodgeBallPlayer.HasABall())
            {
                _dodgeBallPlayer.ShootBall();
            }
        }
    }

    private DodgeBallPlayer _dodgeBallPlayer;
}
