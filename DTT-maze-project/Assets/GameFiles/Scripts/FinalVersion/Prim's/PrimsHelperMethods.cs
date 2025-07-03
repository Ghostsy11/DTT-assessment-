using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Utility class that provides helper methods for Prim's Maze Generation.
/// It manages bounds checking, neighbor detection, frontier growth, and carving logic.
/// Step size is configurable to control how far apart each maze cell is spaced.
/// </summary>
public class PrimsHelperMethods : MonoBehaviour
{
    private Dictionary<Vector2Int, GameObject> cellsByLocation;
    private int width, height;

    [Header("Maze Step Settings")]
    [Tooltip("How far apart cells are connected (2 = classic maze, 1 = tight maze)")]
    private int stepSize = 2;

    private Vector2Int[] cardinalOffsets;

    /// <summary>
    /// Initializes grid data and generates directional offsets based on step size.
    /// </summary>
    public void Initialize(Dictionary<Vector2Int, GameObject> cells, int width, int height, int step)
    {
        this.cellsByLocation = cells;
        this.width = width;
        this.height = height;
        this.stepSize = step;
        GenerateOffsets();
    }

    /// <summary>
    /// Recomputes the direction offsets based on current step size.
    /// </summary>
    private void GenerateOffsets()
    {
        cardinalOffsets = new[]
        {
            new Vector2Int(stepSize, 0), new Vector2Int(-stepSize, 0),
            new Vector2Int(0, stepSize), new Vector2Int(0, -stepSize)
        };
    }

    /// <summary>
    /// Returns all 4-direction neighbors of the given cell using stepSize spacing.
    /// </summary>
    public List<Vector2Int> GetNeighbors(Vector2Int loc)
    {
        var neighbors = new List<Vector2Int>();
        foreach (var offset in cardinalOffsets)
        {
            var neighbor = loc + offset;
            if (IsInBounds(neighbor) && cellsByLocation.ContainsKey(neighbor))
                neighbors.Add(neighbor);
        }
        return neighbors;
    }

    /// <summary>
    /// Returns true if the given location is within the inner bounds of the grid (excludes edges).
    /// </summary>
    public bool IsInBounds(Vector2Int loc)
    {
        //return loc.x > 0 && loc.x < width - 1 && loc.y > 0 && loc.y < height - 1;
        return loc.x >= 0 && loc.x < width && loc.y >= 0 && loc.y < height;

    }
    public bool IsInnerCell(Vector2Int loc)
    {
        return loc.x > 0 && loc.x < width - 1 && loc.y > 0 && loc.y < height - 1;
    }

    /// <summary>
    /// Returns neighbors of a given location that are outside grid bounds.
    /// </summary>
    public List<Vector2Int> GetInnerNeighbors(Vector2Int loc)
    {
        var inner = new List<Vector2Int>();
        foreach (var neighbor in GetNeighbors(loc))
        {
            if (!IsInBounds(neighbor))
                inner.Add(neighbor);
        }
        return inner;
    }

    /// <summary>
    /// Picks and returns a random interior cell (according to IsInBounds) from the given set.
    /// </summary>
    public Vector2Int GetRandomInteriorCell(HashSet<Vector2Int> cells)
    {
        if (cells == null || cells.Count == 0)
        {
            Debug.LogError("GetRandomInteriorCell: provided set is null or empty.");
            return Vector2Int.zero;
        }

        // Only keep those inside the valid interior
        var interior = cells.Where(loc => IsInBounds(loc)).ToList();
        if (interior.Count == 0)
        {
            Debug.LogError("GetRandomInteriorCell: no interior cells found in the provided set.");
            return Vector2Int.zero;
        }

        // Pick one at random
        int randomIndex = UnityEngine.Random.Range(0, interior.Count);
        return interior[randomIndex];
    }

    /// <summary>
    /// Adds eligible neighbors of the given cell to the frontier set.
    /// </summary>
    public void AddNeighborsToFrontier(Vector2Int loc, HashSet<Vector2Int> visited, HashSet<Vector2Int> frontier)
    {
        foreach (var neighbor in GetNeighbors(loc))
        {
            if (!IsInBounds(neighbor)) continue;
            if (!visited.Contains(neighbor) && !frontier.Contains(neighbor))
                frontier.Add(neighbor);
        }
    }

    /// <summary>
    /// Carves a path between two cells spaced by stepSize, coloring and marking them in the map.
    /// </summary>
    public void CarveBetween(Vector2Int a, Vector2Int b, byte[,] map)
    {
        Vector2Int delta = b - a;
        Vector2Int dir = new Vector2Int(
            (delta.x != 0) ? delta.x / Mathf.Abs(delta.x) : 0,
            (delta.y != 0) ? delta.y / Mathf.Abs(delta.y) : 0
        );

        Vector2Int pos = a;
        for (int i = 0; i <= stepSize; i++)
        {
            if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
            {
                if (cellsByLocation.TryGetValue(pos, out GameObject cube) && cube != null)
                {
                    var renderer = cube.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = new Color(0.5f, 0, 0.5f);
                }

                map[pos.x, pos.y] = 0;
            }

            pos += dir;
        }
        Debug.Log($"🪓 Carved path: {a} <-> {b}");
    }

    /// <summary>
    /// Carves a path between two cells spaced by stepSize, coloring and marking them in the map.
    /// </summary>
    public void CarveSingleStepBetween(Vector2Int a, Vector2Int b, byte[,] map)
    {
        Vector2Int delta = b - a;

        if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) != 1)
        {
            Debug.LogWarning($"🚫 Invalid carve between {a} and {b}. Skipping.");
            return;
        }

        foreach (var pos in new[] { a, b })
        {
            if (IsInBounds(pos))
            {
                map[pos.x, pos.y] = 0;

                if (cellsByLocation.TryGetValue(pos, out GameObject cube) && cube != null)
                {
                    var renderer = cube.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = new Color(0.5f, 0, 0.5f);
                }
            }
        }

        Debug.Log($"🪓 Carved path: {a} <-> {b}");
    }

    public void CarvePath(Vector2Int a, Vector2Int b, byte[,] map)
    {
        Vector2Int delta = b - a;
        Vector2Int dir = new Vector2Int(
            (delta.x != 0) ? delta.x / Mathf.Abs(delta.x) : 0,
            (delta.y != 0) ? delta.y / Mathf.Abs(delta.y) : 0
        );

        int distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);

        if (distance < 1 || (delta.x != 0 && delta.y != 0))
        {
            Debug.LogWarning($"🚫 Invalid carve between {a} and {b}. Skipping.");
            return;
        }

        Vector2Int pos = a;
        for (int i = 0; i <= distance; i++)
        {
            if (IsInBounds(pos))
            {
                map[pos.x, pos.y] = 0;

                if (cellsByLocation.TryGetValue(pos, out GameObject cube) && cube != null)
                {
                    var renderer = cube.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = new Color(0.5f, 0, 0.5f);
                }
            }
            pos += dir;
        }

        Debug.Log($"🪓 Carved path: {a} <-> {b}");
    }

    /// <summary>
    /// Returns 4-direction unit-step neighbors on a checkerboard grid (odd cells only).
    /// </summary>
    public List<Vector2Int> GetUnitStepNeighbors(Vector2Int loc)
    {
        var neighbors = new List<Vector2Int>();
        var offsets = new[]
        {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
         };

        foreach (var offset in offsets)
        {
            Vector2Int neighbor = loc + offset;

            //  Ensure neighbor is within valid maze interior bounds and exists in grid
            if (IsInnerCell(neighbor) && cellsByLocation.ContainsKey(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Y-animation to pull object on Y-axis
    /// </summary>
    public IEnumerator Yanimation(GameObject obj, float time)
    {
        float currentSize;
        Vector3 startPos = obj.transform.position;
        if (obj.transform.localScale.x == 2f)
        {
            currentSize = -2f;
        }
        else if (obj.transform.localScale.x == 3f)
        {
            currentSize = -3f;
        }
        else if (obj.transform.localScale.x == 4f)
        {
            currentSize = -4f;
        }
        else if (obj.transform.localScale.x == 5f)
        {
            currentSize = -5f;
        }
        else { currentSize = -1f; }

        Vector3 endPos = new Vector3(startPos.x, startPos.y + currentSize, startPos.z);

        float elapsed = 0f;

        while (elapsed < time)
        {
            float t = elapsed / time;

            // Move upward/downwards smoothly
            obj.transform.position = Vector3.Lerp(startPos, endPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPos;

    }

    /// <summary>
    /// Helper to recolor a cube at grid position
    /// </summary>
    public void SetCubeColor(MazeGenerator mazeGenerator, Vector2Int pos, Color c)
    {
        if (mazeGenerator.cellsByLocation.TryGetValue(pos, out var cube))
        {
            var rend = cube.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = c;
        }
    }

    /// <summary>
    /// It resets the cube Y postion
    /// </summary>
    public void SetCubeBackToYPosition(MazeGenerator mazeGenerator)
    {
        // 1) Snap every cube to Y=0 and reset its color to black (wall)
        foreach (var cube in mazeGenerator.cellsByLocation.Values)
        {
            // position
            var p = cube.transform.position;
            cube.transform.position = new Vector3(p.x, 0f, p.z);

            // color
            var rend = cube.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = Color.black;
        }
    }

    /// <summary>
    /// Starts a Y-animation coroutine on every carved (grid==0) cube.
    /// </summary>
    /// <param name="duration">How long each cube takes to drop</param>
    public void PullDownPath(float duration, Dictionary<Vector2Int, GameObject> cellsByLocation, byte[,] map)
    {
        if (cellsByLocation == null || map == null)
        {
            Debug.LogError("❌ cellsByLocation or map is null!");
            return;
        }

        foreach (var kv in cellsByLocation)
        {
            Vector2Int pos = kv.Key;
            GameObject cube = kv.Value;

            if (cube != null && map[pos.x, pos.y] == 0)
            {
                // Start animation on this cube
                StartCoroutine(Yanimation(cube, duration));
            }
        }
    }

    /// <summary>
    /// Logs the entire grid to the console, using ‘1’ for walls and ‘0’ for corridors.
    /// </summary>
    public void DebugPrintMap(byte[,] map)
    {
        var sb = new System.Text.StringBuilder();
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
                sb.Append(map[x, y] == 1 ? '1' : '0').Append(' ');
            sb.AppendLine();
        }
        Debug.Log($"Full grid ({w}×{h}):\n{sb}");
    }

    /// <summary>
    /// Allocates a new byte[,] of the given dimensions and fills every cell with 1 (wall).
    /// </summary>
    public byte[,] InitializeMapArray(int width, int height)
    {
        var map = new byte[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = 1;
        return map;
    }

}
