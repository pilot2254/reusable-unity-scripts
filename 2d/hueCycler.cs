using UnityEngine;

public class HueCycler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetRenderer; // Assign your sprite renderer here
    [SerializeField] private float cycleSpeed = 1f; // Speed of the hue shift
    [SerializeField] private float saturation = 1f; // Saturation value (0 to 1)
    [SerializeField] private float brightness = 1f; // Brightness value (0 to 1)
    [SerializeField] private float timeScale;

    private float hue; // Current hue value

    void Update()
    {
        Time.timeScale = timeScale;

        // Increment the hue over time
        hue += Time.deltaTime * cycleSpeed;
        if (hue > 1f) hue -= 1f; // Wrap hue to stay within [0, 1]

        // Convert HSV to RGB
        Color newColor = Color.HSVToRGB(hue, saturation, brightness);

        // Apply the color to the sprite renderer
        if (targetRenderer != null)
        {
            targetRenderer.color = newColor;
        }
    }
}

// Ahoj, potrebujem pomoct. Som uplne strateny na hodine Unity v skole. Mame totiz spravit hru "EarthDefender" co je vlastne simple unity hra kde v strede obrazovky mas 2D Circle "Earth" a niaky kanon ktory sa otaca tym smerom kam smeruje 