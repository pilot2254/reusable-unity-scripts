using UnityEngine;

public class flipOnMove : MonoBehaviour
{
    // The SpriteRenderer component to flip. Auto-assigned if not set
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Flip Settings")]
    // If true, uses flipX. If false, uses flipY
    [SerializeField] private bool useFlipX = true;
    
    // Enable this if your sprite is exported facing left by default
    // This will invert the flip logic
    [SerializeField] private bool invertFlip = false;
    
    // Minimum movement speed required to trigger a flip
    // Prevents flickering when the object is nearly stationary
    [SerializeField] private float flipThreshold = 0.01f;

    [Header("Movement Source")]
    // Optional Rigidbody2D for velocity-based detection
    // If null, falls back to position tracking
    [SerializeField] private Rigidbody2D rb;
    
    // Internal reference to the transform (cached for performance)
    private Transform targetTransform;
    
    // Stores the last frame's position for position-based movement detection
    private Vector3 lastPosition;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Try to get SpriteRenderer from this GameObject if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Try to get Rigidbody2D from this GameObject if not assigned
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        // Cache the transform reference for better performance
        targetTransform = transform;
        
        // Initialize last position to current position
        lastPosition = targetTransform.position;
    }

    // LateUpdate is called after all Update functions have been called
    // This ensures movement has been processed before we check direction
    private void LateUpdate()
    {
        // Get the horizontal movement direction (-1 for left, +1 for right)
        float direction = GetMovementDirection();
        
        // Only flip if movement exceeds the threshold
        // This prevents unwanted flipping when idle or barely moving
        if (Mathf.Abs(direction) > flipThreshold)
        {
            ApplyFlip(direction);
        }
    }

    // Determines the horizontal movement direction
    // Returns: Negative for left movement, positive for right movement
    private float GetMovementDirection()
    {
        // If a Rigidbody2D is attached, use its velocity (more accurate for physics objects)
        if (rb != null)
        {
            return rb.velocity.x;
        }
        // Otherwise, calculate direction from position change (works for non-physics movement)
        else
        {
            float direction = targetTransform.position.x - lastPosition.x;
            lastPosition = targetTransform.position;
            return direction;
        }
    }
    
    // Applies the flip to the sprite based on movement direction
    // Parameters:
    //   direction - The horizontal movement direction (negative = left, positive = right)
    private void ApplyFlip(float direction)
    {
        // By default, flip when moving left (direction < 0)
        bool shouldFlip = direction < 0;
        
        // If invertFlip is enabled, reverse the flip logic
        // Useful for sprites that are drawn facing left by default
        if (invertFlip)
            shouldFlip = !shouldFlip;
        
        // Apply the flip to the selected axis
        if (useFlipX)
        {
            spriteRenderer.flipX = shouldFlip;
        }
        else
        {
            spriteRenderer.flipY = shouldFlip;
        }
    }
}
