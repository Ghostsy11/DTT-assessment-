using UnityEngine.InputSystem;    // Import Unity's new Input System for handling input
using UnityEngine;
using System.Collections;         // Needed for using Coroutines like IEnumerator

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))] // Ensure Rigidbody and PlayerInput are attached
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f; // Player movement speed

    [Tooltip("Time (in seconds) after stopping before re-rendering maze.")]
    public float idleTimeBeforeRender = 0.5f; // Delay before re-rendering full maze after stopping

    private float idleTimer = 0f;            // Timer to track how long player has been idle
    private Rigidbody rb;                    // Reference to Rigidbody for physics-based movement

    private PlayerInput playerInput;         // Unity Input System component
    private InputAction moveAction;          // The "Move" action from the input map
    private Vector2 movementInput;           // 2D input from keyboard/joystick (WASD)

    private MazeDictMeshRenderer renderer;   // Used to gradually render full maze
    private MazeRenderCulling renderCulling; // Used to cull (hide) maze cubes based on distance

    private bool isCurrentlyMoving = false;      // Flag: is player currently moving?
    private bool isRenderingBackMaze = false;    // Flag: has full maze started rendering back?

    // Called when the object is created (before Start)
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();                     // Get Rigidbody component
        playerInput = GetComponent<PlayerInput>();          // Get PlayerInput component
        renderCulling = GetComponent<MazeRenderCulling>();  // Get culling component
        moveAction = playerInput.actions["Move"];           // Get the "Move" action from input system
    }

    // Called once at the beginning
    private void Start()
    {
        // Find the renderer from the scene (might be in a persistent/don't destroy object)
        renderer = FindObjectOfType<MazeDictMeshRenderer>();
    }

    // Called every frame — used here to manage idle time and culling/render logic
    private void Update()
    {
        // Detect if input is enough to consider as "moving"
        bool isMoving = movementInput.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            idleTimer = 0f; // Reset idle timer

            if (!isCurrentlyMoving)
            {
                isCurrentlyMoving = true;     // Mark player as now moving
                isRenderingBackMaze = false;  // Reset re-render flag

                if (renderCulling != null)
                    renderCulling.enabled = true; // Enable culling while moving
            }
        }
        else if (isCurrentlyMoving)
        {
            idleTimer += Time.deltaTime; // Count time spent idle

            // If player was moving and now idle long enough
            if (idleTimer >= idleTimeBeforeRender && !isRenderingBackMaze)
            {
                isCurrentlyMoving = false;        // Mark as no longer moving
                isRenderingBackMaze = true;       // Prevent double re-render

                StartCoroutine(StopCullingAfterRender()); // Begin render full maze after delay
            }
        }
    }

    // Called every physics frame — ideal for Rigidbody movement
    private void FixedUpdate()
    {
        // Convert 2D input into 3D movement vector
        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        Vector3 velocity = move * speed; // Scale movement by speed

        // Preserve Y velocity for physics/gravity consistency
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity; // Apply velocity to Rigidbody
    }

    // When this script is enabled
    private void OnEnable()
    {
        moveAction.Enable(); // Enable movement action

        // Listen to input started (key down, joystick push)
        moveAction.performed += OnMovePerformed;

        // Listen to input stopped (key up, joystick release)
        moveAction.canceled += OnMoveCanceled;
    }

    // When this script is disabled
    private void OnDisable()
    {
        // Unsubscribe from input events to prevent memory leaks
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;

        moveAction.Disable(); // Disable input
    }

    // Called when input is pressed or performed
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>(); // Read movement input vector
    }

    // Called when movement input is released
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        movementInput = Vector2.zero; // Reset input to zero
    }

    // Coroutine: wait for maze render to finish before disabling culling
    private IEnumerator StopCullingAfterRender()
    {
        if (renderCulling != null)
            renderCulling.enabled = false; // Turn off culling

        yield return new WaitForSeconds(1f); // Wait briefly to allow coroutine to clean up

        if (renderer != null)
            yield return renderer.RenderCubesGraduallyOnHold(); // Gradually render full maze
    }
}
