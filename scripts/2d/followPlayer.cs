using UnityEngine;

/// followPlayer: This script makes a sprite follow the player with configurable speed
/// and stopping distance. It can be used for companions, enemies, collectibles that follow
/// the player, or any object that needs to track and move toward the player.

public class followPlayer : MonoBehaviour
{
    [SerializeField] private Transform player; // Reference to the player's Transform component to track their position
    [SerializeField] private float followSpeed = 5f; // How quickly the sprite moves toward the player (higher = faster)
    [SerializeField] private float stoppingDistance = 1f; // How close the sprite gets before stopping (prevents overlapping)

    private void Update()
    {
        // Safety check to prevent errors if the player reference is missing
        // This will show a warning in the console but won't crash the game
        if (player == null)
        {
            Debug.LogWarning("Player Transform not assigned!");
            return;
        }

        // Calculate the current distance between this sprite and the player
        // Using Vector2 treats both objects as being on the same 2D plane
        float distance = Vector2.Distance(transform.position, player.position);

        // Only move toward the player if we're farther away than the stopping distance
        // This prevents the sprite from constantly moving when it's already close enough
        if (distance > stoppingDistance)
        {
            // Calculate the direction vector toward the player (normalized to get just the direction)
            // This line calculates the direction but doesn't actually use it directly
            Vector2 direction = (player.position - transform.position).normalized;
            
            // Move this sprite toward the player's position at the specified follow speed
            // MoveTowards ensures smooth movement with consistent speed
            transform.position = Vector2.MoveTowards(transform.position, player.position, followSpeed * Time.deltaTime);
        }
    }

    // This method is only used in the Unity Editor for visualization
    // It draws a yellow circle around the player showing the stopping distance
    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, stoppingDistance);
        }
    }
}
