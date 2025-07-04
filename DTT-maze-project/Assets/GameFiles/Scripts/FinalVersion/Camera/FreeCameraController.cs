using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base camera movement speed for WASD controls.")]
    public float moveSpeed = 20f;

    [Tooltip("Multiplier when holding Left Shift to move camera faster.")]
    public float boostMultiplier = 2f;

    [Tooltip("Speed at which the camera rotates based on mouse movement.")]
    public float mouseSensitivity = 3f;

    [Header("Bounds")]
    [Tooltip("Horizontal movement limits along the X-axis.")]
    public Vector2 xBounds = new Vector2(0f, 250f);

    [Tooltip("Horizontal movement limits along the Z-axis.")]
    public Vector2 zBounds = new Vector2(0f, 250f);

    [Tooltip("Vertical movement limits along the Y-axis (camera height).")]
    public Vector2 yBounds = new Vector2(10f, 100f);

    [Header("Target to Follow")]
    [Tooltip("Reference to the player object the camera follows.")]
    public Transform playerTransform;

    [Tooltip("Reference to the player movement script (optional).")]
    public PlayerMovement playerMovement;

    [Tooltip("Distance behind the player when following.")]
    public Vector3 followOffset = new Vector3(0f, 10f, -10f);

    private Vector3 currentRotation;
    private bool isLooking = false;
    private bool isFollowing = false;

    private Vector3 lastPlayerPosition;

    void Start()
    {
        if (playerTransform != null)
            lastPlayerPosition = playerTransform.position;
    }

    void Update()
    {
        HandleMouseLook();
        HandleModeSwitch();
        HandleMovement();
        HandleFollowPlayer();
    }

    /// <summary>
    /// Enables or disables free look mode with RMB.
    /// </summary>
    private void HandleMouseLook()
    {
        // If the player presses the Right Mouse Button (RMB) down
        if (Input.GetMouseButtonDown(1))
        {
            isLooking = true; // Enable free look mode

            // Disable the player movement so they don't move while free looking
            if (playerMovement != null) playerMovement.enabled = false;
        }

        // If the player releases the Right Mouse Button
        if (Input.GetMouseButtonUp(1))
        {
            isLooking = false; // Disable free look mode

            // Re-enable player movement when exiting free look
            if (playerMovement != null) playerMovement.enabled = true;
        }

        // If we're in free look mode
        if (isLooking)
        {
            // Lock the cursor to the center of the screen and hide it
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Read mouse movement on both axes
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Adjust camera's vertical rotation (pitch)
            currentRotation.x -= mouseY;

            // Adjust camera's horizontal rotation (yaw)
            currentRotation.y += mouseX;

            // Clamp the pitch so we don't rotate upside down
            currentRotation.x = Mathf.Clamp(currentRotation.x, -89f, 89f);

            // Apply the rotation to the camera
            transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
        }
        else
        {
            // If not in free look, unlock the cursor and make it visible again
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Determines if camera should follow player based on movement and look state.
    /// </summary>
    private void HandleModeSwitch()
    {
        // Skip if no player is assigned
        if (playerTransform == null) return;

        // Calculate how much the player has moved since last frame
        float movement = (playerTransform.position - lastPlayerPosition).sqrMagnitude;

        // Follow player if we're not in free look and the player has moved a little
        isFollowing = !isLooking && movement > 0.001f;

        // Update the last position to compare on next frame
        lastPlayerPosition = playerTransform.position;
    }

    /// <summary>
    /// Free camera movement using WASD when not following.
    /// </summary>
    private void HandleMovement()
    {
        // Only move camera freely if we're in free look mode
        if (!isLooking) return;

        // Calculate movement speed; boost if holding Shift
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f);

        // Get input vector from horizontal (A/D) and vertical (W/S)
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        // Convert local direction to world direction based on camera rotation
        Vector3 movement = transform.TransformDirection(direction) * speed * Time.deltaTime;

        // Calculate new target position
        Vector3 newPosition = transform.position + movement;

        // Clamp new position to stay within X bounds
        newPosition.x = Mathf.Clamp(newPosition.x, xBounds.x, xBounds.y);

        // Clamp Y position to avoid going too low or too high
        newPosition.y = Mathf.Clamp(newPosition.y, yBounds.x, yBounds.y);

        // Clamp Z position to stay inside the level
        newPosition.z = Mathf.Clamp(newPosition.z, zBounds.x, zBounds.y);

        // Move the camera to the new, clamped position
        transform.position = newPosition;
    }

    /// <summary>
    /// Smoothly follow the player when allowed. Rotate the player with mouse.
    /// </summary>
    private void HandleFollowPlayer()
    {
        // Skip if we are not in follow mode or there's no player
        if (!isFollowing || playerTransform == null) return;

        // Read mouse X-axis movement to rotate the player (simulate turning left/right)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;

        // Rotate the player on the Y axis (yaw) based on mouse
        playerTransform.Rotate(Vector3.up * mouseX);

        // Calculate where the camera should be behind the player with the given offset
        Vector3 targetPosition = playerTransform.position + playerTransform.rotation * followOffset;

        // Smoothly move the camera to the follow position using Lerp
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

        // Make the camera look at the player’s head area (or slightly above)
        transform.LookAt(playerTransform.position + Vector3.up * 2f);
    }
}
