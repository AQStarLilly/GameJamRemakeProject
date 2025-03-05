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
    private bool isFrozen = false; // Track frozen state

    private PlayerInputActions inputActions;

    private static List<PlayerController> allPlayers = new List<PlayerController>(); // Active players
    private static DynamicCamera cameraScript;

    private AudioSource footstepAudio;
    private bool isMoving = false;

    private static bool isReloadingScene = false; // Prevent multiple scene reloads

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

        SceneManager.sceneLoaded += ResetSceneChangingFlag;

        allPlayers.Add(this);
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJumpPerformed;

        SceneManager.sceneLoaded -= ResetSceneChangingFlag;

        inputActions.Player.Disable();

        allPlayers.Remove(this);
        CheckForRespawn();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        footstepAudio = GetComponent<AudioSource>();

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
        if (isDead || isFrozen) return;

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);

        bool isCurrentlyMoving = moveInput != Vector2.zero && IsGrounded();
        if (isCurrentlyMoving && !isMoving)
        {
            if (!footstepAudio.isPlaying) footstepAudio.Play();
            isMoving = true;
        }
        else if (!isCurrentlyMoving && isMoving)
        {
            footstepAudio.Stop();
            isMoving = false;
        }

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
        return isFrozen;
    }

    public void ResetFrozenState()
    {
        isFrozen = false;
        isDead = false;

        //  Ensure Rigidbody exists before modifying
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        //  Ensure Renderer exists before modifying
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && rend.material != null)
        {
            rend.material.color = Color.white;
        }

        //  Re-enable input actions
        if (inputActions != null)
        {
            inputActions.Player.Enable();
        }
        else
        {
            Debug.LogWarning("InputActions is null on ResetFrozenState()");
        }
    }

    private IEnumerator DieAndSwitchControl()
    {
        isDead = true;
        isFrozen = true; // Mark as frozen when dying

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.magenta;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

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

        allPlayers.Remove(this);
        ClonePlate.HandleCloneDeath(gameObject);

        if (allPlayers.Count == 1 && allPlayers[0].CompareTag("PlayerClone"))
        {
            allPlayers[0].tag = "Player";
        }

        if (allPlayers.Count > 0)
        {
            PlayerController newPlayer = allPlayers[0];

            if (cameraScript != null)
            {
                cameraScript.SwitchToNewTarget(newPlayer.transform);
            }

            Destroy(gameObject);
        }
        else
        {
            CheckForRespawn();
        }
    }

    private IEnumerator DieFromLaser()
    {
        isDead = true;
        isFrozen = true;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.blue;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        allPlayers.Remove(this);
        ClonePlate.ResetCloneAvailability();

        if (allPlayers.Count == 1)
        {
            PlayerController newPlayer = allPlayers[0];
            if (cameraScript != null)
            {
                cameraScript.SwitchToNewTarget(newPlayer.transform);
            }
        }
        else
        {
            CheckForRespawn();
        }

        DisableInput();
        yield break;
    }

    private static bool isSceneChanging = false;

    private static void CheckForRespawn()
    {
        if (isSceneChanging)
        {
            Debug.Log("Scene is changing, skipping respawn check.");
            return; //  If we're transitioning, don't reload the scene
        }

        if (!isReloadingScene && allPlayers.Count == 0)
        {
            isReloadingScene = true;
            Debug.Log("All players gone. Restarting level...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    //  Method to manually set scene transition status
    public static void SetSceneChanging(bool value)
    {
        isSceneChanging = value;
        Debug.Log("Scene changing state set to: " + value);
    }

    //  Reset the scene-changing flag when a new scene loads
    private static void ResetSceneChangingFlag(Scene scene, LoadSceneMode mode)
    {
        isSceneChanging = false;
        isReloadingScene = false; //  Reset reload flag too
        Debug.Log("New scene loaded. Resetting scene-changing state.");
    }

    public static void RemovePlayer(GameObject player)
    {
        PlayerController playerScript = player.GetComponent<PlayerController>();
        if (playerScript != null && allPlayers.Contains(playerScript))
        {
            allPlayers.Remove(playerScript);
            CheckForRespawn(); // Ensure scene reload happens if needed
        }
    }

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
        isFrozen = true;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.magenta;
        }

        allPlayers.Remove(this);
        ClonePlate.HandleCloneDeath(gameObject);

        if (allPlayers.Count == 1)
        {
            PlayerController newPlayer = allPlayers[0];
            if (cameraScript != null)
            {
                cameraScript.SwitchToNewTarget(newPlayer.transform);
            }

            Destroy(gameObject);
        }
        else
        {
            CheckForRespawn();
        }

        yield break;
    }
}