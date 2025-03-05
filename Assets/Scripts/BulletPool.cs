using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance; // Singleton reference

    [Header("Bullet Pool Settings")]
    public GameObject bulletPrefab; // Bullet prefab to instantiate
    public int poolSize = 20; // Number of bullets in the pool

    private Queue<GameObject> bulletPool = new Queue<GameObject>(); // Bullet queue

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //  Initialize the bullet pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false); //  Deactivate bullet before adding to pool
            bulletPool.Enqueue(bullet);
        }
    }

    //  Get a bullet from the pool
    public GameObject GetBullet()
    {
        if (bulletPool.Count > 0)
        {
            GameObject bullet = bulletPool.Dequeue();
            bullet.SetActive(true); //  Activate bullet when taken from pool
            return bullet;
        }
        else
        {
            Debug.LogWarning("Bullet pool is empty! Instantiating a new bullet.");
            return Instantiate(bulletPrefab); // If pool is empty, create a new bullet
        }
    }

    //  Return bullet to pool instead of destroying it
    public void ReturnBullet(GameObject bullet)
    {
        if (bullet == null)
        {
            Debug.LogWarning("Tried to return a null bullet to the pool!");
            return;
        }

        bullet.SetActive(false); //  Ensure bullet is disabled
        bulletPool.Enqueue(bullet); //  Add it back to the pool
    }
}
