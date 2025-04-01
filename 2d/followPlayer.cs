using UnityEngine;

public class SpriteFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player; // Reference to the player object
    [SerializeField] private float followSpeed = 5f; // Speed at which the sprite follows the player
    [SerializeField] private float stoppingDistance = 1f; // Minimum distance to stop following

    private void Update()
    {
        if (player == null)
        {
            Debug.LogWarning("Player Transform not assigned!");
            return;
        }

        // Calculate the distance between the sprite and the player
        float distance = Vector2.Distance(transform.position, player.position);

        // If the distance is greater than the stopping distance, move towards the player
        if (distance > stoppingDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, followSpeed * Time.deltaTime);
        }
    }

    // Optional: Draw the stopping distance in the editor for visualization
    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, stoppingDistance);
        }
    }
}