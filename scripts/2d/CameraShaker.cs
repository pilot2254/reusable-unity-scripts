using UnityEngine;

// CameraShaker: This script provides a versatile camera shake effect for 2D games.
// It can be triggered from other scripts to create screen shake effects during
// explosions, impacts, or other dramatic moments. The shake can be customized
// in terms of duration, intensity, and decay rate.

// Usage Examples:
// 1. Get reference to the camera shaker:
//    CameraShaker shaker = Camera.main.GetComponent<CameraShaker>();

// 2. Use default shake parameters:
//    shaker.Shake();

// 3. Customize the shake (duration, intensity):
//    shaker.Shake(0.3f, 0.5f);

// 4. Fully customize (duration, intensity, decay):
//    shaker.Shake(0.7f, 1.0f, 2.0f);

// Typical use cases:
// - Player taking damage: shaker.Shake(0.2f, 0.3f);
// - Explosion effect: shaker.Shake(0.5f, 0.8f, 1.2f);
// - Boss impact: shaker.Shake(0.7f, 1.0f, 0.8f);

public class CameraShaker : MonoBehaviour
{
    [SerializeField] private float defaultShakeDuration = 0.5f; // Default duration of the shake effect in seconds
    [SerializeField] private float defaultShakeIntensity = 0.7f; // Default intensity/magnitude of the shake
    [SerializeField] private float defaultShakeDecay = 1.5f; // How quickly the shake effect fades out (higher = faster fade)
    
    private Vector3 originalPosition; // Stores the camera's starting position
    private float currentShakeDuration = 0f; // Tracks the remaining time for the current shake
    private float currentShakeIntensity = 0f; // Tracks the current intensity of the shake
    private float currentShakeDecay = 0f; // Tracks the decay rate for the current shake
    
    void Awake()
    {
        // Store the original position of the camera when the script initializes
        originalPosition = transform.localPosition;
    }
    
    void Update()
    {
        // If there's an active shake effect (duration > 0)
        if (currentShakeDuration > 0)
        {
            // Calculate a random position offset based on the current intensity
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeIntensity;
            
            // For 2D games, we typically only want to shake in X and Y directions
            shakeOffset.z = 0;
            
            // Apply the random offset to the camera position
            transform.localPosition = originalPosition + shakeOffset;
            
            // Reduce the remaining shake duration based on time
            currentShakeDuration -= Time.deltaTime;
            
            // Gradually reduce the shake intensity based on the decay rate
            // This creates a more natural effect where the shake fades out over time
            currentShakeIntensity = Mathf.Lerp(currentShakeIntensity, 0f, currentShakeDecay * Time.deltaTime);
            
            // If the shake effect has completed, reset the camera position
            if (currentShakeDuration <= 0)
            {
                currentShakeDuration = 0f;
                transform.localPosition = originalPosition;
            }
        }
    }
    
    // Triggers a camera shake with default parameters
    public void Shake()
    {
        Shake(defaultShakeDuration, defaultShakeIntensity, defaultShakeDecay);
    }
    
    // Triggers a camera shake with custom duration and intensity, using default decay
    public void Shake(float duration, float intensity)
    {
        Shake(duration, intensity, defaultShakeDecay);
    }
    
    // Triggers a camera shake with fully customized parameters
    public void Shake(float duration, float intensity, float decay)
    {
        // Only start a new shake if it would be more intense than the current one
        // This prevents smaller shakes from overriding larger ones that are in progress
        if (intensity >= currentShakeIntensity)
        {
            currentShakeDuration = duration;
            currentShakeIntensity = intensity;
            currentShakeDecay = decay;
        }
    }
    
    // Stops any active camera shake immediately and resets the camera position
    public void StopShake()
    {
        currentShakeDuration = 0f;
        currentShakeIntensity = 0f;
        transform.localPosition = originalPosition;
    }
}
