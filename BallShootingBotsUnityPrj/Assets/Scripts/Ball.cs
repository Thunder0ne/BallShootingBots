using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    [SerializeField]
    float damageValue = 1.0f;

    private GameManager gameManager;

    void OnDestroy()
    {
        gameManager.RemoveBall(this);
    }

    public void SetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

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
    
    // Use this for initialization
	void Start ()
    {
        ballRigidBody = GetComponent<Rigidbody>();
        GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().transform.forward * 10.0f;
        _radius = GetComponent<SphereCollider>().radius;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.LogWarning("Bullet OnCollisionEnter other name " + collision.collider.gameObject.name);
        
        //TODO double check if the rigidbody of the object we hit is not null!
        if (collision.rigidbody.tag == "Agent")
        {
            //TODO this could be slow, needs optimization
            AgentGameState agentGameState = collision.rigidbody.gameObject.GetComponent<AgentGameState>();
            agentGameState.ApplyDamage(damage);
        }
        GameObject.Destroy(this.gameObject);
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
}
