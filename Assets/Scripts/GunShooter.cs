using System.Collections;
using UnityEngine;

public class GunShooter : MonoBehaviour
{
    [Header("Bullet Settings")]
    public Transform firePoint;      //  Point where bullets are spawned
    public float bulletSpeed = 10f;  //  How fast bullets travel
    public float fireRate = 0.3f;    //  Time between each bullet (within a burst)
    public int bulletsPerBurst = 5;  //  Number of bullets per burst

    [Header("Pause Settings")]
    public float burstPauseTime = 2f; //  Time between bursts

    private void Start()
    {
        StartCoroutine(ShootLoop());
    }

    private IEnumerator ShootLoop()
    {
        while (true)
        {
            yield return StartCoroutine(FireBurst());
            yield return new WaitForSeconds(burstPauseTime);
        }
    }

    private IEnumerator FireBurst()
    {
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            ShootBullet();
            yield return new WaitForSeconds(fireRate);
        }
    }

    private void ShootBullet()
    {
        if (firePoint == null) return;

        // Get a bullet from the pool
        GameObject bullet = BulletPool.Instance.GetBullet();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;

        //  Make the bullet move forward
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * bulletSpeed;
        }

        //  Ignore collision between bullet and shooter
        Physics.IgnoreCollision(bullet.GetComponent<Collider>(), firePoint.parent.GetComponent<Collider>(), true);

        //  Return bullet to pool after 5 seconds if it doesn't hit anything
        StartCoroutine(ReturnBulletToPool(bullet));
    }

    private IEnumerator ReturnBulletToPool(GameObject bullet)
    {
        yield return new WaitForSeconds(5f); //  Bullet lifespan
        BulletPool.Instance.ReturnBullet(bullet);
    }
}