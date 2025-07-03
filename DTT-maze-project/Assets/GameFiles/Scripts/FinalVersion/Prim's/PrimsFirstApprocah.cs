using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(
    typeof(MazeGridPoolManager),
    typeof(PrimsHelperMethods))]
public class PrimsFirstApproach : MonoBehaviour
{
    [Header("Component References (set automatically)")]
    [SerializeField] MazeGenerator mazeGenerator;              // Reference to main maze generator
    [SerializeField] MazeGridPoolManager primsGridPoolManager; // Reference to cube pool manager
    [SerializeField] PrimsHelperMethods helper;                // Shared helper methods for Prim's logic
    [SerializeField] MazeDictMeshRenderer mazeDictMeshRenderer;

    public byte[,] map; // Internal map to mark walls (1) and paths (0)
    private int width, height;


    [Header("Maze Start Position")]
    [SerializeField] int xStartingPoint = 2;
    [SerializeField] int zStartingPoint = 2;

    [SerializeField][Range(0f, 1f)] private float stepDelay = 0.03f; // ⏱️ Delay per step for visualization

    [SerializeField][Range(2, 6)] int stepSize = 2;

    private readonly Color CurrentColor = Color.red;
    private readonly Color NeighborColor = Color.yellow;
    private readonly Color PathColor = new Color(0.5f, 0, 0.5f); // Purple
    private readonly Color WallColor = Color.black;
    private HashSet<Vector2Int> visited;
    private HashSet<Vector2Int> frontier;
    private void Awake()
    {
        // Get required components
        mazeGenerator = GetComponent<MazeGenerator>();
        primsGridPoolManager = GetComponent<MazeGridPoolManager>();
        mazeDictMeshRenderer = GetComponent<MazeDictMeshRenderer>();
        helper = GetComponent<PrimsHelperMethods>();
    }

    private void Start()
    {
        StartCoroutine(GenerateAndRun());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(ReRunRoutine());
        }
    }
    private IEnumerator GenerateAndRun()
    {
        // Step 1: Build full grid
        mazeGenerator.Generate();

        // Step 2: Wait until maze grid is actually finished generating
        while (!mazeGenerator.GenerateMazeBatchedIsDone)
            yield return null;

        // Step 3: Now it's safe to access the cubes
        primsGridPoolManager.SetCellsDictionary(mazeGenerator.cellsByLocation);
        helper.Initialize(mazeGenerator.cellsByLocation, mazeGenerator.xWidth, mazeGenerator.zLength, stepSize);

        primsGridPoolManager.EnableAllCubes();
        mazeDictMeshRenderer.RenderCubesGradually();

        // Step 4: Run maze generation algorithm
        yield return StartCoroutine(AnimatePrimsAlgorithm());

    }

    private IEnumerator AnimatePrimsAlgorithm()
    {
        width = mazeGenerator.xWidth;
        height = mazeGenerator.zLength;
        map = new byte[width, height];

        // Fill entire map with walls (1)
        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
                map[x, z] = 1;

        visited = new HashSet<Vector2Int>();
        frontier = new HashSet<Vector2Int>();

        int safeStartX = Mathf.Clamp(xStartingPoint, 1, width - 2);
        int safeStartZ = Mathf.Clamp(zStartingPoint, 1, height - 2);
        safeStartX = (safeStartX % stepSize == 0) ? safeStartX + 1 : safeStartX;
        safeStartZ = (safeStartZ % stepSize == 0) ? safeStartZ + 1 : safeStartZ;

        Vector2Int start = new Vector2Int(safeStartX, safeStartZ);

        visited.Add(start);
        map[start.x, start.y] = 0;

        helper.SetCubeColor(mazeGenerator, start, Color.green);
        helper.AddNeighborsToFrontier(start, visited, frontier);

        yield return new WaitForSeconds(stepDelay);

        while (frontier.Count > 0)
        {
            Vector2Int current = helper.GetRandomInteriorCell(frontier);
            helper.SetCubeColor(mazeGenerator, current, Color.red); //  Show current being visited

            yield return new WaitForSeconds(stepDelay);

            var neighbors = helper.GetNeighbors(current);
            var visitedNeighbors = new List<Vector2Int>();
            foreach (var n in neighbors)
            {
                if (visited.Contains(n))
                    visitedNeighbors.Add(n);
                else
                    helper.SetCubeColor(mazeGenerator, n, Color.yellow); //  Mark unvisited neighbors
            }

            if (visitedNeighbors.Count > 0)
            {
                Vector2Int neighbor = visitedNeighbors[Random.Range(0, visitedNeighbors.Count)];

                helper.CarvePath(current, neighbor, map);

                //if (stepSize == 1)
                //    helper.CarvePath(current, neighbor, map);
                //else
                //    helper.CarveBetween(current, neighbor, map);


                visited.Add(current);

                helper.SetCubeColor(mazeGenerator, current, new Color(0.5f, 0, 0.5f)); //  Purple: final path

                helper.AddNeighborsToFrontier(current, visited, frontier);
            }

            frontier.Remove(current);
        }

        //  Final Pass: turn unvisited walls to black
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[x, z] == 1)
                {
                    Vector2Int pos = new Vector2Int(x, z);
                    helper.SetCubeColor(mazeGenerator, pos, Color.black);
                }
            }
        }

        mazeGenerator.map = map;
        Debug.Log("Animated Prim’s Maze Generation Complete.");
        helper.PullDownPath(1f, mazeGenerator.cellsByLocation, map);

    }

    public IEnumerator ReRunRoutine()
    {
        visited = null;

        frontier = null;

        helper.SetCubeBackToYPosition(mazeGenerator);

        helper.Initialize(mazeGenerator.cellsByLocation, mazeGenerator.xWidth, mazeGenerator.zLength, stepSize);

        // wait a bit if you want…
        yield return new WaitForSeconds(0.2f);

        // carve and wait
        yield return StartCoroutine(AnimatePrimsAlgorithm());

        helper.PullDownPath(1f, mazeGenerator.cellsByLocation, map);

    }

}
