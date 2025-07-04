using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MazeGridPoolManager))]
public class MazeDictMeshRenderer : MonoBehaviour
{
    [Header("References (set automatically)")]
    [SerializeField] MazeGridPoolManager poolManager;

    [Header("Batch Settings")]
    [Tooltip("How many cubes to toggle before waiting")]
    [SerializeField] int batchSize = 1000;
    [Tooltip("Seconds to wait between each batch")]
    [SerializeField] float delayBetweenBatches = 0.05f;

    // Keep track so we can stop an in-flight coroutine
    private Coroutine _toggleRoutine;

    /// <summary>
    /// Fired when a full render or unrender pass completes.
    /// </summary>
    public event Action OnRenderFinished;

    private void Awake()
    {
        poolManager = GetComponent<MazeGridPoolManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RenderCubesGradually();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UnrenderCubesGradually();
        }
    }

    /// <summary>
    /// Starts gradually enabling all cube renderers.
    /// </summary>
    public void RenderCubesGradually()
    {
        if (_toggleRoutine != null) StopCoroutine(_toggleRoutine);
        _toggleRoutine = StartCoroutine(SetRenderersGradually(true));
    }

    public IEnumerator RenderCubesGraduallyOnHold()
    {
        if (_toggleRoutine != null) StopCoroutine(_toggleRoutine);
        _toggleRoutine = StartCoroutine(SetRenderersGradually(true));
        yield return null;
    }

    /// <summary>
    /// Starts gradually disabling all cube renderers.
    /// </summary>
    public void UnrenderCubesGradually()
    {
        if (_toggleRoutine != null) StopCoroutine(_toggleRoutine);
        _toggleRoutine = StartCoroutine(SetRenderersGradually(false));
    }

    private IEnumerator SetRenderersGradually(bool enable)
    {
        int count = 0;
        var cubes = new List<GameObject>(poolManager.cellsByLocation.Values);

        foreach (var cube in cubes)
        {
            if (cube == null) continue;

            var rend = cube.GetComponent<Renderer>();
            if (rend != null)
                rend.enabled = enable;

            if (++count >= batchSize)
            {
                count = 0;
                yield return new WaitForSeconds(delayBetweenBatches);
            }
        }

        yield return null;
        _toggleRoutine = null;
        OnRenderFinished?.Invoke();
    }
}
