using UnityEngine;

/// HueCycler: This script continuously cycles through colors by modifying the hue value
/// of a sprite renderer. It creates a rainbow effect that can be used for visual effects,
/// power-ups, or to indicate special states in a game.

public class HueCycler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetRenderer; // The sprite renderer component whose color will be changed
    [SerializeField] private float cycleSpeed = 1f; // Controls how quickly the colors cycle (higher = faster cycling)
    [SerializeField] private float saturation = 1f; // Controls color intensity (0 = grayscale, 1 = full color)
    [SerializeField] private float brightness = 1f; // Controls how bright the colors appear (0 = black, 1 = full brightness)
    [SerializeField] private float timeScale; // Controls the game's time scale, affecting all time-based operations

    private float hue; // Tracks the current hue value (0-1 range representing the color wheel)

    void Update()
    {
        // Apply the timeScale value to the global Time.timeScale
        // This affects the speed of all time-dependent operations in the game
        Time.timeScale = timeScale;

        // Increment the hue value based on time and cycle speed
        // Higher cycleSpeed will make colors change faster
        hue += Time.deltaTime * cycleSpeed;
        
        // Reset hue when it exceeds 1 to create a continuous cycle
        // This creates a loop around the color wheel (red->yellow->green->cyan->blue->magenta->red)
        if (hue > 1f) hue -= 1f; // Wrap hue to stay within [0, 1]

        // Convert the HSV (Hue, Saturation, Value/Brightness) values to an RGB Color
        // HSV is more intuitive for cycling through colors than directly manipulating RGB
        Color newColor = Color.HSVToRGB(hue, saturation, brightness);

        // Apply the calculated color to the sprite renderer if it exists
        // This changes the visual appearance of the sprite
        if (targetRenderer != null)
        {
            targetRenderer.color = newColor;
        }
    }
}