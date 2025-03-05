using UnityEngine;

public class Bullet : MonoBehaviour
{
    public LayerMask hitLayer;
    public float bulletSpeed = 10f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Bullet"); // Ensure bullet is on correct layer
    }

    private void OnEnable()
    {
        if (rb != null)
        {
            rb.isKinematic = false; //  Ensure bullets are not kinematic when reactivated
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the bullet hits a player, kill them immediately
        if (other.CompareTag("Player") || other.CompareTag("PlayerClone"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.HandleInstantDeath();
            }

            DeactivateBullet();
            return;
        }

        // Check if the object hit is part of the hitLayer (e.g., walls)
        if (((1 << other.gameObject.layer) & hitLayer) != 0)
        {
            Debug.Log("Bullet hit wall: " + other.gameObject.name);
            DeactivateBullet();
            return;
        }
    }

    //  Function to properly deactivate the bullet
    private void DeactivateBullet()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero; //  Stop movement
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; //  Prevents further physics interactions
        }

        gameObject.SetActive(false); //  Disable before returning to pool
        BulletPool.Instance.ReturnBullet(gameObject);
    }
}
