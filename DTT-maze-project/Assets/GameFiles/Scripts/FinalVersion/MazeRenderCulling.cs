using System.Collections;
using UnityEngine;

/// <summary>
/// Enables/disables cube renderers based on player distance.
/// Keeps performance high while preserving maze structure.
/// </summary>
public class MazeRenderCulling : MonoBehaviour
{
    [Header("References")]
    public MazeGenerator generator;
    public Transform player;

    [Header("Settings")]
    public float viewDistance = 40f;
    public float updateInterval = 0.5f; // seconds
    private Coroutine cullingRoutine; // Track the coroutine so we can stop it

    private void Start()
    {
        if (generator == null)
        {

            generator = FindObjectOfType<MazeGenerator>();
        }
        // Only start if enabled
        if (enabled)
        {

            cullingRoutine = StartCoroutine(CullLoop());
        }
    }

    private void OnEnable()
    {
        // Restart coroutine when script is re-enabled
        if (cullingRoutine == null)
        {

            cullingRoutine = StartCoroutine(CullLoop());
        }
    }

    private void OnDisable()
    {
        // Stop culling coroutine when disabled
        if (cullingRoutine != null)
        {
            StopCoroutine(cullingRoutine);
            cullingRoutine = null;
        }
    }

    private IEnumerator CullLoop()
    {
        while (true)
        {
            if (enabled)
            {
                CullCubes();
            } // Extra guard in case coroutine is still running while being disabled


            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void CullCubes()
    {
        if (generator == null || player == null || generator.cellsByLocation == null)
            return;

        float viewDistanceSqr = viewDistance * viewDistance;

        foreach (var kvp in generator.cellsByLocation)
        {
            GameObject cube = kvp.Value;
            if (cube == null) continue;

            float distSqr = (player.position - cube.transform.position).sqrMagnitude;
            bool shouldRender = distSqr <= viewDistanceSqr;

            Renderer rend = cube.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.enabled = shouldRender;

            }

            Collider col = cube.GetComponent<Collider>();
            if (col != null)
            {

                col.enabled = shouldRender;
            }
        }
    }
}