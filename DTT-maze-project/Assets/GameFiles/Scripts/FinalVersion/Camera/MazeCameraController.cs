using UnityEngine;
using System;

public class MazeCameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;

    [Tooltip("Vertical distance above the maze for top-down view.")]
    public float heightOffset = 50f;

    [Tooltip("Tilted view distance for isometric or angled views.")]
    public float angledDistance = 30f;

    float zoomMultiplier = 2f; // Multiplier for ultra far-out views

    // Enum defining available camera viewing angles
    public enum ViewMode
    {
        TopDown,
        Isometric,
        CornerLeft,
        CornerRight,
        FarTopDown,
        WideAngle,
        FarIsometric,
        UltraFarTopDown,
        UltraFarCornerLeft,
        UltraFarCornerRight
    }

    public ViewMode currentView = ViewMode.TopDown; // The active camera view

    // Called when the script starts
    private void Start()
    {
        mainCamera = Camera.main; // Cache reference to main camera in scene
    }

    // Runs every frame
    private void Update()
    {
        // If the mainCamera wasn't assigned in Start (e.g. loaded later), try again
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        else
        {
            return; // If already assigned, exit early to avoid repeated assignment
        }
    }

    /// <summary>
    /// Focuses the camera on the maze center using the current view mode.
    /// </summary>
    public void FocusOnMaze(int width, int length)
    {
        if (mainCamera == null) return; // Abort if no camera

        // Calculate the center of the maze
        Vector3 center = new Vector3(width / 2f, 0f, length / 2f);

        // Position and rotate the camera based on selected view mode
        switch (currentView)
        {
            case ViewMode.TopDown:
                mainCamera.transform.position = center + Vector3.up * heightOffset;
                mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Look straight down
                break;

            case ViewMode.Isometric:
                mainCamera.transform.position = center + new Vector3(-angledDistance, heightOffset, -angledDistance);
                mainCamera.transform.rotation = Quaternion.Euler(45f, 45f, 0f); // Diagonal view
                break;

            case ViewMode.CornerLeft:
                mainCamera.transform.position = center + new Vector3(-angledDistance, heightOffset, angledDistance);
                mainCamera.transform.rotation = Quaternion.Euler(60f, 135f, 0f); // From left corner
                break;

            case ViewMode.CornerRight:
                mainCamera.transform.position = center + new Vector3(angledDistance, heightOffset, -angledDistance);
                mainCamera.transform.rotation = Quaternion.Euler(60f, 45f, 0f); // From right corner
                break;

            case ViewMode.FarTopDown:
                mainCamera.transform.position = center + Vector3.up * (heightOffset * 2.5f);
                mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Higher vertical
                break;

            case ViewMode.WideAngle:
                mainCamera.transform.position = center + new Vector3(0, heightOffset * 1.5f, -length);
                mainCamera.transform.rotation = Quaternion.Euler(35f, 0f, 0f); // Low-angle front
                break;

            case ViewMode.FarIsometric:
                mainCamera.transform.position = center + new Vector3(-angledDistance * 2f, heightOffset * 2f, -angledDistance * 2f);
                mainCamera.transform.rotation = Quaternion.Euler(50f, 45f, 0f); // High and far isometric
                break;

            case ViewMode.UltraFarTopDown:
                mainCamera.transform.position = center + Vector3.up * (heightOffset * zoomMultiplier * 2f);
                mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Very high top-down
                break;

            case ViewMode.UltraFarCornerLeft:
                mainCamera.transform.position = center + new Vector3(-angledDistance * zoomMultiplier, heightOffset * zoomMultiplier, angledDistance * zoomMultiplier);
                mainCamera.transform.rotation = Quaternion.Euler(65f, 135f, 0f); // Farther left corner
                break;

            case ViewMode.UltraFarCornerRight:
                mainCamera.transform.position = center + new Vector3(angledDistance * zoomMultiplier, heightOffset * zoomMultiplier, -angledDistance * zoomMultiplier);
                mainCamera.transform.rotation = Quaternion.Euler(65f, 45f, 0f); // Farther right corner
                break;
        }
    }

    /// <summary>
    /// Changes the current camera view mode using an index.
    /// </summary>
    public void SetViewMode(int index)
    {
        int modeCount = Enum.GetValues(typeof(ViewMode)).Length; // Total number of view modes
        currentView = (ViewMode)(index % modeCount); // Loop index around enum range
    }
}
