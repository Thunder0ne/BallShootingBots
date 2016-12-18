using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    [SerializeField]
    float damageValue = 1.0f;

    public TeamId teamId { get; set; }

    public Rigidbody GetRigidBody()
    {
        return ballRigidBody;
    }

    public bool Harmfull { get; private set; }

    public float damage
    {
        get
        {
            return damageValue;
        }
    }

    public float GetRadius()
    {
        return _radius;
    }

    public Vector3 GetVelocity()
    {
        return ballRigidBody.velocity;
    }

    public Vector3 GetPosition()
    {
        return ballRigidBody.position;
    }

    public Vector3 GetVelocityBeforePhysicsUpdate()
    {
        return _velocityBeforePhysicsUpdate;
    }

    // Use this for initialization
    void Awake()
    {
        teamId = TeamId.NoTeam;
        ballRigidBody = GetComponent<Rigidbody>();
        //GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().transform.forward * 10.0f;
        _radius = GetComponent<Renderer>().bounds.extents.x;
    }

    void FixedUpdate()
    {
        _velocityBeforePhysicsUpdate = ballRigidBody.velocity;
    }

    void Update()
    {
        //Debug.Log("ball velocity " + ballRigidBody.velocity);

    }

    private void OnEnable()
    {
        Harmfull = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.LogWarning("Bullet OnCollisionEnter other name " + collision.collider.gameObject.name);

        if (collision.rigidbody != null)
        {
            if (collision.rigidbody.tag == "Wall")
            {
                Harmfull = false;
                //TODO this could be slow, needs optimization
                //AgentGameState agentGameState = collision.rigidbody.gameObject.GetComponent<AgentGameState>();
                //agentGameState.ApplyDamage(damage);
            }
        }
        //GameObject.Destroy(this.gameObject);
    }

    private float _radius;
    private Rigidbody ballRigidBody;

    //void OnTriggerEnter(Collider other)
    //{
    //    Debug.LogWarning("Bullet OnTriggerenter other name " + other.gameObject.name);

    //    else if(other.tag != "Boundary")
    //    {
    //        Debug.LogWarning("Bullet OnTriggerEnter other " + other.gameObject.name);
    //        Destroy(gameObject);
    //    }
    //}

    private Vector3 _velocityBeforePhysicsUpdate;
}
