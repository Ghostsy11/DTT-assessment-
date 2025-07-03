using System;
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
    [SerializeField][Range(2, 8)] private int stepSize = 2;

    private void Awake()
    {
        mazeGenerator = GetComponent<MazeGenerator>();
        helper = GetComponent<PrimsHelperMethods>();
        poolManager = GetComponent<MazeGridPoolManager>();
        renderer = GetComponent<MazeDictMeshRenderer>();
    }

    private void Start()
    {
        StartCoroutine(OrderOfExecution());

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
    private IEnumerator OrderOfExecution()
    {
        // 1. Generate the base grid (instantiated or batched)
        mazeGenerator.Generate();

        // Wait until the appropriate generation flag is set
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
        // 2. Give the pool manager the generated map
        poolManager.cellsByLocation = mazeGenerator.cellsByLocation;

        // 3. Enable/activate all pooled cubes
        poolManager.EnableAllCubes();

        // 4. Render cubes gradually (likely a coroutine)
        renderer.RenderCubesGradually();

        // 5. Wait one frame (or subscribe to renderer.OnRenderFinished if you want to be precise)
        yield return null;

        // 6. Initialize your byte[,] grid to all “1”s
        grid = helper.InitializeMapArray(mazeGenerator.xWidth, mazeGenerator.zLength);

        // 7. Dump it to the console so you can verify every cell is a wall
        helper.DebugPrintMap(grid);

        // 8. initialize the PrimsHelperMethods
        helper.Initialize(mazeGenerator.cellsByLocation, mazeGenerator.xWidth, mazeGenerator.zLength, stepSize);

        // 5. Automatically carve until no valid frontier remains
        while (true)
        {
            // carve one step
            StepPrim();

            // once your frontier is empty, StepPrim() will log “Maze truly complete!”,
            // so we break out here
            if (primsInitialized && frontier != null && frontier.Count == 0)
                break;

            // wait a frame so we can visually watch it happen
            yield return null;
        }
        Debug.Log("Automatic Prim’s run complete!");
        helper.DebugPrintMap(grid);

        // Need to be called after after resize as well
        // animate every magenta cell down by 1 unit over 1 second
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
    #region Algorthm Logics Circle
    private IEnumerator ReRunTheMaze()
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
        Vector2Int neighbor = nbrs[UnityEngine.Random.Range(0, nbrs.Count)];

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
