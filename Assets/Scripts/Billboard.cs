using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _cam;

    public bool rotateY;
    public float rotateAmount = -70;
    public bool following = false; // This controls if the billboard follows the camera
    public GameObject target; // The target to follow if following a target
    public Vector3 offset; // The offset following the object
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    private void SetPosition()
    {
        if (following)
        {
            transform.position = target.transform.position + offset;
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        transform.LookAt(new Vector3(_cam.transform.position.x, (rotateY ? _cam.transform.position.y : -90), _cam.transform.position.z));
        transform.Rotate((rotateAmount - 180) * (rotateY ? 1f : 0f), 0, 0);
        SetPosition();
    }
}
