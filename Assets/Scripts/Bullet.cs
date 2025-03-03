using UnityEngine;

public class Bullet : MonoBehaviour
{
    public LayerMask hitLayer; 
    public float bulletSpeed = 10f;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Bullet"); // Ensure bullet is on correct layer
    }

    private void Update()
    {
        //  Move the bullet forward manually
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) //  Use Trigger Instead of Collision
    {
        //  If the bullet hits a player, kill them immediately
        if (other.CompareTag("Player") || other.CompareTag("PlayerClone"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.HandleInstantDeath(); //  Kill player on hit
            }
            
            Destroy(gameObject); //  Destroy bullet after hitting the player
            return;
        }

        //  Check if the object hit is part of the hitLayer (e.g., walls)
        if (((1 << other.gameObject.layer) & hitLayer) != 0)
        {
            Destroy(gameObject); //  Destroy bullet if it hits a hitLayer object
            return;
        }
    }
}