using UnityEngine;
using UnityEngine.InputSystem;

public class FollowPC : MonoBehaviour
{
    private GameObject _player;
    [SerializeField] private InputAction lookAction;
    
    private const float TurnSpeed = 90f; // Degrees
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        lookAction = InputSystem.actions.FindAction("Player/Look");
    }

    // Update is called once per frame
    void Update()
    {
        // Get player's position and apply it to the current position
        transform.position = _player.transform.position;

        // Get rotation of camera
        Vector3 rotation = transform.eulerAngles;
        
        // Get mouse input axis and translate to camera rotation
        float x = lookAction.ReadValue<Vector2>().y; 
        x *= -1;
        rotation.x += x * TurnSpeed * Time.deltaTime;
        
        float y = lookAction.ReadValue<Vector2>().x;
        rotation.y += y * TurnSpeed * Time.deltaTime;

        // https://discussions.unity.com/t/mathf-clamp-negative-rotation-for-the-10th-million-time/191669/2
        // Check if the Y rotation is below 180, if so, subtract 360 from it as well as the rotation.
        Vector3 finalRotation = new Vector3
        (
            Mathf.Clamp
            (
                rotation.x < 180 ? rotation.x : -(360 - rotation.x), 
                -45, 
                75
            ), 
            rotation.y, 
            0
        );
        
        transform.localEulerAngles = finalRotation;
    }
}
