using UnityEngine;
using System.Collections;

public class CameraFrustumAdapter : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToView;

	// Use this for initialization
	void Start ()
    {
        //assuming the object has the correct rotation
        //and its x is along the camera width and the Z along the camera height
        _thisCamera = this.GetComponent<Camera>();
        Renderer[] renderers = objectToView.GetComponentsInChildren<Renderer>(true);
        Bounds objectBounds = new Bounds();                
        for (int i = 0; i < renderers.Length; i++)
        {
            if (objectBounds.size.sqrMagnitude > 0)
            {
                objectBounds.Encapsulate(renderers[i].bounds);
            }
            else
            {
                objectBounds.SetMinMax(renderers[i].bounds.min, renderers[i].bounds.max);
            }
        }
        _thisCamera.orthographic = true;
        _thisCamera.aspect = objectBounds.size.x / objectBounds.size.z;
        //aspect = width / height
        if (_thisCamera.aspect > 1.0f)
        {
            _thisCamera.orthographicSize = objectBounds.size.z * 0.5f;// * 0.5f;
        }
        else
        {
            float desiredWidth = objectBounds.size.x;
            float desiredHeight = desiredWidth / _thisCamera.aspect;
            _thisCamera.orthographicSize = desiredHeight * 0.5f;
        }
        _thisCamera.transform.position = objectToView.transform.position + Vector3.up * (objectBounds.size.y + _thisCamera.nearClipPlane + 10.0f);
    }

    // Update is called once per frame
    void Update ()
    {
        _thisCamera.aspect = Screen.width / (float)Screen.height;
	}

    private Camera _thisCamera;
}
