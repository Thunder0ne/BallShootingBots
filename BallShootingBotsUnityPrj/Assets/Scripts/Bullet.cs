using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    float damageValue = 1.0f;

    public float damage
    {
        get
        {
            return damageValue;
        }
    }

	// Use this for initialization
	void Start ()
    {
        GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().transform.forward * 10.0f;
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
