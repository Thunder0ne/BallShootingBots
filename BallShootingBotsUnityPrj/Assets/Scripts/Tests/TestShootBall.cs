using UnityEngine;
using System.Collections;

public class TestShootBall : MonoBehaviour {

	// Use this for initialization
	void Start () {
        ShootingController shootingController = GetComponent<ShootingController>();
        shootingController.ShootBall(this.GetComponent<Ball>());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
