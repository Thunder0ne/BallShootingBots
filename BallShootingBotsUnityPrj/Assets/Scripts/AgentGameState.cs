using UnityEngine;
using System.Collections;

public class AgentGameState : MonoBehaviour
{
    [SerializeField]
    private float initialHealth;

    public float health { get; private set; }

	// Use this for initialization
	void Start ()
    {
        health = initialHealth;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ApplyDamage(float damage)
    {
        health -= damage;
        if(health <=0 )
        {
            Destroy(gameObject);
        }
    }
}
