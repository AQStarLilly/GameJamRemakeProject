using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 5f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool jumpRequested;
    private bool isDead = false;

    private PlayerInputActions inputActions;

    private static List<PlayerController> allPlayers = new List<PlayerController>(); // Stores all active players
    private static DynamicCamera cameraScript; // Reference to the camera script

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Jump.performed += OnJumpPerformed;

        allPlayers.Add(this); // Register this player/clone
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJumpPerformed;

        inputActions.Player.Disable();

        allPlayers.Remove(this); // Remove from active players list when destroyed
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Assign camera reference if it hasn't been set yet
        if (cameraScript == null)
        {
            cameraScript = Camera.main.GetComponent<DynamicCamera>();
        }
    }

    private void Update()
    {
        if (!isDead && transform.position.y < -3f)
        {
            StartCoroutine(DieAndSwitchControl());
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // Apply movement in world space.
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);

        // Check if the jump input was triggered and the player is grounded.
        if (jumpRequested && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpRequested = true;
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }
    public bool IsFrozen()
    {
        return isDead;
    }

    public static int GetActivePlayerCount()
    {
        return allPlayers.Count; //  Returns how many players/clones exist
    }

    public static void UpdateActivePlayers()
    {
        allPlayers.Clear();
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            allPlayers.Add(player);
        }
    }


    // Coroutine that handles the death sequence and control switch.
    private IEnumerator DieAndSwitchControl()
    {
        isDead = true;

        // Change the player's color to pink.
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.magenta;
        }

        // Freeze the player's movement.
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Slowly fade out the player
        float fadeDuration = 1.5f;
        float elapsedTime = 0f;
        Color initialColor = rend.material.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            rend.material.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        // Remove this player from the active list
        allPlayers.Remove(this);

        //  Reactivate clone plates if only one player remains
        ClonePlate.ResetCloneAvailability();

        //  If only a clone is left, promote it to the main player
        if (allPlayers.Count == 1 && allPlayers[0].CompareTag("PlayerClone"))
        {
            allPlayers[0].tag = "Player";  //  Convert clone to player
            PlayerController.UpdateActivePlayers();
        }

        // If there are remaining players, switch control to them
        if (allPlayers.Count > 0)
        {
            PlayerController newPlayer = allPlayers[0]; // Pick the first remaining player

            // Switch camera to follow the new player
            if (cameraScript != null)
            {
                cameraScript.SwitchToNewTarget(newPlayer.transform);
            }

            // Destroy the dead player
            Destroy(gameObject);
        }
        else
        {
            // No players left, restart the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator DieFromLaser()
    {
        isDead = true;

        //  Change color to indicate they were hit
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.blue;  // Change to blue to show they are frozen
        }

        //  Freeze movement and physics
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true; //  Prevents movement but keeps collisions
        }

        //  Remove from active players
        if (allPlayers.Contains(this))
        {
            allPlayers.Remove(this);
        }

        //  Reactivate clone plates if only one player remains. Might need this later
        ClonePlate.ResetCloneAvailability();

        //  If only one player is left, switch control
        if (allPlayers.Count == 1)
        {
            PlayerController newPlayer = allPlayers[0];
            if (cameraScript != null)
            {
                cameraScript.SwitchToNewTarget(newPlayer.transform);
            }
        }
        else if (allPlayers.Count == 0)
        {
            //  If both players freeze, restart the level
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        //  Disable input so the player can't move after freezing
        DisableInput();

        yield break; //  Exit the coroutine properly
    }

    //  Prevents player from accepting movement input
    private void DisableInput()
    {
        inputActions.Player.Disable();
    }

    public void HandleLaserDeath()
    {
        if (!isDead)
        {
            StartCoroutine(DieFromLaser());
        }
    }

    public static void RemovePlayer(GameObject player)
    {
        PlayerController playerScript = player.GetComponent<PlayerController>();
        if (playerScript != null && allPlayers.Contains(playerScript))
        {
            allPlayers.Remove(playerScript);
        }
    }


    public void HandleInstantDeath()
    {
        if (!isDead)
        {
            StartCoroutine(DieInstantly());
        }
    }

    private IEnumerator DieInstantly()
    {
        isDead = true;

        //  Change color to indicate they were vaporized
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.magenta;  //  Change to magenta for instant kill
        }

        //  Remove from active players
        allPlayers.Remove(this);

        //  Reactivate clone plates if only one player remains
        ClonePlate.ResetCloneAvailability();

        //  If only one player is left, switch control
        if (allPlayers.Count == 1)
        {
            PlayerController newPlayer = allPlayers[0];
            if (cameraScript != null)
            {
                cameraScript.SwitchToNewTarget(newPlayer.transform);
            }

            Destroy(gameObject); //  Remove the dead player from the scene
        }
        else if (allPlayers.Count == 0)
        {
            // If both players die, restart the level
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        yield break; //  Ensures coroutine exits properly
    }
}
