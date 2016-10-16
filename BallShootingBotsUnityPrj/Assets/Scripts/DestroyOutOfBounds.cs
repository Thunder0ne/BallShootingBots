using UnityEngine;
using System.Collections;

public class DestroyOutOfBounds : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    void OnTriggerExit(Collider other)
    {
        GameObject.Destroy(other.gameObject);
    }
}
