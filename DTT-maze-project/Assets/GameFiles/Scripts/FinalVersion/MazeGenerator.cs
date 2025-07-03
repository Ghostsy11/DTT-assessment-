using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    public enum GenerateType
    {
        GenerateOnce,
        GenerateBatched,
        PreInstantiated
    }

    /// <summary>
    /// Maps each grid‐coordinate to its instantiated cube.
    /// </summary>
    public Dictionary<Vector2Int, GameObject> cellsByLocation { get; private set; } = new Dictionary<Vector2Int, GameObject>();

    [Tooltip("List to represent and holds data of the maze grid for later use in object-pooling")]
    // I initialize to prevent null refs error
    public List<GameObject> mazeGridObjectsList = new List<GameObject>();

    public byte[,] map; // using byte is efficient way to store the maze layout in memory. It's faster and smaller than using int or bool[,].

    [Header("Which generation method to run on Start")]
    [Tooltip("Pick whether to build all at once, in batches, or grab an existing prefab.")]
    public GenerateType generateType = GenerateType.GenerateBatched;

    [Header("Maze Dimensions")]
    [Range(10, 250)]
    [Tooltip("The width of the maze")]
    public int xWidth;

    [Range(10, 250)]
    [Tooltip("The length of the maze")]
    public int zLength;

    [Header("Cell Settings")]
    [Tooltip("Single cube prefab")]
    [SerializeField] GameObject singleCell;

    [Tooltip("Pre Scale the maze")]
    [Range(1, 6)]
    [SerializeField] int scale = 1;

    [Header("Batch Instantiation")]
    [Tooltip("How many cubes to create before yielding to next frame")]
    public int batchSize = 1000;

    [Header("Parent object to keep hierarchy clean")]
    [SerializeField] private Transform gridParent;

    [Header("Generator Flags to give us info that its done with creating the grid")]
    public bool generateMazeAtOnceIsDone { get; private set; }
    public bool GenerateMazeBatchedIsDone { get; private set; }
    public bool GetPreInstantiatedMazeIsDone { get; set; }

    private void Awake()
    {
        if (singleCell == null)
        {
            Debug.LogError("MazeGenerator: singleCell prefab is not assigned!");
            return;
        }

    }

    public void Generate()
    {
        // Ensure maze dimensions are odd so that path and wall cells align properly.
        // Maze carving requires odd-sized grids: even indices are walls, odd indices are paths.
        xWidth = ((xWidth - 1) / 2) * 2 + 1;
        zLength = ((zLength - 1) / 2) * 2 + 1;

        // Initialize dictionary (in case you regenerate multiple times)
        cellsByLocation.Clear();

        switch (generateType)
        {
            case GenerateType.GenerateOnce:
                GenerateMazeOnce();
                break;
            case GenerateType.GenerateBatched:
                StartCoroutine(GenerateMazeBatched());
                break;
            case GenerateType.PreInstantiated:
                GetPreInstantiatedMaze();
                break;
        }
    }

    #region First Approach: Not performance-friendly
    /// <summary>
    /// Creates the grid maze of cubes at once not really performance good but it has its own uses.
    /// </summary>
    private void GenerateMazeOnce()
    {
        // Clear the old data
        mazeGridObjectsList.Clear();
        cellsByLocation.Clear();

        //This guarantees no internal reallocations as am using Add.
        mazeGridObjectsList.Capacity = xWidth * zLength;
        // initializeMap();

        for (int z = 0; z < zLength; z++)
        {
            for (int x = 0; x < xWidth; x++)
            {


                // position * scale to push cells away from each other
                Vector3 cellPosition = new Vector3(x * scale, 0, z * scale);

                // Make Cube
                var singleCubeCell = Instantiate(singleCell, cellPosition, Quaternion.identity);


                // resizing it to fit into each other
                singleCubeCell.transform.localScale = new Vector3(scale, scale, scale);

                // Make it static for preformace boost
                //   singleCubeCell.isStatic = true;

                // add it to the list
                mazeGridObjectsList.Add(singleCubeCell);


                //  Add to our dictionary
                var loc = new Vector2Int(x, z);
                cellsByLocation[loc] = singleCubeCell;

                // Add to the parent to keep hierarchy organized
                singleCubeCell.transform.SetParent(gridParent, true);

            }
        }

        generateMazeAtOnceIsDone = true;
    }
    #endregion

    #region Second Approach: Performance-friendly batched
    /// <summary>
    /// Creates the grid maze of cubes in batches to avoid frame spikes.
    /// </summary>
    private IEnumerator GenerateMazeBatched()
    {
        // Clear existing
        mazeGridObjectsList.Clear();
        cellsByLocation.Clear();

        //This guarantees no internal reallocations as am using Add.
        mazeGridObjectsList.Capacity = xWidth * zLength;
        //initializeMap();
        int createdInBatch = 0;

        for (int z = 0; z < zLength; z++)
        {
            for (int x = 0; x < xWidth; x++)
            {


                // position * scale to push cells away from each other
                Vector3 cellPosition = new Vector3(x * scale, 0, z * scale);

                // Make Cube
                GameObject singleCubeCell = Instantiate(singleCell, cellPosition, Quaternion.identity, gridParent);

                // resizing it to fit into each other
                singleCubeCell.transform.localScale = Vector3.one * scale;

                // Make it static for preformace boost
                // singleCubeCell.isStatic = true;

                // Add it to the list  
                mazeGridObjectsList.Add(singleCubeCell);

                // Add it to the Dictionary
                var loc = new Vector2Int(x, z);
                cellsByLocation[loc] = singleCubeCell;

                // throttle
                createdInBatch++;
                if (createdInBatch >= batchSize)
                {
                    createdInBatch = 0;
                    // wait one frame before continuing
                    yield return null;
                }

            }
        }
        GenerateMazeBatchedIsDone = true;
    }
    #endregion

    #region Third Approach: Grab existing prefab

    /// <summary>
    /// This function get the children of the entire maze. Because we have it set in hierarchy it's less costly to get the children instead of creating 62500 gameobject and ensures slightly better performance
    /// </summary>
    private void GetPreInstantiatedMaze()
    {
        // 1) Find the root once
        var entireMaze = GameObject.FindWithTag("WholeMaze");
        if (entireMaze == null)
        {
            Debug.LogWarning("No GameObject found with tag ‘WholeMaze’");
            return;
        }

        // 2) Point to your dictionary & clear
        cellsByLocation.Clear();

        // 3) Loop every direct child cube
        foreach (Transform child in entireMaze.transform)
        {
            var singleCubeCell = child.gameObject;
            Vector3 pos = child.localPosition;    // or .position if they aren’t parented

            int x = Mathf.RoundToInt(pos.x);
            int z = Mathf.RoundToInt(pos.z);

            var loc = new Vector2Int(x, z);
            cellsByLocation[loc] = singleCubeCell;
        }

        GetPreInstantiatedMazeIsDone = true;
    }
    #endregion

}

