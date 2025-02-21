using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
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

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        // Subscribe to input events with named methods
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Jump.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJumpPerformed;

        // Disable the action map
        inputActions.Player.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if(!isDead && transform.position.y < -3f)
        {
            StartCoroutine(DieAndRestart());
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

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

    // Called when the Move action is performed
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Called when the Move action is canceled
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    // Called when the Jump action is performed
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpRequested = true;
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // Coroutine that handles the death sequence and scene reload.
    private IEnumerator DieAndRestart()
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

        // Wait for 2 seconds before reloading the scene.
        yield return new WaitForSeconds(1.5f);

        // Reload the current scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
