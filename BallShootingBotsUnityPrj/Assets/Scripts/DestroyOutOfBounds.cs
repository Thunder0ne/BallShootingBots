using UnityEngine;
using System.Collections;

public class DestroyOutOfBounds : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        Debug.LogWarning("DestroyOutOfBounds OnTriggerExit other " + other.gameObject.name);
        GameObject.Destroy(other.gameObject);
    }
}
