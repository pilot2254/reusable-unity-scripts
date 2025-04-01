using UnityEngine;

/// RotateOnMove: This script rotates a 2D object based on its horizontal movement direction.
/// It creates a rolling or spinning effect that responds to the object's movement speed,
/// making objects like wheels, balls, or coins appear to roll naturally when they move.

public class RotateOnMove : MonoBehaviour
{
    [SerializeField] private float baseRotationSpeed = 100f; // The minimum rotation speed when the object is moving
    [SerializeField] private float speedFactor = 1f; // How much the movement speed affects rotation (higher = more responsive)

    private Vector3 previousPosition; // Stores the object's position from the last frame to calculate movement

    private void Start()
    {
        // Initialize the previous position to the current position when the script starts
        // This prevents unexpected rotation on the first frame
        previousPosition = transform.position; // Initialize the previous position
    }

    private void Update()
    {
        // Calculate how fast the object is moving by comparing its current position to its previous position
        // The magnitude gives us the distance moved, divided by Time.deltaTime to get units per second
        float movementSpeed = (transform.position - previousPosition).magnitude / Time.deltaTime;

        // Calculate the final rotation speed by adding the base speed and a speed-dependent component
        // This makes the object rotate faster when it moves faster
        float dynamicRotationSpeed = baseRotationSpeed + (movementSpeed * speedFactor);

        // Determine rotation direction based on horizontal movement
        if (transform.position.x > previousPosition.x) // Moving to the right
        {
            // Rotate clockwise (negative Z rotation) when moving right
            // This simulates the natural rotation of an object rolling to the right
            transform.Rotate(0, 0, -dynamicRotationSpeed * Time.deltaTime); // Rotate clockwise
        }
        else if (transform.position.x < previousPosition.x) // Moving to the left
        {
            // Rotate counterclockwise (positive Z rotation) when moving left
            // This simulates the natural rotation of an object rolling to the left
            transform.Rotate(0, 0, dynamicRotationSpeed * Time.deltaTime); // Rotate counterclockwise
        }

        // Store the current position for the next frame's calculation
        // This allows us to determine the movement direction and speed in the next update
        previousPosition = transform.position;
    }
}