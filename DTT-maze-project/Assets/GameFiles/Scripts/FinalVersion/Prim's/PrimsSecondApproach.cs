using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PrimsHelperMethods), typeof(MazeGridPoolManager), typeof(MazeDictMeshRenderer))]
public class PrimsSecondApproach : MonoBehaviour
{
    [Header("References")]
    private PrimsHelperMethods helper;
    private MazeGenerator mazeGenerator;
    private MazeGridPoolManager poolManager;
    private MazeDictMeshRenderer renderer;

    public byte[,] grid;

    // State fields
    private bool primsInitialized = false;
    private HashSet<Vector2Int> visited;
    private HashSet<Vector2Int> frontier;

    [Header("Prim’s Start Settings")]
    [Tooltip("Grid cell (x,z) to begin carving from. Must be inside the interior bounds.")]
    [SerializeField]
    private Vector2Int inspectorStartCell = new Vector2Int(1, 1);

    [Header("Maze Step Settings")]
    [Tooltip("How far apart cells are connected")]
    [SerializeField][Range(2, 8)] int stepSize = 2;

    private void Awake()
    {
        mazeGenerator = GetComponent<MazeGenerator>();
        helper = GetComponent<PrimsHelperMethods>();
        poolManager = GetComponent<MazeGridPoolManager>();
        renderer = GetComponent<MazeDictMeshRenderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(ReRunTheMaze());
        }
    }

    /// <summary>
    /// Runs each phase in sequence, waiting for async operations to complete.
    /// </summary>
    public IEnumerator GenerateOrder()
    {
        // Step 1: Generate the base grid using the MazeGenerator
        mazeGenerator.Generate();

        // Step 2: Wait until the maze generation is complete, depending on the generation type
        switch (mazeGenerator.generateType)
        {
            case MazeGenerator.GenerateType.GenerateOnce:
                while (!mazeGenerator.generateMazeAtOnceIsDone)
                    yield return null;
                break;

            case MazeGenerator.GenerateType.GenerateBatched:
                while (!mazeGenerator.GenerateMazeBatchedIsDone)
                    yield return null;
                break;

            case MazeGenerator.GenerateType.PreInstantiated:
                while (!mazeGenerator.GetPreInstantiatedMazeIsDone)
                    yield return null;
                break;
        }

        // Step 3: Pass the generated cube references to the pool manager
        poolManager.cellsByLocation = mazeGenerator.cellsByLocation;

        // Step 4: Enable all cubes in the pool
        poolManager.EnableAllCubes();

        // Step 5: Wait one frame to ensure they are fully activated before rendering
        yield return null;

        // Step 6: Start rendering cubes gradually and wait until rendering is complete
        yield return StartCoroutine(renderer.RenderCubesGraduallyOnHold());

        // Step 7: Initialize the internal byte[,] grid as fully walled
        grid = helper.InitializeMapArray(mazeGenerator.xWidth, mazeGenerator.zLength);

        // Step 8: Debug print to confirm full wall setup
        helper.DebugPrintMap(grid);

        // Step 9: Initialize Prim’s logic (set visited, frontier, etc.)
        helper.Initialize(
            mazeGenerator.cellsByLocation,
            mazeGenerator.xWidth,
            mazeGenerator.zLength,
            stepSize
        );

        // Step 10: Begin carving using Prim’s algorithm step-by-step
        while (true)
        {
            StepPrim();

            if (primsInitialized && frontier != null && frontier.Count == 0)
                break;

            yield return null;
        }

        Debug.Log("Automatic Prim’s run complete!");
        helper.DebugPrintMap(grid);

        // Step 11: Animate carved path cubes down over 1 second
        if (mazeGenerator == null)
        {
            Debug.LogError("mazeGenerator is still null before PullDownPath!");
        }
        else
        {
            helper.PullDownPath(1f, mazeGenerator.cellsByLocation, grid);
        }
    }
    #region Algorthm Logics Circle
    public IEnumerator ReRunTheMaze()
    {

        // 1) Snap every cube to Y=0 and reset its color to black (wall)
        helper.SetCubeBackToYPosition(mazeGenerator);

        // 2) Re-init helper for fresh frontier/visited
        helper.Initialize(
            mazeGenerator.cellsByLocation,
            mazeGenerator.xWidth,
            mazeGenerator.zLength,
            stepSize
        );

        grid = helper.InitializeMapArray(mazeGenerator.xWidth, mazeGenerator.zLength);
        primsInitialized = false;
        visited = null;
        frontier = null;

        // 3) Carve until done
        while (true)
        {
            StepPrim();
            if (primsInitialized && frontier != null && frontier.Count == 0)
                break;
            yield return null;
        }

        Debug.Log("Prim’s carve complete");
        helper.DebugPrintMap(grid);

        // 4) Drop only the path cubes
        if (mazeGenerator == null)
        {
            Debug.LogError("mazeGenerator is still null before PullDownPath!");
        }
        else
        {
            helper.PullDownPath(1f, mazeGenerator.cellsByLocation, grid);
        }

        yield break;

    }

    /// <summary>
    /// Algorthm logic order
    /// </summary>
    private void StepPrim()
    {
        // helper already initialized in OrderOfExecution with stepSize=2
        helper.Initialize(mazeGenerator.cellsByLocation, mazeGenerator.xWidth, mazeGenerator.zLength, stepSize);
        if (!primsInitialized)
        {

            // pick start (inspector or random fallback)
            Vector2Int start = inspectorStartCell;
            if (!helper.IsInBounds(start) ||
                !mazeGenerator.cellsByLocation.ContainsKey(start))
            {
                Debug.LogWarning($"Start {start} invalid, picking random interior.");
                helper.GetInnerNeighbors(start);
            }

            // carve the start cell
            grid[start.x, start.y] = 0;
            helper.SetCubeColor(mazeGenerator, start, Color.magenta);

            // setup visited & frontier
            visited = new HashSet<Vector2Int> { start };
            frontier = new HashSet<Vector2Int>();
            helper.AddNeighborsToFrontier(start, visited, frontier);

            primsInitialized = true;
            Debug.Log($"[Prim’s] Initialized at {start}");
            return;
        }

        // 2) Subsequent presses: carve exactly one step
        // filter frontier down to cells that actually border visited via a 2-step neighbor
        var valid = frontier
            .Where(f => helper.GetNeighbors(f)
                              .Any(n => visited.Contains(n)))
            .ToList();

        if (valid.Count == 0)
        {
            Debug.Log("[Prim’s] Maze truly complete!");
            return;
        }

        // pick & remove one valid frontier cell
        Vector2Int cell = valid[UnityEngine.Random.Range(0, valid.Count)];
        frontier.Remove(cell);

        // pick one already-visited neighbor (at distance 2)
        var nbrs = helper.GetNeighbors(cell)
                         .Where(n => visited.Contains(n))
                         .ToList();
        Vector2Int neighbor = nbrs[Random.Range(0, nbrs.Count)];

        // carve the entire segment between them (this sets grid=0 and colors magenta)
        helper.CarvePath(neighbor, cell, grid);

        // mark the new cell visited
        visited.Add(cell);

        // grow frontier out from it
        helper.AddNeighborsToFrontier(cell, visited, frontier);

        Debug.Log($"[Prim’s] Carved {neighbor} → {cell}");
        helper.DebugPrintMap(grid);
    }
    #endregion
}
