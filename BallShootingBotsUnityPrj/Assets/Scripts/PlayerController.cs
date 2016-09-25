using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float maxForce = 5.0f;
    public float maxTorque = 2.0f;

    private Rigidbody rb;
    private float groundHeight;

    void Start()
    {
        Debug.Log("Start called");
        rb = GetComponent<Rigidbody>();
        groundHeight = rb.position.y;
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
}
