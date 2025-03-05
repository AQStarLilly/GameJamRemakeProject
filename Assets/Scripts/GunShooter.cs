using System.Collections;
using UnityEngine;

public class GunShooter : MonoBehaviour
{
    public enum FireMode { Burst, Continuous } // Choose between Burst or Continuous Mode
    [Header("Shooting Mode")]
    public FireMode fireMode = FireMode.Burst; // Default to Burst Mode

    [Header("Bullet Settings")]
    public Transform firePoint;      // Point where bullets are spawned
    public float bulletSpeed = 10f;  // How fast bullets travel
    public float fireRate = 0.1f;    // Time between each bullet
    public int bulletsPerBurst = 5;  // Number of bullets per burst

    [Header("Pause Settings (Burst Mode Only)")]
    public float burstPauseTime = 2f; // Time between bursts

    private void Start()
    {
        // Start the correct firing mode based on the selected option
        if (fireMode == FireMode.Burst)
        {
            StartCoroutine(ShootBurstMode());
        }
        else
        {
            StartCoroutine(ShootContinuousMode());
        }
    }

    private IEnumerator ShootBurstMode()
    {
        while (true) // Keeps looping forever
        {
            for (int i = 0; i < bulletsPerBurst; i++)
            {
                ShootBullet();
                yield return new WaitForSeconds(fireRate); // Wait between each bullet
            }

            yield return new WaitForSeconds(burstPauseTime); // Pause between bursts
        }
    }

    private IEnumerator ShootContinuousMode()
    {
        while (true) // Shoots nonstop
        {
            ShootBullet();
            yield return new WaitForSeconds(fireRate); // No pause, just fire continuously
        }
    }

    private void ShootBullet()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint is missing in GunShooter!");
            return;
        }

        // Get a bullet from the pool
        GameObject bullet = BulletPool.Instance.GetBullet();

        if (bullet == null)
        {
            Debug.LogWarning("No bullets available in pool!");
            return;
        }

        //  Reset bullet position and rotation to firePoint
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        bullet.SetActive(true); //  Ensure bullet is active

        //  Ensure bullet has a Rigidbody and apply movement force
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; //  Ensure it's not kinematic
            rb.velocity = Vector3.zero; //  Reset old velocity
            rb.angularVelocity = Vector3.zero; //  Reset any rotation effects
            rb.AddForce(firePoint.forward * bulletSpeed, ForceMode.Impulse); //  Apply movement
        }
        else
        {
            Debug.LogWarning("Bullet is missing Rigidbody!");
        }

        //  Ignore collision between bullet and shooter
        Collider bulletCollider = bullet.GetComponent<Collider>();
        Collider shooterCollider = firePoint.parent?.GetComponent<Collider>();

        if (bulletCollider != null && shooterCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, shooterCollider, true);
        }

        // Return bullet to pool after 5 seconds if it doesn't hit anything
        StartCoroutine(ReturnBulletToPool(bullet));
    }



    private IEnumerator ReturnBulletToPool(GameObject bullet)
    {
        yield return new WaitForSeconds(5f); // Bullet lifespan

        if (bullet != null && bullet.activeInHierarchy) //  Ensure bullet exists before returning
        {
            BulletPool.Instance.ReturnBullet(bullet);
        }
    }
}

