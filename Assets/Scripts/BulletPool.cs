using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance; // Singleton for easy access

    [Header("Pool Settings")]
    public GameObject bulletPrefab;   //  Assign bullet prefab in Inspector
    public int poolSize = 10;         //  Number of bullets to keep in pool

    private Queue<GameObject> bulletPool = new Queue<GameObject>();

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

        FillPool();
    }

    private void FillPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false); //  Hide bullet initially
            bulletPool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        if (bulletPool.Count == 0) //  If pool is empty, create a new bullet
        {
            GameObject newBullet = Instantiate(bulletPrefab);
            return newBullet;
        }

        GameObject bullet = bulletPool.Dequeue();
        bullet.SetActive(true);
        return bullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}