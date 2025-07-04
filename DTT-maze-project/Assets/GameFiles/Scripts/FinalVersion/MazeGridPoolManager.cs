using System.Collections.Generic;
using UnityEngine;

// For safety if we forgot adding it
[RequireComponent(typeof(MazeGenerator))]
public class MazeGridPoolManager : MonoBehaviour
{
    [Header("Refreances that managed via code programmatically")]
    [SerializeField] MazeGenerator mazeGenerator;

    // No more need for a separate HashSet of locations—
    // the dictionary’s Keys gives you exactly that.
    public Dictionary<Vector2Int, GameObject> cellsByLocation;

    private void Awake()
    {
        mazeGenerator = GetComponent<MazeGenerator>();
    }

    // Set reference from outside (like from MazeGenerator or controller)
    public void SetCellsDictionary(Dictionary<Vector2Int, GameObject> cells)
    {
        cellsByLocation = cells;
    }

    /// <summary>
    /// Enable (set active) all cubes in the maze.
    /// </summary>
    public void EnableAllCubes()
    {
        cellsByLocation = mazeGenerator.cellsByLocation;
        if (cellsByLocation == null) return;
        foreach (var cube in cellsByLocation.Values)
            cube.SetActive(true);
    }

    /// <summary>
    /// Disable (set inactive) all cubes in the maze.
    /// </summary>
    public void DisableAllCubes()
    {
        cellsByLocation = mazeGenerator.cellsByLocation;
        if (cellsByLocation == null) return;
        foreach (var cube in cellsByLocation.Values)
            cube.SetActive(false);
    }

    /// <summary>
    /// Destroys all cubes in the maze and clears the dictionary.
    /// Useful for hard resets or reinitialization.
    /// </summary>
    public void DestroyAllCubes()
    {
        if (cellsByLocation == null || cellsByLocation.Count == 0) return;

        foreach (var cube in cellsByLocation.Values)
        {
            if (cube != null)
                Destroy(cube);
        }

        cellsByLocation.Clear();

        if (mazeGenerator != null)
            mazeGenerator.cellsByLocation.Clear();
    }

}
