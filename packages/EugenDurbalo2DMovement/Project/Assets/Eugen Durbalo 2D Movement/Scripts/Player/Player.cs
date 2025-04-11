using Unity.VisualScripting;
using UnityEngine;

public enum PlayerMovementType
{
    None,
    Platformer,
    Topdown
}

public class Player : MonoBehaviour
{
    public static Player Instance;

    public PlayerMovementType playerMovementType;
    public Transform gfx;

    private void Awake()
    {
        Instance = this;

        if(playerMovementType != PlayerMovementType.None)
        {
            if (playerMovementType == PlayerMovementType.Platformer && !GetComponent<PlatformerMovement>()) transform.AddComponent<PlatformerMovement>();
            else if (playerMovementType == PlayerMovementType.Topdown && !GetComponent<TopDownMovement>()) transform.AddComponent<TopDownMovement>();
        }
        else
        {
            if (GetComponent<PlatformerMovement>()) playerMovementType = PlayerMovementType.Platformer;
            else if (GetComponent<TopDownMovement>()) playerMovementType = PlayerMovementType.Topdown;
            else print("You need to add Movement Script to Player GameObject or set Player Movement Type from None (can be found in Player GameObject).");
        }
    }
}
