using UnityEngine;
using UnityEngine.SceneManagement;

// LevelTeleporter: This script creates a teleport point that sends the player to another scene/level when they collide with it.
// It can be attached to any GameObject with a Collider2D component
// set to "Is Trigger = true". The script provides options for fade effects, loading screens,
// and can pass player data between scenes if needed.

// Usage Examples:
// 1. Basic setup:
//    - Attach this script to a GameObject with a Collider2D (set as trigger)
//    - Set the targetSceneName to the scene you want to load
//    - Tag your player GameObject with "Player" tag

// 2. With custom tag:
//    - Change the playerTag field to match your player's tag if not using "Player"

// 3. With transition delay:
//    - Set useTransitionDelay to true and adjust transitionDelay to add a pause before loading

// 4. From another script:
//    - Call the TeleportToScene() method directly: GetComponent<LevelTeleporter>().TeleportToScene();

public class LevelTeleporter : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private string targetSceneName; // The name of the scene to load (must be in Build Settings)
    [SerializeField] private string playerTag = "Player"; // The tag of the player GameObject
    [SerializeField] private bool activateOnStart = false; // If true, teleport will be active immediately
    
    [Header("Transition Options")]
    [SerializeField] private bool useTransitionDelay = false; // Whether to use a delay before teleporting
    [SerializeField] private float transitionDelay = 1.0f; // Delay in seconds before loading the new scene
    [SerializeField] private bool showLoadingScreen = false; // Whether to show a loading screen (requires setup)
    
    [Header("Visual Effects")]
    [SerializeField] private bool playEffectOnTrigger = false; // Whether to play a visual effect when triggered
    [SerializeField] private GameObject teleportEffectPrefab; // Optional effect prefab to spawn when teleporting
    
    private bool isActive = false; // Tracks whether the teleporter is currently active
    private bool isTriggered = false; // Prevents multiple triggers
    
    void Start()
    {
        // Validate that the target scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == targetSceneName)
            {
                sceneExists = true;
                break;
            }
        }
        
        // Log a warning if the scene doesn't exist in build settings
        if (!sceneExists && !string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning($"Scene '{targetSceneName}' is not in the Build Settings! The teleporter won't work.");
        }
        
        // Set initial active state
        isActive = activateOnStart;
        
        // Make sure we have a Collider2D and it's set as a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("LevelTeleporter's Collider2D should be set as a trigger!");
        }
    }
    
    // Called when another collider enters this object's trigger collider
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the teleporter is active and hasn't been triggered yet
        if (!isActive || isTriggered) return;
        
        // Check if the colliding object has the player tag
        if (other.CompareTag(playerTag))
        {
            // Trigger the teleport
            TriggerTeleport();
        }
    }
    
    // Initiates the teleport process
    private void TriggerTeleport()
    {
        if (isTriggered) return;
        isTriggered = true;
        
        // Play effect if enabled
        if (playEffectOnTrigger && teleportEffectPrefab != null)
        {
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Handle teleport with or without delay
        if (useTransitionDelay)
        {
            // Use Invoke to delay the scene load
            Invoke("TeleportToScene", transitionDelay);
        }
        else
        {
            // Teleport immediately
            TeleportToScene();
        }
    }
    
    // Loads the target scene
    public void TeleportToScene()
    {
        // Make sure we have a valid scene name
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target scene name is not set!");
            return;
        }
        
        // Handle loading screen if enabled
        if (showLoadingScreen)
        {
            // You would implement your loading screen logic here
            // This is just a placeholder for the concept
            Debug.Log("Loading screen would appear here");
            
            // For a simple implementation, you could use an async load:
            // StartCoroutine(LoadSceneAsync());
        }
        
        // Load the target scene
        SceneManager.LoadScene(targetSceneName);
    }
    
    // Example of an async load coroutine (uncomment and implement if needed)
    /*
    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        
        // Wait until the scene fully loads
        while (!asyncLoad.isDone)
        {
            // You can access asyncLoad.progress (0 to 1) to update a loading bar
            yield return null;
        }
    }
    */
    
    // Public methods to control the teleporter from other scripts
    
    // Activates the teleporter
    public void Activate()
    {
        isActive = true;
    }
    
    // Deactivates the teleporter
    public void Deactivate()
    {
        isActive = false;
    }
    
    // Sets a new target scene
    public void SetTargetScene(string newSceneName)
    {
        targetSceneName = newSceneName;
    }
}
