// Originally used in https://github.com/mike-forked/Shared_Skyro_Unity_3D_URP/blob/michal-flaska/Assets/PersonalAssets/Scripts/PlayerShooting.cs

// Setup Instructions:
// 1. Attach this script to the player GameObject
// 2. Assign a projectile prefab to the projectilePrefab field in the Inspector
// 3. Create an empty GameObject as a child of the player to serve as the fire point and assign it to firePoint
// 4. Position the fire point where projectiles should spawn (typically at weapon muzzle)

// Dependencies:
// - Projectile prefab must have a Rigidbody component
// - Fire point Transform must be properly positioned and oriented

// Controls:
// - Left Mouse Button: Fire weapon (hold for automatic, click for semi-automatic)
// - U Key: Reload weapon
// - B Key: Toggle between semi-automatic and automatic fire modes

// Configuration:
// - projectileSpeed: How fast projectiles travel (default: 20)
// - maxAmmo: Maximum ammunition capacity (default: 10)
// - reloadingSpeed: Time in seconds between each ammo unit reload (default: 0.5)
// - fireRate: Minimum time between shots in automatic mode (default: 0.1)

using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    // Projectile configuration
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 20f;
    
    // Ammo system
    public int currentAmmo = 10;
    public int maxAmmo = 10;
    public bool isReloading = false;
    public float reloadingSpeed = 0.5f;
    
    // Fire mode configuration
    public float fireRate = 0.1f;
    private bool isAutomatic = false;
    private float nextFireTime = 0f;

    void Start()
    {
        // Ensure ammo starts at maximum capacity
        if (currentAmmo != maxAmmo)
        {
            currentAmmo = maxAmmo;
        }
    }

    void Update()
    {
        // Toggle fire mode between semi-automatic and automatic
        if (Input.GetKeyDown(KeyCode.B))
        {
            isAutomatic = !isAutomatic;
            Debug.Log("Fire mode: " + (isAutomatic ? "AUTO" : "SEMI"));
        }

        // Handle automatic fire mode (hold to shoot)
        if (isAutomatic)
        {
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        // Handle semi-automatic fire mode (click to shoot)
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }

        // Initiate reload when U is pressed
        if (Input.GetKeyDown(KeyCode.U) && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    // Fires a projectile from the fire point if ammo is available and not reloading
    void Shoot()
    {
        if (currentAmmo > 0 && !isReloading)
        {
            // Instantiate projectile at fire point
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            // Apply velocity to projectile
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.linearVelocity = firePoint.forward * projectileSpeed;

            // Decrease ammo count
            currentAmmo--;

            // Clean up projectile after 3 seconds
            Destroy(projectile, 3f);
        }
    }

    // Reloads the weapon one bullet at a time until magazine is full
    
    IEnumerator Reload()
    {
        isReloading = true;

        // Reload ammunition one unit at a time
        while (currentAmmo < maxAmmo)
        {
            yield return new WaitForSeconds(reloadingSpeed);
            currentAmmo++;
        }

        isReloading = false;
    }
}
