using UnityEngine;

public class RotateOnMove : MonoBehaviour
{
    [SerializeField] private float baseRotationSpeed = 100f; // Base rotation speed multiplier
    [SerializeField] private float speedFactor = 1f; // Multiplier for rotation speed based on movement speed

    private Vector3 previousPosition; // Track the previous position to calculate movement speed

    private void Start()
    {
        previousPosition = transform.position; // Initialize the previous position
    }

    private void Update()
    {
        // Calculate the object's movement speed based on the change in position
        float movementSpeed = (transform.position - previousPosition).magnitude / Time.deltaTime;

        // Determine the rotation speed dynamically
        float dynamicRotationSpeed = baseRotationSpeed + (movementSpeed * speedFactor);

        // Rotate the object continuously based on movement direction
        if (transform.position.x > previousPosition.x) // Moving to the right
        {
            transform.Rotate(0, 0, -dynamicRotationSpeed * Time.deltaTime); // Rotate clockwise
        }
        else if (transform.position.x < previousPosition.x) // Moving to the left
        {
            transform.Rotate(0, 0, dynamicRotationSpeed * Time.deltaTime); // Rotate counterclockwise
        }

        // Update the previous position for the next frame
        previousPosition = transform.position;
    }
}