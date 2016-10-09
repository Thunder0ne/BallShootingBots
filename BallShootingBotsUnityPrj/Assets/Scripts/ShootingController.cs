using UnityEngine;
using System.Collections.Generic;

public class ShootingController : MonoBehaviour
{
    [SerializeField]
    private Transform bulletSpawn;

    [SerializeField]
    private GameObject bullet;

    [SerializeField]
    private float shootingPeriod = 1.0f;

    [SerializeField]
    private GameManager gameManager;


    private float _time;

    // Use this for initialization
    void Start ()
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
	}

	// Update is called once per frame
	void Update ()
    {
        _time += Time.deltaTime;
        if(Input.GetButton("Fire1"))
        {
            if (_time > shootingPeriod)
            {
                _time = 0;
                Shoot();
            }
        }
	}

    private void Shoot()
    {
        GameObject ballGobj = GameObject.Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation) as GameObject;
        Ball ball = ballGobj.GetComponent<Ball>();
        gameManager.AddBall(ball);
    }
}
