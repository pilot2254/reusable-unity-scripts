using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 20f;
    public int currentAmmo = 10;
    public int maxAmmo = 10;
    public bool isReloading = false;
    public float reloadingSpeed = 0.5f;
    public float fireRate = 0.1f;
    private bool isAutomatic = false;
    private float nextFireTime = 0f;

    void Start()
    {
        if (currentAmmo != maxAmmo)
        {
            currentAmmo = maxAmmo;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isAutomatic = !isAutomatic;
            Debug.Log("Fire mode: " + (isAutomatic ? "AUTO" : "SEMI"));
        }

        if (isAutomatic)
        {
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
        }

        if (Input.GetKeyDown(KeyCode.U) && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (currentAmmo > 0 && !isReloading)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.linearVelocity = firePoint.forward * projectileSpeed;

            currentAmmo--;

            Destroy(projectile, 3f);
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;

        while (currentAmmo < maxAmmo)
        {
            yield return new WaitForSeconds(reloadingSpeed);
            currentAmmo++;
        }

        isReloading = false;
    }
}
